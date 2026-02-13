using System.Net;

namespace FacturacionVERIFACTU.Web.Services;

public sealed record ApiResult(bool Success, HttpStatusCode StatusCode, string? ErrorMessage);
