using System;

namespace MyAvaloniaApp.Models;

public class CalibrationPointModel
{
    public int LoadingPointId { get; set; }      // Id из LoadingPoints (для UPDATE)
    public int PointIndex { get; set; }          // № точки из LoadingPointsDefault
    public double PointValueMpa { get; set; }    // Точка, МПа из LoadingPointsDefault

    public double? TemplateValue { get; set; }
    public double? CalcValue { get; set; }
    public double? InstrumentValue { get; set; }
    public double? Error { get; set; }
    public double? Variation { get; set; }
    public bool Approved { get; set; }           // Approved (0/1 → bool)
}
