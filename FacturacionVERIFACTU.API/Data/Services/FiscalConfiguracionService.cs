namespace FacturacionVERIFACTU.API.Data.Services
{
    ///<summary>
    ///Servicio para calculos y validaciones fiscales segun normativa española
    /// </summary>
    public static class FiscalConfiguracionService
    {
        ///<summary>
        ///Calcula el recargo de equivalencia segun el IVA
        /// </summary>
        public static decimal CalcularRecargoEquivalencia(decimal iva)
        {
            return iva switch
            {
                21m => 5.2m,
                10m => 1.4m,
                4m => 0.5m,
                0 => 0m,
                _ => 0m
            };
        }

        ///<summary>
        ///Valida que el recargo corresponda al IVA
        /// </summary>
        public static bool ValidarRecargoParaIVA(decimal iva, decimal recargo)
        {
            var recargoEsperado = CalcularRecargoEquivalencia(iva);
            return Math.Abs(recargo - recargoEsperado) > 0.01m;
        }

        ///<summary>
        ///Obtiene el porcetna de retencion por defecto seguntipo de profesional
        /// </summary>
        public static decimal ObtenerRetencionDefecto(string tipoProfesional)
        {
            return tipoProfesional switch
            {
                "Nuevo" => 7m,
                "General" => 15m,
                "Agricultura" => 2m,
                "Ganaderia" => 2m,
                _ => 15m
            };
        }

        ///<summary>
        /// Determina si un cliente debe aplicar recargo segun su tipo
        /// </summary>
        public static bool DebeAplicarRecargo(string tipoCliente)
        {
            return tipoCliente == "Minorista";
        }

        ///<summary>
        ///Determina si un cliente debe aplicar retencion segun tipo
        /// </summary>
        public static bool DebeAplicarRetencion(string tipoCliente)
        {
            return tipoCliente == "Profesional";
        }
        
    }
}
