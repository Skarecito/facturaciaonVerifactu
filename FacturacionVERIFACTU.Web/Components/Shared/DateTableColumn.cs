namespace FacturacionVERIFACTU.Web.Components.Shared;

public class DataTableColumn<TItem>
{
    public string Header { get; init; } = string.Empty;
    public Func<TItem, object?> Value { get; init; } = _ => string.Empty;
}
