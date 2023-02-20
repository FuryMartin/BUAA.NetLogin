using System;
using BUAALogin.SrunLogin;
using BUAALogin.Encryption;
using BUAALogin.Storge;

namespace BUAALogin
{
    class Program{
        static async Task Main(string[] args)
        {
            SrunLoginClass srunLogin = new SrunLoginClass();
            await srunLogin.Login();
        }
    }
}

