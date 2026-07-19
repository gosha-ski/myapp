using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyAvaloniaApp.Models;
using Avalonia; 
using Avalonia.VisualTree;

namespace MyAvaloniaApp.Views
{
    public partial class LoaderRangeView : UserControl
    {
        
        // Коллекция точек (хранит пары: индекс, значение)
        private readonly ObservableCollection<LoadingPointDefaultModel> _points = new();
        private readonly NewVerificationWindow _ownerWindow;


        public LoaderRangeView(Window ownerWindow)
        {
            InitializeComponent();
            _ownerWindow = (NewVerificationWindow)ownerWindow;
            InitializeLogic();
            LoadRangeData();
        }

        private void LoadRangeData()
        {
            List<LoadingPointDefaultModel> data = DbHelper.GetLoadingPointsByVerificationId(_ownerWindow.VerificationId);
            PointsGrid.ItemsSource = data;
        }

        private void InitializeLogic()
        {

            // Настройка колонок (чтобы отображать значения с нужным числом знаков)
            // ColIndex.Binding = new AvaloniaPropertyBinding(typeof((int, double)), nameof(Item.Index));
            // ColValue.Binding = new AvaloniaPropertyBinding(typeof((int, double)), nameof(Item.Value));

            // // Установка начального значения для NumericUpDown
            // DecimalPlacesInput.Value = 3;

            // Подписка на события кнопок
            BtnMetro.Click += BtnMetroClicked;
            // BtnAdd.Click += OnAddClick;
            // BtnDelete.Click += OnDeleteClick;
            
            // BtnLoad.Click += OnLoadClick;
            // BtnSave.Click += OnSaveClick;

            // Пересчет индексов при изменении коллекции (упрощенно — при действиях пользователя)
            // В реальном проекте лучше использовать отдельный класс-обертку с INotifyPropertyChanged
        }

        // Вспомогательный метод для переиндексации
        // private void RenumberPoints()
        // {
        //     for (int i = 0; i < _points.Count; i++)
        //     {
        //         var old = _points[i];
        //         _points[i] = (i + 1, old.Value);
        //     }
        //     // DataGrid автоматически обновит колонку индекса благодаря привязке
        // }

        // Обработчики кнопок
        private async  void BtnMetroClicked(object? sender, RoutedEventArgs e)
        {
   
            var dialog = new AddPointsDialog();
            await dialog.ShowDialog(_ownerWindow);
            if(dialog.Result.Start != null && dialog.Result.End != null && dialog.Result.Count != null){
                //Console.WriteLine($"ADD LOAD RANGE start:{dialog.Result.Start} end:{dialog.Result.End} count:{dialog.Result.Count}");

                _points.Clear();
                double delta = (dialog.Result.End - dialog.Result.Start) / (dialog.Result.Count-1);
                int rangeId = DbHelper.SaveLoaderRangeForVerification(_ownerWindow.VerificationId, "МПа");
                for(int i=0; i<dialog.Result.Count; i++){
                    double val = dialog.Result.Start + delta * i;
                    double rounded = Math.Round(val, 3);
                    LoadingPointDefaultModel point = new LoadingPointDefaultModel();
                    point.LoaderRangeId = rangeId;
                    point.PointIndex = i + 1;
                    point.PointValue = rounded;
                    int loadingpointId = DbHelper.AddLoadingPointDefault(point.LoaderRangeId, point.PointIndex, point.PointValue);
                    _points.Add(point);
                }

                List<InstrumentModel> instruments = DbHelper.GetInstrumentsByVerificationId(_ownerWindow.VerificationId);
                List<LoadingPointDefaultModel> loadPoints = DbHelper.GetLoadingPointsByVerificationId(_ownerWindow.VerificationId);

                foreach(var item in instruments){
                    foreach(var point in loadPoints){
                        //Console.WriteLine($"INSTRUMENTId:{item.Id} defaultPointId:{point.Id}");
                        DbHelper.AddLoadingPoint(point.Id, item.Id, null, null, null, null, null, false);
                    }
                }

                PointsGrid.ItemsSource = _points;
            }
            
        }

        // private void OnAddClick(object? sender, RoutedEventArgs e)
        // {
        //     // Для простоты добавляем точку со значением 0.0
        //     // В реальном проекте можно открыть диалог для ввода значения
        //     var nextIndex = _points.Count + 1;
        //     _points.Add((nextIndex, 0.0));
        // }

        // private void OnDeleteClick(object? sender, RoutedEventArgs e)
        // {
        //     var selected = PointsGrid.SelectedItem as LoadingPointModel?;
        //     if (selected.Value && selected.Index)
        //     {
        //         _points.Remove(selected.Value);
        //         RenumberPoints();
        //     }
        // }

        

        // private void OnLoadClick(object? sender, RoutedEventArgs e)
        // {
        //     // Здесь будет логика загрузки из файла/хранилища
        //     // Для примера — сброс к дефолту
        //     OnMetroClick(sender, e);
        // }

        private void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            // Здесь будет логика сохранения
            // Например, сериализация _points в JSON
        }

        // Вспомогательный класс для корректной привязки (так как кортежи плохо работают с привязками в некоторых версиях Avalonia)
        // Если у тебя Avalonia 11+, кортежи могут работать, но надежнее использовать класс.
        // Ниже — альтернативная реализация через класс-обертку, если кортежи не обновляют UI.
    }
}