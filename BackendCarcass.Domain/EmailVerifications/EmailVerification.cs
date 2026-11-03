using System;
using SystemTools.SharedKernel;

namespace BackendCarcass.Domain.EmailVerifications;

//მომხმარებლის ელფოსტაზე გაგზავნილი დადასტურების კოდი და ბმული.
//თვითონ კოდი და ბმულის ტოკენი ბაზაში არ ინახება, მხოლოდ მათი SHA-256 ჰეშები
public sealed class EmailVerification : Entity
{
    public int EvfId { get; set; }
    public int UserId { get; set; }
    public EEmailVerificationPurpose Purpose { get; set; }
    public required string CodeHash { get; set; }
    public required string LinkTokenHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    //კოდის შეყვანის წარუმატებელი ცდების რაოდენობა
    public int Attempts { get; set; }

    //როდის იქნა გამოყენებული კოდი, ან ბმული. null ნიშნავს, რომ ჯერ არ გამოუყენებიათ
    public DateTime? UsedAt { get; set; }
}
