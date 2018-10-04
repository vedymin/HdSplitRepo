namespace HdSplit.Framework
{
    public class LoginConfirmedEvent
    {
        public LoginConfirmedEvent(bool _loginConfirmed)
        {
            LoginConfirmed = _loginConfirmed;
        }

        public bool LoginConfirmed { get; private set; }
    }
}
