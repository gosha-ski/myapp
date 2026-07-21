using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Linq;
using MyAvaloniaApp.Models;
using MyAvaloniaApp;

namespace MyAvaloniaApp.Views
{
    public partial class DataReadingWindow : Window
    {
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
            // Таблица точек поверки (21 точка: прямой и обратный ход)
            var points = DbHelper.GetLoadingPointsByVerificationAndInstrument(_ownerWindow.VerificationId, _instrumentId);





            //CalibrationPointsGrid.ItemsSource = points;
        }
    }

    
}
