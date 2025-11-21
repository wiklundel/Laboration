using System.Text.Json;
using Microsoft.AspNetCore.Http;


namespace Laboration.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return json == null ? default(T) : System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }
}