﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using IsabelDb.Serializers;

namespace IsabelDb.Collections
{
	internal sealed class Point2DCollection<TValue>
		: AbstractCollection<TValue>
		, IPoint2DCollection<TValue>
	{
		private readonly SQLiteConnection _connection;
		private readonly string _tableName;
		private readonly ISQLiteSerializer<TValue> _valueSerializer;
		private readonly string _getAllKeysQuery;
		private readonly string _existsKeyQuery;
		private readonly string _existsRowQuery;
		private readonly string _getByExactCoordinatesQuery;
		private readonly string _getByRowId;
		private readonly string _getAllQuery;
		private readonly string _getKeysWithinQuery;
		private readonly string _getValuesWithinQuery;
		private readonly string _getWithinQuery;
		private readonly string _putQuery;
		private readonly string _removeQuery;
		private readonly string _removeRowQuery;
		private readonly string _removeWithinQuery;

		public Point2DCollection(SQLiteConnection connection,
		                           string name,
		                           string tableName,
		                           ISQLiteSerializer<TValue> valueSerializer,
		                           bool isReadOnly)
			: base(connection, name, tableName, valueSerializer, isReadOnly)
		{
			_connection = connection;
			_tableName = tableName;
			_valueSerializer = valueSerializer;
			
			_getAllQuery = string.Format("SELECT x, y, value FROM {0}", tableName);
			_getAllKeysQuery = string.Format("SELECT x, y FROM {0}", tableName);
			_existsKeyQuery = string.Format("SELECT EXISTS(SELECT * FROM {0} WHERE x = @x AND y = @y)", tableName);
			_existsRowQuery = string.Format("SELECT EXISTS(SELECT * FROM {0} WHERE rowid = @rowid)", tableName);
			_getByRowId = string.Format("SELECT value FROM {0} WHERE rowid = @rowid", tableName);
			_getByExactCoordinatesQuery = string.Format("SELECT value FROM {0} WHERE x = @x AND y = @y", tableName);
			_getKeysWithinQuery = string.Format("SELECT x, y FROM {0} WHERE x >= @minX AND x <= @maxX AND y >= @minY AND y <= @maxY", tableName);
			_getValuesWithinQuery = string.Format("SELECT value FROM {0} WHERE x >= @minX AND x <= @maxX AND y >= @minY AND y <= @maxY", tableName);
			_getWithinQuery = string.Format("SELECT x, y, value FROM {0} WHERE x >= @minX AND x <= @maxX AND y >= @minY AND y <= @maxY", tableName);
			_putQuery = string.Format("INSERT INTO {0} (x, y, value) VALUES (@x, @y, @value)", tableName);
			_removeQuery = string.Format("DELETE FROM {0} WHERE x = @x AND y = @y", tableName);
			_removeRowQuery = string.Format("DELETE FROM {0} WHERE rowid = @rowid", tableName);
			_removeWithinQuery = string.Format("DELETE FROM {0} WHERE x >= @minX AND x <= @maxX AND y >= @minY AND y <= @maxY", tableName);

			CreateObjectTableIfNecessary();
		}

		#region Overrides of AbstractCollection

		public override CollectionType Type => CollectionType.Point2DCollection;

		public override Type KeyType => null;

		public override string KeyTypeName => KeyType.FullName;

		#endregion

		#region Implementation of IReadOnlyMultiValueDictionary<Point2D,TValue>

		public IEnumerable<Point2D> GetAllKeys()
		{
			ThrowIfDropped();
			return GetAllKeysInternal();
		}

		public bool ContainsKey(Point2D key)
		{
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _existsKeyQuery;
				command.Parameters.AddWithValue("@x", key.X);
				command.Parameters.AddWithValue("@y", key.Y);

				var value = Convert.ToInt64(command.ExecuteScalar());
				return value != 0;
			}
		}

		public bool ContainsRow(RowId row)
		{
			ThrowIfDropped();
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _existsRowQuery;
				command.Parameters.AddWithValue("@rowid", row.Id);

				var value = Convert.ToInt64(command.ExecuteScalar());
				return value != 0;
			}
		}

		public TValue GetValue(RowId row)
		{
			if (!TryGetValue(row, out var value))
				throw new KeyNotFoundException();

			return value;
		}

		public bool TryGetValue(RowId row, out TValue value)
		{
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByRowId;
				command.Parameters.AddWithValue("@rowid", row.Id);

				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
					{
						value = default(TValue);
						return false;
					}

					return _valueSerializer.TryDeserialize(reader, 0, out value);
				}
			}
		}

		public IEnumerable<TValue> GetValues(IEnumerable<RowId> rows)
		{
			ThrowIfDropped();
			return GetValuesInternal(rows);
		}

		public IEnumerable<TValue> GetValues(Point2D key)
		{
			ThrowIfDropped();
			return GetValuesInternal(key);
		}

		public IEnumerable<TValue> GetValues(IEnumerable<Point2D> keys)
		{
			ThrowIfDropped();
			return GetValuesInternal(keys);
		}

		public IEnumerable<KeyValuePair<Point2D, IEnumerable<TValue>>> GetAll()
		{
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getAllQuery;
				
				var tmp = new System.Collections.Generic.Dictionary<Point2D, List<TValue>>();
				var ret = new System.Collections.Generic.Dictionary<Point2D, IEnumerable<TValue>>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_valueSerializer.TryDeserialize(reader, 2, out var value))
						{
							var x = reader.GetDouble(0);
							var y = reader.GetDouble(1);
							var point = new Point2D(x, y);
							if (!tmp.TryGetValue(point, out var values))
							{
								values = new List<TValue>();
								tmp.Add(point, values);
								ret.Add(point, values);
							}

							values.Add(value);
						}
					}
				}

				return ret;
			}
		}

		#region Overrides of Object

		public override string ToString()
		{
			if (IsDropped)
				return string.Format("This collection (\"{0}\") has been removed from the database and may no longer be used", Name);

			return string.Format("Point2DCollection<{0}, {1}>(\"{2}\")", typeof(Point2D).FullName, ValueType.FullName, Name);
		}

		#endregion

		#endregion

		#region Implementation of IReadOnlySpatial2DCollection<TValue>

		public IEnumerable<Point2D> GetKeysWithin(Rectangle2D rectangle)
		{
			ThrowIfDropped();
			return GetKeysWithinInternal(rectangle);
		}

		public IEnumerable<TValue> GetValuesWithin(Rectangle2D rectangle)
		{
			ThrowIfDropped();
			return GetValuesWithinInternal(rectangle);
		}

		public IEnumerable<KeyValuePair<Point2D, TValue>> GetWithin(Rectangle2D rectangle)
		{
			ThrowIfDropped();
			return GetWithinInternal(rectangle);
		}

		#endregion

		#region Implementation of IMultiValueDictionary<Point2D,TValue>

		public RowId Put(Point2D key, TValue value)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;
				command.Parameters.AddWithValue("@x", key.X);
				command.Parameters.AddWithValue("@y", key.Y);
				command.Parameters.AddWithValue("@value", _valueSerializer.Serialize(value));
				command.ExecuteNonQuery();

				var id = _connection.LastInsertRowId;
				return new RowId(id);
			}
		}

		public IReadOnlyList<RowId> PutMany(Point2D key, IEnumerable<TValue> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			var ids = new List<RowId>();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;
				command.Parameters.AddWithValue("@x", key.X);
				command.Parameters.AddWithValue("@y", key.Y);
				var parameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);
				foreach (var value in values)
				{
					parameter.Value = _valueSerializer.Serialize(value);
					command.ExecuteNonQuery();

					var id = _connection.LastInsertRowId;
					ids.Add(new RowId(id));
				}

				transaction.Commit();
			}

			return ids;
		}

		public IReadOnlyList<RowId> PutMany(IEnumerable<KeyValuePair<Point2D, IEnumerable<TValue>>> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			var ids = new List<RowId>();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;
				var x = command.Parameters.Add("@x", DbType.Double);
				var y = command.Parameters.Add("@y", DbType.Double);
				var parameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);
				foreach (var pair in values)
				{
					x.Value = pair.Key.X;
					y.Value = pair.Key.Y;
					foreach (var value in pair.Value)
					{
						parameter.Value = _valueSerializer.Serialize(value);
						command.ExecuteNonQuery();

						var id = _connection.LastInsertRowId;
						ids.Add(new RowId(id));
					}
				}

				transaction.Commit();
			}

			return ids;
		}

		public IReadOnlyList<RowId> PutMany(IEnumerable<KeyValuePair<Point2D, TValue>> values)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			var ids = new List<RowId>();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _putQuery;
				var x = command.Parameters.Add("@x", DbType.Double);
				var y = command.Parameters.Add("@y", DbType.Double);
				var parameter = command.Parameters.Add("@value", _valueSerializer.DatabaseType);
				foreach (var pair in values)
				{
					x.Value = pair.Key.X;
					y.Value = pair.Key.Y;
					parameter.Value = _valueSerializer.Serialize(pair.Value);
					command.ExecuteNonQuery();

					var id = _connection.LastInsertRowId;
					ids.Add(new RowId(id));
				}

				transaction.Commit();
			}

			return ids;
		}

		public void Remove(RowId row)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeRowQuery;
				command.Parameters.AddWithValue("@rowid", row.Id);
				command.ExecuteNonQuery();
			}
		}

		public void RemoveMany(IEnumerable<RowId> rows)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeRowQuery;
				var rowId = command.Parameters.Add("@rowid", DbType.Int64);
				foreach (var row in rows)
				{
					rowId.Value = row.Id;
					command.ExecuteNonQuery();
				}
			}
		}

		public void RemoveAll(Point2D key)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeQuery;
				command.Parameters.AddWithValue("@x", key.X);
				command.Parameters.AddWithValue("@y", key.Y);
				command.ExecuteNonQuery();
			}
		}

		public void RemoveMany(IEnumerable<Point2D> keys)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var transaction = _connection.BeginTransaction())
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeQuery;
				var x = command.Parameters.Add("@x", DbType.Double);
				var y = command.Parameters.Add("@y", DbType.Double);
				foreach (var key in keys)
				{
					x.Value = key.X;
					y.Value = key.Y;
					command.ExecuteNonQuery();
				}
				transaction.Commit();
			}
		}

		public void RemoveMany(Rectangle2D rectangle)
		{
			ThrowIfReadOnly();
			ThrowIfDropped();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _removeWithinQuery;
				command.Parameters.AddWithValue("@minX", rectangle.MinX);
				command.Parameters.AddWithValue("@maxX", rectangle.MaxX);
				command.Parameters.AddWithValue("@minY", rectangle.MinY);
				command.Parameters.AddWithValue("@maxY", rectangle.MaxY);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		private IEnumerable<TValue> GetValuesInternal(IEnumerable<Point2D> keys)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByExactCoordinatesQuery;

				foreach (var key in keys)
				{
					command.Parameters.AddWithValue("@x", key.X);
					command.Parameters.AddWithValue("@y", key.Y);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							if (_valueSerializer.TryDeserialize(reader, 0, out var value))
								yield return value;
						}
					}
				}
			}
		}

		private IEnumerable<Point2D> GetAllKeysInternal()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getAllKeysQuery;
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var x = reader.GetDouble(0);
						var y = reader.GetDouble(1);
						yield return new Point2D(x, y);
					}
				}
			}
		}

		private IEnumerable<TValue> GetValuesInternal(IEnumerable<RowId> rows)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByRowId;
				var rowId = command.Parameters.Add("@rowid", DbType.Int64);

				foreach (var row in rows)
				{
					rowId.Value = row.Id;
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							if (_valueSerializer.TryDeserialize(reader, 0, out var value))
								yield return value;
						}
					}
				}
			}
		}

		private IEnumerable<TValue> GetValuesInternal(Point2D key)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getByExactCoordinatesQuery;
				command.Parameters.AddWithValue("@x", key.X);
				command.Parameters.AddWithValue("@y", key.Y);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_valueSerializer.TryDeserialize(reader, 0, out var value))
							yield return value;
					}
				}
			}
		}

		private IEnumerable<Point2D> GetKeysWithinInternal(Rectangle2D rectangle)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getKeysWithinQuery;
				command.Parameters.AddWithValue("@minX", rectangle.MinX);
				command.Parameters.AddWithValue("@maxX", rectangle.MaxX);
				command.Parameters.AddWithValue("@minY", rectangle.MinY);
				command.Parameters.AddWithValue("@maxY", rectangle.MaxY);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var x = reader.GetDouble(0);
						var y = reader.GetDouble(1);
						yield return new Point2D(x, y);
					}
				}
			}
		}

		private IEnumerable<TValue> GetValuesWithinInternal(Rectangle2D rectangle)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getValuesWithinQuery;
				command.Parameters.AddWithValue("@minX", rectangle.MinX);
				command.Parameters.AddWithValue("@maxX", rectangle.MaxX);
				command.Parameters.AddWithValue("@minY", rectangle.MinY);
				command.Parameters.AddWithValue("@maxY", rectangle.MaxY);
				
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_valueSerializer.TryDeserialize(reader, 0, out var value))
							yield return value;
					}
				}
			}
		}

		private IEnumerable<KeyValuePair<Point2D, TValue>> GetWithinInternal(Rectangle2D rectangle)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = _getWithinQuery;
				command.Parameters.AddWithValue("@minX", rectangle.MinX);
				command.Parameters.AddWithValue("@maxX", rectangle.MaxX);
				command.Parameters.AddWithValue("@minY", rectangle.MinY);
				command.Parameters.AddWithValue("@maxY", rectangle.MaxY);
				
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (_valueSerializer.TryDeserialize(reader, 2, out var value))
						{
							var x = reader.GetDouble(0);
							var y = reader.GetDouble(1);
							var point = new Point2D(x, y);
							yield return new KeyValuePair<Point2D, TValue>(point, value);
						}
					}
				}
			}
		}

		private void CreateObjectTableIfNecessary()
		{
			using (var command = _connection.CreateCommand())
			{
				var builder = new StringBuilder();
				builder.AppendFormat("CREATE TABLE IF NOT EXISTS {0} (", _tableName);
				builder.AppendFormat("rowid INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ");
				builder.AppendFormat("x REAL NOT NULL, ");
				builder.AppendFormat("y REAL NOT NULL, ");
				builder.AppendFormat("value {0} NOT NULL", SQLiteHelper.GetAffinity(_valueSerializer.DatabaseType));
				builder.Append(");");
				builder.AppendFormat("CREATE INDEX IF NOT EXISTS {0}_x on {0}(x);", _tableName);
				command.CommandText = builder.ToString();
				command.ExecuteNonQuery();
			}
		}
	}
}
