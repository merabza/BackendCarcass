using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//შესვლის დაუსრულებელი ნაბიჯის (ელფოსტის დადასტურება, სავალდებულო პაროლის ცვლილება) მდგომარეობის შენახვა.
//ნაგულისხმევი რეალიზაცია (JwtPendingLoginStore) მდგომარეობას დროებით ტოკენში ინახავს და ტოკენი კლიენტს გადაეცემა;
//სხვა ჰოსტს შეუძლია ის სერვერულ, დაცულ cookie-ში შეინახოს. Save-ის შედეგი LoginResult.Token-ში ხვდება,
//Read კი მომდევნო ნაბიჯზე კლიენტისგან მიღებულ იმავე მნიშვნელობას იღებს
public interface IPendingLoginStore
{
    Result<string> Save(AppUser user, PendingTokenData data);

    Result<PendingTokenData> Read(string? token);
}
