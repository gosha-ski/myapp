using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Views
{
    public partial class VerificationOperationsView : UserControl
    {
        public ObservableCollection<InstrumentModel> Instruments { get; } = new();
        //public ObservableCollection<object> ProcessData { get; } = new();

        private readonly NewVerificationWindow _ownerWindow;

        public VerificationOperationsView(Window ownerWindow)
        {
            InitializeComponent();
            InitializeSampleData();

            _ownerWindow = (NewVerificationWindow)ownerWindow;

            DgDevices.ItemsSource = Instruments;
            //DgProcessData.ItemsSource = ProcessData;

            DgDevices.SelectionChanged += OnDeviceSelectionChanged;
        }

        private void InitializeSampleData()
        {
            
           
        }

        private void OnDeviceSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Здесь можно подгружать данные процесса для выбранного прибора
            // Например, по Channel или SerialNumber
        }
    }

    
}
