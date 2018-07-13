using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Dictionary
{
	[TestFixture]
	public sealed class DictionaryMultiThreadingTest
	{
		private static IEnumerable<Type> NoCustomTypes => new Type[0];

		[Test]
		public void TestConcurrentPutDifferentCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values1 = db.GetDictionary<int, string>("Values1");
				var values2 = db.GetDictionary<int, string>("Values1");

				const int count = 10000;
				var task1 = WriteValuesAsync(values1, count);
				var task2 = WriteValuesAsync(values2, count);

				EnsureWritten(values1, task1.Result);
				EnsureWritten(values2, task2.Result);
			}
		}

		[Test]
		public void TestConcurrentPutSameCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<int, string>("Values");

				const int count = 10000;
				var task1 = WriteValuesAsync(values, 0, count);
				var task2 = WriteValuesAsync(values, count, count);

				EnsureWritten(values, task1.Result);
				EnsureWritten(values, task2.Result);
			}
		}

		[Test]
		public void TestConcurrentRemoveSameCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetDictionary<int, string>("Values");

				const int count = 10000;
				var values1 = GenerateValues(0, count);
				var values2 = GenerateValues(count, count);
				values.PutMany(values1);
				values.PutMany(values2);

				var task1 = RemoveValuesAsync(values, values1);
				var task2 = RemoveValuesAsync(values, values2);
				Task.WaitAll(task1, task2);

				EnsureRemoved(values, values1);
				EnsureRemoved(values, values2);
			}
		}

		private static Task RemoveValuesAsync<TKey, TValue>(IDictionary<TKey, TValue> values, IReadOnlyList<KeyValuePair<TKey, TValue>> valuesToRemove)
		{
			return Task.Factory.StartNew(() =>
			{
				foreach (var pair in valuesToRemove)
				{
					values.Remove(pair.Key);
				}
			});
		}

		private static Task<IReadOnlyList<KeyValuePair<int, string>>> WriteValuesAsync(IDictionary<int, string> dictionary, int count)
		{
			return WriteValuesAsync(dictionary, 0, count);
		}

		private static Task<IReadOnlyList<KeyValuePair<int, string>>> WriteValuesAsync(IDictionary<int, string> dictionary, int startIndex, int count)
		{
			return Task.Factory.StartNew(() =>
			{
				var values = GenerateValues(startIndex, count);
				foreach(var pair in values)
				{
					var key = pair.Key;
					var value = pair.Value;
					dictionary.Put(key, value);
				}

				return values;
			});
		}

		[Pure]
		private static IReadOnlyList<KeyValuePair<int, string>> GenerateValues(int count)
		{
			return GenerateValues(0, count);
		}

		[Pure]
		private static IReadOnlyList<KeyValuePair<int, string>> GenerateValues(int startIndex, int count)
		{
			var values = new List<KeyValuePair<int, string>>(count);
			for (int i = 0; i < count; ++i)
			{
				var key = startIndex + count;
				var value = key.ToString();
				values.Add(new KeyValuePair<int, string>(key, value));
			}

			return values;
		}

		private static void EnsureWritten<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IReadOnlyList<KeyValuePair<TKey, TValue>> values)
		{
			foreach (var pair in values)
			{
				dictionary.TryGet(pair.Key, out var actualValue)
				          .Should().BeTrue();
				actualValue.Should().Be(pair.Value);
			}
		}

		private void EnsureRemoved<TKey, TValue>(IDictionary<TKey, TValue> values, IReadOnlyList<KeyValuePair<TKey, TValue>> removedValues)
		{
			foreach (var pair in removedValues)
			{
				values.ContainsKey(pair.Key).Should().BeFalse();
			}
		}
	}
}
