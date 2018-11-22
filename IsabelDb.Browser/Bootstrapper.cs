using System;
using System.Windows;

namespace IsabelDb.Browser
{
	static class Bootstrapper
	{
		[STAThread]
		public static int Main(string[] args)
		{
			try
			{
				var application = new Application();
				var mainWindow = new MainWindow
				{
					DataContext = new MainWindowViewModel()
				};
				mainWindow.Show();
				return application.Run();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return -1;
			}
		}
	}
}
