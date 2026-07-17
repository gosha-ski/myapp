using System;

namespace MyAvaloniaApp.Models
{
    public class InstrumentResultModel
    {
        public int Channel { get; set; }
        public string InstrumentName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        // Результаты опробования
        public bool IsInspected { get; set; }
        public bool IsFunctional { get; set; }
        public bool IsZeroOk { get; set; }
        public bool IsSealed { get; set; } // Герметичность

        // Для отображения причин несоответствия (если нужно хранить текст)
        public string? MismatchReason { get; set; }
    }
}
