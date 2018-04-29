using System;
using System.Collections.Generic;

namespace IsabelDb
{
	/// <summary>
	///     Responsible for registering types under their former names, so that
	///     these objects can be loaded from old databases again.
	/// </summary>
	public sealed class TypeMapper
		: ITypeResolver
	{
		private readonly object _syncRoot;
		private readonly TypeResolver _typeResolver;
		private readonly Dictionary<string, Type> _types;

		/// <summary>
		/// </summary>
		public TypeMapper()
		{
			_types = new Dictionary<string, Type>();
			_syncRoot = new object();
			_typeResolver = new TypeResolver();
		}

		/// <inheritdoc />
		public Type Resolve(string typeName)
		{
			lock (_syncRoot)
			{
				if (_types.TryGetValue(typeName, out var value))
					return value;
			}

			return _typeResolver.Resolve(typeName);
		}

		/// <inheritdoc />
		public string GetName(Type type)
		{
			return _typeResolver.GetName(type);
		}

		/// <summary>
		///     Adds a mapping to resolve
		/// </summary>
		/// <param name="oldTypeName"></param>
		/// <param name="currentType"></param>
		public void AddMapping(string oldTypeName, Type currentType)
		{
			lock (_syncRoot)
			{
				_types.Add(oldTypeName, currentType);
			}
		}
	}
}