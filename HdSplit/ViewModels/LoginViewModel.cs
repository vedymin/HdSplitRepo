using Caliburn.Micro;
using HdSplit.Framework;
using HdSplit.Models;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace HdSplit.ViewModels
{
	[Export(typeof(LoginViewModel))]
	public class LoginViewModel : Screen, IHandle<LoginConfirmedEvent>
	{
		public bool ClosedByX = true;
		private readonly IEventAggregator _events;

		private string _login;

		private string _password;

		[ImportingConstructor]
		public LoginViewModel(IEventAggregator events)
		{
			_events = events;
			events.Subscribe(this);
			ReflexTerminalModel reflex = new ReflexTerminalModel();
		}

		public string Login {
			get {
				return _login;
			}

			set {
				_login = value;
				NotifyOfPropertyChange(() => Login);
			}
		}

		public string Password {
			get {
				return _password;
			}

			set {
				_password = value;
				NotifyOfPropertyChange(() => Password);
			}
		}

		public void Handle(LoginConfirmedEvent message)
		{
			if (message.LoginConfirmed)
			{
				ClosedByX = false;
				TryClose(true);
				ClosedByX = true;
			}
		}

		public void OkButton()
		{
			_events.PublishOnUIThread(new LoginEvent(Login, Password));

			//MessageBox.Show("działa");
			//ClosedByX = false;
			//TryClose(true);
			//ClosedByX = true;
		}

		public void OnClose(KeyEventArgs keyArgs)
		{
			if (ClosedByX)
			{
				_events.PublishOnUIThread(new CloseOnLoginEvent(true));
			}
		}
	}
}