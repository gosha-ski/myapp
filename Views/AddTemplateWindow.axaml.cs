using Avalonia.Controls;
using Avalonia.Interactivity;      
using MyAvaloniaApp; 
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;


namespace MyAvaloniaApp.Views;

public partial class AddTemplateWindow : Window
{
	public event Action? OnSaved;
	private readonly Dictionary<string, List<string>> _rangesByDeviceType = new()
    {
        { "Внешний контроллер", new List<string> { "Давление" } },
        { "Внешний мультиметр", new List<string> { "Ток", "Давление" } }
    };

    private readonly Dictionary<string, List<string>> _unitsByDeviceType = new()
    {
        { "Внешний контроллер", new List<string> { "МПа", "кПа", "Па" } },
        { "Внешний мультиметр", new List<string> { "мА", "А", "В" } }
    };

    private ComboBox? _cbDeviceType;
    private ComboBox? _cbRange;
    private ComboBox? _cbUnit;

	public AddTemplateWindow()
	{
	    InitializeComponent();

	    _cbDeviceType = this.FindControl<ComboBox>("cbDeviceType");
	    _cbRange = this.FindControl<ComboBox>("cbRange");
	    _cbUnit = this.FindControl<ComboBox>("cbUnit");

	    if (_cbDeviceType == null || _cbRange == null)
	        return;

	    // 1. Заполняем список типов, но НЕ выбираем ничего
	    var types = new List<DeviceTypeModel>
	    {
	        new() { Id = 1, Name = "Внешний контроллер" },
	        new() { Id = 2, Name = "Внешний мультиметр" },
	    };
	    _cbDeviceType.ItemsSource = types;
	    
	    // 2. Подписываемся на событие
	    _cbDeviceType.SelectionChanged += OnDeviceTypeChanged;

	    // !!! ГЛАВНОЕ: УБРАЛИ строку _cbDeviceType.SelectedIndex = 0;
	    // Теперь SelectedIndex по умолчанию равен -1 (ничего не выбрано)
	    _cbDeviceType.SelectedIndex = -1;
	}

	private void BtnOkClicked(object? sender, RoutedEventArgs e)
	{
		string deviceType = cbDeviceType?.Text?.Trim() ?? "";
        string fullName = tbFullName?.Text?.Trim() ?? "";
        string serialNumber = tbSerialNumber?.Text?.Trim() ?? "";
        double inaccuracy = Convert.ToDouble(tbInaccuracy?.Text?.Trim() ?? "0");
		string currentRange = cbRange?.Text?.Trim() ?? "";
		string units = "МПа";
		double lowerLimit = Convert.ToDouble(tbLowerLimit?.Text);
        double upperLimit = Convert.ToDouble(tbUpperLimit?.Text);


		TemplateModel template = new TemplateModel
		{
            DeviceType = deviceType,
			FullName = fullName,
			SerialNumber = serialNumber,
			Inaccuracy = inaccuracy,
			InaccuracyMethodCode = 0, //потом надо поправить
			CurrentRange = currentRange,
			Units = units,
			LowerLimit = lowerLimit,
			UpperLimit = upperLimit
		};

		DbHelper.SaveTemplate(template);

        OnSaved?.Invoke();
        this.Close();


        //Console.WriteLine($"{deviceType} {fullName} {inaccuracy} {serialNumber}");
        


    }


    private void OnDeviceTypeChanged(object? sender, SelectionChangedEventArgs? e)
	{
	    // Если ничего не выбрано (SelectedIndex == -1), выходим
	    if (_cbDeviceType?.SelectedItem is not DeviceTypeModel device) 
	        return;

	    // Только если пользователь реально кликнул и выбрал тип — показываем диапазоны
	    if (_rangesByDeviceType.TryGetValue(device.Name, out var ranges))
	    {
	        _cbRange.ItemsSource = ranges;
	        //_cbRange.SelectedIndex = -1; // Тут можно выбрать первый диапазон, это нормально
	    }
	    else
	    {
	        _cbRange.ItemsSource = null;
	    }
	}

}