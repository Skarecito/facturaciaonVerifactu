using API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Entities;
using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace FacturacionVERIFACTU.API.Services
{
    /// <summary>
    /// Servicio para implementar el sistema VERIFACTU de la AEAT.
    /// Genera huellas encadenadas y códigos QR según normativa oficial.
    /// </summary>
    public class VERIFACTUService
    {
        /// <summary>
        /// Genera la huella SHA-256 de una factura según el algoritmo VERIFACTU.
        /// Cadena de entrada: CIF + NumFactura + Fecha(yyyyMMdd) + TipoFactura + 
        ///                    CuotaIVA + ImporteTotal + "" + HuellaAnterior
        /// </summary>
        /// <param name="factura">Factura a procesar</param>
        /// <param name="huellaAnterior">Huella de la factura anterior (cadena vacía si es la primera)</param>
        /// <returns>Huella en formato Base64</returns>
        public string GenerarHuellaFactura(Factura factura, string huellaAnterior = "")
        {
            if (factura == null)
                throw new ArgumentNullException(nameof(factura));

            // Construir cadena según especificación VERIFACTU
            var cadena = new StringBuilder();

            // 1. CIF del emisor (desde Tenant)
            cadena.Append(factura.Tenant?.NIF ?? string.Empty);

            // 2. Número de factura completo (Serie + Numero)
            cadena.Append(factura.Serie?.Codigo ?? string.Empty);
            cadena.Append(factura.Numero);

            // 3. Fecha de emisión en formato yyyyMMdd
            cadena.Append(factura.FechaEmision.ToString("yyyyMMdd"));

            // 4. Tipo de factura (F1=Normal, F2=Simplificada, F3=Rectificativa)
            var tipoFactura = DeterminarTipoFactura(factura);
            cadena.Append(tipoFactura);

            // 5. Cuota total de IVA (con 2 decimales)
            cadena.Append(factura.TotalIVA.ToString("F2"));

            // 6. Importe total (con 2 decimales)
            cadena.Append(factura.Total.ToString("F2"));

            // 7. Campo vacío (reservado)
            cadena.Append("");

            // 8. Huella de la factura anterior
            cadena.Append(huellaAnterior);

            // Calcular SHA-256
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(cadena.ToString());
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Procesa una factura completa: genera huella, QR y actualiza campos VERIFACTU.
        /// </summary>
        /// <param name="factura">Factura a procesar</param>
        /// <param name="huellaAnterior">Huella de la factura anterior</param>
        public void ProcesarFacturaVERIFACTU(Factura factura, string huellaAnterior = "")
        {
            if (factura == null)
                throw new ArgumentNullException(nameof(factura));

            // 1. Determinar y guardar tipo de factura
            factura.TipoFacturaVERIFACTU = DeterminarTipoFactura(factura);

            // 2. Generar y guardar huella anterior
            factura.HuellaAnterior = huellaAnterior;

            // 3. Generar y guardar huella actual
            factura.Huella = GenerarHuellaFactura(factura, huellaAnterior);

            // 4. Generar y guardar QR
            factura.QRVerifactu = GenerarCodigoQR(factura);

            // 5. Guardar URL de verificación
            factura.UrlVERIFACTU = ConstruirUrlVerificacion(factura);

            // 6. Establecer ejercicio si no está definido
            if (factura.Ejercicio == 0)
            {
                factura.Ejercicio = factura.FechaEmision.Year;
            }
        }

        /// <summary>
        /// Genera un código QR con la URL de verificación de la AEAT.
        /// URL: https://prewww2.aeat.es/wlpl/TIKE-CONT/?nif=X&num=Y&fecha=Z&importe=W
        /// </summary>
        /// <param name="factura">Factura a incluir en el QR</param>
        /// <returns>Imagen QR en formato byte array (PNG)</returns>
        public byte[] GenerarCodigoQR(Factura factura)
        {
            if (factura == null)
                throw new ArgumentNullException(nameof(factura));

            // Construir URL de verificación AEAT
            var url = ConstruirUrlVerificacion(factura);

            // Generar QR con QRCoder
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
                using (var qrCode = new PngByteQRCode(qrData))
                {
                    return qrCode.GetGraphic(20); // 20 píxeles por módulo
                }
            }
        }

        /// <summary>
        /// Construye la URL de verificación para el portal TIKE de la AEAT.
        /// </summary>
        private string ConstruirUrlVerificacion(Factura factura)
        {
            var baseUrl = "https://prewww2.aeat.es/wlpl/TIKE-CONT/";

            var nif = Uri.EscapeDataString(factura.Tenant?.NIF ?? string.Empty);
            var num = Uri.EscapeDataString($"{factura.Serie?.Codigo}{factura.Numero}");
            var fecha = factura.FechaEmision.ToString("ddMMyyyy");
            var importe = factura.Total.ToString("F2").Replace(",", ".");

            return $"{baseUrl}?nif={nif}&num={num}&fecha={fecha}&importe={importe}";
        }

        /// <summary>
        /// Determina el tipo de factura según criterios VERIFACTU.
        /// F1: Factura normal
        /// F2: Factura simplificada (importe inferior a 400€)
        /// F3: Factura rectificativa
        /// </summary>
        private string DeterminarTipoFactura(Factura factura)
        {
            // Verificar si es rectificativa
            if (!string.IsNullOrEmpty(factura.NumeroFacturaRectificada) ||
                !string.IsNullOrEmpty(factura.TipoRectificacion))
            {
                return "F3";
            }

            // Verificar si es simplificada (< 400€)
            if (factura.Total < 400)
            {
                return "F2";
            }

            // Por defecto es normal
            return "F1";
        }

        /// <summary>
        /// Valida que una huella calculada coincida con la esperada.
        /// Útil para verificar la integridad de la cadena de facturas.
        /// </summary>
        /// <param name="factura">Factura a validar</param>
        /// <param name="huellaAnterior">Huella de la factura anterior</param>
        /// <param name="huellaEsperada">Huella almacenada en la factura</param>
        /// <returns>True si la huella es válida</returns>
        public bool ValidarHuella(Factura factura, string huellaAnterior, string huellaEsperada)
        {
            var huellaCalculada = GenerarHuellaFactura(factura, huellaAnterior);
            return huellaCalculada == huellaEsperada;
        }

        /// <summary>
        /// Genera el código QR como cadena Base64 (útil para APIs/JSON).
        /// </summary>
        public string GenerarCodigoQRBase64(Factura factura)
        {
            var qrBytes = GenerarCodigoQR(factura);
            return Convert.ToBase64String(qrBytes);
        }

        /// <summary>
        /// Valida la cadena de facturas completa para un tenant.
        /// </summary>
        /// <param name="facturas">Lista de facturas ordenadas por fecha</param>
        /// <returns>True si toda la cadena es válida</returns>
        public bool ValidarCadenaFacturas(List<Factura> facturas)
        {
            if (facturas == null || !facturas.Any())
                return true;

            string huellaAnterior = "";

            foreach (var factura in facturas.OrderBy(f => f.FechaEmision))
            {
                // Validar que la huella anterior coincida
                if (factura.HuellaAnterior != huellaAnterior)
                    return false;

                // Validar que la huella actual sea correcta
                var huellaCalculada = GenerarHuellaFactura(factura, huellaAnterior);
                if (factura.Huella != huellaCalculada)
                    return false;

                // Actualizar huella anterior para la siguiente iteración
                huellaAnterior = factura.Huella ?? "";
            }

            return true;
        }
    }
}