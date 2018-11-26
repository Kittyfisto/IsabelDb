using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using IsabelDb.Browser.Settings;

namespace IsabelDb.Browser
{
	public partial class MainWindow
	{
		private readonly ApplicationSettings _settings;

		public MainWindow(ApplicationSettings settings)
		{
			_settings = settings;
			InitializeComponent();

			SizeChanged += OnSizeChanged;
			LocationChanged += OnLocationChanged;
			Closing += OnClosing;
			Drop += OnDrop;
		}

		private void OnLocationChanged(object sender, EventArgs eventArgs)
		{
			_settings.MainWindow.UpdateFrom(this);
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			_settings.MainWindow.UpdateFrom(this);
		}

		private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
		{
			_settings.MainWindow.UpdateFrom(this);
			_settings.SaveAsync().Wait();
		}

		private void OnDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// Note that you can have more than one file.
				var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
				var file = files?.FirstOrDefault();
				if (file != null)
				{
					// Assuming you have one file that you care about, pass it off to whatever
					// handling code you have defined.
					((MainWindowViewModel) DataContext).OpenFile(file);
				}
			}
		}
	}
}
