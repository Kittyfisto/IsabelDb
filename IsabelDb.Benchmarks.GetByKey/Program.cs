using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Running;
using IsabelDb.Benchmark.Entities;

namespace IsabelDb.Benchmarks.GetByKey
{
	public class Program
	{
		public class GetByKey
			: IDisposable
		{
			private readonly IsabelDb _database;
			private readonly IDictionaryObjectStore<int, Book> _intKeyDictionary;
			private readonly int _count;
			private readonly IDictionaryObjectStore<long, Book> _longKeyDictionary;
			private readonly IDictionaryObjectStore<string, Book> _stringKeyDictionary;
			private readonly string _stringKey;
			private readonly int _intKey;
			private readonly long _longKey;

			public GetByKey()
			{
				const string filename = "GetByKey.isdb";
				if (File.Exists(filename))
					File.Delete(filename);
				_database = IsabelDb.OpenOrCreate(filename);
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

		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<GetByKey>();
		}
	}
}
