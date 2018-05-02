using System;

// ReSharper disable once CheckNamespace
namespace IsabelDb
{
	/// <summary>
	/// TODO: Find a better name
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum)]
	public class SerializableContractAttribute
		: Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Namespace { get; set; }
	}
}
