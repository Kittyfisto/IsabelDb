using System;

namespace IsabelDb.Test
{
	/// <summary>
	/// This attribute is sed to to show that a unit test is related to an issue
	/// on github.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class IssueAttribute
		: Attribute
	{
		public IssueAttribute(string issueUri)
		{ }
	}
}