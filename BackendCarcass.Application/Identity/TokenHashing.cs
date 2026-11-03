using System;
using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace BackendCarcass.Application.Identity;

//შემთხვევითი ტოკენების და კოდების შექმნა და მათი ჰეშირება ბაზაში შესანახად
public static class TokenHashing
{
    private const int TokenBytesCount = 32;
    private const int CodeUpperBound = 1_000_000;

    //256-ბიტიანი შემთხვევითი ტოკენი URL-ში უსაფრთხო base64 ფორმით (ბმულებისთვის და განახლების ტოკენებისთვის)
    public static string CreateRandomToken()
    {
        return Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenBytesCount));
    }

    //6-ნიშნა ციფრული კოდი ელფოსტით გასაგზავნად
    public static string CreateNumericCode()
    {
        return RandomNumberGenerator.GetInt32(0, CodeUpperBound).ToString("D6", CultureInfo.InvariantCulture);
    }

    public static string Sha256Hex(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    //შედარება მუდმივ დროში, რომ პასუხის დროით ჰეშის გამოცნობა არ გახდეს შესაძლებელი
    public static bool HashMatches(string value, string storedHash)
    {
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(Sha256Hex(value)),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
