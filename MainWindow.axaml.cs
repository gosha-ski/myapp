using Avalonia.Controls;
using System;
using Avalonia.Interactivity;
using MyAvaloniaApp.Views;

namespace MyAvaloniaApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void MenuDeviceClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("InstrumentListWindow OKEY");
        var dialog = new InstrumentListWindow();
        dialog.ShowDialog(this);   
    }

    public void MenuInspectorClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("InspectorListWindow OKEY");
        var dialog = new InspectorListWindow();
        dialog.ShowDialog(this);   
    }

    public void MenuTemplateClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("TEMPLATE LIST");
        var dialog = new TemplateListWindow();
        dialog.ShowDialog(this);   
    }


}