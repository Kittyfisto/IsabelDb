using System.ComponentModel;

namespace IsabelDb.Browser.Inspectors
{
	public interface ICollectionViewModel
		: INotifyPropertyChanged
	{
		int RowsPerPage { get; }
		long PageCount { get; }
		long CurrentPage { get; set; }
		object SelectedRow { get; set; }

		long Count { get; }
		bool HasValues { get; }
		bool InspectSelectedObject { get; set; }

		/// <summary>
		///     A view model which allows a complete inspection of the objects in the selected
		///     row.
		/// </summary>
		ObjectInspectorViewModel SelectedRowInspector { get; }
	}
}