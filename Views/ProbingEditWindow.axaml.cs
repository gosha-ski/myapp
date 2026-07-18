using Avalonia.Controls;
using Avalonia.Interactivity;
using MyAvaloniaApp.Models;
using System;

namespace MyAvaloniaApp.Views
{
    public partial class ProbingEditWindow : Window
    {
        private ProbingModel? _currentRow;
        public event Action? OnSaved;

        public ProbingEditWindow()
        {
            InitializeComponent();
        }

        // Метод для инициализации окна данными (обязательно)
        // public void Initialize(InstrumentResultModel row)
        // {
        //     _currentRow = row;
        //     // тут заполни UI из row (как мы делали раньше)
        // }

        public void BtnOkClicked(object? sender, RoutedEventArgs e)
        {
            if (_currentRow == null)
            {
                
                this.Close();
                return;
            }

            
            Console.WriteLine("OK CLIKCED");
            OnSaved?.Invoke();

            this.Close();
        }

        public void BtnCancelClicked(object? sender, RoutedEventArgs e)
        {
            //this.ReturnResult = false;
            this.Close();
        }
    }
}