using Avalonia.Controls;
using Avalonia.Interactivity;      
using MyAvaloniaApp; 
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;
using System.Collections.ObjectModel;

namespace MyAvaloniaApp.Views;

public partial class InspectorListWindow : Window
{
    private readonly ObservableCollection<InspectorModel> _inspectors = new();

    public InspectorListWindow()
    {
        InitializeComponent();
        LoadData();

    }

    public void BtnAddInspectorClicked(object? sender, RoutedEventArgs e){
        var dialog = new AddInspectorWindow();
        dialog.OnSaved += () =>
        {
            System.Console.WriteLine("Данные сохранены, обновляем список...");
            LoadData();
        };
        dialog.ShowDialog(this);
    }

    private void BtnDeleteInspectorClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            var inspector = InspectorsGrid.SelectedItem as InspectorModel;
            Console.WriteLine("INSIDE");
                // 1. Удаляем из БД
            DbHelper.DeleteInspector(inspector.Id);

                // 2. Удаляем из коллекции — DataGrid сам обновится
            _inspectors.Remove(inspector);
            LoadData();

            System.Console.WriteLine($"Инспектор {inspector.Id} удалён.");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Ошибка при удалении: {ex.Message}");
                // Тут можно оставить только лог, без окна
        }
        
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