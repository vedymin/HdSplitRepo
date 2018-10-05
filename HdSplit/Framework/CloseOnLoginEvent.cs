namespace HdSplit.Framework
{
    public class CloseOnLoginEvent
    {
        public CloseOnLoginEvent(bool _closeOnLogin)
        {
            CloseOnLogin = _closeOnLogin;
        }

        public bool CloseOnLogin { get; private set; }
    }
}