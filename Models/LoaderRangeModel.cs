using System;

namespace MyAvaloniaApp.Models;

public class LoaderRangeModel
{
    public int Id { get; set; }
    public int VerificationId { get; set; }
    //public int InstrumentId { get; set; }
    //public string? RangeName { get; set; }
    public string Unit { get; set; } = string.Empty;
    //public string? Comment { get; set; }
}