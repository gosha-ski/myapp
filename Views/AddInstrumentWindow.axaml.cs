using Avalonia.Controls;
using Avalonia.Interactivity;      
using MyAvaloniaApp; 
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;


namespace MyAvaloniaApp.Views;

    public partial class AddInstrumentWindow : Window
    {
     

        public AddInstrumentWindow()
        {
            InitializeComponent();
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            // --- ИСПРАВЛЕНИЕ 1: Interval (нельзя делать Items = array) ---
            if (CbInterval != null)
            {
                CbInterval.Items.Clear();
                var intervals = new[] { "1 год", "2 года", "3 года", "4 года", "5 лет" };
                foreach (var item in intervals)
                {
                    CbInterval.Items.Add(item);
                }
                CbInterval.SelectedIndex = 0;
            }

            // --- ИСПРАВЛЕНИЕ 2: Units (аналогично) ---
            if (CbUnits != null)
            {
                CbUnits.Items.Clear();
                var units = new List<string>
                {
                    "Па", "кПа", "МПа", "бар", "мбар",
                    "мм.вод.ст", "мм.рт.ст", "кгс/м²"
                };
                foreach (var unit in units)
                {
                    CbUnits.Items.Add(unit);
                }
                CbUnits.SelectedIndex = 1; // по умолчанию кПа
            }
        }

        private void CreateInstrument(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("CreateInstrument BTN CLICKED");
            if (TbModel == null || string.IsNullOrWhiteSpace(TbModel.Text))
            {
                // Тут можно показать MessageBox (например, через MessageBox.Show)
                Console.WriteLine("RETURN");
                return;
            }

            var instrument = new InstrumentModel
            {
                TypeCode = GetSelectedTypeCode(),
                Model = TbModel.Text,
                SerialNumber = TbSerial?.Text,
                InventoryNumber = TbInventory?.Text,

                IntervalYears = ParseIntervalYears(CbInterval?.SelectedItem?.ToString()),
                Location = TbLocation?.Text,

                // --- ИСПРАВЛЕНИЕ 3: конвертация DateTimeOffset? -> DateTime ---
                InServiceDate = DpInService?.SelectedDate?.UtcDateTime ?? DateTime.Today,

                Units = CbUnits?.SelectedItem?.ToString(),

                LowerLimit = ParseDouble(TbLowerLimit?.Text),
                UpperLimit = ParseDouble(TbUpperLimit?.Text),
                AccuracyClass = ParseDouble(TbAccuracy?.Text),
                VariationLimit = ParseDouble(TbVariation?.Text),

                AccuracyMethodCode = GetSelectedAccuracyMethodCode()
            };

            try
            {
                DbHelper.SaveInstrument(instrument);
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        private int GetSelectedTypeCode()
        {
            if (RbTypeManometer?.IsChecked == true) return 0;
            if (RbTypeDdUnified?.IsChecked == true) return 1;
            if (RbTypeDdVirtual?.IsChecked == true) return 2;
            if (RbTypeDdHart?.IsChecked == true) return 3;
            return 0;
        }

        private int GetSelectedAccuracyMethodCode()
        {
            // Реализуй аналогично радиокнопкам для метода точности
            return 0;
        }

        private int? ParseIntervalYears(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var firstWord = value.Split(' ')[0];
            if (int.TryParse(firstWord, out var years))
                return years;
            return null;
        }

        private double? ParseDouble(string? value)
        {
            if (double.TryParse(value, out var result))
                return result;
            return null;
        }
    }
