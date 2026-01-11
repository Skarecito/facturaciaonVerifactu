using API.Data.Entities;
using FacturacionVERIFACTU.API.Data;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Services;
using FacturacionVERIFACTU.API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FacturacionVERIFACTU.Tests
{
    public class VERIFACTUServiceTests
    {
        private readonly VERIFACTUService _service;
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<AEATClient> _mockAEATClient;
        private readonly Mock<ILogger<VERIFACTUService>> _mockLogger;

        public VERIFACTUServiceTests()
        {
            // Crear mocks
            _mockContext = new Mock<ApplicationDbContext>();
            _mockAEATClient = new Mock<AEATClient>();
            _mockLogger = new Mock<ILogger<VERIFACTUService>>();

            // Crear servicio con todas las dependencias
            _service = new VERIFACTUService(
                _mockContext.Object,
                _mockAEATClient.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public void GenerarHuellaFactura_PrimeraFactura_GeneraHuellaCorrecto()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );

            // Act
            var huella = _service.GenerarHuellaFactura(factura, "");

            // Assert
            Assert.NotNull(huella);
            Assert.NotEmpty(huella);
            Assert.True(huella.Length > 40, "La huella SHA-256 en Base64 debe tener más de 40 caracteres");
        }

        [Fact]
        public void GenerarHuellaFactura_FacturasEncadenadas_GeneranHuellasDiferentes()
        {
            // Arrange
            var factura1 = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );

            var factura2 = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "002",
                fecha: new DateTime(2024, 1, 16),
                totalIva: 42.00m,
                total: 242.00m
            );

            // Act
            var huella1 = _service.GenerarHuellaFactura(factura1, "");
            var huella2 = _service.GenerarHuellaFactura(factura2, huella1);

            // Assert
            Assert.NotEqual(huella1, huella2);
            Assert.NotEmpty(huella1);
            Assert.NotEmpty(huella2);
        }

        [Fact]
        public void GenerarHuellaFactura_MismosParametros_GeneraMismaHuella()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );

            // Act
            var huella1 = _service.GenerarHuellaFactura(factura, "");
            var huella2 = _service.GenerarHuellaFactura(factura, "");

            // Assert
            Assert.Equal(huella1, huella2);
        }

        [Fact]
        public void GenerarHuellaFactura_FacturaNula_LanzaExcepcion()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.GenerarHuellaFactura(null!, ""));
        }

        [Fact]
        public void GenerarCodigoQR_FacturaValida_GeneraImagenQR()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );

            // Act
            var qrBytes = _service.GenerarCodigoQR(factura);

            // Assert
            Assert.NotNull(qrBytes);
            Assert.True(qrBytes.Length > 0, "El QR debe generar bytes");

            // Verificar que es una imagen PNG (primeros bytes: 89 50 4E 47)
            Assert.Equal(0x89, qrBytes[0]);
            Assert.Equal(0x50, qrBytes[1]);
            Assert.Equal(0x4E, qrBytes[2]);
            Assert.Equal(0x47, qrBytes[3]);
        }

        [Fact]
        public void GenerarCodigoQR_FacturaNula_LanzaExcepcion()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.GenerarCodigoQR(null!));
        }

        [Fact]
        public void ValidarHuella_HuellaCorrecto_RetornaTrue()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );
            var huellaAnterior = "";
            var huellaEsperada = _service.GenerarHuellaFactura(factura, huellaAnterior);

            // Act
            var esValido = _service.ValidarHuella(factura, huellaAnterior, huellaEsperada);

            // Assert
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarHuella_HuellaIncorrecto_RetornaFalse()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );
            var huellaAnterior = "";
            var huellaIncorrecto = "HuellaInvalidoParaPrueba123==";

            // Act
            var esValido = _service.ValidarHuella(factura, huellaAnterior, huellaIncorrecto);

            // Assert
            Assert.False(esValido);
        }

        [Fact]
        public void GenerarCodigoQRBase64_FacturaValida_RetornaCadenaBase64()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );

            // Act
            var qrBase64 = _service.GenerarCodigoQRBase64(factura);

            // Assert
            Assert.NotNull(qrBase64);
            Assert.NotEmpty(qrBase64);

            // Verificar que es Base64 válido
            var bytes = Convert.FromBase64String(qrBase64);
            Assert.True(bytes.Length > 0);
        }

        [Theory]
        [InlineData(350.00, "F2")] // Simplificada < 400€
        [InlineData(500.00, "F1")] // Normal >= 400€
        [InlineData(1000.00, "F1")] // Normal
        public void ProcesarFacturaVERIFACTU_DiferentesTiposFactura_AsignaTipoCorrecto(
            decimal total, string tipoEsperado)
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: total * 0.21m,
                total: total
            );

            // Act
            _service.ProcesarFacturaVERIFACTU(factura, "");

            // Assert
            Assert.Equal(tipoEsperado, factura.TipoFacturaVERIFACTU);
            Assert.NotNull(factura.Huella);
            Assert.NotNull(factura.QRVerifactu);
            Assert.NotNull(factura.UrlVERIFACTU);
        }

        [Fact]
        public void ProcesarFacturaVERIFACTU_FacturaCompleta_GeneraTodosCampos()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );
            var huellaAnterior = "HuellaAnteriorTest123==";

            // Act
            _service.ProcesarFacturaVERIFACTU(factura, huellaAnterior);

            // Assert
            Assert.NotNull(factura.Huella);
            Assert.NotEmpty(factura.Huella);
            Assert.Equal(huellaAnterior, factura.HuellaAnterior);
            Assert.NotNull(factura.TipoFacturaVERIFACTU);
            Assert.NotNull(factura.QRVerifactu);
            Assert.True(factura.QRVerifactu.Length > 0);
            Assert.NotNull(factura.UrlVERIFACTU);
            Assert.Contains("prewww2.aeat.es", factura.UrlVERIFACTU);
            Assert.True(factura.Ejercicio > 0);
        }

        [Fact]
        public void ValidarCadenaFacturas_CadenaValida_RetornaTrue()
        {
            // Arrange
            var facturas = new List<Factura>
            {
                CrearFacturaPrueba("B12345678", "001", new DateTime(2024, 1, 15), 21.00m, 121.00m),
                CrearFacturaPrueba("B12345678", "002", new DateTime(2024, 1, 16), 42.00m, 242.00m),
                CrearFacturaPrueba("B12345678", "003", new DateTime(2024, 1, 17), 63.00m, 363.00m)
            };

            // Generar huellas encadenadas
            string huellaAnterior = "";
            foreach (var factura in facturas)
            {
                _service.ProcesarFacturaVERIFACTU(factura, huellaAnterior);
                huellaAnterior = factura.Huella ?? "";
            }

            // Act
            var esValida = _service.ValidarCadenaFacturas(facturas);

            // Assert
            Assert.True(esValida);
        }

        [Fact]
        public void ValidarCadenaFacturas_HuellaAnteriorIncorrecta_RetornaFalse()
        {
            // Arrange
            var facturas = new List<Factura>
            {
                CrearFacturaPrueba("B12345678", "001", new DateTime(2024, 1, 15), 21.00m, 121.00m),
                CrearFacturaPrueba("B12345678", "002", new DateTime(2024, 1, 16), 42.00m, 242.00m)
            };

            // Procesar primera factura correctamente
            _service.ProcesarFacturaVERIFACTU(facturas[0], "");

            // Procesar segunda con huella anterior incorrecta
            _service.ProcesarFacturaVERIFACTU(facturas[1], "HuellaIncorrecta123==");

            // Act
            var esValida = _service.ValidarCadenaFacturas(facturas);

            // Assert
            Assert.False(esValida);
        }

        // Helper para crear facturas de prueba
        private Factura CrearFacturaPrueba(
            string cif,
            string numero,
            DateTime fecha,
            decimal totalIva,
            decimal total)
        {
            return new Factura
            {
                Id = 1,
                Numero = numero,
                FechaEmision = fecha,
                TotalIVA = totalIva,
                Total = total,
                BaseImponible = total - totalIva,
                Tenant = new Tenant
                {
                    Id = 1,
                    NIF = cif,
                    Nombre = "Empresa Test"
                },
                Serie = new SerieNumeracion
                {
                    Id = 1,
                    Codigo = "FAC",
                    ProximoNumero = 1,
                    TipoDocumento = "Factura",
                    Ejercicio = fecha.Year
                },
                Cliente = new Cliente
                {
                    Id = 1,
                    Nombre = "Cliente Test"
                }
            };
        }
    }
}