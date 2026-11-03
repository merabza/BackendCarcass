namespace BackendCarcass.Application.Identity.Models;

public sealed class IdentitySettings
{
    public string? JwtSecret { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtAudience { get; set; }

    //წვდომის ტოკენის (JWT) მოქმედების ხანგრძლივობა წუთებში
    public int AccessTokenMinutes { get; set; } = 60;

    //განახლების ტოკენის მოქმედების ხანგრძლივობა დღეებში
    public int RefreshTokenDays { get; set; } = 7;

    //ფრონტის მისამართი, რომლითაც ელფოსტაში გაგზავნილი ბმულები იწყება (მაგალითად https://app.grammar.ge).
    //თუ ცარიელია, წერილებში მხოლოდ კოდი იგზავნება
    public string? FrontendBaseUrl { get; set; }
}
