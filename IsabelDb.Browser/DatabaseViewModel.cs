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
		private ICollectionInspectionViewModel _selectedCollectionInspectionViewModel;
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

		public ICollectionInspectionViewModel SelectedCollectionInspection
		{
			get { return _selectedCollectionInspectionViewModel;}
			private set
			{
				if (_selectedCollectionInspectionViewModel == value)
					return;

				_selectedCollectionInspectionViewModel = value;
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

				SelectedCollectionInspection = CreateInspectionViewModel(value.Collection);
			}
		}

		private ICollectionInspectionViewModel CreateInspectionViewModel(ICollection collection)
		{
			if (collection == null)
				return null;

			switch (collection.Type)
			{
				case CollectionType.Bag:
				case CollectionType.Queue:
					return CreateValueCollectionInspector(collection);

				case CollectionType.Dictionary:
					return CreateDictionary(collection);

				default:
					throw new NotImplementedException();
			}
		}

		private ICollectionInspectionViewModel CreateDictionary(ICollection collection)
		{
			var method = typeof(DictionaryInspectorViewModel).GetMethod("Create", BindingFlags.Public | BindingFlags.Static)
			                                           .MakeGenericMethod(collection.KeyType, collection.ValueType);
			var viewModel = (ICollectionInspectionViewModel) method.Invoke(null, new object[] {collection});
			return viewModel;
		}

		private ICollectionInspectionViewModel CreateValueCollectionInspector(ICollection collection)
		{
			var methodName = nameof(ValueCollectionInspectionViewModel.Create);
			var method = typeof(ValueCollectionInspectionViewModel).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(collection.ValueType);
			var viewModel = (ICollectionInspectionViewModel) method.Invoke(null, new object[] {collection});
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