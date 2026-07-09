using Avalonia.Controls;
using Avalonia.Interactivity;      
using MyAvaloniaApp; 
using System.Collections.Generic;
using System;
using MyAvaloniaApp.Models;


namespace MyAvaloniaApp.Views;

public partial class AddInspectorWindow : Window
{
	public event Action? OnSaved;

	public AddInspectorWindow()
	{
		InitializeComponent();
	}

	private void BtnSaveInspectorClicked(object? sender, RoutedEventArgs e){
        // 1. Забираем данные из полей
		string lastName = TxtLastName.Text?.Trim() ?? "";
		string firstName = TxtFirstName.Text?.Trim() ?? "";
		string middleName = TxtMiddleName.Text?.Trim() ?? "";

            // 2. Простая валидация
		if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
		{
                // В реальном проекте лучше сделать MessageBox, но для старта просто меняем статус (если бы он был)
                // Или можно вывести простой Alert, если подключена библиотека, а пока просто игнорируем клик
                // Для наглядности можно вывести в консоль
			System.Console.WriteLine("Ошибка: Фамилия и Имя обязательны!");

                // Если хочешь, чтобы окно не закрывалось при ошибке, просто вернись:
			return; 
		}

		try
		{
                // 3. Создаем объект и сохраняем в БД
			var inspector = new InspectorModel
			{
				LastName = lastName,
				FirstName = firstName,
				MiddleName = middleName
			};

			DbHelper.SaveInspector(inspector);

			OnSaved?.Invoke();

            // 4. Если всё успешно — закрываем окно с результатом "OK"
			this.Close(true); 
		}
		catch (Exception ex)
		{
			System.Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            // Тут можно добавить обработку ошибки БД
		}
	}

	private void OnCancelClick(object? sender, RoutedEventArgs e){
        // Закрываем окно с результатом "Отмена" (false)
		this.Close(false);
	}
}