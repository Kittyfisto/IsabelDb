using System.Reflection;
using System.Runtime.Serialization;

namespace IsabelDb.TypeModels
{
	internal sealed class MemberDescription
	{
		private readonly TypeDescription _typeDescription;
		private readonly string _name;
		private readonly MemberInfo _member;
		private readonly int _memberId;

		public MemberDescription(TypeDescription typeDescription, string name, MemberInfo member, int memberId)
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

		public static MemberDescription Create(MemberInfo member, TypeDescription typeDescription, int memberId)
		{
			var name = GetName(member);
			return new MemberDescription(typeDescription, name, member, memberId);
		}

		public static MemberDescription Create(string name, TypeDescription memberType, int memberId)
		{
			return new MemberDescription(memberType,
			                             name,
			                             null,
			                             memberId);
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