using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using IsabelDb.Benchmark.Entities;

namespace IsabelDb.Benchmarks.GetByKey
{
	public class DictionaryGetByKey
		: IDisposable
	{
		private readonly Database _database;
		private readonly IDictionary<int, Book> _intKeyDictionary;
		private readonly int _count;
		private readonly IDictionary<long, Book> _longKeyDictionary;
		private readonly IDictionary<string, Book> _stringKeyDictionary;
		private readonly string _stringKey;
		private readonly int _intKey;
		private readonly long _longKey;

		public DictionaryGetByKey()
		{
			const string filename = "DictionaryGetByKey.isdb";
			if (File.Exists(filename))
				File.Delete(filename);
			_database = Database.OpenOrCreate(filename, new []{typeof(Book)});
			_intKeyDictionary = _database.GetDictionary<int, Book>("BooksByInt");
			_longKeyDictionary = _database.GetDictionary<long, Book>("BooksByLong");
			_stringKeyDictionary = _database.GetDictionary<string, Book>("BooksByString");

			var intBooks = new List<KeyValuePair<int, Book>>();
			var longBooks = new List<KeyValuePair<long, Book>>();
			var stringBooks = new List<KeyValuePair<string, Book>>();
			_count = 10000;
			for (int i = 0; i < _count; ++i)
			{
				var book = new Book();
				intBooks.Add(new KeyValuePair<int, Book>(i, book));
				longBooks.Add(new KeyValuePair<long, Book>(i, book));
				stringBooks.Add(new KeyValuePair<string, Book>(i.ToString(), book));
			}
			_intKeyDictionary.PutMany(intBooks);
			_longKeyDictionary.PutMany(longBooks);
			_stringKeyDictionary.PutMany(stringBooks);

			_intKey = _count / 2;
			_longKey = _count / 2;
			_stringKey = (_count / 2).ToString();
		}

		[Benchmark]
		public void GetIntKey()
		{
			_intKeyDictionary.Get(_intKey);
		}

		[Benchmark]
		public void GetLongKey()
		{
			_longKeyDictionary.Get(_longKey);
		}

		[Benchmark]
		public void GetStringKey()
		{
			_stringKeyDictionary.Get(_stringKey);
		}

		#region IDisposable

		public void Dispose()
		{
			_database?.Dispose();
		}

		#endregion
	}
}