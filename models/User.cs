using System.Text.Json.Serialization;

namespace Trackey;

class User
{
    [JsonInclude]
    public Guid Id { get; set; }
    public string Username { get; set; } = "N/A";
    public string Password { get; set; } = "N/A";
    // TODO: Store Hashed password instead

    public override string ToString()
    {
        return $"{{_id: {Id} | username: {Username} | password: {Password}}}";
    }
}
