using Avalonia.Controls;
using Avalonia.Interactivity;
//using MyAvaloniaApp.Db;      // если DbHelper в папке Db
using MyAvaloniaApp; // если DbHelper прямо в корне
  // Person
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Views;

public partial class SelectTemplateListWindow : Window
{
    public TemplateModel? SelectedTemplate { get; private set; }
    public SelectTemplateListWindow()
    {
        InitializeComponent();
        Console.WriteLine("SelectTemplateListWindow()");
        LoadTemplates();
    }

    public void BtnAddTemplateClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("BtnAddInstrumentClicked");
        var dialog = new AddTemplateWindow();
        dialog.OnSaved += () =>
        {
            System.Console.WriteLine("Данные сохранены, обновляем список...");
            LoadTemplates();
        };
        dialog.ShowDialog(this);
    }

    public void LoadTemplates()
    {
        List<TemplateModel> templates = DbHelper.GetAllTemplates();
        TemplateDataGrid.ItemsSource = templates;
    }

    public void BtnSelectTemplateClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("HEELLO WORLD");
        var selected = TemplateDataGrid.SelectedItem as TemplateModel;
        if (selected != null)
        {
            SelectedTemplate = selected;
            this.Close();
        }


    }
}