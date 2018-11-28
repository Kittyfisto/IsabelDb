using System.Net;
using System.Runtime.Serialization;

namespace IsabelDb.TypeModels.Surrogates
{
	/// <summary>
	/// 
	/// </summary>
	[DataContract]
	[DataContractSurrogateFor(typeof(IPAddress))]
	// ReSharper disable once InconsistentNaming
	public sealed class IPAddressSurrogate
	{
		/// <summary>
		/// 
		/// </summary>
		[DataMember]
		public byte[] Data;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="surrogate"></param>
		public static implicit operator IPAddress(IPAddressSurrogate surrogate)
		{
			return surrogate != null
				? new IPAddress(surrogate.Data)
				: null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		public static implicit operator IPAddressSurrogate(IPAddress address)
		{
			return address != null
				? new IPAddressSurrogate {Data = address.GetAddressBytes()}
				: null;
		}
	}
}