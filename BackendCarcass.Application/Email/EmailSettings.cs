namespace BackendCarcass.Application.Email;

//SMTP სერვერის პარამეტრები. კონფიგურაციის სექცია: "EmailSettings"
public sealed class EmailSettings
{
    //თუ ცარიელია და გარემო Development-ია, წერილები არ იგზავნება და მათი ტექსტი ლოგში იწერება
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SenderEmailAddress { get; set; }

    //გამგზავნის სახელი, რომელიც წერილის სათაურშიც გამოიყენება (მაგალითად აპლიკაციის სახელი)
    public string? SenderName { get; set; }
}
