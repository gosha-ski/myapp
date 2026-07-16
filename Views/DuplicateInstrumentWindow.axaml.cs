using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace MyAvaloniaApp.Views
{
    public partial class DuplicateInstrumentWindow : Window
    {
        public string? SerialNumber { get; private set; }
        public string? InventoryNumber { get; private set; }
        public bool IsConfirmed { get; private set; } = false;

        private readonly Window _ownerWindow;
        public event Action? OnSaved;

        public DuplicateInstrumentWindow()
        {
            InitializeComponent();
            
            OkButton.Click += OnOkClick;
            CancelButton.Click += OnCancelClick;
        }

        private void OnOkClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("HELLO FROM DUPLICTAE");
            SerialNumber = SerialNumberInput.Text;
            InventoryNumber = InventoryNumberInput.Text;
            IsConfirmed = true;
            OnSaved?.Invoke();
            this.Close();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}