using API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Entities;
using FacturacionVERIFACTU.API.Data.Services;
using FacturacionVERIFACTU.API.Services;
using Xunit;

namespace FacturacionVERIFACTU.Test
{
    public class VERIFACTUServiceTest
    {
        public readonly VERIFACTUService _service;

        public VERIFACTUServiceTest()
        {
            _service = new VERIFACTUService();
        }

        [Fact]
        public void GenerarHashFactura_PrimeraFactura_GenerarHashCorrecto()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );

            //Act
            var hash = _service.GenerarHuellaFactura(factura, "");

            //Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.True(hash.Length > 40, "El hash SAH-256 en Base64 debe tener mas de 40 caracteres");
        }

        [Fact]
        public void GenerarHashFactura_FacturasEncadenadas_GeneranHashesDiferentes()
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
            var hash1 = _service.GenerarHuellaFactura(factura1, "");
            var hash2 = _service.GenerarHuellaFactura(factura2, hash1);

            // Assert
            Assert.NotEqual(hash1, hash2);
            Assert.NotEmpty(hash1);
            Assert.NotEmpty(hash2);
        }


        [Fact]
        public void GenerarHashFactura_MismosParametros_GenerarMismoHash()
        {
            //Arranque
            var factura = CrearFacturaPrueba(
                    cif: "B12345678",
                    numero: "001",
                    fecha: new DateTime(2024, 1, 15),
                    totalIva: 21.00m,
                    total: 121.00m
                );

            //Act
            var hash1 = _service.GenerarHuellaFactura(factura, "");
            var hash2 = _service.GenerarHuellaFactura(factura, "");

            //Assert
            Assert.Equal(hash1,hash2 );
        }

        [Fact]
        public void GenerarHashFactura_FacturaNula_LanzaExcepcion()
        {
            //act & Assert
            Assert.Throws < ArgumentNullException>(() =>
                _service.GenerarHuellaFactura(null!, ""));
        }


        [Fact]
        public void GenerarCodigoQR_FacturaValida_GeneraImagenQR()
        {
            //Arranque
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m);

            //Act
            var qrBytes = _service.GenerarCodigoQR(factura);

            //Assert
            Assert.NotNull(qrBytes);
            Assert.True(qrBytes.Length > 0, "El QR debe generar bytes");

            //Verificar que es una imgaen PNG
            Assert.Equal(0x89, qrBytes[0]);
            Assert.Equal(0x50, qrBytes[1]);
            Assert.Equal(0x4E, qrBytes[2]);
            Assert.Equal(0x47, qrBytes[3]);
        }


        [Fact]
        public void GenerarCodigoQR_FacturaNula_LanzaExcepcion()
        {
            // Act y Assert
            Assert.Throws<ArgumentNullException>(() =>
                _service.GenerarCodigoQR(null!));
        }


        [Fact]
        public void ValidarHash_HashCorrecto_RetornaTrue()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );
            var hashAnterior = "";
            var hashEsperado = _service.GenerarHuellaFactura(factura, hashAnterior);

            // Act
            var esValido = _service.ValidarHuella(factura, hashAnterior, hashEsperado);

            // Assert
            Assert.True(esValido);
        }

        [Fact]
        public void ValidarHash_HashIncorrecto_RetornaFalse()
        {
            // Arrange
            var factura = CrearFacturaPrueba(
                cif: "B12345678",
                numero: "001",
                fecha: new DateTime(2024, 1, 15),
                totalIva: 21.00m,
                total: 121.00m
            );
            var hashAnterior = "";
            var hashIncorrecto = "HashInvalidoParaPrueba123==";

            // Act
            var esValido = _service.ValidarHuella(factura, hashAnterior, hashIncorrecto);

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
        public void GenerarHash_DiferentesTiposFactura_GeneraHashesCorrectos(
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
            var hash = _service.GenerarHuellaFactura(factura, "");

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            // El tipo de factura está incluido en el hash internamente
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
                    ProximoNumero = 1
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