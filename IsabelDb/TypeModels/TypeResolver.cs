using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb.TypeModels
{
	/// <summary>
	///     Responsible for providing fast lookups between .NET types and their names
	///     (in both directions).
	/// </summary>
	internal sealed class TypeResolver
	{
		private readonly Dictionary<Type, string> _namesByType;
		private readonly Dictionary<string, Type> _typesByName;

		/// <summary>
		/// </summary>
		/// <param name="supportedTypes">The list of types for which a mapping shall be provided</param>
		public TypeResolver(IEnumerable<Type> supportedTypes)
		{
			_typesByName = new Dictionary<string, Type>();
			_namesByType = new Dictionary<Type, string>();

			foreach (var type in supportedTypes)
				Register(type);
		}

		public IEnumerable<Type> RegisteredTypes => _namesByType.Keys;

		/// <summary>
		///     Registers the given type with this resolver.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Register<T>()
		{
			Register(typeof(T));
		}

		/// <summary>
		///     Registers the given type with this resolver.
		/// </summary>
		/// <param name="type"></param>
		public void Register(Type type)
		{
			if (!_namesByType.ContainsKey(type))
			{
				var baseType = type.BaseType;
				if (baseType != null &&
				    baseType != typeof(ValueType) &&
				    baseType != typeof(Array))
					Register(baseType);

				var typename = TypeDescription.GetTypename(type);
				_namesByType.Add(type, typename);
				_typesByName.Add(typename, type);
			}
		}

		public string GetName(Type type)
		{
			_namesByType.TryGetValue(type, out var name);
			return name;
		}

		public Type Resolve(string typeName)
		{
			_typesByName.TryGetValue(typeName, out var type);
			return type;
		}

		[Pure]
		public bool IsRegistered(Type type)
		{
			return _namesByType.ContainsKey(type);
		}
	}
}