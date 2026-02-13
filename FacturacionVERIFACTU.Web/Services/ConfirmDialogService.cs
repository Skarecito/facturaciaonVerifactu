namespace FacturacionVERIFACTU.Web.Services;

public sealed class ConfirmDialogService
{
    private Func<Task>? _onConfirm;

    public bool IsVisible { get; private set; }
    public bool IsBusy { get; private set; }
    public string Title { get; private set; } = "Confirmar";
    public string Message { get; private set; } = string.Empty;
    public string ConfirmButtonText { get; private set; } = "Confirmar";
    public string CancelButtonText { get; private set; } = "Cancelar";
    public string? ErrorMessage { get; private set; }

    public event Action? OnChange;

    public void Show(ConfirmDialogOptions options, Func<Task> onConfirm)
    {
        Title = options.Title;
        Message = options.Message;
        ConfirmButtonText = options.ConfirmButtonText;
        CancelButtonText = options.CancelButtonText;
        ErrorMessage = null;
        _onConfirm = onConfirm;
        IsVisible = true;
        Notify();
    }

    public async Task ConfirmAsync()
    {
        if (_onConfirm is null || IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        Notify();

        try
        {
            await _onConfirm();
            Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(ex.Message)
                ? "No se pudo completar la acción."
                : ex.Message;
        }
        finally
        {
            IsBusy = false;
            Notify();
        }
    }

    public void Cancel()
    {
        if (IsBusy)
        {
            return;
        }

        Close();
        Notify();
    }

    private void Close()
    {
        IsVisible = false;
        _onConfirm = null;
    }

    private void Notify() => OnChange?.Invoke();
}

public sealed record ConfirmDialogOptions(
    string Title,
    string Message,
    string ConfirmButtonText = "Confirmar",
    string CancelButtonText = "Cancelar");
