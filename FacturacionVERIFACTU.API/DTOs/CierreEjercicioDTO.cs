using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.CompilerServices;

namespace FacturacionVERIFACTU.API.DTOs
{
    public class CierreEjercicioDTO
    {
        public int Id { get; set; }
        public int TentantId {  get; set; }
        public int Ejercicio {  get; set; }
        public DateTime FechaCierre {  get; set; }
        public int UsuarioId {  get; set; }
        public string NombreUsuario {  get; set; }

        //VERIFACTU
        public string HashFinal { get; set; } = string.Empty;
        public bool EnviadoVERIFACTU {  get; set; }
        public DateTime? FechaEnvio { get; set; }

        //Estadisticas
        public int TotalFactuas {  get; set; }
        public decimal TotalBaseImponible {  get; set; }
        public decimal TotalIVA {  get; set; }
        public decimal TotalRecargo {  get; set; }
        public decimal TotalRetencion {  get; set; }
        public decimal TotalImporte {  get; set; }

        //Archivos
        public string? RutaLibroFacturas {  get; set; }
        public string? RutaResumenIVA { get; set; }
        public bool TieneLibroFacturas => !string.IsNullOrEmpty(RutaLibroFacturas);
        public bool TieneResumenIVA => !string.IsNullOrEmpty(RutaResumenIVA);


        //Reapertura
        public bool EstaAbierto {  get; set; }
        public string? MotivoReapertura {  get; set; }
        public DateTime? FechaReapertura { get; set; }
        public int? UsuarioReaperturaId {  get; set; }
        public string? NombreUsuarioReapertura { get; set; }

        //Auditoria
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn {  get; set; }
    }


    public class ResumenTrimestreDTO
    {
        public int Trimestre {  get; set; }
        public string NombreTimestre {  get; set; }
        public int NumFacturas { get; set; }
        public decimal BaseImponible {  get; set; }
        public decimal TotalIva { get; set; }
        public decimal Recargo {  get; set; }
        public decimal Total {  get; set; }
    }

    public class EstadisticasCierreDTO
    {
        public int Ejercicio {  get; set; }
        public int TotalFacturas { get; set; }
        public int FacturasEnviadas {  get; set; }
        public int FacturasPendientes {  get; set; }
        public bool PuedeCerrar => FacturasPendientes == 0 && TotalFacturas > 0;

        public decimal TotalBase {  get; set; }
        public decimal TotalIVA { get; set; }
        public decimal TotalRecargo {  set; get; }
        public decimal TotalRetencion {  get; set; }
        public decimal TotalGeneral {  get; set; }

        public List<ResumenTrimestreDTO> ResumenTrimestral { get; set; } = new();
    }

    public class CierreRealizadoDTO
    {
        public int CierreId {  get; set; }
        public int Ejercicio {  set; get; }
        public string Mensaje { get; set; } = string.Empty;
        public string HashFinal { get; set; } = string.Empty;
        public EstadisticasDTO Estadisticas { get; set; } = new();
        public ArchivosGeneradosDTO Archivos {  get; set; } = new();
    }


    public class EstadisticasDTO
    {
        public int TotalFacturas { get; set; }
        public decimal TotalBaseImponible {  get; set; }
        public decimal TotalIVA {  set; get; }
        public decimal TotalRecargo {  get; set; }
        public decimal TotalRetencion {  set; get; }
        public decimal TotalImporte {  set; get; }
    }


    public class ArchivosGeneradosDTO
    {
        public string LibroFacturas { get; set; } = string.Empty;
        public string ResumenIVA { get; set; } = string.Empty;
        public bool LibroGenerado => !string.IsNullOrEmpty(LibroFacturas);
        public bool ResumenGenerado => !string.IsNullOrEmpty(ResumenIVA);
    }


    // ========== Request DTOs ==========

    public class CierrarEjercicioRequest
    {
        public int Ejercicio {  set; get; }
    }

    public class ReabrirEjercicioRequest
    {
        public int CierreId {  set; get; }
        public string Motivo { set; get; } = string.Empty;
    }
}
