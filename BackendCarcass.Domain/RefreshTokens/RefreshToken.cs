using System;
using SystemTools.SharedKernel;

namespace BackendCarcass.Domain.RefreshTokens;

//წვდომის ტოკენის განახლების ტოკენი. თვითონ ტოკენი ბაზაში არ ინახება, მხოლოდ მისი SHA-256 ჰეში
public sealed class RefreshToken : Entity
{
    public int RftId { get; set; }
    public int UserId { get; set; }
    public required string TokenHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    //როდის გაუქმდა ტოკენი (განახლებისას, გასვლისას, პაროლის ცვლილებისას). null ნიშნავს, რომ მოქმედია
    public DateTime? RevokedAt { get; set; }
}
