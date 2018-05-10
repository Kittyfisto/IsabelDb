using System;
using System.Runtime.Serialization;

namespace IsabelDb.AcceptanceTest.History
{
	[DataContract]
	public sealed class DocumentEntity
	{
		public DocumentEntity()
		{}

		public DocumentEntity(Document document)
		{
			Name = document.Name;
			LastModified = (EpochTimestamp) document.LastModified;
		}

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public EpochTimestamp LastModified { get; set; }

		[DataMember]
		public string Author { get; set; }

		[DataMember]
		public string Content { get; set; }

		public Document ToDocument()
		{
			return new Document
			{
				Name = Name,
				LastModified = (DateTime) LastModified,
				Author = Author,
				Content = Content
			};
		}
	}
}