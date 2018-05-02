using System;
using System.Collections.Generic;
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
		public readonly TypeDescription BaseType;

		private TypeDescription(string name, string @namespace, TypeDescription baseType)
		{
			Name = name;
			Namespace = @namespace;
			FullTypeName = string.Format("{0}.{1}", @namespace, name);
			BaseType = baseType;
		}

		#region Overrides of Object

		public override string ToString()
		{
			return FullTypeName;
		}

		#endregion

		public static TypeDescription Create(Type type)
		{
			ExtractTypename(type, out var @namespace, out var name);

			var baseType = type.BaseType;
			TypeDescription baseTypeDescription = null;
			if (baseType != null &&
			    baseType != typeof(ValueType) &&
			    baseType != typeof(Array))
				baseTypeDescription = Create(baseType);

			return new TypeDescription(name, @namespace, baseTypeDescription);
		}

		public static TypeDescription Create(string typename,
		                                     TypeDescription baseTypeDescription,
		                                     IEnumerable<PropertyDescription> properties)
		{
			var idx = typename.LastIndexOf(".", StringComparison.InvariantCulture);
			var @namespace = typename.Substring(0, idx);
			var name = typename.Substring(idx + 1);
			return new TypeDescription(name, @namespace,  baseTypeDescription);
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
						if (!ProtobufTypeModel.IsBuiltIn(type))
							throw new ArgumentException(string.Format("The type '{0}' is not serializable: It should have the DataContractAttribute applied", type));
					}

					@namespace = type.Namespace;
					name = type.Name;
				}
			}
		}

		public void ThrowIfIncompatibleTo(TypeDescription otherDescription)
		{
			if (Name != otherDescription.Name)
				throw new BreakingChangeException();
			if (Namespace != otherDescription.Namespace)
				throw new BreakingChangeException();

			if (BaseType == null && otherDescription.BaseType != null)
				throw new BreakingChangeException();
			if (BaseType != null && otherDescription.BaseType == null)
				throw new BreakingChangeException();

			if (BaseType != null && otherDescription.BaseType != null)
			{
				BaseType.ThrowIfIncompatibleTo(otherDescription.BaseType);
			}
		}
	}
}
