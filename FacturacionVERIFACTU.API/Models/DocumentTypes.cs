namespace FacturacionVERIFACTU.API.Models
{
    public static class DocumentTypes
    {
        public const string FACTURA = "FACTURA";
        public const string PRESUPUESTO = "PRESUPUESTO";
        public const string ALBARAN = "ALBARAN";

        private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            FACTURA,
            PRESUPUESTO,
            ALBARAN
        };

        public static bool IsValid(string? tipoDocumento)
        {
            if (string.IsNullOrWhiteSpace(tipoDocumento))
            {
                return false;
            }

            return Allowed.Contains(tipoDocumento);
        }

        public static string Normalize(string tipoDocumento)
        {
            return tipoDocumento.Trim().ToUpperInvariant();
        }
    }
}
