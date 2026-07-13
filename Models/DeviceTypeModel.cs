using System;

namespace MyAvaloniaApp.Models;

public class DeviceTypeModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public override string ToString() => Name;
}