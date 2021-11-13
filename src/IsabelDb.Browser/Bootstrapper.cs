using System;
using System.Windows;
using IsabelDb.Browser.Settings;

namespace IsabelDb.Browser
{
	static class Bootstrapper
	{
		[STAThread]
		public static int Main(string[] args)
		{
			try
			{
				ApplicationSettings settings = ApplicationSettings.Create();
				settings.Restore();

				var application = new Application();
				var mainWindow = new MainWindow(settings)
				{
					DataContext = new MainWindowViewModel()
				};

				settings.MainWindow.RestoreTo(mainWindow);
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
