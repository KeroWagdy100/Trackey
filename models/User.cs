using System.Text.Json;
using System.Text.Json.Serialization;
using Trackey.Utils;

namespace Trackey;

class User
{
    [JsonInclude]
    public int UserId { get; private set; }
    public string Username { get; set; } = "N/A";
    public string Password { get; set; } = "N/A";

    private static List<User> users = new();

    public static bool Login(out User user)
    {
        user = new();
        for (int trial = 0; trial < Application.MX_TRIALS; ++trial)
        {
            if (Terminal.Input("Username", out string inputUsername))
                if (GetUser(inputUsername, out user!))
                    break;

            if (trial == Application.MX_TRIALS - 1)
            {
                Terminal.OperationFailed("Login Failed");
                return false;
            }
            Terminal.InvalidInputWarning("Username {", inputUsername, "} doesn't exist, try again");
        }

        for (int trial = 0; trial < Application.MX_TRIALS; ++trial)
        {
            if (Terminal.Input("Password", out string inputPassword))
                if (user.Password == inputPassword)
                    break;

            if (trial == Application.MX_TRIALS - 1)
            {
                Terminal.OperationFailed("Login Failed");
                return false;
            }
            Terminal.InvalidInputWarning("{", inputPassword, "} is a wrong password");
        }

        return true;
    }


    public static bool Register(out User newUser)
    {
        Console.WriteLine("\t-- Registering a new user --");

        newUser = new() { UserId = users.Count };

        Terminal.WriteLine("Username Rules:");
        Terminal.Write(USERNAME_RULES, Terminal.ShadowColor);
        for (int trial = 0; trial < Application.MX_TRIALS; ++trial)
        {
            if (Terminal.Input("Create Username", out string username))
            {
                if (IsValidUsername(username, out string invalidReason))
                {
                    newUser.Username = username;
                    break;
                }
                else
                    Terminal.Warning(invalidReason!, false);

            }
            if (trial == Application.MX_TRIALS - 1)
            {
                Terminal.OperationFailed("Registering Failed");
                return false;
            }
            Terminal.InvalidInputWarning("Username {", username, "} is not valid, try again");
        }

        Terminal.WriteLine("Password Rules:");
        Terminal.WriteLine(PASSWORD_RULES, Terminal.ShadowColor);
        for (int trial = 0; trial < Application.MX_TRIALS; ++trial)
        {
            if (Terminal.Input("Create Password", out string password))
            {
                if (IsValidPassword(password, out string invalidReason))
                {
                    newUser.Password = password;
                    break;
                }
                else
                    Terminal.Warning(invalidReason!, false);
            }

            if (trial == Application.MX_TRIALS - 1)
            {
                Terminal.OperationFailed("Registering Failed");
                return false;
            }
            Terminal.InvalidInputWarning("Password {", password, "} is not valid, try again");
        }

        users.Add(newUser);

        return true;
    }

    private const int MIN_USERNAME_LEN = 3;
    private const int MAX_USERNAME_LEN = 15;

    private const string USERNAME_RULES =
        "\t1. Should be unique"
        + "\n\t2. It's length should be in range[3, 15]"
        + "\n\t3. Has only (upper letters, lower letters, underscores or digits)"
        + "\n\t4. Has at least one alphabet letter (upper or lower)\n";
    private static bool IsValidUsername(string username, out string invalidReason)
    {
        invalidReason = "";
        bool validLength = MyMath.Between(username.Length, MIN_USERNAME_LEN, MAX_USERNAME_LEN);
        bool validChars = username.All(c => char.IsUpper(c) || char.IsLower(c) || char.IsDigit(c) || c == '_');
        bool alphaFound = username.Any(c => char.IsUpper(c) || char.IsLower(c));
        bool unique = !GetUser(username, out User? user);

        if (!validLength)
            invalidReason += $"\tusername length cannot be {username.Length}!\n";
        if (!validChars)
            invalidReason += $"\tinvalid character/s found!\n";
        if (!alphaFound)
            invalidReason += $"\tno alphabet letter found!\n";
        if (!unique)
            invalidReason += $"\t{username} is used before, try another one\n";

        return validLength && validChars && alphaFound;
    }

    private const int MIN_PASSWORD_LEN = 7;
    private const int MAX_PASSWORD_LEN = 31;
    private const string VALID_SPECIAL_CHARS = "!@#$%^&*_";

    private const string PASSWORD_RULES =
        "\t1. It's length should be in range[7, 31]"
        + $"\n\t2. Has only (upper letters, lower letters, digits or special characters ({VALID_SPECIAL_CHARS}))"
        + "\n\t3. Has at least one upper letter"
        + "\n\t4. Has at least one lower letter"
        + "\n\t5. Has at least one digit"
        + "\n\t6. Has at least one special character";
    private static bool IsValidPassword(string password, out string invalidReason)
    {
        invalidReason = "";
        bool validLength = MyMath.Between(password.Length, MIN_PASSWORD_LEN, MAX_PASSWORD_LEN);
        bool upper = false, lower = false, digit = false, special = false;
        bool allValidChars = true;
        foreach (char c in password)
        {
            upper |= char.IsUpper(c);
            lower |= char.IsLower(c);
            digit |= char.IsDigit(c);
            special |= VALID_SPECIAL_CHARS.Contains(c);

            allValidChars &= char.IsUpper(c) || char.IsLower(c) || char.IsDigit(c) || VALID_SPECIAL_CHARS.Contains(c);
        }

        if (!validLength)
            invalidReason += $"\tpassword length cannot be {password.Length}!\n";
        if (!allValidChars)
            invalidReason += $"\tinvalid character/s found!\n";
        if (!upper)
            invalidReason += $"\tno upper (capital) letter found!\n";
        if (!lower)
            invalidReason += $"\tno lower (small) letter found!\n";
        if (!digit)
            invalidReason += $"\tno digit found!\n";
        if (!special)
            invalidReason += $"\tno special character found!\n";

        return validLength && upper && lower && digit && special && allValidChars;
    }

    public static bool GetUser(string username, out User? user)
    {
        user = users.Find(u => u.Username == username);
        return user is not null;
    }

    public static bool GetUser(int userId, out User? user)
    {
        user = users.Find(u => u.UserId == userId);
        return user is not null;
    }

    public static bool LoadUsers()
    {
        // TODO: Handle Better
        try
        {
            using (FileStream usersFile = File.Open("users.json", FileMode.OpenOrCreate))
            {
                users = JsonSerializer.Deserialize<List<User>>(usersFile) ?? [];
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public static bool SaveUsers()
    {
        try
        {
            using (FileStream fs = File.Open("users.json", FileMode.Create))
            {
                var jsonUsers = JsonSerializer.Serialize(users);
                MyFile.AddText(fs, jsonUsers);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public override string ToString()
    {
        return $"{{_id: {UserId} | username: {Username} | password: {Password}}}";
    }
}
