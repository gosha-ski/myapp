using Avalonia.Controls;
using System;
using System.Collections.Generic;
using Avalonia.Interactivity;
using MyAvaloniaApp.Views;
using MyAvaloniaApp;
using MyAvaloniaApp.Models;

namespace MyAvaloniaApp;

public partial class MainWindow : Window
{
    //private readonly MeasurementService _measurement;

    public MainWindow(MeasurementService measurement)
    {
        
        InitializeComponent();
        LoadVerifications();

    }

    public void LoadVerifications()
    {
        List<VerificationModel> verifications = DbHelper.GetAllVerifications();
        VerificationsGrid.ItemsSource = verifications;
    }

    public void MenuDeviceClicked(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("InstrumentListWindow OKEY");
        var dialog = new InstrumentListWindow();
        dialog.ShowDialog(this);   
    }

    public void MenuInspectorClicked(object? sender, RoutedEventArgs e)
    {
        
        var dialog = new InspectorListWindow();
        dialog.ShowDialog(this);   
    }

    public void MenuTemplateClicked(object? sender, RoutedEventArgs e)
    {
        
        var dialog = new TemplateListWindow();
        dialog.ShowDialog(this);   
    }

    
    public void BtnNewVerificationClicked(object? sender, RoutedEventArgs e)
    {
        int verificationId = DbHelper.SaveVerification("example");
        var dialog = new NewVerificationWindow(verificationId);
        dialog.ShowDialog(this);
        LoadVerifications();
    }

    public void BtnContinueVerificationClicked(object? sender, RoutedEventArgs e)
    {
        var selectedItem = VerificationsGrid.SelectedItem as VerificationModel;
        int verificationId = selectedItem.Id;
        Console.WriteLine($"BtnContinueVerificationClicked selectedItem.Id:{verificationId}");
  
        var dialog = new NewVerificationWindow(verificationId);
        dialog.ShowDialog(this);
    }

    public void BtnDeleteVerificationClicked(object? sender, RoutedEventArgs e)
    {
        var selectedItem = VerificationsGrid.SelectedItem as VerificationModel;
        int verificationId = selectedItem.Id;


    }


}