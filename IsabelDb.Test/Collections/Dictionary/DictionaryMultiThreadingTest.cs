using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
				var values1 = db.GetOrCreateDictionary<int, string>("Values1");
				var values2 = db.GetOrCreateDictionary<int, string>("Values1");

				const int count = 10000;
				var task1 = PutValuesAsync(values1, count);
				var task2 = PutValuesAsync(values2, count);

				EnsureWritten(values1, task1.Result);
				EnsureWritten(values2, task2.Result);
			}
		}

		[Test]
		public void TestConcurrentPutSameCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetOrCreateDictionary<int, string>("Values");

				const int count = 10000;
				var task1 = PutValuesAsync(values, 0, count);
				var task2 = PutValuesAsync(values, count, count);

				EnsureWritten(values, task1.Result);
				EnsureWritten(values, task2.Result);
			}
		}

		[Test]
		public void TestConcurrentRemoveSameCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetOrCreateDictionary<int, string>("Values");

				const int count = 10000;
				var values1 = GenerateValues(0, count);
				var values2 = GenerateValues(count, count);
				values.PutMany(values1);
				values.PutMany(values2);

				var task1 = RemoveValuesAsync(values, values1.Select(x => x.Key));
				var task2 = RemoveValuesAsync(values, values2.Select(x => x.Key));
				Task.WaitAll(task1, task2);

				EnsureRemoved(values, values1);
				EnsureRemoved(values, values2);
			}
		}

		[Test]
		public void TestConcurrentPutReadSameCollection()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var dictionary = db.GetOrCreateDictionary<int, string>("Values");

				const int count = 10000;
				var values1 = GenerateValues(count);
				var values2 = GenerateValues(values1.Count, count);

				dictionary.PutMany(values1);

				var task1 = GetValuesAsync(dictionary, values1.Select(x => x.Key));
				var task2 = PutValuesAsync(dictionary, values2);

				Task.WaitAll(task1, task2);

				EnsureWritten(dictionary, values2);
			}
		}

		private static Task<IReadOnlyList<KeyValuePair<TKey, TValue>>> GetValuesAsync<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
		{
			return Task.Factory.StartNew(() =>
			{
				var values = new List<KeyValuePair<TKey,TValue>>();
				foreach (var key in keys)
				{
					if (dictionary.TryGet(key, out var value))
					{
						values.Add(new KeyValuePair<TKey, TValue>(key, value));
					}
				}

				return (IReadOnlyList<KeyValuePair<TKey, TValue>>)values;
			});
		}

		private static Task RemoveValuesAsync<TKey, TValue>(IDictionary<TKey, TValue> values, IEnumerable<TKey> keysToRemove)
		{
			return Task.Factory.StartNew(() =>
			{
				foreach (var key in keysToRemove)
				{
					values.Remove(key);
				}
			});
		}

		private static Task<IReadOnlyList<KeyValuePair<int, string>>> PutValuesAsync(IDictionary<int, string> dictionary, int count)
		{
			return PutValuesAsync(dictionary, 0, count);
		}

		private static async Task<IReadOnlyList<KeyValuePair<int, string>>> PutValuesAsync(IDictionary<int, string> dictionary, int startIndex, int count)
		{
			var values = GenerateValues(startIndex, count);
			await PutValuesAsync(dictionary, values);
			return values;
		}

		private static Task PutValuesAsync<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> values)
		{
			return Task.Factory.StartNew(() =>
			{
				foreach(var pair in values)
				{
					var key = pair.Key;
					var value = pair.Value;
					dictionary.Put(key, value);
				}
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

		private void EnsureRemoved<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IReadOnlyList<KeyValuePair<TKey, TValue>> removedValues)
		{
			foreach (var pair in removedValues)
			{
				dictionary.ContainsKey(pair.Key).Should().BeFalse();
			}
		}
	}
}
