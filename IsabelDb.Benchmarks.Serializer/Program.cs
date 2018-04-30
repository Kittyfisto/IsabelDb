﻿using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ProtoBuf.Meta;

namespace IsabelDb.Benchmarks.Serializer
{
	public class Program
	{
		public class Serializer
		{
			private readonly RuntimeTypeModel _runtimeTypeModel;
			private readonly TypeModel _compiledTypeModel;

			public Serializer()
			{
				_runtimeTypeModel = TypeModel.Create();

				var typeModel = TypeModel.Create();
				_compiledTypeModel = typeModel.Compile();
			}

			[Benchmark]
			public void RuntimeTypeModelSerializeInt()
			{
				using (var stream = new MemoryStream())
				{
					_runtimeTypeModel.Serialize(stream, 42);
				}
			}

			[Benchmark]
			public void CompiledTypeModelSerializeInt()
			{
				using (var stream = new MemoryStream())
				{
					_compiledTypeModel.Serialize(stream, 42);
				}
			}
		}

		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<Serializer>();
		}
	}
}
