using System;

namespace IsabelDb
{
	public interface ITypeResolver
	{
		Type Resolve(string typeName);
		string GetName(Type type);
	}
}