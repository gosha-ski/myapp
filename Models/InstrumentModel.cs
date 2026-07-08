using System;

namespace MyAvaloniaApp.Models;

    public class InstrumentModel
    {
        public int Id { get; set; }
        public int TypeCode { get; set; } // 0=Манометр, 1=ДД униф, 2=ДД вирт, 3=ДД HART

        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? InventoryNumber { get; set; }

        public int? IntervalYears { get; set; }
        public string? Location { get; set; }
        public DateTime InServiceDate { get; set; } = DateTime.Today;

        // Единицы измерения строкой: "кПа", "МПа", "Па" и т.д.
        public string? Units { get; set; }

        public double? LowerLimit { get; set; }
        public double? UpperLimit { get; set; }
        public double? AccuracyClass { get; set; }
        public double? VariationLimit { get; set; }
        public int AccuracyMethodCode { get; set; } = 0;
    }
