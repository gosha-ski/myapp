        using System;


        namespace MyAvaloniaApp.Models;

        public class TemplateModel
        {
            public int Id { get; set; }

            public string? DeviceType { get; set; }
            public string? FullName { get; set; }
            public string? SerialNumber { get; set; }
            public double? Inaccuracy { get; set; }
            public int InaccuracyMethodCode { get; set; } = 0; // 0=%Д    1=%ИВ+Const   2=%Д/%ИВ   3=Абс
            public string? CurrentRange { get; set; }

            // Единицы измерения строкой: "кПа", "МПа", "Па" и т.д.
            public string? Units { get; set; }

            public double? LowerLimit { get; set; }
            public double? UpperLimit { get; set; }
   
        }