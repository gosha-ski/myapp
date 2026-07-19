using System;

namespace MyAvaloniaApp.Models;

// public class LoadingPointModel
// {
//     public int Index { get; set; }
//     public double Value { get; set; }

//     public LoadingPointModel(int index, double value)
//     {
//         Index = index;
//         Value = value;
//     }
// }

public class LoadingPointModel
{
    public int Id { get; set; }
    public int DefaultPointId { get; set; }
    public int InstrumentId { get; set; }

    public double? TemplateValue { get; set; }
    public double? CalcValue { get; set; }
    public double? InstrumentValue { get; set; }
    public double? Error { get; set; }
    public double? Variation { get; set; }
    public bool Approved { get; set; } = false;
}
