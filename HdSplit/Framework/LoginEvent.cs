namespace HdSplit.Framework
{
    public class LoginEvent
    {
        public LoginEvent(string _login, string _password)
        {
            Login = _login;
            Password = _password;
        }

        public string Login { get; private set; }
        public string Password { get; private set; }
    }
}