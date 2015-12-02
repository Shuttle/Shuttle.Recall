using System;

namespace Shuttle.Recall.Core
{
	public class SerializerUnknownTypeExcption : Exception
	{
		public SerializerUnknownTypeExcption(string type)
			: base(string.Format(RecallResources.SerializerUnknownTypeExcption, type))
		{
		}
	}
}