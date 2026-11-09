namespace BackendCarcass.Domain.EmailVerifications;

//რისთვის გაიგზავნა ელფოსტაზე კოდი და ბმული
public enum EEmailVerificationPurpose
{
    //ელფოსტის მისამართის დადასტურება (რეგისტრაციისას, ან პირველი ავტორიზაციისას)
    EmailConfirmation = 0,

    //ავტორიზაციისას მოთხოვნილი სავალდებულო პაროლის ცვლილება
    PasswordChange = 1,

    //დავიწყებული პაროლის აღდგენა
    PasswordReset = 2,

    //ელფოსტის მისამართის შეცვლა: კოდი ახალ მისამართზე იგზავნება
    EmailChange = 3
}
