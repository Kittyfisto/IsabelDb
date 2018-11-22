using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;

namespace IsabelDb.Browser
{
	public sealed class MainWindowViewModel
		: INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private DatabaseViewModel _database;

		public DatabaseViewModel Database
		{
			get { return _database; }
			set { _database = value; EmitPropertyChanged(); }
		}

		public MainWindowViewModel()
		{
			Open(@"C:\Users\Simon\Documents\GitHub\IsabelDb\IsabelDb.Browser\Test.isdb");
		}

		public void Open(string fileName)
		{
			Database = new DatabaseViewModel(new DatabaseProxy(fileName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
