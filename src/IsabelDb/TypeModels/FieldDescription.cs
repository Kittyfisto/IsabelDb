using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace IsabelDb.TypeModels
{
	internal sealed class FieldDescription
	{
		private readonly TypeDescription _fieldTypeDescription;
		private readonly string _name;
		private readonly MemberInfo _member;
		private readonly int _memberId;

		public FieldDescription(TypeDescription fieldTypeDescription, string name, MemberInfo member, int memberId)
		{
			_fieldTypeDescription = fieldTypeDescription;
			_name = name;
			_member = member;
			_memberId = memberId;
		}

		public TypeDescription FieldTypeDescription => _fieldTypeDescription;

		public int MemberId => _memberId;

		public string Name => _name;

		public MemberInfo Member => _member;

		[Pure]
		public static FieldDescription Create(string name, int id, TypeDescription underlyingEnumTypeDescription)
		{
			return new FieldDescription(underlyingEnumTypeDescription, name, null, id);
		}

		[Pure]
		public static FieldDescription Create(MemberInfo member, TypeDescription typeDescription, int memberId)
		{
			var name = GetName(member);
			return new FieldDescription(typeDescription, name, member, memberId);
		}

		[Pure]
		public static MemberInfo TryGetMemberInfo(Type type, string name)
		{
			var field = FindSerializableFields(type).FirstOrDefault(x => GetName(x) == name);
			if (field != null)
				return field;

			var property = FindSerializableProperties(type).FirstOrDefault(x => GetName(x) == name);
			if (property != null)
				return property;

			return null;
		}

		[Pure]
		public static IReadOnlyList<FieldInfo> FindSerializableFields(Type type)
		{
			if (type == null)
				return new FieldInfo[0];

			return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			           .Where(HasDataMemberAttribute)
			           .ToList();
		}

		[Pure]
		public static IReadOnlyList<PropertyInfo> FindSerializableProperties(Type type)
		{
			if (type == null)
				return new PropertyInfo[0];

			return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			           .Where(HasDataMemberAttribute)
			           .ToList();
		}

		[Pure]
		public static IReadOnlyList<KeyValuePair<string, int>> FindSerializableEnumMembers(Type type)
		{
			if (type == null || !type.IsEnum)
				return new KeyValuePair<string, int>[0];

			var names = type.GetEnumNames();
			var values = type.GetEnumValues();
			var ret = new List<KeyValuePair<string, int>>(names.Length);
			for (int i = 0; i < names.Length; ++i)
			{
				ret.Add(new KeyValuePair<string, int>(names[i], Convert.ToInt32(values.GetValue(i))));
			}

			return ret;
		}

		[Pure]
		private static bool HasDataMemberAttribute(MemberInfo field)
		{
			var dataMemberAttribute = field.GetCustomAttribute<DataMemberAttribute>();
			return dataMemberAttribute != null;
		}

		[Pure]
		private static string GetName(MemberInfo member)
		{
			var dataMember = member.GetCustomAttribute<DataMemberAttribute>();
			if (dataMember != null)
			{
				return dataMember.Name ?? member.Name;
			}

			return member.Name;
		}

		public void ThrowOnBreakingChanges(FieldDescription otherField)
		{
			if (FieldTypeDescription.ResolvedType != otherField.FieldTypeDescription.ResolvedType)
				throw new
					BreakingChangeException(string.Format("The type of field '{0}' changed from '{1}' to '{2}' which is a breaking change!",
					                                      Name,
					                                      FieldTypeDescription,
					                                      otherField.FieldTypeDescription));
		}

		#region Overrides of Object

		public override string ToString()
		{
			return string.Format("{0} {1}", FieldTypeDescription, Name);
		}

		#endregion

		public static IReadOnlyList<FieldDescription> Merge(IReadOnlyList<FieldDescription> previousFields, IReadOnlyList<FieldDescription> currentFields)
		{
			var fields = previousFields.ToDictionary(x => x.Name, x => x);
			foreach (var currentField in currentFields)
			{
				if (!fields.ContainsKey(currentField.Name))
				{
					fields.Add(currentField.Name, currentField);
				}
			}

			return fields.Values.ToList();
		}
	}
}