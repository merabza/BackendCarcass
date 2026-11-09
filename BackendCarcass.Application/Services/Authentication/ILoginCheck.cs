using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//შესვლის დამატებითი შემოწმება (მაგალითად sso.ltg.ge-ში არააქტიური მომხმარებლის უარყოფა).
//ეშვება პაროლის წარმატებული შემოწმების შემდეგ და სრული წვდომის გაცემამდე. შეიძლება რამდენიმე რეალიზაცია
//დარეგისტრირდეს; ნაგულისხმევად არცერთი არ არის. უარის შემთხვევაში დაბრუნებული შეცდომა (ქართული ტექსტით)
//მომხმარებელს უცვლელად მიუვა
public interface ILoginCheck
{
    Task<Result> CheckAsync(AppUser user, CancellationToken cancellationToken = default);
}
