using System;

namespace HdSplit.Framework
{
	public interface IRequestFocus
	{
		event EventHandler<FocusRequestedEventArgs> FocusRequsted;
	}

	public class FocusRequestedEventArgs
	{
		public FocusRequestedEventArgs(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; private set; }
		}
}