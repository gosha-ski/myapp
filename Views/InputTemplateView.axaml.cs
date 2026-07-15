using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Views
{
    public partial class InputTemplateView : UserControl
    {
        private readonly Window _ownerWindow;
        public InputTemplateView(Window ownerWindow)
        {
            InitializeComponent();
            _ownerWindow = ownerWindow;
        }

        public async void SelectTemplateClicked(object? sender, RoutedEventArgs e)
        {
            var dialog = new SelectTemplateListWindow();
            await dialog.ShowDialog(_ownerWindow);
            if (dialog.SelectedTemplate != null)
            {
                TemplateModel template = dialog.SelectedTemplate;
                Console.WriteLine(template.DeviceType);
                FillFields(template);
            }

        }

        private void FillFields(TemplateModel item)
        {
            DeviceTypeBox.Text = item.DeviceType;
            EtalonNameBox.Text = item.FullName;      // Тут можно переименовать поле в TemplateNameBox
            SerialNumberBox.Text = item.SerialNumber.ToString();
            MeasurementErrorBox.Text = item.Inaccuracy.ToString();
            ErrorNormalizationBox.Text = item.InaccuracyMethodCode.ToString();
            CurrentRangeBox.Text = item.CurrentRange;
            UnitsBox.Text = item.Units;
            NpiBox.Text = item.LowerLimit.ToString();
            VpiBox.Text = item.UpperLimit.ToString();
        }


    }
}