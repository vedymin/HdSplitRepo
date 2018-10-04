using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using HdSplit.Framework;
using HdSplit.Models;

namespace HdSplit.ViewModels
{
    [Export(typeof(LoginViewModel))]
    public class LoginViewModel : Screen
    {
        private readonly IEventAggregator _events;

        private string _login;
        public string Login {
            get {
                return _login;
            }

            set {
                _login = value;
                NotifyOfPropertyChange(() => Login);
            }
        }

        private string _password;
        public string Password {
            get {
                return _password;
            }

            set {
                _password = value;
                NotifyOfPropertyChange(() => Password);
            }
        }

        [ImportingConstructor]
        public LoginViewModel(IEventAggregator events)
        {
            _events = events;
            ReflexTerminalModel reflex = new ReflexTerminalModel();
        }

        public bool ClosedByX = true;

        public void OkButton()
        {
            _events.PublishOnUIThread(new LoginEvent(Login,Password));

            //MessageBox.Show("działa");
            //ClosedByX = false;
            //TryClose(true);
            //ClosedByX = true;
        }

        public void OnClose(KeyEventArgs keyArgs)
        {
            if (ClosedByX)
            {
                Application.Current.Shutdown();
                
            }
        }
    }
}