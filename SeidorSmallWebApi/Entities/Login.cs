using System.Text.Json.Serialization;

namespace SeidorSmallWebApi.Entities;

public class Login
{
    public string UserName { get; set; }
    public string Key { get; set; }
}
