using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Linq;
using MyAvaloniaApp.Models;
using MyAvaloniaApp;
using System;
using System.Threading.Tasks;

namespace MyAvaloniaApp.Views
{
    public partial class DataReadingWindow : Window
    {
        //public ObservableCollection<CalibrationPointModel> CalibrationPoints { get; } = new();
        private readonly NewVerificationWindow _ownerWindow;
        private readonly VerificationOperationsView _ownerControl;
        private int _instrumentId;

        private readonly MeasurementService _measurement;
        private readonly PressureService _pressureService;

        public DataReadingWindow(
            Window ownerWindow,
            UserControl ownerControl,
            int instrumentId, 
            MeasurementService measurement,
            PressureService pressureService
            )
        {
            InitializeComponent();
            _measurement = measurement;
            _pressureService = pressureService;
            _ownerWindow = (NewVerificationWindow)ownerWindow;
            _ownerControl = (VerificationOperationsView)ownerControl;
            _instrumentId = instrumentId;
            InitializeData();
        }

        private void InitializeData()
        {            
            List<CalibrationPointModel> points = DbHelper.GetLoadingPointsByVerificationAndInstrument(_ownerWindow.VerificationId, _instrumentId);           
            CalibrationPointsGrid.ItemsSource = points;
        }

        private async void BtnFixPointClicked(object? sender, RoutedEventArgs e)
        {
            var selectedItem = CalibrationPointsGrid.SelectedItem as CalibrationPointModel;
            if (selectedItem != null)
            {
                //double InputTemplateValue = Double.Parse(InputTemplateValueBox?.Text ?? "0") ;
                //double OutputTemplateValue = Double.Parse(OutputTemplateValueBox?.Text ?? "0");

                double InputTemplateValue = 0 ; 
                double OutputTemplateValue = 0 ;

                double? AverageCurrent = 0;
                await _pressureService.Unlock();
                var Result = await _pressureService.RunAsync(TimeSpan.FromSeconds(5));

     

                Console.WriteLine(
                    $"BtnFixPointClicked|| InputTemplateValue: {InputTemplateValue};  " +
                    $"OutputTemplateValue: {OutputTemplateValue} current: {Result.Current ?? null}, pressure:{Result.Pressure ?? null}"
                    );
                DbHelper.UpdateLoadingPointValues(selectedItem.LoadingPointId, Result.Pressure, Result.Current );
                InitializeData();
                _ownerControl.LoadPoints(_instrumentId);
            }

        }
    }

    
}
