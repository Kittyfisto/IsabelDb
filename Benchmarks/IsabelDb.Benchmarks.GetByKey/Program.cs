using BenchmarkDotNet.Running;

namespace IsabelDb.Benchmarks.GetByKey
{
	public class Program
	{
		static void Main(string[] args)
		{
			//BenchmarkRunner.Run<DictionaryGetByKey>();
			BenchmarkRunner.Run<DictionaryPutMany>();
			//BenchmarkRunner.Run<MultiValueDictionaryGetByKey>();
			//BenchmarkRunner.Run<MultiValueDictionaryRemoveByKey>();
		}
	}
}
