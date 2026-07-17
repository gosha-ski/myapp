using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using MyAvaloniaApp;
using System.Threading.Tasks;
using MyAvaloniaApp.Models;
using System;

namespace MyAvaloniaApp.Views
{
    public partial class DevicesView : UserControl
    {
        public ObservableCollection<InstrumentModel> Devices { get; } = new();
        private readonly NewVerificationWindow _ownerWindow;

        public DevicesView(Window ownerWindow)
        {
            InitializeComponent();
            //this.DataContext = this; 
            _ownerWindow = (NewVerificationWindow)ownerWindow;
        }

        private async void OnAddClick(object? sender, RoutedEventArgs e)
        {
            var dialog = new SelectInstrumentListWindow();
            await dialog.ShowDialog(_ownerWindow);
            if(dialog.SelectedInstrument!=null){
                InstrumentModel instrument = dialog.SelectedInstrument;
                //Console.WriteLine($"INSIDE OnAddClick TypeCode:{instrument.TypeCode}");
                Devices.Add(instrument);
                _ownerWindow.SelectedInstruments.Add(instrument);
                SelectedInstrumentsGrid.ItemsSource = Devices;
            }
        }

        private void OnRemoveClick(object? sender, RoutedEventArgs e)
        {
            if (Devices.Count > 0)
                Devices.RemoveAt(Devices.Count - 1);
        }

        private void OnSetChannelClick(object? sender, RoutedEventArgs e)
        {
            // Логика установки номера канала (можно добавить диалог ввода)
        }

        

    }

}