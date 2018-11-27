using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using IsabelDb.Browser.Inspectors;

namespace IsabelDb.Browser
{
	public sealed class DatabaseViewModel
	: INotifyPropertyChanged
	{
		private readonly List<CollectionViewModel> _collections;

		private readonly DatabaseProxy _database;
		private CollectionViewModel _selectedCollection;
		private ICollectionViewModel _selectedCollectionInspectorViewModel;
		private readonly string _filename;

		public string Filename => _filename;

		public DatabaseViewModel(DatabaseProxy database)
		{
			_database = database;
			_filename = database.FileName;
			_collections= new List<CollectionViewModel>();
			foreach (var collection in database.Database.Collections)
			{
				_collections.Add(new CollectionViewModel(collection));
			}
		}

		public ICollectionViewModel SelectedCollectionInspector
		{
			get
			{
				return _selectedCollectionInspectorViewModel;
			}
			private set
			{
				if (_selectedCollectionInspectorViewModel == value)
					return;

				_selectedCollectionInspectorViewModel = value;
				EmitPropertyChanged();
			}
		}

		public CollectionViewModel SelectedCollection
		{
			get { return _selectedCollection; }
			set
			{
				if (value == _selectedCollection)
					return;

				_selectedCollection = value;
				EmitPropertyChanged();

				SelectedCollectionInspector = CreateInspectionViewModel(value.Collection);
			}
		}

		private ICollectionViewModel CreateInspectionViewModel(ICollection collection)
		{
			if (collection == null)
				return null;

			switch (collection.Type)
			{
				case CollectionType.Bag:
					return CreateBagViewModel(collection);

				case CollectionType.Queue:
					return CreateValueCollectionInspector(collection);

				case CollectionType.Dictionary:
					return CreateDictionaryViewModel(collection);

				default:
					throw new NotImplementedException();
			}
		}

		private ICollectionViewModel CreateBagViewModel(ICollection collection)
		{
			var methodName = nameof(BagViewModel.Create);
			var method = typeof(BagViewModel).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(collection.ValueType);
			var viewModel = (BagViewModel) method.Invoke(null, new object[] {collection});
			return viewModel;
		}

		private ICollectionViewModel CreateValueCollectionInspector(ICollection collection)
		{
			var methodName = nameof(QueueViewModel.Create);
			var method = typeof(QueueViewModel).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(collection.ValueType);
			var viewModel = (ICollectionViewModel) method.Invoke(null, new object[] {collection});
			return viewModel;
		}

		private ICollectionViewModel CreateDictionaryViewModel(ICollection collection)
		{
			var method = typeof(DictionaryViewModel).GetMethod("Create", BindingFlags.Public | BindingFlags.Static)
			                                                 .MakeGenericMethod(collection.KeyType, collection.ValueType);
			var viewModel = (ICollectionViewModel) method.Invoke(null, new object[] {collection});
			return viewModel;
		}

		public IEnumerable<CollectionViewModel> Collections => _collections;
		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}