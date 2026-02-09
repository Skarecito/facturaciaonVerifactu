using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace FacturacionVERIFACTU.Web.Services
{
    public class ConfirmDialogService
    {
        public event Action? OnChange;

        public bool IsOpen { get; private set; }
        public string Title { get; private set; } = "";
        public string Message { get; private set; } = "";
        public string ConfirmText { get; private set; } = "Confirmar";
        public string CancelText { get; private set; } = "Cancelar";
        public bool Danger { get; private set; }
        public bool IsBusy { get; private set; }

        private Func<Task> _onConfirm;
        private Func<Task> _onCancel;

        public void Show(
            string title,
            string message,
            Func<Task> onConfirm,
            Func<Task> onCancel,
            string confirmText = "Confirmar",
            string cancelText = "Cancelar",
            bool danger = false)
        {
            Title = title;
            Message = message;
            ConfirmText = confirmText;
            CancelText = cancelText;
            Danger = danger;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            IsBusy = false;
            IsOpen = true;
            Notify();
        }

        private void Close()
        {
            IsOpen = false;
            IsBusy = false;
            Notify();
        }

        public async Task ConfirmAsync()
        {
            if (_onConfirm == null) return;
            IsBusy = true;
            Notify();

            try
            {
                await _onConfirm();
            }
            finally
            {
                IsBusy = false;
                IsOpen= false;
                Notify();
            }
        }

        public async Task CancelAsync()
        {
            if (IsBusy) return;

            if(_onCancel != null) await _onCancel();

            Close();
        }




        private void Notify() => OnChange?.Invoke();
    }

    
}
