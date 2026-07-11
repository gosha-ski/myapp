using Avalonia.Controls;
using Avalonia.Interactivity;
//using MyAvaloniaApp.Db;      // если DbHelper в папке Db
using MyAvaloniaApp; // если DbHelper прямо в корне
  // Person
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Views;

public partial class SelectInstrumentListWindow : Window
{
    public InstrumentModel? SelectedInstrument { get; private set; }

    public SelectInstrumentListWindow()
    {
        InitializeComponent();
        LoadInstruments();
    }

    private void LoadInstruments()
    {
        try
        {
            var instruments = DbHelper.GetAllInstruments(); // сделаем этот метод ниже
            InstrumentsGrid.ItemsSource = instruments;
            Console.WriteLine("LoadInstruments");
            foreach (var inst in instruments)
            {
                // подставь реальные свойства твоего класса Instrument
                Console.WriteLine($"ID: {inst.Id}, Название: {inst.TypeCode}, Тип: {inst.Model}");
            }
        }
        catch (Exception ex)
        {
            // В реальном приложении лучше отдельное окно ошибки
            System.Console.WriteLine("Ошибка загрузки списка: " + ex.Message);
        }
    }

    public void BtnAddInstrumentClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("BtnAddInstrumentClicked");
        var dialog = new AddInstrumentWindow();
        dialog.OnSaved += () =>
        {
            System.Console.WriteLine("Данные сохранены, обновляем список...");
            LoadInstruments();
        };
        dialog.ShowDialog(this);
    }

    private void BtnDeleteInstrumentClicked(object? sender, RoutedEventArgs e)
    {
        // Получаем выделенный элемент в DataGrid
        var selectedItem = InstrumentsGrid.SelectedItem as InstrumentModel;

        if (selectedItem == null)
        {
            StatusText.Text = "Статус: Выберите прибор для удаления";
            return;
        }

        try
        {
            DbHelper.DeleteInstrument(selectedItem.Id);
            LoadInstruments(); // Перезагружаем список
            StatusText.Text = $"Статус: Прибор '{selectedItem.Model}' удалён";
            Console.WriteLine($"Удален прибор с Id={selectedItem.Id}");
        }
        catch (Exception ex)
        {
            StatusText.Text = "Статус: Ошибка при удалении прибора";
            System.Console.WriteLine("Ошибка удаления: " + ex.Message);
            // В реальном приложении лучше показывать отдельное окно ошибки
        }
    }

    private void BtnDoneClicked(object? sender, RoutedEventArgs e){
        var selected = InstrumentsGrid.SelectedItem as InstrumentModel;
        if (selected != null)
        {
            SelectedInstrument = selected;
            Console.WriteLine($"BtnDoneClicked TypeCode:{selected.TypeCode}");
            
            this.Close();
        }
    }


}