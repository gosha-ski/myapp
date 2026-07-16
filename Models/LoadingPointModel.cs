using System;

namespace MyAvaloniaApp.Models;

public class LoadingPointModel
{
    public int Index { get; set; }
    public double Value { get; set; }

    public LoadingPointModel(int index, double value)
    {
        Index = index;
        Value = value;
    }
}