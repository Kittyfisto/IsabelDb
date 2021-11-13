using System;
using System.Runtime.Serialization;

namespace IsabelDb.TypeModels.Surrogates
{
	/// <summary>
	/// 
	/// </summary>
	[DataContract]
	[DataContractSurrogateFor(typeof(Version))]
	public sealed class VersionSurrogate
	{
		/// <summary>
		/// 
		/// </summary>
		[DataMember] public int Major;

		/// <summary>
		/// 
		/// </summary>
		[DataMember] public int Minor;

		/// <summary>
		/// 
		/// </summary>
		[DataMember] public int Build;

		/// <summary>
		/// 
		/// </summary>
		[DataMember] public int Revision;

		/// <summary>
		/// 
		/// </summary>
		public VersionSurrogate()
		{}

		private VersionSurrogate(int major, int minor, int build, int revision)
		{
			Major = major;
			Minor = minor;
			Build = build;
			Revision = revision;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="version"></param>
		public static implicit operator Version(VersionSurrogate version)
		{
			if (version == null)
				return null;

			if (version.Build < 0)
				return new Version(version.Major, version.Minor);

			if (version.Revision < 0)
				return new Version(version.Major, version.Minor, version.Build);

			return new Version(version.Major, version.Minor, version.Build, version.Revision);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="version"></param>
		public static implicit operator VersionSurrogate(Version version)
		{
			return version != null
				? new VersionSurrogate(version.Major, version.Minor, version.Build, version.Revision)
				: null;
		}
	}
}
