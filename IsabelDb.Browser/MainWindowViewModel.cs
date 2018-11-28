using System;
using System.ComponentModel;
using System.Net;
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
			var bag = database.GetBag<object>("A");
			bag.Put(1337);
			bag.Put(9001);
			bag.Put(DateTime.Now);
			bag.Put(DateTime.UtcNow);
			bag.Put(IPAddress.Loopback);
			bag.Put(IPAddress.Any);
			bag.Put(IPAddress.IPv6Loopback);
			bag.Put(IPAddress.IPv6Any);
			bag.Put("Stuff");

			var queue = database.GetQueue<int>("B");
			queue.Enqueue(42);
			queue.Enqueue(9001);
			queue.Enqueue(1337);

			var dictionary = database.GetDictionary<int, string>("C");
			dictionary.Put(42, "Answer to the Ultimate Question of Life, the Universe, and Everything");

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
