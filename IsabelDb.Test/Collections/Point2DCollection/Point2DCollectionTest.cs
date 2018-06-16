using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IsabelDb.Collections;
using NUnit.Framework;

namespace IsabelDb.Test.Collections.Point2DCollection
{
	[TestFixture]
	public sealed class Point2DCollectionTest
		: AbstractCollectionTest<IPoint2DCollection<string>>
	{
		private int _nextX;
		private int _nextY;
		private bool _incrementX;
		private Point2D _lastPoint;

		[SetUp]
		public void Setup()
		{
			_nextX = 0;
			_nextY = 0;
			_incrementX = true;
		}

		[Test]
		public void TestGetAllKeys1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.GetAllKeys().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetAllKeys2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1.1, 1), "C");

				values.GetAllKeys().Should().Equal(new Point2D(0, 0),
				                                   new Point2D(1, 2),
				                                   new Point2D(1.1, 1));
			}
		}

		[Test]
		public void TestContainsKey1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.ContainsKey(new Point2D(0, 0)).Should().BeFalse();
				values.ContainsKey(new Point2D(-1, -2)).Should().BeFalse();
			}
		}

		[Test]
		public void TestContainsKey2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1.1, 1), "C");
				values.ContainsKey(new Point2D(0, 0)).Should().BeTrue();
				values.ContainsKey(new Point2D(-1, -2)).Should().BeFalse();
				values.ContainsKey(new Point2D(1.1, 1)).Should().BeTrue();
			}
		}

		[Test]
		public void TestGetValues1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");

				values.GetValues(new Point2D(1, 2)).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetValues2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1, 2), "C");
				values.Put(new Point2D(1.1, 1), "D");

				values.GetValues(new Point2D(1, 2)).Should().Equal("B", "C");
			}
		}

		[Test]
		public void TestGetValues3()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");

				values.GetValues(new[]{new Point2D(1, 2), new Point2D(2, 3) }).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetValues4()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1, 2), "C");
				values.Put(new Point2D(1.1, 1), "D");

				values.GetValues(new []{new Point2D(1, 2), new Point2D(1.1, 1) }).Should().Equal("B", "C", "D");
			}
		}

		[Test]
		public void TestGetAll1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetAll2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1, 2), "C");
				values.Put(new Point2D(1.1, 1), "D");

				var allValues = values.GetAll().ToList();
				allValues.Should().HaveCount(3);
				allValues[0].Key.Should().Be(new Point2D(0, 0));
				allValues[0].Value.Should().Equal("A");
				allValues[1].Key.Should().Be(new Point2D(1, 2));
				allValues[1].Value.Should().Equal("B", "C");
				allValues[2].Key.Should().Be(new Point2D(1.1, 1));
				allValues[2].Value.Should().Equal("D");
			}
		}

		[Test]
		public void TestGetWithin1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1.1, 1), "C");

				values.GetWithin(new Rectangle2D
				{
					MinX = -1,
					MaxX = 1,
					MinY = 3,
					MaxY = 4
				}).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetWithin2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1.1, 1), "C");

				values.GetWithin(new Rectangle2D
				{
					MinX = 0.5,
					MaxX = 1.5,
					MinY = 2,
					MaxY = 2
				}).Should().Equal(new KeyValuePair<Point2D, string>(new Point2D(1, 2), "B"));
			}
		}

		[Test]
		public void TestGetValuesWithin1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1.1, 1), "C");

				values.GetValuesWithin(new Rectangle2D
				{
					MinX = -1,
					MaxX = 1,
					MinY = 3,
					MaxY = 4
				}).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetValuesWithin2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1.1, 1), "C");

				values.GetValuesWithin(new Rectangle2D
				{
					MinX = 0.5,
					MaxX = 1.5,
					MinY = 2,
					MaxY = 2
				}).Should().Equal("B");
			}
		}

		[Test]
		public void TestGetKeysWithin1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1.1, 1), "C");

				values.GetKeysWithin(new Rectangle2D
				{
					MinX = -1,
					MaxX = 1,
					MinY = 3,
					MaxY = 4
				}).Should().BeEmpty();
			}
		}

		[Test]
		public void TestGetKeysWithin2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(0, 0), "A");
				values.Put(new Point2D(1, 2), "B");
				values.Put(new Point2D(1.1, 1), "C");

				values.GetKeysWithin(new Rectangle2D
				{
					MinX = 0.5,
					MaxX = 1.5,
					MinY = 2,
					MaxY = 2
				}).Should().Equal(new Point2D(1, 2));
			}
		}

		[Test]
		public void TestPutMany1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.GetAll().Should().BeEmpty();
				values.PutMany(new List<KeyValuePair<Point2D, IEnumerable<string>>>());
				values.GetAll().Should().BeEmpty();
			}
		}

		[Test]
		public void TestPutMany2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.PutMany(new List<KeyValuePair<Point2D, IEnumerable<string>>>
				{
					new KeyValuePair<Point2D, IEnumerable<string>>(new Point2D(1, 1), new[]{"A", "B"}),
					new KeyValuePair<Point2D, IEnumerable<string>>(new Point2D(2, 3), new[]{"C", "D"}),
					new KeyValuePair<Point2D, IEnumerable<string>>(new Point2D(4, 5), new[]{"E", "F"})
				});
				var allValues = values.GetAll().ToList();
				allValues.Should().HaveCount(3);
				allValues[0].Key.Should().Be(new Point2D(1, 1));
				allValues[0].Value.Should().Equal("A", "B");
				allValues[1].Key.Should().Be(new Point2D(2, 3));
				allValues[1].Value.Should().Equal("C", "D");
				allValues[2].Key.Should().Be(new Point2D(4, 5));
				allValues[2].Value.Should().Equal("E", "F");
			}
		}

		[Test]
		public void TestPutMany3()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.PutMany(new List<KeyValuePair<Point2D, string>>
				{
					new KeyValuePair<Point2D, string>(new Point2D(1, 1), "A"),
					new KeyValuePair<Point2D, string>(new Point2D(2, 3), "B"),
					new KeyValuePair<Point2D, string>(new Point2D(4, 5), "C")
				});
				var allValues = values.GetAll().ToList();
				allValues.Should().HaveCount(3);
				allValues[0].Key.Should().Be(new Point2D(1, 1));
				allValues[0].Value.Should().Equal("A");
				allValues[1].Key.Should().Be(new Point2D(2, 3));
				allValues[1].Value.Should().Equal("B");
				allValues[2].Key.Should().Be(new Point2D(4, 5));
				allValues[2].Value.Should().Equal("C");
			}
		}

		[Test]
		public void TestPutMany4()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.PutMany(new Point2D(-1, -2), new []{"Homer", "Simpson"});

				var allValues = values.GetAll().ToList();
				allValues.Should().HaveCount(1);
				allValues[0].Key.Should().Be(new Point2D(-1, -2));
				allValues[0].Value.Should().Equal("Homer", "Simpson");
			}
		}

		[Test]
		public void TestRemoveMany1()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(2, 3), "A");
				values.Put(new Point2D(2, 3), "B");
				values.Put(new Point2D(3, 2), "C");
				values.Put(new Point2D(1, 1), "D");

				values.RemoveMany(new []{new Point2D(2, 3), new Point2D(1, 1) });
				var allValues = values.GetAll().ToList();
				allValues.Should().HaveCount(1);
				allValues[0].Key.Should().Be(new Point2D(3, 2));
				allValues[0].Value.Should().Equal("C");
			}
		}

		[Test]
		public void TestRemoveMany2()
		{
			using (var db = Database.CreateInMemory(NoCustomTypes))
			{
				var values = db.GetPoint2DCollection<string>("Cities");
				values.Put(new Point2D(2, 3), "A");
				values.Put(new Point2D(2, 3), "B");
				values.Put(new Point2D(3, 2), "C");
				values.Put(new Point2D(1, 1), "D");

				values.RemoveMany(new Rectangle2D
				{
					MinX = 1.9,
					MaxX = 3.2,
					MinY = 2,
					MaxY = 4
				});
				var allValues = values.GetAll().ToList();
				allValues.Should().HaveCount(1);
				allValues[0].Key.Should().Be(new Point2D(1, 1));
				allValues[0].Value.Should().Equal("D");
			}
		}

		#region Overrides of AbstractCollectionTest<IPoint2DCollection<string>>

		protected override CollectionType CollectionType => CollectionType.Point2DCollection;

		protected override IPoint2DCollection<string> GetCollection(IDatabase db, string name)
		{
			return db.GetPoint2DCollection<string>(name);
		}

		protected override void Put(IPoint2DCollection<string> collection, string value)
		{
			var point = GetNextPoint();
			collection.Put(point, value);
		}

		private Point2D GetNextPoint()
		{
			if (_incrementX)
				++_nextX;
			else
				++_nextY;
			_lastPoint = new Point2D(_nextX, _nextY);
			return _lastPoint;
		}

		protected override void PutMany(IPoint2DCollection<string> collection, params string[] values)
		{
			var stuff = values.Select(x => new KeyValuePair<Point2D, string>(GetNextPoint(), x)).ToList();
			collection.PutMany(stuff);
		}

		protected override void RemoveLastPutValue(IPoint2DCollection<string> collection)
		{
			collection.RemoveAll(_lastPoint);
		}

		#endregion
	}
}
