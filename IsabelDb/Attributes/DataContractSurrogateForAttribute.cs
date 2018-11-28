using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	///    This attribute may be supplied to a type which shall act as a serialization
	///    surrogate for another type which doesn't support serialization.
	/// </summary>
	/// <remarks>
	///    This attribute is very useful when your type model contains a type which
	///    is neither natively supported, nor can be modified to properly implement
	///    the DataContract (for example because the type is part of a 3rd party library).
	///    All that is necessary is to create a new type which implements the DataContract
	///    as well as this attribute, and which offers conversion operators to and from
	///    the two types.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public sealed class DataContractSurrogateForAttribute
		: Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="actualType"></param>
		public DataContractSurrogateForAttribute(Type actualType)
		{
			ActualType = actualType;
		}

		/// <summary>
		/// 
		/// </summary>
		public Type ActualType { get; set; }
	}
}
