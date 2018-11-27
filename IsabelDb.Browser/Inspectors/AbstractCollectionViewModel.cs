using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IsabelDb.Browser.Inspectors
{
	public abstract class AbstractCollectionViewModel
		: ICollectionViewModel
	{
		private readonly ICollection _collection;
		private readonly long _count;
		private long _currentPage;
		private bool _inspectSelectedObject;
		private ObjectInspectorViewModel _selectedRowInspector;
		private object _selectedRow;

		protected AbstractCollectionViewModel(ICollection collection)
		{
			_collection = collection;
			_count = collection.Count();
			_inspectSelectedObject = true;
		}

		public int RowsPerPage => 250;

		public long PageCount => (long) Math.Ceiling(1.0 * Count / RowsPerPage);

		public long CurrentPage
		{
			get => _currentPage;
			set
			{
				if (value == _currentPage)
					return;

				_currentPage = value;
				EmitPropertyChanged();

				DisplayRows(_currentPage * RowsPerPage, RowsPerPage);
			}
		}

		public object SelectedRow
		{
			get { return _selectedRow;}
			set
			{
				if (value == _selectedRow)
					return;

				_selectedRow = value;
				EmitPropertyChanged();

				SelectedRowInspector = value != null
					? new ObjectInspectorViewModel(value)
					: null;
			}
		}

		protected abstract void DisplayRows(long startIndex, long count);

		public long Count => _count;

		public bool HasValues => _count > 0;

		public bool InspectSelectedObject
		{
			get => _inspectSelectedObject;
			set
			{
				if (value == _inspectSelectedObject)
					return;

				_inspectSelectedObject = value;
				EmitPropertyChanged();
			}
		}

		public ObjectInspectorViewModel SelectedRowInspector
		{
			get { return _selectedRowInspector; }
			set
			{
				if (value == _selectedRowInspector)
					return;

				_selectedRowInspector = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}