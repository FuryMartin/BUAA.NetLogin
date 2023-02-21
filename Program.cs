using SrunTools;
using Interact;
using Storge;
using ExceptionHandler;

namespace BUAA.NetLogin
{
    class Program{
        static async Task Main(string[] args)
        {
            IDataManager dataManager = new DataManager();
            if (args.Length == 0)
            {
                Login srunLogin = new Login(dataManager);
                await srunLogin.ExecuteLogin();
            }
            else{
                string command = args[0];
                string[] commandArgs = args.Skip(1).ToArray();
                switch (command)
                {
                    case "-h":
                    case "--help":
                        {
                            InteractiveManager.PrintHelp();
                            break;
                        }
                    case "-r":
                    case "--reset":
                        {
                            if(commandArgs.Count() < 2)
                                InteractiveManager.ExceptionPrinter(new ArgInvalidError());
                            dataManager.AddUser(commandArgs[0], commandArgs[1]);
                            Login srunLogin = new Login(dataManager);
                            await srunLogin.ExecuteLogin();
                            break;
                        }
                    default:
                        InteractiveManager.PrintHelp();
                        return;
                }
            }   
        }
    }
}


// using System;
// using System.Threading;

// class Program
// {
//     static void Main(string[] args)
//     {
//         int totalSteps = 4;
//         int currentStep = 0;
//         Console.CursorVisible = false;

//         Console.ForegroundColor = ConsoleColor.Green; // 设置前景色为绿色
//         Console.WriteLine("Starting process...");
//         Console.ForegroundColor = ConsoleColor.Yellow; // 设置前景色为绿色

//         // Create a timer to update the progress bar
//         Timer timer = new Timer(_ =>
//         {
//             int width = Console.WindowWidth - 2; // Account for borders
//             int progressWidth = (int)Math.Floor((double)(currentStep * width) / totalSteps);
//             string progressBar = "[" + new string('=', progressWidth) + new string(' ', width - progressWidth) + "]";
//             Console.SetCursorPosition(0, Console.CursorTop);
//             Console.Write(progressBar);
//         }, null, 0, 100);

//         // Perform the steps
//         for (int i = 0; i < totalSteps; i++)
//         {
//             currentStep = i;
//             Thread.Sleep(1000); // Simulate work being done
//         }

//         // Clean up
//         timer.Dispose();
//         Console.CursorVisible = true;
//         Console.WriteLine("");
//         Console.SetCursorPosition(0, Console.CursorTop);
//         Console.WriteLine("\nDone!");
//     }
// }