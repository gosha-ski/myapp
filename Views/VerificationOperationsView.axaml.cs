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

            Console.WriteLine("VerificationOperationsView");

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

        public void LoadPoints(int instrumentId)
        {
            List<CalibrationPointModel> points = DbHelper.GetLoadingPointsByVerificationAndInstrument(_ownerWindow.VerificationId, instrumentId);
            if (points != null)
            {
                DgProcessData.ItemsSource = points;
            }
            return;
        }

        private void InitializeSampleData()
        {
            
           
        }

        private void OnDeviceSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("OnDeviceSelectionChanged");
            var selectedInstrument = DgDevices.SelectedItem as InstrumentModel;
            if (selectedInstrument != null)
            {
                LoadPoints(selectedInstrument.Id);
            }
        }

        public void BtnStartReadingClicked(object? sender, RoutedEventArgs e)
        {
            var selectedInstrument = DgDevices.SelectedItem as InstrumentModel;
            if(selectedInstrument != null)
            {   
                var dialog = new DataReadingWindow(
                    _ownerWindow, 
                    this,
                    selectedInstrument.Id, 
                    Bootstrapper.Container.Resolve<MeasurementService>(),
                    Bootstrapper.Container.Resolve<PressureService>()
                    );
                dialog.ShowDialog(_ownerWindow);
            }
            
        }
    }

    
}
