using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using Interact;

namespace Storge
{
    public class DataManager:IDataManager
    {
        public DataManager()
        {
            if (!File.Exists(databasePath))
            {
                InitializeDatabase();
                AddUser(InteractiveManager.AskForAccount());
            }
            else
            {
                try { connection.Open();}
                catch (SQLiteException e)
                {
                    InteractiveManager.ExceptionPrinter(e);
                }
                try { GetUser(); }
                catch (InvalidOperationException){AddUser(InteractiveManager.AskForAccount());}
            }
        }
        private const string databaseName = "BUAALogin.sqlite";
        private const string sqlPassword = "K@xB$m87bUa6aU";
        private const string connectionString = $"Data Source={databaseName};";

        // 拼接相对路径
        private string databasePath = System.IO.Path.Combine(Environment.CurrentDirectory, databaseName);

        private SQLiteConnection connection = new SQLiteConnection(connectionString);

        private void InitializeDatabase()
        {
            InteractiveManager.PrintDatabaseInitial();
            SQLiteConnection.CreateFile(databasePath);
            connection.Open();
            using (var command = new SQLiteCommand("CREATE TABLE Users (Username TEXT, Password TEXT)", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public void AddUser((string username, string password) account)
        {
            AddUser(account.Item1, account.Item2);
        }
        public void AddUser(string username, string password)
        {
            // 先删除已有用户
            var deleteCommand = new SQLiteCommand("DELETE FROM Users", connection);
            deleteCommand.ExecuteNonQuery();

            // 添加新用户
            var command = new SQLiteCommand("INSERT INTO Users (Username, Password) VALUES (@Username, @Password)", connection);
            command.Parameters.AddWithValue("@Username", AesEncryption.EncryptString(username));
            command.Parameters.AddWithValue("@Password", AesEncryption.EncryptString(password));
            command.ExecuteNonQuery();
        }

        public (string, string) GetUser()
        {
            using (var command = new SQLiteCommand("SELECT Username, Password FROM Users", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    return (AesEncryption.DecryptString(reader.GetString(0)), AesEncryption.DecryptString(reader.GetString(1)));
                }
            }
        }
    }
    public interface IDataManager
    {
        void AddUser(string username, string password);
        (string, string) GetUser();
    }


    public static class AesEncryption
    {
        private static byte[] Key = Encoding.UTF8.GetBytes("YG5^v%bb&FX6YS!Y");
        private static byte[] IV = Encoding.UTF8.GetBytes("36JGQ%bYssG$k*v#");

        public static string EncryptString(string plainText)
        {
            byte[] encrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        encrypted = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string DecryptString(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

}

