﻿using System;
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

		public TypeResolver(IEnumerable<TypeDescription> supportedTypes)
		{
			_typesByName = new Dictionary<string, Type>();
			_namesByType = new Dictionary<Type, string>();

			foreach (var description in supportedTypes)
			{
				_typesByName.Add(description.FullTypeName, description.ResolvedType);
				_namesByType.Add(description.ResolvedType, description.FullTypeName);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="supportedTypes">The list of types for which a mapping shall be provided</param>
		public TypeResolver(IEnumerable<Type> supportedTypes)
			: this(TypeModel.Create(supportedTypes).TypeDescriptions)
		{}

		public string GetName(Type type)
		{
			_namesByType.TryGetValue(type, out var name);
			return name;
		}

		public Type Resolve(string typeName)
		{
			if (typeName == null)
				return null;

			_typesByName.TryGetValue(typeName, out var type);
			return type;
		}

		public void Register(string name, Type type)
		{
			_typesByName.Add(name, type);
			_namesByType.Add(type, name);
		}

		[Pure]
		public bool IsRegistered(Type type)
		{
			return _namesByType.ContainsKey(type);
		}
	}
}