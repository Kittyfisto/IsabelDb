using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using IsabelDb.Benchmark.Entities;

namespace IsabelDb.Benchmarks.GetByKey
{
	public class DictionaryPutMany
		: IDisposable
	{
		private readonly IDatabase _database;
		private IDictionary<int, Book> _books;

		public DictionaryPutMany()
		{
			const string filename = "DictionaryPutMany.isdb";
			if (File.Exists(filename))
				File.Delete(filename);
			_database = Database.OpenOrCreate(filename, new []{typeof(Book)});
		}

		[Params(100, 1000, 10000, 100000)]
		public int N;

		[GlobalSetup]
		public void Setup()
		{
			_books = _database.GetDictionary<int, Book>("Books");
			_books.Clear();
		}

		[Benchmark]
		public void PutMany()
		{
			var books = new List<KeyValuePair<int, Book>>(N);
			for (int i = 0; i < N; ++i)
			{
				var book = new Book();
				books.Add(new KeyValuePair<int, Book>(i, book));
			}
			_books.PutMany(books);
		}

		#region IDisposable

		public void Dispose()
		{
			_database?.Dispose();
		}

		#endregion
	}
}