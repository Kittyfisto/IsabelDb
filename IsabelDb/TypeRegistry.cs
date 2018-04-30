using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	/// A <see cref="ITypeResolver"/> implementation which **only** resolves types
	/// which have been **explicitly** registered with this object. Any other type
	/// will NOT be resolved.
	/// </summary>
	/// <remarks>
	/// You should definitely use this implementation when you plan to open untrusted databases.
	/// Doing so will ensure that you only load the types which you **want** to load, otherwise
	/// an attacker might create a specifically crafted database to load any assembly (and thus type)
	/// into your application.
	/// </remarks>
	public sealed class TypeRegistry
		: ITypeResolver
	{
		private readonly HashSet<Type> _registeredTypes;
		private readonly Dictionary<string, Type> _typesByName;
		private readonly Dictionary<Type, string> _namesByType;

		/// <summary>
		/// 
		/// </summary>
		public TypeRegistry()
		{
			_registeredTypes = new HashSet<Type>();
			_typesByName = new Dictionary<string, Type>();
			_namesByType = new Dictionary<Type, string>();

			Register<object>();
			Register<string>();
			Register<ushort>();
			Register<short>();
			Register<uint>();
			Register<int>();
			Register<long>();
		}

		/// <summary>
		/// Registers the given type with this resolver.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void Register<T>()
		{
			Register(typeof(T));
		}

		/// <summary>
		/// Registers the given type with this resolver.
		/// </summary>
		/// <param name="type"></param>
		public void Register(Type type)
		{
			if (_registeredTypes.Add(type))
			{
				var name = ExtractTypename(type);
				_typesByName.Add(name, type);
				_namesByType.Add(type, name);
			}
		}

		/// <inheritdoc />
		public string GetName(Type type)
		{
			_namesByType.TryGetValue(type, out var name);
			return name;
		}

		/// <inheritdoc />
		public Type Resolve(string typeName)
		{
			_typesByName.TryGetValue(typeName, out var type);
			return type;
		}

		[Pure]
		private static string ExtractTypename(Type type)
		{
			var dataContractAttributes = type.GetCustomAttributes(typeof(DataContractAttribute), true);
			if (dataContractAttributes != null && dataContractAttributes.Length == 1)
			{
				var dataContract = (DataContractAttribute)dataContractAttributes[0];
				var @namespace = dataContract.Namespace;
				var name = dataContract.Name;
				if (name != null)
				{
					if (@namespace != null)
						return string.Format("{0}.{1}", @namespace, name);

					return string.Format("{0}.{1}", type.Namespace, name);
				}
				if (@namespace != null)
					return string.Format("{0}.{1}", @namespace, type.Name);
			}

			return type.AssemblyQualifiedName;
		}
	}
}
