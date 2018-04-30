﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	///     Responsible for providing fast lookups between .NET types and
	///     their string representations which are actually persisted in the database.
	/// </summary>
	internal sealed class TypeRegistry
	{
		private readonly Dictionary<Type, string> _namesByType;
		private readonly HashSet<Type> _registeredTypes;
		private readonly Dictionary<string, Type> _typesByName;

		/// <summary>
		/// </summary>
		/// <param name="supportedTypes"></param>
		public TypeRegistry(IEnumerable<Type> supportedTypes)
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
			Register<float>();
			Register<double>();
			Register<IPAddress>();

			foreach (var type in supportedTypes) Register(type);
		}

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
			if (_registeredTypes.Add(type))
			{
				var name = ExtractTypename(type);
				_typesByName.Add(name, type);
				_namesByType.Add(type, name);

				var baseType = type.BaseType;
				if (baseType != null)
					Register(baseType);

				var interfaces = type.GetInterfaces();
				foreach (var @interface in interfaces)
				{
					Register(@interface);
				}
			}
		}

		public IEnumerable<Type> RegisteredTypes => _registeredTypes;

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
			return _registeredTypes.Contains(type);
		}

		[Pure]
		private static string ExtractTypename(Type type)
		{
			var dataContractAttributes = type.GetCustomAttributes(typeof(DataContractAttribute), inherit: true);
			if (dataContractAttributes != null && dataContractAttributes.Length == 1)
			{
				var dataContract = (DataContractAttribute) dataContractAttributes[0];
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