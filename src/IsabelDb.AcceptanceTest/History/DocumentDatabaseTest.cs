using System;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.AcceptanceTest.History
{
	[TestFixture]
	[Ignore("Not yet implemented")]
	public sealed class DocumentDatabaseTest
	{
		private DocumentDatabase _database;

		[SetUp]
		public void SetUp()
		{
			_database = new DocumentDatabase(Database.CreateInMemory(new []{typeof(DocumentEntity)}));
		}

		[Test]
		public void TestAddDocument()
		{
			var document = new Document
			{
				Name = "Test.txt",
				Author = "Simon the Scorcerer",
				Content = "Simon says....",
				LastModified = new DateTime(2018, 5, 10, 11, 29, 30)
			};
			_database.Put(document);

			_database.Get("Test.txt").Should().HaveCount(1);
		}
	}
}