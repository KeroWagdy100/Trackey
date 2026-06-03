using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Trackey.Utils;

namespace Trackey;

class UserService
{
    private List<User> users = [];

    public ValidationResult Login(string username, string password, out User? user)
    {
        if (!GetUser(username, out user))
            return new(false, "username", [$"{username} doesn't exist."]);

        if (user.Password != password)
            return new(false, "password", [$"Invalid password"]);

        return new(true, null, null);
    }


    public ValidationResult Register(string username, string password, out User? newUser)
    {
        newUser = null;

        var usernameResult = ValidateUsername(username);
        if (!usernameResult.Success)
            return usernameResult;

        var passwordResult = ValidatePassword(password);
        if (!passwordResult.Success)
            return passwordResult;

        newUser = new() { Id = Guid.NewGuid(), Username = username, Password = password };
        users.Add(newUser);

        _ = SaveUsers();
        return new(true, null, null);
    }

    private const int MIN_USERNAME_LEN = 3;
    private const int MAX_USERNAME_LEN = 15;

    private const string USERNAME_RULES =
        "1. Should be unique"
        + "\n2. It's length should be in range[3, 15]"
        + "\n3. Has only (upper letters, lower letters, underscores or digits)"
        + "\n4. Has at least one alphabet letter (upper or lower)\n";
    public ValidationResult ValidateUsername(string username)
    {
        List<string> invalidReasons = [];
        bool validLength = MyMath.Between(username.Length, MIN_USERNAME_LEN, MAX_USERNAME_LEN);
        bool validChars = username.All(c => char.IsLetter(c) || char.IsDigit(c) || c == '_');
        bool alphaFound = username.Any(c => char.IsLetter(c));
        bool unique = !GetUser(username, out User? user);

        if (!validLength)
            invalidReasons.Add($"username length cannot be {username.Length}!\n");
        if (!validChars)
            invalidReasons.Add($"invalid character/s found!\n");
        if (!alphaFound)
            invalidReasons.Add($"no alphabet letter found!\n");
        if (!unique)
            invalidReasons.Add($"{username} is used before, try another one\n");

        bool valid = validLength && validChars && alphaFound && unique;
        return new(
        valid,
        "username",
        invalidReasons
        );
    }

    private const int MIN_PASSWORD_LEN = 7;
    private const int MAX_PASSWORD_LEN = 31;
    public const string VALID_SPECIAL_CHARS = "!@#$%^&*_";

    public static Predicate<char> ValidateChar = c => char.IsLetterOrDigit(c) || VALID_SPECIAL_CHARS.Contains(c);

    private const string PASSWORD_RULES =
        "1. It's length should be in range[7, 31]"
        + $"\n2. Has only (upper letters, lower letters, digits or special characters ({VALID_SPECIAL_CHARS}))"
        + "\n3. Has at least one upper letter"
        + "\n4. Has at least one lower letter"
        + "\n5. Has at least one digit"
        + "\n6. Has at least one special character";
    public ValidationResult ValidatePassword(string password)
    {
        List<string> invalidReasons = [];
        bool validLength = MyMath.Between(password.Length, MIN_PASSWORD_LEN, MAX_PASSWORD_LEN);
        bool upper = false, lower = false, digit = false, special = false;
        bool allValidChars = true;
        foreach (char c in password)
        {
            upper |= char.IsUpper(c);
            lower |= char.IsLower(c);
            digit |= char.IsDigit(c);
            special |= VALID_SPECIAL_CHARS.Contains(c);

            allValidChars &= char.IsLetter(c) || char.IsDigit(c) || VALID_SPECIAL_CHARS.Contains(c);
        }

        if (!validLength)
            invalidReasons.Add($"password length cannot be {password.Length}!");
        if (!allValidChars)
            invalidReasons.Add($"invalid character/s found!");
        if (!upper)
            invalidReasons.Add($"no upper (capital) letter found!");
        if (!lower)
            invalidReasons.Add($"no lower (small) letter found!");
        if (!digit)
            invalidReasons.Add($"no digit found!");
        if (!special)
            invalidReasons.Add($"no special character found!");

        bool valid = validLength && upper && lower && digit && special && allValidChars;
        return new(
        valid,
        "password",
        invalidReasons
        );
    }

    public bool GetUser(string username, [NotNullWhen(true)] out User? user)
    {
        user = users.Find(u => u.Username == username);
        return user is not null;
    }

    public bool TryGetUser(Guid userId, [NotNullWhen(true)] out User? user)
    {
        user = users.Find(u => u.Id == userId);
        return user is not null;
    }

    private static readonly string USERS_FILEPATH = Paths.UsersFile;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<OperationResult> LoadUsers()
    {
        users = [];
        if (!File.Exists(USERS_FILEPATH))
            return OperationResult.Ok();

        try
        {
            using FileStream fs = File.Open(USERS_FILEPATH, FileMode.Open);

            var data = await JsonSerializer.DeserializeAsync<List<User>>(fs);

            if (data is null)
                return OperationResult.Fail("Failed to load users");

            users = data;

            Logger.Log($"Loaded Users Successfully");
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return OperationResult.Fail("Failed to load users");
        }
    }

    public async Task<OperationResult> SaveUsers()
    {
        try
        {
            using FileStream fs = File.Open(USERS_FILEPATH, FileMode.Create);

            await JsonSerializer.SerializeAsync(fs, users, JsonOptions);

            Logger.Log($"Saved Users Successfully");
            return OperationResult.Ok();
        }
        catch (Exception ex)
        {
            Logger.Log(ex.ToString());
            return OperationResult.Fail("Failed to save users");
        }
    }
}
