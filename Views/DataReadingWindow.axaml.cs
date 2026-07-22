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
        private int _instrumentId;

        private readonly MeasurementService _measurement;
        private readonly PressureService _pressureService;

        public DataReadingWindow(
            Window ownerWindow, 
            int instrumentId, 
            MeasurementService measurement,
            PressureService pressureService
            )
        {
            InitializeComponent();
            _measurement = measurement;
            _pressureService = pressureService;
            _ownerWindow = (NewVerificationWindow)ownerWindow;
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
                double InputTemplateValue = Double.Parse(InputTemplateValueBox.Text);
                double OutputTemplateValue = Double.Parse(OutputTemplateValueBox.Text);

                double? AverageCurrent = await _measurement.ReadAverageAsync(TimeSpan.FromSeconds(2));
                double? AveragePressure = await _pressureService.RunAsync(TimeSpan.FromSeconds(2));

                //var currentTask = _measurement.ReadAverageAsync(TimeSpan.FromSeconds(5));
                //var pressureTask = _pressureService.RunAsync(TimeSpan.FromSeconds(5));

                //await Task.WhenAll(currentTask, pressureTask);

                //double? AverageCurrent = await currentTask;
                //double? AveragePressure = await pressureTask;
                Console.WriteLine(
                    $"BtnFixPointClicked|| InputTemplateValue: {InputTemplateValue};  " +
                    $"OutputTemplateValue: {OutputTemplateValue} current: {AverageCurrent ?? null}, pressure:{AveragePressure}"
                    );
                DbHelper.UpdateLoadingPointValues(selectedItem.LoadingPointId, AveragePressure, AverageCurrent );
                InitializeData();
            }

        }
    }

    
}
