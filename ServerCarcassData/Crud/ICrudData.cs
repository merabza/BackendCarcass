namespace ServerCarcassData.Crud;

public interface ICrudData
{
    int Id { get; }

    //ამ მეთოდის დანიშნულებაა შეამოწმოს, გამოდგება თუ არა მოწოდებული ინფორმაცია შესანახად
    string? CheckValidateData(ICrudRepository repo, int? id);
}