﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using FluentAssertions;
using IsabelDb.Test.Entities.V1;
using NUnit.Framework;

namespace IsabelDb.Test
{
	[TestFixture]
	public sealed class CompatibilityTest
	{
		private SQLiteConnection _connection;

		[SetUp]
		public void Setup()
		{
			_connection = new SQLiteConnection("Data Source=:memory:");
			_connection.Open();
			IsabelDb.CreateTables(_connection);
		}

		[TearDown]
		public void TearDown()
		{
			_connection.Dispose();
		}

		[Test]
		[Description("Verifies that it's possible to add fields to entites and still read back 'old' database")]
		public void TestBackwardCompatibilityAddProperty()
		{
			using (var database = CreateDatabase(typeof(Entities.V1.Comic)))
			{
				var comics = database.GetDictionary<int, Entities.V1.Comic>("Comcis");
				comics.Put(1, new Entities.V1.Comic
				{
					Name = "Watchmen",
					Writer = "Alan Moore"
				});
			}

			using (var database = CreateDatabase(typeof(Entities.V2.Comic)))
			{
				var comics = database.GetDictionary<int, Entities.V2.Comic>("Comcis");
				var comic = comics.Get(1);
				comic.Name.Should().Be("Watchmen");
				comic.Writer.Should().Be("Alan Moore");
				comic.Artist.Should().BeNull();
			}
		}

		[Test]
		[RequriedBehaviour]
		[Description("Verifies that it's possible to add fields to entites and still read back 'old' database")]
		public void TestForwardCompatibilityAddProperty()
		{
			using (var database = CreateDatabase(typeof(Entities.V2.Comic)))
			{
				var comics = database.GetDictionary<int, Entities.V2.Comic>("Comcis");
				comics.Put(1, new Entities.V2.Comic
				{
					Name = "Watchmen",
					Writer = "Alan Moore",
					Artist = "Dave Gibbons"
				});
			}

			using (var database = CreateDatabase(typeof(Entities.V1.Comic)))
			{
				var comics = database.GetDictionary<int, Entities.V1.Comic>("Comcis");
				var comic = comics.Get(1);
				comic.Name.Should().Be("Watchmen");
				comic.Writer.Should().Be("Alan Moore");
			}
		}

		[Test]
		public void TestRenamedListArgument()
		{
			using (var database = CreateDatabase(typeof(Entities.V1.Motherboard)))
			{
				var comics = database.GetDictionary<int, Entities.V1.Motherboard>("Motherboards");
				comics.Put(1, new Entities.V1.Motherboard
				{
					Cpus = new List<Cpu>
					{
						new Cpu { Model = "i7" },
						new Cpu { Model = "i5" }
					}
				});
			}

			using (var database = CreateDatabase(typeof(Entities.V2.Motherboard)))
			{
				var comics = database.GetDictionary<int, Entities.V2.Motherboard>("Motherboards");
				var motherboard = comics.Get(1);
				motherboard.Cpus.Should().HaveCount(2);
				motherboard.Cpus[0].Model.Should().Be("i7");
				motherboard.Cpus[1].Model.Should().Be("i5");
			}
		}

		private IsabelDb CreateDatabase(params Type[] type)
		{
			return new IsabelDb(_connection, type, disposeConnection: false);
		}
	}
}
