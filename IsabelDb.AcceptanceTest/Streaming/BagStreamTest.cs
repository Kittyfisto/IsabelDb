using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace IsabelDb.AcceptanceTest.Streaming
{
	[TestFixture]
	public sealed class BagStreamTest
	{
		const string filename = "1GB.isdb";

		[Test]
		[Explicit]
		[Order(1)]
		public void TestWrite1Gb()
		{
			if (File.Exists(filename))
				File.Delete(filename);

			var filePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
			TestContext.Progress.WriteLine("Writing to {0}", filePath);

			using (var db = Database.OpenOrCreate(filePath, new []{typeof(Record)}))
			{
				const int recordsPerBatch = 10000;
				const int recordSize = 921;
				const int fileSize = 1024*1024*1024;
				const int batchCount = fileSize/recordSize/recordsPerBatch;
				const int recordCount = batchCount*recordsPerBatch;

				TestContext.Progress.WriteLine("Writing a total of {0} records in {1} batches, {2} records each...",
					recordCount, batchCount, recordsPerBatch);

				var bag = db.GetBag<IRecord>("Records");
				for (int i = 0; i < batchCount; ++i)
				{
					var records = CreateRecords(recordsPerBatch);
					bag.PutMany(records);

					var currentSize = ((double) new FileInfo(filePath).Length)/1024/1024/1024;
					TestContext.Progress.WriteLine("Batch {0} of {1}: {2:F2}Gb total", i + 1, batchCount,
						currentSize);
				}
			}
		}

		[Test]
		[Explicit]
		[Order(2)]
		public void TestRead1Gb()
		{
			using (var db = Database.OpenRead(filename, new[] {typeof(Record)}))
			{
				var bag = db.GetBag<IRecord>("Records");

				var recordCount = bag.Count();
				Console.WriteLine("{0} records in total", recordCount);

				int i = 0;
				const int recordsPerBatch = 10000;
				foreach (var record in bag.GetAllValues())
				{
					++i;
					if (i%recordsPerBatch == 0)
					{
						var progress = (double) i/recordCount;
						TestContext.Progress.WriteLine("Read {0:P1} of records", progress);
					}
				}
			}
		}

		private IEnumerable<IRecord> CreateRecords(int recordsPerBatch)
		{
			var records = new List<IRecord>(recordsPerBatch);
			var rng = new Random();
			const int numMeasurementValues = 100;

			for (int i = 0; i < recordsPerBatch; ++i)
			{
				var record = new Record
				{
					Name = "Stuff",
					PseudoId = i,
					MeasurementValues = Enumerable.Range(0, numMeasurementValues).Select(unused => rng.NextDouble()).ToArray()
				};
				records.Add(record);
			}
			return records;
		}
	}
}
