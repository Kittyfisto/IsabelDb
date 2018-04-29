using System;
using System.Diagnostics;
using System.IO;

namespace IsabelDb.Benchmark
{
	internal sealed class TestBench<T>
		: IDisposable
		where T : IDisposable
	{
		private readonly T _db;

		private readonly string _filePath;
		private readonly string _implementation;

		public TestBench(T db, string filePath)
		{
			_db = db;
			_filePath = filePath;
			_implementation = typeof(T).Name;
		}

		public T Db => _db;

		public void Dispose()
		{
			_db.Dispose();
		}

		public void Measure(string name, Action<T> fn)
		{
			var stopwatch = Stopwatch.StartNew();
			Execute(fn);
			stopwatch.Start();

			Console.WriteLine("{0}:", _implementation);
			Console.WriteLine("{0} took {1}ms", name, stopwatch.ElapsedMilliseconds);
			Console.WriteLine("Filesize: {0}kB", new FileInfo(_filePath).Length / 1024);
		}

		public void Execute(Action<T> fn)
		{
			fn(_db);
		}
	}
}