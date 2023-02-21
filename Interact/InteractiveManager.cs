namespace Interact
{
    public static class InteractiveManager
    {
        public static void ExceptionPrinter(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"错误类型：{e.GetType().Name}\n错误信息：{e.Message}");
            Console.ResetColor();
            Environment.Exit(0);
        }

        public static (string,string) AskForAccount()
        {
            Console.Write("\u001b[33m学号：");
            string username = Console.ReadLine();
            Console.Write("\u001b[33m密码：");
            string password = EncryptedInput();
            Console.Write("\u001b[0m");
            return (username, password);
        }

        public static string EncryptedInput()
        {
            int cursorIndex = Console.CursorLeft;
            string password = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if(Console.CursorLeft > cursorIndex)
                    {
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        if (!string.IsNullOrEmpty(password))
                            password = password.Substring(0, password.Length - 1);
                    }
                    continue;
                }
                password += key.KeyChar;
                Console.Write("*");
            }
            return password;
        }

        public static void PrintHelp()
        {
            string help = "-h|--help";
            string reset = "-r|--reset";
            Console.WriteLine("\u001b[32m按照\u001b[33m BUAANETLogin \u001b[34m[options] \u001b[36m[arguments] \u001b[32m的格式使用下列参数运行本工具");
            Console.WriteLine($"\u001b[32m帮助: \u001b[33m{help}\t");
            Console.WriteLine($"\u001b[32m重置: \u001b[33m{reset} \u001b[34musername \u001b[36mpassword\u001b[0m");
        }

        public static void PrintDatabaseInitial()
        {
            Console.WriteLine("\u001b[33m未发现账号，开始初始化数据库\u001b[0m");
        }
        public static void Success()
        {
            Console.WriteLine("\u001b[32m您已通过校园网认证\u001b[0m");
            Environment.Exit(0);
        }

        public static void ColorfulProgress(string currentStep, string totalSteps, string info)
        {
            Console.Write($"\u001b[34mStep \u001b[33m{currentStep}\u001b[0m/\u001b[32m{totalSteps}\u001b[34m {info} ");
        }
        public static void ColorfulProgress(string res)
        {
            Console.Write($"\u001b[36m{res}\u001b[0m \n");
        }
    }
    
}