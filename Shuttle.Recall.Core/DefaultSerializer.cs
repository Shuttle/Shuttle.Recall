using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class DefaultSerializer : ISerializer
	{
		private const string SoapInclude = "SoapInclude";
		private const string TypeError = "--- could not retrieve type name from exception ---";
		private const string XmlInclude = "XmlInclude";

		private static readonly Regex Expression = new Regex(@"The\stype\s(?<Type>[\w\\.]*)");

		private static readonly object Padlock = new object();

		private readonly List<Type> _knownTypes = new List<Type>();
		private readonly XmlWriterSettings _xmlSettings;
		private Dictionary<string, XmlSerializer> _serializers = new Dictionary<string, XmlSerializer>();
		private Dictionary<string, XmlSerializerNamespaces> _serializerNamespaces = new Dictionary<string, XmlSerializerNamespaces>();

		public DefaultSerializer()
		{
			_xmlSettings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				OmitXmlDeclaration = true,
				Indent = true
			};
		}

		private void AddKnownType(Type type)
		{
			Guard.AgainstNull(type, "type");

			if (HasKnownType(type))
			{
				return;
			}

			lock (Padlock)
			{
				if (HasKnownType(type))
				{
					return;
				}

				_knownTypes.Add(type);

				foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (!HasKnownType(nested))
					{
						AddKnownType(nested);
					}
				}

				ResetSerializers();
			}
		}

		public bool HasKnownType(Type type)
		{
		    lock (Padlock)
		    {
		        return _knownTypes.Find(candidate => (candidate.AssemblyQualifiedName ?? String.Empty).Equals(type.AssemblyQualifiedName, StringComparison.InvariantCultureIgnoreCase)) != null;
		    }
		}

	    public Stream Serialize(object instance)
		{
			Guard.AgainstNull(instance, "instance");

			var messageType = instance.GetType();

			AddKnownType(messageType);

			var serializer = GetSerializer(messageType);

			var xml = new StringBuilder();

			using (var writer = XmlWriter.Create(xml, _xmlSettings))
			{
				try
				{
					serializer.Serialize(writer, instance, GetSerializerNamespaces(messageType));
				}
				catch (InvalidOperationException ex)
				{
					var exception = UnknownTypeException(ex);

					if (exception != null)
					{
						throw new SerializerUnknownTypeExcption(GetTypeName(exception));
					}

					throw;
				}

				writer.Flush();
			}

			return new MemoryStream(Encoding.UTF8.GetBytes(xml.ToString()));
		}

		public object Deserialize(Type type, Stream stream)
		{
			Guard.AgainstNull(type, "type");
			Guard.AgainstNull(stream, "stream");

			using (var copy = stream.Copy())
			using (var reader = XmlDictionaryReader.CreateTextReader(copy, Encoding.UTF8,
																  new XmlDictionaryReaderQuotas
																  {
																	  MaxArrayLength = Int32.MaxValue,
																	  MaxStringContentLength = int.MaxValue,
																	  MaxNameTableCharCount = int.MaxValue
																  }, null))
			{
				return GetSerializer(type).Deserialize(reader);
			}
		}

		private void ResetSerializers()
		{
			_serializers = new Dictionary<string, XmlSerializer>();
			_serializerNamespaces = new Dictionary<string, XmlSerializerNamespaces>();
		}

		private static string GetTypeName(Exception exception)
		{
			var match = Expression.Match(exception.Message);

			var group = match.Groups["Type"];

			return group == null
					   ? TypeError
					   : (!string.IsNullOrEmpty(group.Value)
							  ? group.Value
							  : TypeError);
		}

		private static Exception UnknownTypeException(Exception exception)
		{
			var ex = exception;

			while (ex != null)
			{
				if (ex.Message.Contains(XmlInclude) || ex.Message.Contains(SoapInclude))
				{
					return ex;
				}

				ex = ex.InnerException;
			}

			return null;
		}

		private XmlSerializer GetSerializer(Type type)
		{
			lock (Padlock)
			{
				var key = type.AssemblyQualifiedName;

			    if (string.IsNullOrEmpty(key))
			    {
			        throw new ApplicationException();
			    }

			    if (!_serializers.ContainsKey(key))
				{
					_serializers.Add(key, new XmlSerializer(type, _knownTypes.ToArray()));
				}

				return _serializers[key];
			}
		}

		private XmlSerializerNamespaces GetSerializerNamespaces(Type type)
		{
			lock (Padlock)
			{
				var key = type.AssemblyQualifiedName;

                if (string.IsNullOrEmpty(key))
                {
                    throw new ApplicationException();
                }
                
                if (!_serializerNamespaces.ContainsKey(key))
				{
					var namespacesAdded = new List<string>();
					var namespaces = new XmlSerializerNamespaces();

					var q = 1;

					foreach (var knownType in _knownTypes)
					{
						if (string.IsNullOrEmpty(knownType.Namespace) || namespacesAdded.Contains(knownType.Namespace))
						{
							continue;
						}

						namespaces.Add(string.Format("q{0}", q++), knownType.Namespace);
						namespacesAdded.Add(knownType.Namespace);
					}

					_serializerNamespaces.Add(key, namespaces);
				}

				return _serializerNamespaces[key];
			}
		}
	}
}