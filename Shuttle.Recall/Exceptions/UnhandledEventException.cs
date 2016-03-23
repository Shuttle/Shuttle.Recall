using System;
using System.Runtime.Serialization;

namespace Shuttle.Recall
{
	public class UnhandledEventException : Exception
	{
		public UnhandledEventException()
		{
		}

		public UnhandledEventException(string message) : base(message)
		{
		}

		public UnhandledEventException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected UnhandledEventException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}