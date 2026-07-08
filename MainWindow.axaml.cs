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
}