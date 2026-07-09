using System;

namespace MyAvaloniaApp.Models;

public class InspectorModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;   // Имя
    public string LastName { get; set; } = string.Empty;    // Фамилия
    public string MiddleName { get; set; } = string.Empty;  // Отчество
}
