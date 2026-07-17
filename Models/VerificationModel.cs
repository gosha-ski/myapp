using System;

namespace MyAvaloniaApp.Models;

public class VerificationModel
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string Comment { get; set; } = "";
    
}