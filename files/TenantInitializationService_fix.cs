// ─────────────────────────────────────────────────────────────────────────────
// FIX: Reemplazar el método CrearSeriesNumeracionAsync completo
// El bug: se llamaba AddRangeAsync ANTES de verificar los existentes,
// causando duplicados si se llamaba dos veces al servicio.
// ─────────────────────────────────────────────────────────────────────────────

private async Task CrearSeriesNumeracionAsync(int tenantId)
{
    var anoActual = DateTime.Now.Year;

    // 1. Verificar cuáles ya existen ANTES de crear nada
    var seriesExistentes = await _context.SeriesNumeracion
        .Where(s => s.TenantId == tenantId && s.Ejercicio == anoActual)
        .Select(s => new { s.Codigo, TipoDoc = s.TipoDocumento.ToUpperInvariant() })
        .ToListAsync();

    var existentesSet = new HashSet<string>(
        seriesExistentes.Select(s => $"{s.Codigo}|{s.TipoDoc}"));

    // 2. Definir series por defecto
    var series = new List<SerieNumeracion>
    {
        new SerieNumeracion
        {
            TenantId      = tenantId,
            Codigo        = "F",
            TipoDocumento = DocumentTypes.FACTURA,
            Ejercicio     = anoActual,
            ProximoNumero = 1,
            Formato       = "{SERIE}-{NUMERO}/{EJERCICIO}",
            Descripcion   = "Serie F de facturas",
            Activo        = true
        },
        new SerieNumeracion
        {
            TenantId      = tenantId,
            Codigo        = "A",
            TipoDocumento = DocumentTypes.ALBARAN,
            Ejercicio     = anoActual,
            ProximoNumero = 1,
            Formato       = "{SERIE}-{NUMERO}/{EJERCICIO}",
            Descripcion   = "Serie A de albaranes",
            Activo        = true
        },
        new SerieNumeracion
        {
            TenantId      = tenantId,
            Codigo        = "P",
            TipoDocumento = DocumentTypes.PRESUPUESTO,
            Ejercicio     = anoActual,
            ProximoNumero = 1,
            Formato       = "{SERIE}-{NUMERO}/{EJERCICIO}",
            Descripcion   = "Serie principal de presupuestos",
            Activo        = true
        },
        new SerieNumeracion
        {
            TenantId      = tenantId,
            Codigo        = "R",
            TipoDocumento = DocumentTypes.FACTURA,
            Ejercicio     = anoActual,
            ProximoNumero = 1,
            Formato       = "{SERIE}-{NUMERO}/{EJERCICIO}",
            Descripcion   = "Serie de facturas rectificativas",
            Activo        = true
        }
    };

    // 3. Filtrar solo las que NO existen ya
    var seriesNuevas = series
        .Where(s => !existentesSet.Contains(
            $"{s.Codigo}|{s.TipoDocumento.ToUpperInvariant()}"))
        .ToList();

    if (seriesNuevas.Count == 0)
    {
        _logger.LogInformation(
            "Series de numeración ya existentes para tenant {TenantId} y ejercicio {Ano}",
            tenantId, anoActual);
        return;
    }

    await _context.SeriesNumeracion.AddRangeAsync(seriesNuevas);

    _logger.LogInformation("Creadas {Count} series de numeración para tenant {TenantId}",
        seriesNuevas.Count, tenantId);
}
