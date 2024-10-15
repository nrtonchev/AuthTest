using System.Globalization;

namespace Core
{
	public class DomainException : Exception
	{
		public DomainException() : base() { }

		public DomainException(string message) : base(message) { }

		public DomainException(string message, params object[] args)
			: base(String.Format(CultureInfo.CurrentCulture, message, args)) { }
	}
}
