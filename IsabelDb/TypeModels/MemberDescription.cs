using System.Reflection;
using System.Runtime.Serialization;

namespace IsabelDb.TypeModels
{
	internal sealed class MemberDescription
	{
		private readonly TypeDescription _typeDescription;
		private readonly string _name;
		private readonly MemberInfo _member;

		public MemberDescription(TypeDescription typeDescription, string name, MemberInfo member)
		{
			_typeDescription = typeDescription;
			_name = name;
			_member = member;
		}

		public TypeDescription TypeDescription => _typeDescription;

		public string Name => _name;

		public MemberInfo Member => _member;

		public static MemberDescription Create(MemberInfo member, TypeDescription typeDescription)
		{
			var name = GetName(member);
			return new MemberDescription(typeDescription, name, member);
		}

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