using System;

namespace IsabelDb.Test
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class DefectAttribute
		: Attribute
	{
		public DefectAttribute(string defectUri)
		{ }
	}
}
