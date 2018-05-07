using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using IsabelDb.Benchmark.Entities;

namespace IsabelDb.Benchmarks.GetByKey
{
	public class MultiValueDictionaryGetByKey
		: IDisposable
	{
		private readonly Database _database;
		private IMultiValueDictionary<int, Book> _multiValueDictionary;
		private readonly Random _random;

		public MultiValueDictionaryGetByKey()
		{
			const string filename = "DictionaryGetByKey.isdb";
			if (File.Exists(filename))
				File.Delete(filename);
			_database = Database.OpenOrCreate(filename, new[] {typeof(Book)});
			_random = new Random();
		}

		[Params(100, 1000, 10000, 100000, 1000000, 10000000)]
		public int N;

		[GlobalSetup]
		public void Setup()
		{
			_multiValueDictionary = _database.GetMultiValueDictionary<int, Book>("BooksByInt");
			_multiValueDictionary.Clear();

			var books = new List<KeyValuePair<int, IEnumerable<Book>>>();
			for (int i = 0; i < N; ++i)
			{
				var book = new Book();
				books.Add(new KeyValuePair<int, IEnumerable<Book>>(i, new []{book}));
			}
			_multiValueDictionary.PutMany(books);
		}

		[Benchmark]
		public void Get()
		{
			var key = _random.Next(0, N - 1);
			_multiValueDictionary.Get(key).ToList();
		}

		#region IDisposable

		public void Dispose()
		{
			_database?.Dispose();
		}

		#endregion
	}
}