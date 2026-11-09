using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//შესვლის ბოლო ნაბიჯი: რას იღებს მომხმარებელი, როცა ყველა შემოწმება გაიარა.
//ნაგულისხმევი რეალიზაცია (JwtLoginTokenIssuer) წვდომის და განახლების ტოკენებს ქმნის;
//სხვა ჰოსტს (მაგალითად sso.ltg.ge) შეუძლია საკუთარი რეალიზაციით სესია სხვაგვარად შექმნას
public interface ILoginTokenIssuer
{
    Task<Result<LoginResult>> IssueFullAsync(AppUser user, CancellationToken cancellationToken = default);
}
