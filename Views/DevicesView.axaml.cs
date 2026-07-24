using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using MyAvaloniaApp;
using System.Threading.Tasks;
using MyAvaloniaApp.Models;
using System;
using System.Collections.Generic;

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
            LoadInstruments();
        }

        private void LoadInstruments()
        {
            //InstrumentWithChannelModel
            //List<InstrumentModel> instruments = DbHelper.GetInstrumentsByVerificationId(_ownerWindow.VerificationId);
            List<InstrumentWithChannelModel> instruments = DbHelper.GetInstrumentsWithChannelsByVerificationId(_ownerWindow.VerificationId);
            SelectedInstrumentsGrid.ItemsSource = instruments;
        }

        private async void OnAddClick(object? sender, RoutedEventArgs e)
        {
            var dialog = new SelectInstrumentListWindow();
            await dialog.ShowDialog(_ownerWindow);
            if(dialog.SelectedInstrument!=null){
                InstrumentModel instrument = dialog.SelectedInstrument;
                Devices.Add(instrument);
                _ownerWindow.SelectedInstruments.Add(instrument);
                
                DbHelper.AddInstrumentToVerification(_ownerWindow.VerificationId, instrument.Id, null);
                LoadInstruments();
            }
        }

       

        private void OnRemoveClick(object? sender, RoutedEventArgs e)
        {
            if (Devices.Count > 0) 
                Devices.RemoveAt(Devices.Count - 1);
            var selectedItem = SelectedInstrumentsGrid.SelectedItem as InstrumentWithChannelModel;
            if(selectedItem != null)
            {
                DbHelper.RemoveInstrumentFromVerification(_ownerWindow.VerificationId, selectedItem.Id);
                LoadInstruments();
            }
            
        }

        private async void OnSetChannelClick(object? sender, RoutedEventArgs e)
        {
            var selectedItem = SelectedInstrumentsGrid.SelectedItem as InstrumentWithChannelModel;
            Console.WriteLine($"OnSetChannelClick {selectedItem.Id}");
            if (selectedItem != null)
            {
                var dialog = new SetChannelWindow(_ownerWindow.VerificationId, selectedItem.Id);
                await dialog.ShowDialog(_ownerWindow);
                LoadInstruments();
            }
           
        }

        

    }

}