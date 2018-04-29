using System;

namespace IsabelDb
{
	internal sealed class TypeResolver
		: ITypeResolver
	{
		public Type Resolve(string typeName)
		{
			return Type.GetType(typeName);
		}

		public string GetName(Type type)
		{
			return type.AssemblyQualifiedName;
		}
	}
}