using Avalonia.Controls;
using Avalonia.Interactivity;
//using MyAvaloniaApp.Db;      // если DbHelper в папке Db
using MyAvaloniaApp; // если DbHelper прямо в корне
  // Person
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;
using System.Collections.ObjectModel;

namespace MyAvaloniaApp.Views;

public partial class SelectInspectorListWindow : Window
{
    private readonly ObservableCollection<InspectorModel> _inspectors = new();
    public InspectorModel? SelectedInspector { get; private set; }

    public SelectInspectorListWindow()
    {
        InitializeComponent();
        LoadData();
    }


    public void BtnOkClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("BtnOkClicked"); 
        var selected = InspectorsGrid.SelectedItem as InspectorModel;
        if (selected != null)
        {
            SelectedInspector = selected;
            Console.WriteLine($"BtnOkClicked name:{selected.FirstName}");
            this.Close();
        }
    }


    public void BtnAddInspectorClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("BtnAddInspectorClicked"); 
        var dialog = new AddInspectorWindow();
        dialog.OnSaved += () =>
        {
            System.Console.WriteLine("Данные сохранены, обновляем список...");
            LoadData();
        };
        dialog.ShowDialog(this);
    }

    private void LoadData()
    {

        // 1. Получаем свежие данные из БД
        var list = DbHelper.GetAllInspectors(); 

        // 2. Очищаем текущую коллекцию (это важно, иначе будут дубли!)
        _inspectors.Clear();

        // 3. Добавляем свежие данные
        foreach (var item in list)
        {
            _inspectors.Add(item);
            Console.WriteLine($"{item.Id} {item.FirstName}");
        }
        InspectorsGrid.ItemsSource = _inspectors;

        
    }
}