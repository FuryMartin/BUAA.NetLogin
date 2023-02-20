using System;
using BUAA.NetLogin.SrunLogin;
using BUAA.NetLogin.Encryption;
using BUAA.NetLogin.Storge;

namespace BUAA.NetLogin
{
    class Program{
        static async Task Main(string[] args)
        {
            SrunLoginClass srunLogin = new SrunLoginClass();
            await srunLogin.Login();
        }
    }
}

