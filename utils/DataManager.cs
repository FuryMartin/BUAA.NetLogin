using System.Data.SQLite;
using BUAA.NetLogin.Encryption;
using System.IO;

namespace BUAA.NetLogin.Storge
{
    public class DataManager:IDataManager
    {
        public DataManager()
        {
            if (!File.Exists(databasePath))
                InitializeDatabase();
            else
            {
                try { connection.Open(); }
                catch (SQLiteException)
                {
                    Console.WriteLine("数据库损坏");
                }
            }
        }

        private const string databaseName = "BUAA.NetLogin.sqlite";
        private const string sqlPassword = "K@xB$m87bUa6aU";
        private const string connectionString = $"Data Source={databaseName};";

        // 拼接相对路径
        private string databasePath = System.IO.Path.Combine(Environment.CurrentDirectory, databaseName);

        private SQLiteConnection connection = new SQLiteConnection(connectionString);

        private void InitializeDatabase()
        {
            SQLiteConnection.CreateFile(databasePath);
            connection.Open();
            using (var command = new SQLiteCommand("CREATE TABLE Users (Username TEXT, Password TEXT)", connection))
            {
                command.ExecuteNonQuery();
            }
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
}

