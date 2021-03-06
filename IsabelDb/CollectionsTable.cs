﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Net;
using IsabelDb.Collections;
using IsabelDb.Serializers;
using IsabelDb.TypeModels;

namespace IsabelDb
{
	/// <summary>
	///     Responsible for providing access to various (user) created stores.
	///     Ensures type safety when opening a database again, etc...
	/// </summary>
	internal sealed class CollectionsTable
	{
		public const string TableName = "isabel_collections";

		/// <summary>
		///     A list of serializers for .NET types which natively map to SQLite types
		///     (int, string, etc...).
		/// </summary>
		private static readonly System.Collections.Generic.IReadOnlyDictionary<Type, ISQLiteSerializer> NativeSerializers;

		private readonly SQLiteConnection _connection;
		private readonly bool _isReadOnly;

		private readonly System.Collections.Generic.Dictionary<string, IInternalCollection> _collectionsByName;
		private readonly System.Collections.Generic.HashSet<IInternalCollection> _collections;
		private readonly Serializer _serializer;
		private readonly CompiledTypeModel _typeModel;

		static CollectionsTable()
		{
			var nativeSerializers = new System.Collections.Generic.Dictionary<Type, ISQLiteSerializer>
			{
				{typeof(byte), new ByteSerializer()},
				{typeof(sbyte), new SByteSerializer()},
				{typeof(ushort), new UInt16Serializer()},
				{typeof(short), new Int16Serializer()},
				{typeof(uint), new UInt32Serializer()},
				{typeof(int), new Int32Serializer()},
				{typeof(long), new Int64Serializer()},
				{typeof(ulong), new UInt64Serializer()},
				{typeof(IPAddress), new IpAddressSerializer()},
				{typeof(string), new StringSerializer()},
				{typeof(float), new SingleSerializer()},
				{typeof(double), new DoubleSerializer()},
				{typeof(byte[]), new ByteArraySerializer()},
				{typeof(RowId), new ValueKeySerializer()},
				{typeof(DateTime), new DateTimeSerializer()}
			};
			NativeSerializers = nativeSerializers;
		}

		public CollectionsTable(SQLiteConnection connection,
		                    IReadOnlyList<Type> supportedTypes,
		                    bool isReadOnly)
		{
			_connection = connection;
			_isReadOnly = isReadOnly;

			_typeModel = ProtobufTypeModel.Create(connection, supportedTypes, isReadOnly);
			_serializer = new Serializer(_typeModel);
			_collections = new System.Collections.Generic.HashSet<IInternalCollection>();
			_collectionsByName = new System.Collections.Generic.Dictionary<string, IInternalCollection>();

			CreateCollections();
		}

		public IEnumerable<IInternalCollection> Collections => _collectionsByName.Values;

		public IIntervalCollection<TKey, TValue> GetIntervalCollection<TKey, TValue>(string name, Mode mode)
			where TKey : IComparable<TKey>
		{
			if (!_collectionsByName.TryGetValue(name, out var collection))
			{
				ThrowIfCannotCreate(name, mode);
				ThrowIfNotRegistered<TKey>();
				ThrowIfNotRegistered<TValue>();

				IntervalCollection<TKey, TValue>.ThrowIfInvalidKey();
				var tableName = AddTable(name, CollectionType.IntervalCollection, typeof(TKey), typeof(TValue));
				collection = CreateIntervalCollection<TKey, TValue>(name, tableName);
				AddCollection(name, collection);
			}
			else
			{
				ThrowIfMustCreate(mode);
			}

			EnsureTypeSafety(collection, CollectionType.IntervalCollection, typeof(TKey), typeof(TValue));

			if (!(collection is IIntervalCollection<TKey, TValue> target))
				throw new
					ArgumentException(string.Format("The dictionary '{0}' has a value type of '{1}': If your intent was to create a new dictionary, you have to pick a new name!",
					                                name,
					                                collection.ValueType.FullName));

			return target;
		}

		public IDictionary<TKey, TValue> GetDictionary<TKey, TValue>(string name, Mode mode)
		{
			if (!_collectionsByName.TryGetValue(name, out var collection))
			{
				ThrowIfCannotCreate(name, mode);
				ThrowIfNotRegistered<TKey>();
				ThrowIfNotRegistered<TValue>();

				var tableName = AddTable(name, CollectionType.Dictionary, typeof(TKey), typeof(TValue));
				collection = CreateDictionary<TKey, TValue>(name, tableName);
				AddCollection(name, collection);
			}
			else
			{
				ThrowIfMustCreate(mode);
			}

			EnsureTypeSafety(collection, CollectionType.Dictionary, typeof(TKey), typeof(TValue));

			if (!(collection is IDictionary<TKey, TValue> target))
				throw new
					ArgumentException(string.Format("The dictionary '{0}' has a value type of '{1}': If your intent was to create a new dictionary, you have to pick a new name!",
					                                name,
					                                collection.ValueType.FullName));

			return target;
		}

		public IMultiValueDictionary<TKey, TValue> GetMultiValueDictionary<TKey, TValue>(string name, Mode mode)
		{
			if (!_collectionsByName.TryGetValue(name, out var collection))
			{
				ThrowIfCannotCreate(name, mode);
				ThrowIfNotRegistered<TKey>();
				ThrowIfNotRegistered<TValue>();

				var tableName = AddTable(name, CollectionType.MultiValueDictionary, typeof(TKey), typeof(TValue));
				collection = CreateMultiValueDictionary<TKey, TValue>(name, tableName);
				AddCollection(name, collection);
			}
			else
			{
				ThrowIfMustCreate(mode);
			}

			EnsureTypeSafety(collection, CollectionType.MultiValueDictionary, typeof(TKey), typeof(TValue));

			if (!(collection is IMultiValueDictionary<TKey, TValue> target))
				throw new
					ArgumentException(string.Format("The dictionary '{0}' has a value type of '{1}': If your intent was to create a new dictionary, you have to pick a new name!",
					                                name,
					                                collection.ValueType.FullName));

			return target;
		}

		public IOrderedCollection<TKey, TValue> GetOrderedCollection<TKey, TValue>(string name, Mode mode) where TKey : IComparable<TKey>
		{
			if (!_collectionsByName.TryGetValue(name, out var collection))
			{
				ThrowIfCannotCreate(name, mode);
				ThrowIfNotRegistered<TKey>();
				ThrowIfNotRegistered<TValue>();

				OrderedCollection<TKey, TValue>.ThrowIfUnsupportedKeyType();

				var tableName = AddTable(name, CollectionType.OrderedCollection, typeof(TKey), typeof(TValue));
				collection = CreateOrderedCollection<TKey, TValue>(name, tableName);
				AddCollection(name, collection);
			}
			else
			{
				ThrowIfMustCreate(mode);
			}

			EnsureTypeSafety(collection, CollectionType.OrderedCollection, typeof(TKey), typeof(TValue));

			if (!(collection is IOrderedCollection<TKey, TValue> target))
				throw new
					ArgumentException(string.Format("The dictionary '{0}' has a value type of '{1}': If your intent was to create a new dictionary, you have to pick a new name!",
					                                name,
					                                collection.ValueType.FullName));

			return target;
		}

		public IBag<T> GetBag<T>(string name, Mode mode)
		{
			if (!_collectionsByName.TryGetValue(name, out var collection))
			{
				ThrowIfCannotCreate(name, mode);
				ThrowIfNotRegistered<T>();

				var tableName = AddTable(name, CollectionType.Bag, keyType: null, valueType: typeof(T));
				collection = CreateBag<T>(name, tableName);
				AddCollection(name, collection);
			}
			else
			{
				ThrowIfMustCreate(mode);
			}

			EnsureTypeSafety(collection, CollectionType.Bag, null, typeof(T));

			if (!(collection is IBag<T> target))
				throw new
					ArgumentException(string.Format("The bag '{0}' has a value type of '{1}': If your intent was to create a new collection, you have to pick a new name!",
					                                name,
					                                collection.ValueType.FullName));

			return target;
		}

		public IHashSet<T> GetHashSet<T>(string name, Mode mode)
		{
			if (!_collectionsByName.TryGetValue(name, out var collection))
			{
				ThrowIfCannotCreate(name, mode);
				ThrowIfNotRegistered<T>();

				var tableName = AddTable(name, CollectionType.HashSet, keyType: null, valueType: typeof(T));
				collection = CreateHashSet<T>(name, tableName);
				AddCollection(name, collection);
			}
			else
			{
				ThrowIfMustCreate(mode);
			}

			EnsureTypeSafety(collection, CollectionType.HashSet, null, typeof(T));

			if (!(collection is IHashSet<T> target))
				throw new
					ArgumentException(string.Format("The hash set '{0}' has a value type of '{1}': If your intent was to create a new collection, you have to pick a new name!",
					                                name,
					                                collection.ValueType.FullName));

			return target;
		}

		public IQueue<T> GetQueue<T>(string name, Mode mode)
		{
			if (!_collectionsByName.TryGetValue(name, out var collection))
			{
				ThrowIfCannotCreate(name, mode);
				ThrowIfNotRegistered<T>();

				var tableName = AddTable(name, CollectionType.Queue, keyType: null, valueType: typeof(T));
				collection = CreateQueue<T>(name, tableName);
				AddCollection(name, collection);
			}
			else
			{
				ThrowIfMustCreate(mode);
			}

			EnsureTypeSafety(collection, CollectionType.Queue, null, typeof(T));

			if (!(collection is IQueue<T> target))
				throw new
					ArgumentException(string.Format("The queue '{0}' has a value type of '{1}': If your intent was to create a new collection, you have to pick a new name!",
					                                name,
					                                collection.ValueType.FullName));

			return target;
		}

		public IPoint2DCollection<T> GetPoint2DCollection<T>(string name, Mode mode)
		{
			if (!_collectionsByName.TryGetValue(name, out var collection))
			{
				ThrowIfCannotCreate(name, mode);
				ThrowIfNotRegistered<T>();

				var tableName = AddTable(name, CollectionType.Point2DCollection, null, typeof(T));
				collection = CreatePoint2DCollection<T>(name, tableName);
				AddCollection(name, collection);
			}
			else
			{
				ThrowIfMustCreate(mode);
			}

			EnsureTypeSafety(collection, CollectionType.Point2DCollection, null, typeof(T));

			if (!(collection is IPoint2DCollection<T> target))
				throw new
					ArgumentException(string.Format("The point2d collection '{0}' has a value type of '{1}': If your intent was to create a new collection, you have to pick a new name!",
					                                name,
					                                collection.ValueType.FullName));

			return target;
		}

		public void Drop(ICollection collection)
		{
			if (collection is IInternalCollection collectionToRemove && _collections.Contains(collectionToRemove))
			{
				using (var transaction = _connection.BeginTransaction())
				{
					var tableName = collectionToRemove.TableName;
					using (var command = _connection.CreateCommand())
					{
						command.CommandText = string.Format("DROP TABLE {0}", tableName);
						command.ExecuteNonQuery();
					}

					using (var command = _connection.CreateCommand())
					{
						command.CommandText = string.Format("DELETE FROM {0} WHERE tableName = @tableName", TableName);
						command.Parameters.AddWithValue("@tableName", tableName);
						command.ExecuteNonQuery();
					}

					transaction.Commit();
				}

				_collectionsByName.Remove(collectionToRemove.Name);
				_collections.Remove(collectionToRemove);
				collectionToRemove.MarkAsDropped();
			}
		}

		[Pure]
		public static bool DoesTableExist(SQLiteConnection connection)
		{
			return Database.TableExists(connection, TableName);
		}

		public static void CreateTable(SQLiteConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = string.Format("CREATE TABLE {0} (" +
				                                    "name STRING PRIMARY KEY NOT NULL," +
				                                    "tableName STRING UNIQUE NOT NULL," +
				                                    "collectionType INTEGER NOT NULL," +
				                                    "keyType INTEGER," +
				                                    "valueType INTEGER NOT NULL" +
				                                    ")", TableName);

				command.ExecuteNonQuery();
			}
		}

		private void AddCollection(string name, IInternalCollection collection)
		{
			_collections.Add(collection);
			_collectionsByName.Add(name, collection);
		}

		private void ThrowIfCannotCreate(string name, Mode mode)
		{
			if (_isReadOnly || !mode.HasFlag(Mode.Create))
				throw new NoSuchCollectionException(string.Format("Unable to find a collection named '{0}'", name));
		}

		private void ThrowIfMustCreate(Mode mode)
		{
			if (mode.HasFlag(Mode.Create) && !mode.HasFlag(Mode.Get))
				throw new CollectionNameAlreadyInUseException();
		}

		private void ThrowIfNotRegistered<T>()
		{
			if (!_typeModel.IsRegistered(typeof(T)))
				throw new
					TypeNotRegisteredException(string.Format("The type '{0}' has not been registered when the database was created and thus may not be used as the value type in a collection",
					                                         typeof(T).FullName));
		}

		private void EnsureTypeSafety(ICollection collection,
		                              CollectionType expectedCollectionType,
		                              Type expectedKeyType,
		                              Type expectedValueType)
		{
			if (collection.Type != expectedCollectionType)
				throw new WrongCollectionTypeException(string.Format("The collection '{0}' is a {1} and cannot be treated as a {2}",
				                                                     collection.Name,
				                                                     collection.Type,
				                                                     expectedCollectionType));

			if (collection.ValueType == null)
				throw new
					TypeCouldNotBeResolvedException(string.Format("A {0} named '{1}' already exists but it's value type could not be resolved: If your intent is to re-use this existing collection, then you need to add '{2}' to the list of supported types upon creating the database. If your intent is to create a new collection, then you need to pick a different name!",
					                                   collection.Type,
					                                   collection.Name,
					                                   collection.ValueTypeName));

			if (expectedKeyType != null)
				if (collection.KeyType == null)
					throw new
						TypeCouldNotBeResolvedException(string.Format("A {0} named '{1}' already exists but it's key type could not be resolved: If your intent is to re-use this existing collection, then you need to add '{2}' to the list of supported types upon creating the database. If your intent is to create a new collection, then you need to pick a different name!",
						                                   collection.Type,
						                                   collection.Name,
						                                   collection.KeyTypeName));

			if (collection.KeyType != expectedKeyType)
				throw new
					TypeMismatchException(string.Format("The {0} '{1}' uses keys of type '{2}' which does not match the requested key type '{3}': If your intent was to create a new {0} then you have to pick a new name!",
					                                    collection.Type,
					                                    collection.Name,
					                                    collection.KeyType,
					                                    expectedKeyType));

			if (collection.ValueType != expectedValueType)
				throw new
					TypeMismatchException(string.Format("The {0} '{1}' uses values of type '{2}' which does not match the requested value type '{3}': If your intent was to create a new {0} then you have to pick a new name!",
					                                    collection.Type,
					                                    collection.Name,
					                                    collection.ValueType,
					                                    expectedValueType));
		}

		private IInternalCollection CreateIntervalCollection<T, TValue>(string name, string tableName)
			where T: IComparable<T>
		{
			var keySerializer = GetSerializer<T>();
			var valueSerializer = GetSerializer<TValue>();
			return new IntervalCollection<T, TValue>(_connection,
			                                         name,
			                                         tableName,
			                                         keySerializer,
			                                         valueSerializer,
			                                         _isReadOnly);
		}

		private IInternalCollection CreateDictionary<TKey, TValue>(string name, string tableName)
		{
			var keySerializer = GetSerializer<TKey>();
			var valueSerializer = GetSerializer<TValue>();
			return new Collections.Dictionary<TKey, TValue>(_connection,
			                                                name,
			                                                tableName,
			                                                keySerializer,
			                                                valueSerializer,
			                                                _isReadOnly);
		}

		private IInternalCollection CreateMultiValueDictionary<TKey, TValue>(string name, string tableName)
		{
			var keySerializer = GetSerializer<TKey>();
			var valueSerializer = GetSerializer<TValue>();
			return new MultiValueDictionary<TKey, TValue>(_connection,
			                                              name,
			                                              tableName,
			                                              keySerializer,
			                                              valueSerializer,
			                                              _isReadOnly);
		}

		private IInternalCollection CreateOrderedCollection<TKey, TValue>(string name, string tableName) where TKey : IComparable<TKey>
		{
			var keySerializer = GetSerializer<TKey>();
			var valueSerializer = GetSerializer<TValue>();
			return new OrderedCollection<TKey, TValue>(_connection,
			                                           name,
			                                           tableName,
			                                           keySerializer,
			                                           valueSerializer,
			                                           _isReadOnly);
		}

		private IInternalCollection CreateBag<T>(string name, string tableName)
		{
			var serializer = GetSerializer<T>();
			return new Bag<T>(_connection,
			                  name,
			                  tableName,
			                  serializer,
			                  _isReadOnly);
		}

		private IInternalCollection CreateHashSet<T>(string name, string tableName)
		{
			var serializer = GetSerializer<T>();
			return new Collections.HashSet<T>(_connection,
			                                  name,
			                                  tableName,
			                                  serializer,
			                                  _isReadOnly);
		}

		private IInternalCollection CreateQueue<T>(string name, string tableName)
		{
			var serializer = GetSerializer<T>();
			return new Collections.Queue<T>(_connection,
			                                name,
			                                tableName,
			                                serializer,
			                                _isReadOnly);
		}

		private IInternalCollection CreatePoint2DCollection<T>(string name, string tableName)
		{
			var serializer = GetSerializer<T>();
			return new Point2DCollection<T>(_connection,
			                                name,
			                                tableName,
			                                serializer,
			                                _isReadOnly);
		}

		private ISQLiteSerializer<T> GetSerializer<T>()
		{
			if (NativeSerializers.TryGetValue(typeof(T), out var serializer))
				return (ISQLiteSerializer<T>) serializer;

			return new GenericSerializer<T>(_serializer);
		}

		private string AddTable(string name, CollectionType collectionType, Type keyType, Type valueType)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("INSERT INTO {0} (name, collectionType, tableName, keyType, valueType)" +
				                                    "VALUES (@name, @collectionType, @tableName, @keyType, @valueType)", TableName);

				var keyTypeId = keyType != null ? (int?) _typeModel.GetTypeId(keyType) : null;
				var valueTypeId = _typeModel.GetTypeId(valueType);

				var tableName = CreateTableNameFor(name);

				command.Parameters.AddWithValue("@name", name);
				command.Parameters.AddWithValue("@collectionType", collectionType);
				command.Parameters.AddWithValue("@tableName", tableName);
				command.Parameters.AddWithValue("@keyType", keyTypeId);
				command.Parameters.AddWithValue("@valueType", valueTypeId);

				command.ExecuteNonQuery();

				return tableName;
			}
		}

		private string CreateTableNameFor(string name)
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT COUNT(*) FROM {0}", TableName);
				var count = Convert.ToInt32(command.ExecuteScalar());
				return string.Format("ObjectStore_{0}", count);
			}
		}

		private void CreateCollections()
		{
			using (var command = _connection.CreateCommand())
			{
				command.CommandText = string.Format("SELECT name, collectionType, tableName, keyType, valueType FROM {0}", TableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var name = reader.GetString(0);
						var collectionType = (CollectionType) reader.GetInt32(1);
						var tableName = reader.GetString(2);
						var keyTypeId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
						var valueTypeId = reader.GetInt32(4);

						var collection = CreateCollection(name, collectionType, tableName, keyTypeId, valueTypeId);
						AddCollection(name, collection);
					}
				}
			}
		}

		private IInternalCollection CreateCollection(string name,
		                                     CollectionType collectionType,
		                                     string tableName, int? keyTypeId, int valueTypeId)
		{
			var keyType = keyTypeId != null ? _typeModel.GetType(keyTypeId.Value) : null;
			var valueType = _typeModel.GetType(valueTypeId);
			string keyTypeName = keyTypeId != null ? _typeModel.GetTypeDescription(keyTypeId.Value)?. FullTypeName : null;
			string valueTypeName = _typeModel.GetTypeDescription(valueTypeId)?.FullTypeName;

			if (valueType == null)
				return new UnresolvedTypeCollection(_connection, collectionType, name, tableName, keyType, keyTypeName, null, valueTypeName, _isReadOnly);

			switch (collectionType)
			{
				case CollectionType.Bag:
					return CreateBag(name, tableName, valueType);

				case CollectionType.HashSet:
					return CreateHashSet(name, tableName, valueType);

				case CollectionType.Queue:
					return CreateQueue(name, tableName, valueType);

				case CollectionType.Point2DCollection:
					return CreatePoint2DCollection(name, tableName, valueType);
			}

			if (keyType == null)
				return new UnresolvedTypeCollection(_connection, collectionType, name, tableName, null, keyTypeName, valueType, valueTypeName, _isReadOnly);

			switch (collectionType)
			{
				case CollectionType.Dictionary:
					return CreateDictionary(name, tableName, keyType, valueType);

				case CollectionType.IntervalCollection:
					return CreateIntervalCollection(name, tableName, keyType, valueType);

				case CollectionType.MultiValueDictionary:
					return CreateMultiValueDictionary(name, tableName, keyType, valueType);

				case CollectionType.OrderedCollection:
					return CreateOrderedCollection(name, tableName, keyType, valueType);

				default:
					return new UnknownTypeCollection(_connection, name, tableName, keyTypeId, keyType, valueTypeId, valueType);
			}
		}

		private IInternalCollection CreateBag(string name, string tableName, Type valueType)
		{
			var collectionType = typeof(Bag<>).MakeGenericType(valueType);
			var serializer = CreateSerializer(valueType);
			var bag = Activator.CreateInstance(collectionType, _connection, name, tableName, serializer, _isReadOnly);
			return (IInternalCollection) bag;
		}

		private IInternalCollection CreateHashSet(string name, string tableName, Type valueType)
		{
			var collectionType = typeof(Collections.HashSet<>).MakeGenericType(valueType);
			var serializer = CreateSerializer(valueType);
			var hashSet = Activator.CreateInstance(collectionType, _connection, name, tableName, serializer, _isReadOnly);
			return (IInternalCollection) hashSet;
		}

		private IInternalCollection CreateQueue(string name, string tableName, Type valueType)
		{
			var collectionType = typeof(Collections.Queue<>).MakeGenericType(valueType);
			var serializer = CreateSerializer(valueType);
			var queue = Activator.CreateInstance(collectionType, _connection, name, tableName, serializer, _isReadOnly);
			return (IInternalCollection) queue;
		}

		private IInternalCollection CreateDictionary(string name, string tableName, Type keyType, Type valueType)
		{
			var collectionType = typeof(Collections.Dictionary<,>).MakeGenericType(keyType, valueType);
			var keySerializer = CreateSerializer(keyType);
			var valueSerializer = CreateSerializer(valueType);
			var bag = Activator.CreateInstance(collectionType, _connection, name, tableName, keySerializer, valueSerializer, _isReadOnly);
			return (IInternalCollection) bag;
		}

		private IInternalCollection CreateIntervalCollection(string name, string tableName, Type keyType, Type valueType)
		{
			var collectionType = typeof(IntervalCollection<,>).MakeGenericType(keyType, valueType);
			var keySerializer = CreateSerializer(keyType);
			var valueSerializer = CreateSerializer(valueType);
			var bag = Activator.CreateInstance(collectionType, _connection, name, tableName, keySerializer, valueSerializer, _isReadOnly);
			return (IInternalCollection) bag;
		}

		private IInternalCollection CreateMultiValueDictionary(string name, string tableName, Type keyType, Type valueType)
		{
			var collectionType = typeof(MultiValueDictionary<,>).MakeGenericType(keyType, valueType);
			var keySerializer = CreateSerializer(keyType);
			var valueSerializer = CreateSerializer(valueType);
			var bag = Activator.CreateInstance(collectionType, _connection, name, tableName, keySerializer, valueSerializer, _isReadOnly);
			return (IInternalCollection) bag;
		}

		private IInternalCollection CreateOrderedCollection(string name, string tableName, Type keyType, Type valueType)
		{
			var collectionType = typeof(OrderedCollection<,>).MakeGenericType(keyType, valueType);
			var keySerializer = CreateSerializer(keyType);
			var valueSerializer = CreateSerializer(valueType);
			var bag = Activator.CreateInstance(collectionType, _connection, name, tableName, keySerializer, valueSerializer, _isReadOnly);
			return (IInternalCollection) bag;
		}

		private IInternalCollection CreatePoint2DCollection(string name, string tableName, Type valueType)
		{
			var collectionType = typeof(Point2DCollection<>).MakeGenericType(valueType);
			var valueSerializer = CreateSerializer(valueType);
			var point2DCollection = Activator.CreateInstance(collectionType, _connection, name, tableName, valueSerializer, _isReadOnly);
			return (IInternalCollection) point2DCollection;
		}

		private ISQLiteSerializer CreateSerializer(Type type)
		{
			if (NativeSerializers.TryGetValue(type, out var serializer))
				return serializer;

			var serializerType = typeof(GenericSerializer<>).MakeGenericType(type);
			return (ISQLiteSerializer) Activator.CreateInstance(serializerType, _serializer);
		}

		public void OnRollback(IReadOnlyList<IInternalCollection> collectionsBeforeTransaction)
		{
			// A transaction has been rolled back.
			// This means that the list of collections may not be up-to-date anymore:
			// It's possible that someone has added collections from within the transaction
			// which are now no longer present in the database itself (which has honored the
			// rollback), but are still part of the _collections cache.
			AddPreviouslyRemovedCollections(collectionsBeforeTransaction);
			RemoveNonExistantCollections();
		}

		private void AddPreviouslyRemovedCollections(IReadOnlyList<IInternalCollection> collectionsBeforeTransaction)
		{
			foreach (var collection in collectionsBeforeTransaction)
			{
				if (_collections.Add(collection))
				{
					_collectionsByName.Add(collection.Name, collection);
					collection.UnnmarkAsDropped();
				}
			}
		}

		private void RemoveNonExistantCollections()
		{
			var toRemove = new List<IInternalCollection>();

			using (var command = _connection.CreateCommand())
			{
				command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name = @name";
				var nameParameter = command.Parameters.Add("@name", DbType.String);
				foreach (var collection in _collectionsByName.Values)
				{
					nameParameter.Value = collection.TableName;
					if (Convert.ToInt64(command.ExecuteScalar()) <= 0)
					{
						toRemove.Add(collection);
					}
				}
			}

			foreach (var collection in toRemove)
			{
				_collections.Remove(collection);
				_collectionsByName.Remove(collection.Name);
			}
		}
	}
}