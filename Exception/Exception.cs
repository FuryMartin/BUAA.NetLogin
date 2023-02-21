namespace ExceptionHandler
{
    public class AccountNullError : Exception
    {
        public AccountNullError() : base("账号或密码为空，请使用-r或--reset重置账号") { }
        public AccountNullError(string message) : base(message) { }
        public AccountNullError(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ArgInvalidError : Exception
    {
        public ArgInvalidError() : base("输入的参数不足，请使用-r|--reset username password") { }
        public ArgInvalidError(string message) : base(message) { }
        public ArgInvalidError(string message, Exception innerException) : base(message, innerException) { }
    }
    public class LoginFailError : Exception
    {
        public LoginFailError() : base("登录失败") { }
        public LoginFailError(string message) : base(message) { }
        public LoginFailError(string message, Exception innerException) : base(message, innerException) { }
    }
    
}

