using System;
using System.Collections.Frozen;

namespace BackendCarcass.Application.Identity;

//ყველაზე გავრცელებული პაროლების სია (გაჟონილი ბაზების სტატისტიკის მიხედვით). შედარება რეგისტრს არ არჩევს.
//სია შეიძლება გაფართოვდეს, რიგითობას მნიშვნელობა არ აქვს
public static class CommonPasswords
{
    private static readonly FrozenSet<string> Passwords = new[]
    {
        "123456", "123456789", "12345678", "1234567890", "12345678910", "123456789012", "1234567890123",
        "0123456789", "01234567890", "098765432109", "987654321", "9876543210", "111111", "111111111111",
        "000000", "000000000000", "121212", "123123", "123123123", "123123123123", "112233", "123321", "654321",
        "666666", "888888", "1q2w3e4r", "1q2w3e4r5t", "1q2w3e4r5t6y", "1q2w3e4r5t6y7u", "1q2w3e4r5t6y7u8i",
        "1qaz2wsx", "1qaz2wsx3edc", "1qaz2wsx3edc4rfv", "qwerty", "qwerty123", "qwerty1234", "qwerty12345",
        "qwerty123456", "qwertyuiop", "qwertyuiop123", "qwertyuiop1234", "qwertyuiopasdfghjkl", "qwertyuiop[]",
        "qwerasdf", "qwertyasdfgh", "asdfghjkl", "asdfghjkl123", "asdfghjkl1234", "asdfgh", "asdfasdf", "zxcvbnm",
        "zxcvbnm123", "zxcvbnm1234", "zxcvbnmasdfghjkl", "abc123", "abc123456", "abcd1234", "abcdef123456",
        "abcdefgh1234", "abcdefghijkl", "abcdefghijklmnop", "a1b2c3d4", "a1b2c3d4e5", "a1b2c3d4e5f6",
        "password", "password1", "password12", "password123", "password1234", "password12345", "password123456",
        "password1!", "password!", "password2024", "password2025", "password2026", "passw0rd", "passw0rd1",
        "passw0rd123", "passw0rd1234", "p@ssw0rd", "p@ssw0rd1", "p@ssw0rd123", "p@ssword", "p@ssword1",
        "p@ssword123", "pa55word", "pa55w0rd", "pa$$w0rd", "pa$$word", "mypassword", "mypassword1",
        "mypassword123", "newpassword", "newpassword1", "newpassword123", "strongpassword", "strongpassword1",
        "securepassword", "securepassword1", "passwordpassword", "passwordisstrong", "thisismypassword",
        "letmein", "letmein123", "letmein1234", "letmein12345", "welcome", "welcome1", "welcome123",
        "welcome1234", "welcome12345", "welcome123456", "admin", "admin123", "admin1234", "admin12345",
        "admin123456", "admin1234567", "administrator", "administrator1", "administrator123", "root1234",
        "root123456", "toor123456", "changeme", "changeme1", "changeme123", "changeme1234", "changeme12345",
        "default1234", "guest1234", "guest12345", "user1234", "user12345", "user123456", "test1234", "test12345",
        "test123456", "testtest1234", "testing123", "testing1234", "temp1234", "temporary1", "iloveyou",
        "iloveyou1", "iloveyou123", "iloveyou1234", "iloveyou12345", "loveyou12345", "ilovegod1234",
        "trustno1", "trustno1trustno1", "monkey123", "monkey1234", "dragon123", "dragon1234", "master123",
        "master1234", "shadow123", "shadow1234", "sunshine1", "sunshine123", "sunshine1234", "princess1",
        "princess123", "princess1234", "football1", "football123", "football1234", "baseball1", "baseball123",
        "baseball1234", "basketball1", "basketball12", "basketball123", "superman1", "superman123",
        "superman1234", "batman123", "batman1234", "spiderman1", "spiderman123", "starwars1", "starwars123",
        "pokemon123", "pokemon1234", "minecraft1", "minecraft123", "fortnite123", "michael123", "michael1234",
        "jennifer1", "jennifer123", "jessica123", "charlie123", "charlie1234", "daniel123", "ashley123",
        "nicole123", "hunter123", "killer123", "soccer123", "hockey123", "cheese123", "computer1", "computer123",
        "computer1234", "internet1", "internet123", "samsung123", "google123", "google1234", "facebook1",
        "facebook123", "gmail1234", "yahoo1234", "hello123", "hello1234", "hello12345", "hello123456",
        "helloworld", "helloworld1", "helloworld123", "freedom123", "whatever1", "whatever123", "secret123",
        "secret1234", "summer2024", "summer2025", "summer2026", "winter2024", "winter2025", "winter2026",
        "spring2025", "spring2026", "autumn2025", "january2026", "december2025", "monday123", "friday123",
        "1234qwer", "12345qwert", "123456qwerty", "123qwe", "123qweasd", "123qweasdzxc", "1234abcd",
        "12345abcde", "123456abcdef", "qwe123", "qwe123456", "asd123", "asd123456", "zxc123456", "aaaaaa",
        "aaaaaaaaaaaa", "abcabcabcabc", "123abc", "abc12345", "abcd12345", "abcde12345", "1a2b3c4d", "1a2b3c4d5e",
        "a123456", "a12345678", "a1234567890", "z123456", "s123456", "m123456", "w123456", "qazwsx", "qazwsxedc",
        "qazwsxedcrfv", "zaq12wsx", "zaq1xsw2", "zaq1zaq1", "!qaz2wsx", "!qaz@wsx", "1qazxsw2", "1qazxsw23edc",
        "0987654321", "7777777", "7777777777", "11223344", "1122334455", "112233445566", "1234512345",
        "123451234512345", "12341234", "123412341234", "1234567a", "12345678a", "123456789a", "1234567890a",
        "1234567q", "12345678q", "1234567890q", "123456789q", "123456789z", "123456789m", "123456789s",
        "ilovemyself", "iloveyoubaby", "ilovemyfamily", "letmeinplease", "openSesame", "opensesame1",
        "opensesame123", "liverpool1", "liverpool123", "chelsea123", "arsenal123", "barcelona1", "barcelona123",
        "realmadrid1", "manchester1", "juventus123", "dinamo1234", "tbilisi123", "tbilisi1234", "georgia123",
        "georgia1234", "sakartvelo", "sakartvelo1", "sakartvelo123", "batumi1234", "kutaisi123", "parol1234",
        "paroli1234", "paroli12345", "paroli123456"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool Contains(string password)
    {
        return Passwords.Contains(password.Trim());
    }
}
