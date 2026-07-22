using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyAvaloniaApp.Models;
using System.Collections.Generic;
using System;
using MyAvaloniaApp;
using Autofac;

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
            _ownerWindow = (NewVerificationWindow)ownerWindow;
            BtnStartReading.Click += BtnStartReadingClicked;

            Console.WriteLine("VerificationOperationsView PFPFFPFPFP");

            //InitializeSampleData();
            //LoadInstruments();

            //DgDevices.ItemsSource = Instruments;
            //DgProcessData.ItemsSource = ProcessData;

            DgDevices.SelectionChanged += OnDeviceSelectionChanged;
        }

        public void LoadInstruments()
        {
            List<InstrumentModel> instruments = DbHelper.GetInstrumentsByVerificationId(_ownerWindow.VerificationId);
            if(instruments !=null){
                DgDevices.ItemsSource = instruments;
            }
            return;
        }

        private void InitializeSampleData()
        {
            
           
        }

        private void OnDeviceSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Здесь можно подгружать данные процесса для выбранного прибора
            // Например, по Channel или SerialNumber
        }

        public void BtnStartReadingClicked(object? sender, RoutedEventArgs e)
        {
            var selectedInstrument = DgDevices.SelectedItem as InstrumentModel;
            if(selectedInstrument != null)
            {   
                var dialog = new DataReadingWindow(
                    _ownerWindow, 
                    selectedInstrument.Id, 
                    Bootstrapper.Container.Resolve<MeasurementService>(),
                    Bootstrapper.Container.Resolve<PressureService>()
                    );
                dialog.ShowDialog(_ownerWindow);
            }
            
        }
    }

    
}
