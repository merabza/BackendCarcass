using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ServerCarcass;

public static class ControllerHelpers
{
    public static (T?, string?) CheckBody<T>(this ControllerBase controllerBase) where T : class
    {
        //მივიღოთ ინფორმაცია მიღებული შეტყობინების ტანიდან
        string strBody;
        using (var reader = new StreamReader(controllerBase.Request.Body))
        {
            strBody = reader.ReadToEnd();
        }

        //Json გადავიყვანოთ მოსალოდნელ ობიექტში
        var chpData = JsonConvert.DeserializeObject<T>(strBody);

        if (chpData is null)
            return (null, "ატვირთული ინფორმაციის გაშიფვრა ვერ მოხერხდა");

        //შევამოწმოთ მიღებული ინფორმაცია ვალიდურია თუ არა
        if (controllerBase.TryValidateModel(chpData, nameof(T)))
            return (chpData, null);

        var errMessage =
            string.Join(", ", controllerBase.ModelState.Values.SelectMany(v => v.Errors).Select(s => s.ErrorMessage));
        return (null, "ატვირთული ინფორმაცია არასწორია, " + errMessage);
    }
}