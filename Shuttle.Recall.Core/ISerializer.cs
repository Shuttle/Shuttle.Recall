using System;
using System.IO;

namespace Shuttle.Recall.Core
{
	public interface ISerializer
	{
		Stream Serialize(object instance);
		object Deserialize(Type type, Stream stream);
	}
}