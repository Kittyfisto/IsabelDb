using System;
using System.Collections.Generic;
using System.Linq;

namespace IsabelDb.AcceptanceTest.History
{
	internal sealed class DocumentDatabase
		: IDisposable
	{
		private readonly IDatabase _database;
		private readonly IBag<DocumentEntity> _documents;
		private readonly IMultiValueDictionary<string, ValueKey> _documentIdsByName;

		public DocumentDatabase(IDatabase database)
		{
			_database = database;
			_documents = database.GetBag<DocumentEntity>("Documents");
			_documentIdsByName = database.GetMultiValueDictionary<string, ValueKey>("DocumentIdsByName");
		}

		public IEnumerable<Document> Get(string fileName)
		{
			var ids = _documentIdsByName.GetValues(fileName);
			return _documents.GetManyValues(ids).Select(x => x.ToDocument());
		}

		public void Put(Document document)
		{
			var id = _documents.Put(new DocumentEntity(document));
			_documentIdsByName.Put(document.Name, id);
		}

		#region IDisposable

		public void Dispose()
		{
			_database?.Dispose();
		}

		#endregion
	}
}