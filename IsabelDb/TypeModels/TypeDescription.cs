using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace IsabelDb.TypeModels
{
	internal sealed class TypeDescription
	{
		private readonly IReadOnlyList<MemberDescription> _members;
		public readonly TypeDescription BaseType;
		public readonly string FullTypeName;
		public readonly string Name;
		public readonly string Namespace;
		public readonly Type Type;
		public readonly int TypeId;

		private TypeDescription(string name, string @namespace,
								Type type,
								int typeId,
		                        TypeDescription baseType,
		                        IReadOnlyList<MemberDescription> members)
		{
			Name = name;
			Namespace = @namespace;
			FullTypeName = GetTypename(@namespace, name);
			Type = type;
			TypeId = typeId;

			BaseType = baseType;
			_members = members;
		}

		private TypeDescription(Type type,
		                        string typename,
		                        int typeId,
		                        TypeDescription baseTypeDescription,
		                        IEnumerable<MemberDescription> fields)
		{
			Type = type;
			TypeId = typeId;

			var idx = typename.LastIndexOf(".", StringComparison.InvariantCulture);
			Namespace = typename.Substring(startIndex: 0, length: idx);
			Name = typename.Substring(idx + 1);
			FullTypeName = typename;

			BaseType = baseTypeDescription;
			_members = fields?.ToList() ?? new List<MemberDescription>();
		}

		public IReadOnlyList<MemberDescription> Members => _members;

		#region Overrides of Object

		public override string ToString()
		{
			return FullTypeName;
		}

		#endregion

		public static string GetTypename(Type type)
		{
			ExtractTypename(type, out var @namespace, out var name);
			return GetTypename(@namespace, name);
		}

		public static string GetTypename(string @namespace, string name)
		{
			return string.Format("{0}.{1}", @namespace, name);
		}

		public static TypeDescription Create(Type type,
		                                     int typeId,
		                                     TypeDescription baseTypeDescription,
		                                     IReadOnlyList<MemberDescription> members)
		{
			ExtractTypename(type, out var @namespace, out var name);
			return new TypeDescription(name, @namespace, type, typeId, baseTypeDescription, members);
		}

		public static TypeDescription Create(Type type,
		                                     string typename,
		                                     int typeId,
		                                     TypeDescription baseTypeDescription,
		                                     IEnumerable<MemberDescription> members)
		{
			return new TypeDescription(type,
			                           typename,
			                           typeId,
			                           baseTypeDescription,
			                           members);
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
						if (!TypeModel.IsWellKnown(type))
							throw new
								ArgumentException(string.Format("The type '{0}' is not serializable: It should have the DataContractAttribute applied",
								                                type));

					@namespace = type.Namespace;
					name = type.Name;
				}
			}
		}

		public void ThrowIfIncompatibleTo(TypeDescription otherDescription)
		{
			var otherBaseType = otherDescription.BaseType;
			if (BaseType == null && otherBaseType != null)
				throw new BreakingChangeException();
			if (BaseType != null && otherBaseType == null)
				throw new BreakingChangeException();

			if (BaseType != null && otherBaseType != null)
			{
				if (!AreSameType(BaseType, otherBaseType))
					throw new
						BreakingChangeException(string.Format("The base class of the type '{0}' has been changed from '{1}' to '{2}': This is a breaking change!",
						                                      FullTypeName,
						                                      BaseType.FullTypeName,
						                                      otherBaseType.FullTypeName));

				BaseType.ThrowIfIncompatibleTo(otherDescription.BaseType);
			}
		}

		[Pure]
		private static bool AreSameType(TypeDescription type, TypeDescription otherType)
		{
			return string.Equals(type.FullTypeName, otherType.FullTypeName);
		}

		public static IReadOnlyList<FieldInfo> FindSerializableFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.Public)
			           .Where(HasDataMemberAttribute)
			           .ToList();
		}

		public static IReadOnlyList<PropertyInfo> FindSerializableProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			           .Where(HasDataMemberAttribute)
			           .ToList();
		}

		private static bool HasDataMemberAttribute(MemberInfo field)
		{
			var dataMemberAttribute = field.GetCustomAttribute<DataMemberAttribute>();
			return dataMemberAttribute != null;
		}
	}
}