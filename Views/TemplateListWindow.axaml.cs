using Avalonia.Controls;
using Avalonia.Interactivity;
//using MyAvaloniaApp.Db;      // если DbHelper в папке Db
using MyAvaloniaApp; // если DbHelper прямо в корне
  // Person
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp.Views;

public partial class TemplateListWindow : Window
{
    public TemplateListWindow()
    {
        InitializeComponent();
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

    public void BtnDeleteTemplateClicked(object? sender, RoutedEventArgs e)
    {
        TemplateModel template = TemplateDataGrid.SelectedItem as TemplateModel;
        DbHelper.DeleteTemplate(template.Id);
        LoadTemplates();
    }

    public void LoadTemplates()
    {
        List<TemplateModel> templates = DbHelper.GetAllTemplates();
        TemplateDataGrid.ItemsSource = templates;
    }
}