using System;
using System.Collections.Generic;
using System.Linq;

namespace IsabelDb.TypeModel
{
	/// <summary>
	///     A description of types, their serializable fields and properties as well as their base types.
	/// </summary>
	internal sealed class TypeModel
	{
		private readonly Dictionary<Type, TypeDescription> _typeDescriptions;

		public TypeModel()
		{
			_typeDescriptions = new Dictionary<Type, TypeDescription>();
		}

		public IEnumerable<TypeDescription> Types => _typeDescriptions.Values.ToList();

		public PropertyDescription Create(Type type, string name)
		{
			var typeDescription = _typeDescriptions[type];
			return new PropertyDescription(typeDescription, name);
		}

		public TypeDescription AddType(string typename, Type type, Type baseType, IEnumerable<PropertyDescription> properties)
		{
			var baseTypeDescription = baseType != null ? _typeDescriptions[baseType] : null;
			var description = TypeDescription.Create(typename, baseTypeDescription, properties);
			_typeDescriptions.Add(type, description);
			return description;
		}

		public TypeDescription AddType(Type type)
		{
			var description = TypeDescription.Create(type);
			_typeDescriptions.Add(type, description);
			return description;
		}

		/// <summary>
		///     Verifies if all types in this model are compatible to the types in the other model.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public void ThrowIfIncompatibleTo(TypeModel other)
		{
			foreach (var pair in _typeDescriptions)
			{
				var type = pair.Key;
				var description = pair.Value;
				if (other.TryGetDescription(type, out var otherDescription))
				{
					description.ThrowIfIncompatibleTo(otherDescription);
				}
			}
		}

		private bool TryGetDescription(Type type, out TypeDescription description)
		{
			return _typeDescriptions.TryGetValue(type, out description);
		}
	}
}