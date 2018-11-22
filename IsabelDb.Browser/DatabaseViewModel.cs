using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IsabelDb.Browser
{
	public sealed class DatabaseViewModel
	: INotifyPropertyChanged
	{
		private readonly List<CollectionViewModel> _collections;

		private readonly DatabaseProxy _database;
		private CollectionViewModel _selectedCollection;
		private ICollectionInspectorViewModel _selectedCollectionInspectorViewModel;

		public DatabaseViewModel(DatabaseProxy database)
		{
			_database = database;
			_collections= new List<CollectionViewModel>();
			foreach (var collection in database.Database.Collections)
			{
				_collections.Add(new CollectionViewModel(collection));
			}
		}

		public ICollectionInspectorViewModel SelectedCollectionInspector
		{
			get { return _selectedCollectionInspectorViewModel;}
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

		private ICollectionInspectorViewModel CreateInspectionViewModel(ICollection collection)
		{
			if (collection == null)
				return null;

			switch (collection.Type)
			{
				case CollectionType.Bag:
					return CreateBag(collection);

				default:
					throw new NotImplementedException();
			}
		}

		private ICollectionInspectorViewModel CreateBag(ICollection bag)
		{
			var type = typeof(BagInspectorViewModel<>).MakeGenericType(bag.ValueType);
			var viewModel = (ICollectionInspectorViewModel) Activator.CreateInstance(type, bag);
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