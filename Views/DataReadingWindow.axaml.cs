using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Linq;
using MyAvaloniaApp.Models;
using MyAvaloniaApp;
using System;

namespace MyAvaloniaApp.Views
{
    public partial class DataReadingWindow : Window
    {
        //public ObservableCollection<CalibrationPointModel> CalibrationPoints { get; } = new();
        private readonly NewVerificationWindow _ownerWindow;
        private int _instrumentId;

        public DataReadingWindow(Window ownerWindow, int instrumentId)
        {
            InitializeComponent();
            _ownerWindow = (NewVerificationWindow)ownerWindow;
            _instrumentId = instrumentId;
            InitializeData();
        }

        private void InitializeData()
        {            
            List<CalibrationPointModel> points = DbHelper.GetLoadingPointsByVerificationAndInstrument(_ownerWindow.VerificationId, _instrumentId);           
            CalibrationPointsGrid.ItemsSource = points;
        }

        private void BtnFixPointClicked(object? sender, RoutedEventArgs e)
        {
            var selectedItem = CalibrationPointsGrid.SelectedItem as CalibrationPointModel;
            if (selectedItem != null)
            {
                double InputTemplateValue = Double.Parse(InputTemplateValueBox.Text);
                double OutputTemplateValue = Double.Parse(OutputTemplateValueBox.Text);
                Console.WriteLine($"BtnFixPointClicked|| InputTemplateValue: {InputTemplateValue};  OutputTemplateValue: {OutputTemplateValue}");
                //DbHelper.SetDataByLoadingPointId(someData);
            }

        }
    }

    
}
