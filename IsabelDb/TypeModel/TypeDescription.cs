using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace IsabelDb.TypeModel
{
	internal sealed class TypeDescription
	{
		public readonly string Name;
		public readonly string Namespace;
		public readonly string FullTypeName;

		private TypeDescription(string name, string @namespace)
		{
			Name = name;
			Namespace = @namespace;
			FullTypeName = string.Format("{0}.{1}", @namespace, name);
		}

		public static TypeDescription Create(Type type)
		{
			ExtractTypename(type, out var @namespace, out var name);
			return new TypeDescription(name, @namespace);
		}

		private static void ExtractTypename(Type type, out string @namespace, out string name)
		{
			var dataContractAttributes = type.GetCustomAttribute(typeof(DataContractAttribute), inherit: false);
			if (dataContractAttributes != null)
			{
				var dataContract = (DataContractAttribute) dataContractAttributes;
				@namespace = dataContract.Namespace ?? type.Namespace;
				name = dataContract.Name ?? type.Name;
			}
			else
			{
				var dataContract2Attribute = type.GetCustomAttribute(typeof(SerializableContractAttribute), inherit: false);
				if (dataContract2Attribute != null)
				{
					var dataContract = (SerializableContractAttribute) dataContract2Attribute;
					@namespace = dataContract.Namespace ?? type.Namespace;
					name = dataContract.Name ?? type.Name;
				}
				else
				{
					var interfaces = type.GetInterfaces();
					if (!interfaces.Contains(typeof(ISerializable)))
					{
						if (!TypeModel.IsBuiltIn(type))
							throw new ArgumentException(string.Format("The type '{0}' is not serializable: It should have the DataContractAttribute applied", type));
					}

					@namespace = type.Namespace;
					name = type.Name;
				}
			}
		}
	}
}
