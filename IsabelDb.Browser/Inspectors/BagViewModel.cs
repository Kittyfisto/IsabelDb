using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace IsabelDb.Browser.Inspectors
{
	/// <summary>
	///     Inspector for a collection which holds only values (and no keys).
	/// </summary>
	public sealed class BagViewModel
		: AbstractCollectionViewModel
	{
		private readonly ObservableCollection<Row> _rows;
		private DataGridCellInfo _selectedCell;

		private BagViewModel(ICollection collection, ObservableCollection<Row> rows)
			: base(collection)
		{
			_rows = rows;
		}

		public IEnumerable<Row> Rows => _rows;

		public DataGridCellInfo SelectedCell
		{
			get { return _selectedCell; }
			set
			{
				if (value == _selectedCell)
					return;

				_selectedCell = value;
				EmitPropertyChanged();

				SelectedRow = UnpackSelectedCell(value);
			}
		}

		private object UnpackSelectedCell(DataGridCellInfo value)
		{
			if (value.Item is Row row)
			{
				if (value.Column.DisplayIndex == 0)
					return row.RowId;

				return row.Value;
			}

			return null;
		}

		public static BagViewModel Create<T>(ICollection<T> collection)
		{
			var bag = (IReadOnlyBag<T>) collection;
			var rows = new ObservableCollection<Row>(bag.GetAll().Take(count: 100).Select(CreateRow));
			return new BagViewModel(collection, rows);
		}

		private static Row CreateRow<T>(KeyValuePair<RowId, T> pair)
		{
			return new Row
			{
				RowId = pair.Key,
				Value = new ValueViewModel(pair.Value)
			};
		}

		#region Overrides of AbstractInspectionViewModel

		protected override void DisplayRows(long startIndex, long count)
		{
			throw new NotImplementedException();
		}

		#endregion

		public sealed class Row
		{
			public RowId RowId { get; set; }
			public ValueViewModel Value { get; set; }
		}
	}
}