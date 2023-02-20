using System;
using System.Collections.Generic;
using System.Net.Http;
using SrunLogin;
using SrunEncryption;

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

