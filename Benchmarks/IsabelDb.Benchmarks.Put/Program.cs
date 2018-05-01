using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using IsabelDb.Benchmark.Entities;

namespace IsabelDb.Benchmarks.Put
{
	public class Program
	{
		public class BenchmarkPut
			: IDisposable
		{
			private readonly IsabelDb _database;
			private readonly IDictionaryObjectStore<int, Book> _books;
			private readonly Book _book;
			private readonly IDictionaryObjectStore<int, int> _primes;
			private readonly IDictionaryObjectStore<string, byte[]> _files;
			private readonly byte[] _blob;

			public BenchmarkPut()
			{
				const string filename = "Put.isdb";
				if (File.Exists(filename))
					File.Delete(filename);
				_database = IsabelDb.OpenOrCreate(filename, new []{typeof(Book)});
				_books = _database.GetDictionary<int, Book>("Books");
				_book = new Book
				{
					Title = "Solaris",
					Author = "Stanislaw Lem"
				};
				_primes = _database.GetDictionary<int, int>("Primes");
				_files = _database.GetDictionary<string, byte[]>("Files");

				var random = new Random();
				_blob = new byte[2 * 1024 * 1024];
				random.NextBytes(_blob);
			}

			[Benchmark]
			public void PutBlob()
			{
				_files.Put(@"c:\foo", _blob);
			}

			[Benchmark]
			public void PutInt()
			{
				_primes.Put(41313121, 61463);
			}

			[Benchmark]
			public void PutBook()
			{
				_books.Put(41313121, _book);
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
			var summary = BenchmarkRunner.Run<BenchmarkPut>();
		}
	}
}
