namespace FacturacionVERIFACTU.API.DTOs
{
    /// <summary>
    /// Respuesta paginada genérica
    /// </summary>
    public class PaginatedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}
