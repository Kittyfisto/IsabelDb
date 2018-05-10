using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IsabelDb.AcceptanceTest.History
{
	internal sealed class DocumentDatabase
		: IDisposable
	{
		private readonly Database _database;
		private readonly IDictionary<long, DocumentEntity> _documents;
		private readonly IMultiValueDictionary<string, long> _documentIdsByName;
		private readonly IIntervalCollection<EpochTimestamp, long> _documentsByValidRange;
		private readonly IDictionary<long, ValueKey> _documentIntervalsById;

		private long _lastDocumentId;

		public DocumentDatabase(Database database)
		{
			_database = database;
			_documents = database.GetDictionary<long, DocumentEntity>("Documents");
			_documentIdsByName = database.GetMultiValueDictionary<string, long>("DocumentIdsByName");
			_documentsByValidRange = database.GetIntervalCollection<EpochTimestamp, long>("DocumentsByValidRange");
			_documentIntervalsById = database.GetDictionary<long, ValueKey>("DocumentIntervalsById");
		}

		public IEnumerable<Document> Get(DateTime timestamp)
		{
			var ids = _documentsByValidRange.GetValues((EpochTimestamp) timestamp);
			return _documents.GetManyValues(ids).Select(x => x.ToDocument());
		}

		public IEnumerable<Document> Get(string fileName)
		{
			var ids = _documentIdsByName.Get(fileName);
			return _documents.GetManyValues(ids).Select(x => x.ToDocument());
		}

		public bool TryGet(string fileName, DateTime value, out Document document)
		{
			var ids = _documentIdsByName.Get(fileName);
			var matching = _documentsByValidRange.GetValues((EpochTimestamp) value);
			var actual = ids.Intersect(matching).ToList();
			if (actual.Count == 0)
			{
				document = null;
				return false;
			}

			var id = actual[0];
			if (!_documents.TryGet(id, out var documentEntity))
			{
				document = null;
				return false;
			}

			document = documentEntity.ToDocument();
			return true;
		}

		public void Put(Document document)
		{
			var versionIds = _documentIdsByName.Get(document.Name);
			var keys = _documentIntervalsById.GetManyValues(versionIds).ToList();
			var intervals = _documentsByValidRange.GetManyIntervals(keys).ToList();

			var lastModified = (EpochTimestamp)document.LastModified;
			if (Adjust(keys, intervals, lastModified, out var keyToModify, out var intervalToReplace,
			           out var newInterval))
			{
				_documentsByValidRange.Move(keyToModify, intervalToReplace);
			}

			var id = Interlocked.Increment(ref _lastDocumentId);
			var key = _documentsByValidRange.Put(newInterval, id);
			_documents.Put(id, new DocumentEntity(document));
			_documentIdsByName.Put(document.Name, id);
			_documentIntervalsById.Put(id, key);
		}

		private bool Adjust(List<ValueKey> keys, List<Interval<EpochTimestamp>> intervals, EpochTimestamp lastModified,
		                    out ValueKey keyToModify, out Interval<EpochTimestamp> intervalToModify,
		                    out Interval<EpochTimestamp> newInterval)
		{
			bool modifiedKey = false;
			bool modifiedInterval = false;

			keyToModify = new ValueKey();
			intervalToModify = new Interval<EpochTimestamp>();
			newInterval = new Interval<EpochTimestamp>();

			var count = keys.Count;
			for (int i = 0; i < count; ++i)
			{
				var interval = intervals[i];
				if (interval.Contains(lastModified))
				{
					var maximum = lastModified+1;
					keyToModify = keys[i];
					intervalToModify = Interval.Create(interval.Minimum, maximum);
					modifiedKey = true;
				}
				if (interval.Minimum.CompareTo(lastModified) >= 0)
				{
					newInterval = Interval.Create(lastModified, interval.Minimum);
					modifiedInterval = true;
					break;
				}
			}

			if (!modifiedInterval)
				newInterval = Interval.Create(lastModified, EpochTimestamp.MaxValue);

			return modifiedKey;
		}

		#region IDisposable

		public void Dispose()
		{
			_database?.Dispose();
		}

		#endregion
	}
}