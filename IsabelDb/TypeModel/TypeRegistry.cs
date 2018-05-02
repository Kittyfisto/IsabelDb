using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace IsabelDb.TypeModel
{
	/// <summary>
	///     Responsible for providing fast lookups between .NET types and
	///     their string representations which are actually persisted in the database.
	/// </summary>
	internal sealed class TypeRegistry
	{
		private readonly Dictionary<Type, TypeDescription> _types;
		private readonly Dictionary<string, Type> _typesByName;

		/// <summary>
		/// </summary>
		/// <param name="supportedTypes"></param>
		public TypeRegistry(IEnumerable<Type> supportedTypes)
		{
			_typesByName = new Dictionary<string, Type>();
			_types = new Dictionary<Type, TypeDescription>();

			foreach (var type in supportedTypes)
				Register(type);
		}

		public IEnumerable<Type> RegisteredTypes => _types.Keys;

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
			if (!_types.ContainsKey(type))
			{
				var description = TypeDescription.Create(type);
				_typesByName.Add(description.FullTypeName, type);
				_types.Add(type, description);

				var baseType = type.BaseType;
				if (baseType != null &&
				    baseType != typeof(ValueType) &&
				    baseType != typeof(Array))
					Register(baseType);
			}
		}

		public TypeDescription GetDescription(Type type)
		{
			return _types[type];
		}

		public string GetName(Type type)
		{
			_types.TryGetValue(type, out var description);
			return description?.FullTypeName;
		}

		public Type Resolve(string typeName)
		{
			_typesByName.TryGetValue(typeName, out var type);
			return type;
		}

		[Pure]
		public bool IsRegistered(Type type)
		{
			return _types.ContainsKey(type);
		}
	}
}