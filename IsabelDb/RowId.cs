using System;
using System.Runtime.Serialization;

namespace IsabelDb
{
	/// <summary>
	///    Holds the id of a specific row in a specific collection.
	/// </summary>
	[DataContract]
	public struct RowId
		: IEquatable<RowId>
		, IComparable<RowId>
	{
		/// <summary>
		///     The numeric value of this key: Two keys with the same value are equal.
		/// </summary>
		[DataMember]
		public long Id { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		public RowId(long id)
		{
			Id = id;
		}

		#region Equality members

		/// <inheritdoc />
		public bool Equals(RowId other)
		{
			return Id == other.Id;
		}

		/// <inheritdoc />
		public int CompareTo(RowId other)
		{
			return Id.CompareTo(other.Id);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is RowId && Equals((RowId) obj);
		}

		#region Overrides of ValueType

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("#{0}", Id);
		}

		#endregion

		#region Equality members

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		#endregion

		/// <summary>
		///     Compares two values for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(RowId left, RowId right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares two values for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(RowId left, RowId right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}