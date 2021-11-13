using System;

namespace IsabelDb.Test
{
	/// <summary>
	/// This attribute is sed to to show that a unit test is related to a defect
	/// on github.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class DefectAttribute
		: Attribute
	{
		public DefectAttribute(string defectUri)
		{ }
	}
}
