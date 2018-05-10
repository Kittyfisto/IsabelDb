using System;

namespace IsabelDb.AcceptanceTest.History
{
	public sealed class Document
	{
		public string Name { get; set; }
		public string Content { get; set; }
		public DateTime LastModified { get; set; }
		public string Author { get; set; }
	}
}