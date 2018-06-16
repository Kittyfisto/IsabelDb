using System.Collections.Generic;
using System.Linq;
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
