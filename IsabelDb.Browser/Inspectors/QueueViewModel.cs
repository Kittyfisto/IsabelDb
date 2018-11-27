using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IsabelDb.Browser.Inspectors
{
	/// <summary>
	/// Inspector for a collection which holds only values (and no keys).
	/// </summary>
	public sealed class QueueViewModel
		: AbstractCollectionViewModel
	{
		private readonly ObservableCollection<ValueViewModel> _rows;

		public static QueueViewModel Create<T>(ICollection<T> collection)
		{
			var rows = new ObservableCollection<ValueViewModel>(collection.GetAllValues().Take(100).Select(x => new ValueViewModel(x)));
			return new QueueViewModel(collection, rows);
		}

		private QueueViewModel(ICollection collection, ObservableCollection<ValueViewModel> rows)
			: base(collection)
		{
			_rows = rows;
		}

		public IEnumerable<IValueViewModel> Rows => _rows;

		#region Overrides of AbstractInspectionViewModel

		protected override void DisplayRows(long startIndex, long count)
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}
