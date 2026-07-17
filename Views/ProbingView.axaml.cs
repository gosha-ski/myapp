using Avalonia.Controls;
using Avalonia.Interactivity;
using MyAvaloniaApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Generic;

namespace MyAvaloniaApp.Views
{
    public partial class ProbingView : UserControl
    {
        // Коллекция для привязки данных (ObservableCollection автоматически обновляет UI)
        public ObservableCollection<InstrumentResultModel> Results { get; } = new();
        private readonly NewVerificationWindow _ownerWindow;

        // Флаг состояния фильтра
        private bool _showReasons = false;

        public ProbingView(Window ownerWindow)
        {
            InitializeComponent();
            _ownerWindow = (NewVerificationWindow)ownerWindow;
            
            BtnRunProbing.Click += OnRunProbingClick;
            
        }

        public void InitializeWithInstruments()
        {
            
            Results.Clear();
            int channel = 1;
            foreach (var inst in _ownerWindow.SelectedInstruments)
            {
                Console.WriteLine("InitializeWithInstruments");
                Results.Add(new InstrumentResultModel
                {
                    Channel = channel++,
                    InstrumentName = inst.Model, // или TypeCode, зависит от твоей модели
                    SerialNumber = inst.SerialNumber,
                    IsInspected = false,
                    IsFunctional = false,
                    IsZeroOk = false,
                    IsSealed = false
                });
            }

            ResultsGrid.ItemsSource = Results;
        }

        private async void OnRunProbingClick(object? sender, RoutedEventArgs e)
        {
            var selectedRow = ResultsGrid.SelectedItem as InstrumentResultModel;
            if (selectedRow == null)
            {
                System.Diagnostics.Debug.WriteLine("Выберите строку в таблице!");
                return;
            }

            var dialog = new ProbingEditWindow();
            
            //  ПОДПИСЫВАЕМСЯ ДО показа окна
            dialog.Initialize(selectedRow);
            dialog.OnSaved += () =>
            {
                // Сюда попадает код, который выполняется сразу после нажатия «ОК» в диалоге
                Console.WriteLine("OnSaved сработал: данные сохранены в модели!");
                Console.WriteLine($"IsInspected:{selectedRow.IsInspected}");
                selectedRow.IsInspected = true;
                // Здесь можно добавить доп. логику, если нужно
            };

            
            //  Ждём закрытия окна
            await dialog.ShowDialog(_ownerWindow);
        }
    }
}
