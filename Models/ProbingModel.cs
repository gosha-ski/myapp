using System;

namespace MyAvaloniaApp.Models
{
    public class ProbingModel
    {
        public int VerificationId { get; set; }
        public int InstrumentId { get; set; }

        public bool ExternalInspection { get; set; }
        public bool Operability { get; set; }
        public bool ZeroSettingFunction { get; set; }
        public bool Tightness { get; set; }

        public string? ExternalInspectionComment { get; set; }
        public string? OperabilityComment { get; set; }
        public string? ZeroSettingFunctionComment { get; set; }
        public string? TightnessComment { get; set; }
    }
}
