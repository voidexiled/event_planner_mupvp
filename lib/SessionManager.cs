using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace event_planner_mupvp.lib;

public static class SessionManager
{
    private const string DATA_FILE_PATH = "session.dat";
    private static string itemData = "";
    public static User currentUser { get; set; } = new User();
    public static bool loggedIn { get; set; } = false;
    public static string errorMessage { get; set; } = "";


    private static readonly string Key = "k,Az!EsrRLNK#jQH.7_p-f)`G;/P'J<q";

    public static bool SaveSession(string key)
    {
        if (ApiManager.LogInWithKey(key)) // Verificación en la base de datos
        {
            try
            {
                if (SessionFileExists())
                {
                    File.Delete(DATA_FILE_PATH);
                }

                using (FileStream fs = new FileStream(DATA_FILE_PATH, FileMode.CreateNew))
                using (BinaryWriter w = new BinaryWriter(fs))
                {
                    byte[] encryptedKey = ProtectData(key);
                    w.Write(encryptedKey.Length); // Escribimos la longitud del string cifrado
                    w.Write(encryptedKey);
                }

                //InitUser(key, UserTypes.CREADOR);
                //ApiManager.SendUserToDB(currentUser);
                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
        }

        return false;
    }

    private static void InitUser(string key, UserTypes userType)
    {
        currentUser = new User
        {
            Key = key,
            UserType = userType
        };
        loggedIn = true;
    }

    public static bool UserIsOwner() => currentUser.UserType == UserTypes.CREADOR;
    public static bool UserIsAdministrator() => currentUser.UserType == UserTypes.ADMINISTRADOR || UserIsOwner();
    public static bool UserIsCommunityManager() => currentUser.UserType == UserTypes.COMMUNITYMANAGER || UserIsAdministrator() || UserIsOwner();
    public static bool UserIsGameManager() => currentUser.UserType == UserTypes.GAMEMANAGER || UserIsCommunityManager() || UserIsAdministrator() || UserIsOwner();
    public static bool UserIsTutor() => currentUser.UserType == UserTypes.TUTOR || UserIsGameManager() || UserIsCommunityManager() || UserIsAdministrator() || UserIsOwner();
    public static bool UserIsPremiumGold() => currentUser.UserType == UserTypes.VIP2 || UserIsTutor() || UserIsGameManager() || UserIsCommunityManager() || UserIsAdministrator() || UserIsOwner();
    public static bool UserIsPremiumSilver() => currentUser.UserType == UserTypes.VIP1 || UserIsPremiumGold() || UserIsTutor() || UserIsGameManager() || UserIsCommunityManager() || UserIsAdministrator() || UserIsOwner();
    public static bool UserIsPremiumBronze() => currentUser.UserType == UserTypes.VIP0 || UserIsPremiumSilver() || UserIsPremiumGold() || UserIsTutor() || UserIsGameManager() || UserIsCommunityManager() || UserIsAdministrator() || UserIsOwner();
    public static bool UserIsVip() => UserIsPremiumBronze() || UserIsPremiumSilver() || UserIsPremiumGold() || UserIsTutor() || UserIsGameManager() || UserIsCommunityManager() || UserIsAdministrator() || UserIsOwner();
    public static bool UserIsUser() => currentUser.UserType == UserTypes.USUARIO || UserIsVip() || UserIsTutor() || UserIsGameManager() || UserIsCommunityManager() || UserIsAdministrator() || UserIsOwner();



    public static bool LoadSession()
    {
        string key = "";
        if (File.Exists(DATA_FILE_PATH))
        {
            try
            {
                using FileStream fs = new FileStream(DATA_FILE_PATH, FileMode.Open, FileAccess.Read);
                using BinaryReader r = new BinaryReader(fs);
                int encryptedKeyLength = r.ReadInt32(); // Leemos la longitud del string cifrado
                byte[] encryptedKey = r.ReadBytes(encryptedKeyLength);
                key = UnprotectData(encryptedKey);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
        }
        else
        {
            errorMessage = "Por favor, ingresa tu licencia.";
            return false;
        }

        if (key.Length > 0)
        {
            // TODO: CHECAR SI ES UNA KEY VALIDA EN LA BASE DE DATOS, SINO RETORNAR FALSE
            if (ApiManager.LogInWithKey(key))
            {
                //InitUser(key, UserTypes.CREADOR);
                Console.WriteLine("Sesion cargada");
                //Console.WriteLine(currentUser.Key);
                //Console.WriteLine(currentUser.UserType);
                return true;
            }

        }

        errorMessage = "Ha ocurrido un error, por favor consultalo con un administrador.";
        return false;
    }

    public static bool DeleteSession()
    {
        if (SessionFileExists())
        {
            try
            {
                File.Delete(DATA_FILE_PATH);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
            finally
            {
                loggedIn = false;
            }

            return true;
        }

        return false;
    }

    private static bool SessionFileExists()
    {
        return File.Exists(DATA_FILE_PATH);
    }

    private static byte[] ProtectData(string data)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(Key.PadRight(32).Substring(0, 32)); // Asegura que la clave tenga 32 bytes (256 bits)
        aesAlg.IV = aesAlg.Key.Take(16).ToArray(); // IV también debe tener 16 bytes

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new MemoryStream();
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
            swEncrypt.Write(data);
        }

        return msEncrypt.ToArray();
    }

    private static string UnprotectData(byte[] encryptedData)
    {
        try
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(Key.PadRight(32).Substring(0, 32)); // Asegura que la clave tenga 32 bytes (256 bits)
            aesAlg.IV = aesAlg.Key.Take(16).ToArray(); // IV también debe tener 16 bytes

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new MemoryStream(encryptedData);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
        catch (CryptographicException)
        {
            // Manejar la excepción en caso de fallo al descifrar (puede ocurrir si los datos han sido manipulados)
            return null;
        }
    }
}
