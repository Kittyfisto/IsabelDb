using System.Runtime.Serialization;

namespace IsabelDb.Test.Entities
{
	[DataContract(Name = "Dog", Namespace = "IsabelDb.Test.Entities")]
	public class Dog
		: Animal
	{
		#region Equality members

		protected bool Equals(Dog other)
		{
			return string.Equals(EyeColor, other.EyeColor) && string.Equals(FurColor, other.FurColor);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Dog) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((EyeColor != null ? EyeColor.GetHashCode() : 0) * 397) ^ (FurColor != null ? FurColor.GetHashCode() : 0);
			}
		}

		#endregion

		[DataMember]
		public string EyeColor;

		[DataMember]
		public string FurColor { get; set; }
	}
}