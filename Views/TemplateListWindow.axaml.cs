using Avalonia.Controls;
using Avalonia.Interactivity;
using MyAvaloniaApp; 
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
        Console.WriteLine("BtnDeleteTemplateClicked");
        TemplateModel template = TemplateDataGrid.SelectedItem as TemplateModel;
        if (template != null)
        {
            DbHelper.DeleteTemplate(template.Id);
            LoadTemplates();
        }
    }

    public void BtnEditTemplateClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("BtnEditTemplateClicked");
        TemplateModel template = TemplateDataGrid.SelectedItem as TemplateModel;
        if (template != null)
        {
            var dialog = new EditTemplateWindow(template.Id);
            dialog.OnSaved += () =>
            {
                System.Console.WriteLine("Данные сохранены, обновляем список...");
                LoadTemplates();
            };
            dialog.ShowDialog(this);
        }
    }

    public void LoadTemplates()
    {
        List<TemplateModel> templates = DbHelper.GetAllTemplates();
        TemplateDataGrid.ItemsSource = templates;
    }
}