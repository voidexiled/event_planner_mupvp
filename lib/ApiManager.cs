
using System.Numerics;
using FireSharp.Config;
using FireSharp.Interfaces;

public static class ApiManager
{

    public static Vector4 messageColor { get; set; } = new Vector4(1f, 1f, 1f, 1f);
    public static string adminMessage { get; set; } = "";
    public static IFirebaseConfig config = new FirebaseConfig
    {
        AuthSecret = "pkqvpWgHmWFVGXXq9xTL5SvetIiRwFFeM7msAKzG",
        BasePath = "https://mupvptool-default-rtdb.firebaseio.com/",
    };
    public static IFirebaseClient client = new FireSharp.FirebaseClient(config);
    public static bool IsKeyRegistered(string key)
    {

        return true;
    }

    public static bool IsConnectionActive()
    {
        //SessionManager.errorMessage = "SI HAY CONEXION";
        return client != null;
    }

    public static bool RegisterNewUser(User user)
    {
        try
        {
            var response = client.Set("users/" + user.Key, user);

            messageColor = new Vector4(0f, 1f, 0f, 1f);
            adminMessage = "Success!";

            return true;
        }
        catch (System.Exception)
        {
            messageColor = new Vector4(1f, 0f, 0f, 1f);
            adminMessage = "Error al enviar el usuario a la base de datos.";

            return false;
        }
    }

    public static bool LogInWithKey(string key)
    {
        try
        {
            var response = client.Get("users/" + key);
            if (response.Body != "null")
            {
                var _user = response.ResultAs<Dictionary<string, string>>();
                User user = new()
                {
                    Key = key,
                    UserType = (UserTypes)Enum.Parse(typeof(UserTypes), _user["UserType"])
                };
                Console.WriteLine($"User: {user.Key}, Type: {user.UserType}");
                SessionManager.currentUser = user;
                SessionManager.loggedIn = true;
                return true;
            }
            else
            {
                SessionManager.errorMessage = "La KEY es invalida.";
                return false;
            }
        }
        catch (System.Exception e)
        {
            SessionManager.errorMessage = e.Message;
            return false;
        }
    }

    public static bool PopulateGuidesToDatabase(List<Guide> guides)
    {
        try
        {
            var response = client.Set("guides", guides);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
    public static List<Guide> GetGuidesFromDB()
    {
        try
        {
            var response = client.Get("guides");
            if (response.Body != "null")
            {
                return response.ResultAs<List<Guide>>();
            }
            else
            {
                return new();
            }
        }
        catch (System.Exception)
        {
            return new();
        }
    }

    public static bool UpdateGuide(Guide guide)
    {
        try
        {
            var response = client.Set("guides/" + guide.Id, guide);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public static bool DeleteGuide(Guide guide)
    {
        try
        {
            var response = client.Delete("guides/" + guide.Id);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public static bool AddGuide(Guide guide)
    {
        try
        {
            var response = client.Set("guides/" + guide.Id, guide);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
}