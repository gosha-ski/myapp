using Avalonia.Controls;
using Avalonia.Interactivity;
//using MyAvaloniaApp.Db;      // если DbHelper в папке Db
using MyAvaloniaApp; // если DbHelper прямо в корне
  // Person
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Views;

public partial class InstrumentListWindow : Window
{
    public InstrumentListWindow()
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
        dialog.ShowDialog(this);
    }

}