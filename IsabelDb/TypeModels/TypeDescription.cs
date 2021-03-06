﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace IsabelDb.TypeModels
{
	internal sealed class TypeDescription
	{
		private readonly List<FieldDescription> _fields;

		public readonly TypeDescription BaseType;
		public TypeDescription SurrogateType { get; internal set; }
		public TypeDescription SurrogatedType { get; internal set; }
		public readonly string FullTypeName;
		public readonly string Name;
		public readonly string Namespace;

		/// <summary>
		/// The .NET <see cref="Type"/> represented by this description, IF it could be resolved.
		/// null otherwise.
		/// </summary>
		public readonly Type ResolvedType;

		/// <summary>
		/// The numeric value identifying this values of this type when serialized.
		/// </summary>
		public readonly int TypeId;

		/// <summary>
		/// Describes if this type is a class, interface enum or struct.
		/// </summary>
		public readonly TypeClassification Classification;

		/// <summary>
		/// When this type is an enum (<see cref="Classification"/> == <see cref="TypeClassification.Enum"/>),
		/// then this is a description of the underlying enum type, i.e. <see cref="System.Int32"/>, etc...
		/// null otherwise.
		/// </summary>
		public readonly TypeDescription UnderlyingEnumTypeDescription;

		private TypeDescription(string name, string @namespace,
		                        string fullTypeName,
		                        Type resolvedType,
		                        int typeId,
		                        TypeDescription baseType,
		                        TypeDescription underlyingEnumTypeDescription,
		                        TypeClassification classification,
		                        IReadOnlyList<FieldDescription> fields)
		{
			Name = name;
			Namespace = @namespace;
			FullTypeName = fullTypeName;
			ResolvedType = resolvedType;
			TypeId = typeId;

			BaseType = baseType;
			_fields = fields.ToList();
			Classification = classification;
			UnderlyingEnumTypeDescription = underlyingEnumTypeDescription;
		}

		private TypeDescription(Type resolvedType,
		                        string typename,
		                        int typeId,
		                        TypeDescription baseTypeDescription,
		                        TypeDescription underlyingEnumTypeDescription,
		                        TypeClassification classification,
		                        IEnumerable<FieldDescription> fields)
		{
			ResolvedType = resolvedType;
			TypeId = typeId;

			var idx = typename.LastIndexOf(".", StringComparison.InvariantCulture);
			Namespace = typename.Substring(startIndex: 0, length: idx);
			Name = typename.Substring(idx + 1);
			FullTypeName = typename;
			Classification = classification;

			BaseType = baseTypeDescription;
			UnderlyingEnumTypeDescription = underlyingEnumTypeDescription;
			_fields = fields?.ToList() ?? new List<FieldDescription>();
		}

		public IReadOnlyList<FieldDescription> Fields => _fields;

		public void Add(FieldDescription fieldDescription)
		{
			_fields.Add(fieldDescription);
		}

		#region Overrides of Object

		public override string ToString()
		{
			return FullTypeName;
		}

		#endregion

		public static string GetFullTypeName(Type type)
		{
			ExtractTypename(type, out var unused1, out var unused2, out var fullTypeName);
			return fullTypeName;
		}

		public static TypeDescription Create(Type type,
		                                     int typeId,
		                                     TypeDescription baseTypeDescription,
		                                     TypeDescription underlyingEnumTypeDescription,
		                                     IReadOnlyList<FieldDescription> fields,
		                                     bool hasSurrogate = false)
		{
			ExtractTypename(type, out var @namespace, out var name, out var fullTypeName, hasSurrogate);
			var classification = GetClassification(type);
			return new TypeDescription(name, @namespace, fullTypeName, type, typeId,
			                           baseTypeDescription,
			                           underlyingEnumTypeDescription,
			                           classification, fields);
		}

		[Pure]
		private static TypeClassification GetClassification(Type type)
		{
			if (type.IsEnum)
				return TypeClassification.Enum;
			if (type.IsValueType)
				return TypeClassification.Struct;
			if (type.IsInterface)
				return TypeClassification.Interface;
			if (type.IsClass)
				return TypeClassification.Class;

			throw new NotImplementedException(string.Format("Unable to classfiy the given type: {0}", type.AssemblyQualifiedName));
		}

		public static TypeDescription Create(Type type,
		                                     string typename,
		                                     int typeId,
		                                     TypeDescription baseTypeDescription,
		                                     TypeDescription underlyingEnumTypeDescription,
		                                     TypeClassification classification,
		                                     IEnumerable<FieldDescription> fields)
		{
			return new TypeDescription(type,
			                           typename,
			                           typeId,
			                           baseTypeDescription,
			                           underlyingEnumTypeDescription,
			                           classification,
			                           fields);
		}

		public static TypeDescription Merge(TypeDescription previousDescription,
		                                    TypeDescription currentDescription,
		                                    TypeDescription baseTypeDescription)
		{
			previousDescription.ThrowOnBreakingChanges(currentDescription);

			var type = previousDescription.ResolvedType;
			var typename = currentDescription.FullTypeName;
			var typeId = currentDescription.TypeId;
			var classification = currentDescription.Classification;
			var underlying = currentDescription.UnderlyingEnumTypeDescription;
			var fields = FieldDescription.Merge(previousDescription.Fields, currentDescription.Fields);

			var description = new TypeDescription(type,
			                                      typename,
			                                      typeId,
			                                      baseTypeDescription,
			                                      underlying,
			                                      classification,
			                                      fields);
			return description;
		}

		private static void ExtractTypename(Type type,
		                                    out string @namespace,
		                                    out string name,
		                                    out string fullTypeName,
		                                    bool hasSurrogate = false)
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
					if (!type.IsEnum)
					{
						var interfaces = type.GetInterfaces();
						if (!interfaces.Contains(typeof(ISerializable)))
						{
							if (!TypeModel.IsWellKnown(type) && !hasSurrogate)
								throw new
									ArgumentException(string.Format("The type '{0}' is not serializable: It should have the DataContractAttribute applied",
									                                type));
						}
					}

					@namespace = type.Namespace;
					name = type.Name;
				}
			}
			
			BuildFullType(type, @namespace, name, out fullTypeName);
		}

		private static void BuildFullType(Type type, string @namespace, string name, out string fullTypeName)
		{
			if (type.IsGenericType)
			{
				var args = type.GenericTypeArguments;
				var count = string.Format("`{0}", args.Length);
				var builder = new StringBuilder();
				builder.Append(@namespace);
				builder.Append(".");
				builder.Append(name);
				if (!name.EndsWith(count))
					builder.Append(count);
				builder.Append("[");
				for (int i = 0; i < args.Length; ++i)
				{
					if (i != 0)
						builder.Append(",");

					var argument = args[i];
					var argumentFullTypeName= GetFullTypeName(argument);
					builder.Append(argumentFullTypeName);
				}
				builder.Append("]");
				fullTypeName = builder.ToString();
			}
			else
			{
				fullTypeName = string.Format("{0}.{1}", @namespace, name);
			}
		}

		[Pure]
		private static bool AreSameType(TypeDescription type, TypeDescription otherType)
		{
			return string.Equals(type.FullTypeName, otherType.FullTypeName);
		}

		private void ThrowOnBreakingChanges(TypeDescription otherDescription)
		{
			if (Classification != otherDescription.Classification)
				throw new BreakingChangeException();

			foreach (var field in _fields)
			{
				var otherField = otherDescription._fields.FirstOrDefault(x => Equals(x.Name, field.Name));
				if (otherField != null)
				{
					field.ThrowOnBreakingChanges(otherField);
				}
			}

			var otherUnderlyingType = otherDescription.UnderlyingEnumTypeDescription;
			if (UnderlyingEnumTypeDescription == null && otherUnderlyingType != null)
				throw new BreakingChangeException();
			if (UnderlyingEnumTypeDescription != null && otherUnderlyingType == null)
				throw new BreakingChangeException();
			if (UnderlyingEnumTypeDescription != null && otherUnderlyingType != null)
			{
				if (!AreSameType(UnderlyingEnumTypeDescription, otherUnderlyingType))
					throw new BreakingChangeException();
			}

			var otherBaseType = otherDescription.BaseType;
			if (BaseType == null && otherBaseType != null)
				throw new BreakingChangeException(string.Format("The base class of the type '{0}' has been changed from 'null' to '{1}': This is a breaking change!",
				                                                FullTypeName,
				                                                otherBaseType.FullTypeName));
			if (BaseType != null && otherBaseType == null)
				throw new BreakingChangeException(string.Format("The base class of the type '{0}' has been changed from '{1}' to '{0}': This is a breaking change!",
				                                                FullTypeName,
				                                                BaseType.FullTypeName));

			if (BaseType != null && otherBaseType != null)
			{
				if (!AreSameType(BaseType, otherBaseType))
					throw new
						BreakingChangeException(string.Format("The base class of the type '{0}' has been changed from '{1}' to '{2}': This is a breaking change!",
						                                      FullTypeName,
						                                      BaseType.FullTypeName,
						                                      otherBaseType.FullTypeName));

				BaseType.ThrowOnBreakingChanges(otherDescription.BaseType);
			}
		}
	}
}
