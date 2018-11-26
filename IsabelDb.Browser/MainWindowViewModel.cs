using System;
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
		private string _windowTitle;

		public string WindowTitle
		{
			get { return _windowTitle; }
			private set
			{
				if (value == _windowTitle)
					return;

				_windowTitle = value;
				EmitPropertyChanged();
			}
		}

		public DatabaseViewModel Database
		{
			get { return _database; }
			set
			{
				if (value == _database)
					return;

				_database = value;
				EmitPropertyChanged();

				if (value != null)
				{
					WindowTitle = string.Format("{0} - {1}", Constants.ApplicationTitle, value.Filename);
				}
				else
				{
					WindowTitle = Constants.ApplicationTitle;
				}
			}
		}

		public MainWindowViewModel()
		{
			//OpenFile(@"C:\Users\Simon\Documents\GitHub\IsabelDb\LocalTestData\Test.isdb");
			var database = global::IsabelDb.Database.CreateInMemory(new Type[0]);
			var bag = database.GetBag<string>("A");
			bag.Put("Stuff");

			var queue = database.GetQueue<int>("B");
			queue.Enqueue(42);
			queue.Enqueue(9001);
			queue.Enqueue(1337);

			Database = new DatabaseViewModel(new DatabaseProxy(database));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void OpenFile(string fileName)
		{
			Database = new DatabaseViewModel(new DatabaseProxy(fileName));
		}
	}
}
