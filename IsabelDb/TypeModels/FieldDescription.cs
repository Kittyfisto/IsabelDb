﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace IsabelDb.TypeModels
{
	internal sealed class FieldDescription
	{
		private readonly TypeDescription _typeDescription;
		private readonly string _name;
		private readonly MemberInfo _member;
		private readonly int _memberId;

		public FieldDescription(TypeDescription typeDescription, string name, MemberInfo member, int memberId)
		{
			_typeDescription = typeDescription;
			_name = name;
			_member = member;
			_memberId = memberId;
		}

		public TypeDescription TypeDescription => _typeDescription;

		public int MemberId => _memberId;

		public string Name => _name;

		public MemberInfo Member => _member;

		[Pure]
		public static FieldDescription Create(MemberInfo member, TypeDescription typeDescription, int memberId)
		{
			var name = GetName(member);
			return new FieldDescription(typeDescription, name, member, memberId);
		}

		[Pure]
		public static MemberInfo GetMemberInfo(Type type, string name)
		{
			var field = FindSerializableFields(type).FirstOrDefault(x => GetName(x) == name);
			if (field != null)
				return field;

			var property = FindSerializableProperties(type).FirstOrDefault(x => GetName(x) == name);
			if (property != null)
				return property;

			throw new NotImplementedException();
		}

		[Pure]
		public static IReadOnlyList<FieldInfo> FindSerializableFields(Type type)
		{
			return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			           .Where(HasDataMemberAttribute)
			           .ToList();
		}

		[Pure]
		public static IReadOnlyList<PropertyInfo> FindSerializableProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			           .Where(HasDataMemberAttribute)
			           .ToList();
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
	}
}