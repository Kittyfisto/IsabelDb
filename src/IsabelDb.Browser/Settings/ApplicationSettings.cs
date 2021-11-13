using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using log4net;

namespace IsabelDb.Browser.Settings
{
	public sealed class ApplicationSettings
		: ICloneable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static Task _saveTask;
		private readonly MainWindowSettings _mainWindow;

		private readonly string _fileFolder;
		private readonly string _fileName;

		static ApplicationSettings()
		{
			_saveTask = Task.FromResult(result: 42);
		}

		private ApplicationSettings(ApplicationSettings other)
		{
			_fileName = other._fileName;
			_fileFolder = other._fileFolder;
			_mainWindow = other.MainWindow.Clone();
		}

		public ApplicationSettings(string fileName)
		{
			_fileName = Path.GetFullPath(fileName);
			_fileFolder = Path.GetDirectoryName(_fileName);
			_mainWindow = new MainWindowSettings();
		}

		public static ApplicationSettings Create()
		{
			string fileName = Path.Combine(Constants.LocalAppDataFolder, "settings");
			fileName += ".xml";
			return new ApplicationSettings(fileName);
		}

		public MainWindowSettings MainWindow => _mainWindow;

		public Task SaveAsync()
		{
			var config = Clone();
			return _saveTask = _saveTask.ContinueWith(unused => config.Save());
		}

		public void Restore()
		{
			bool unused;
			Restore(out unused);
		}

		/// <summary>
		/// </summary>
		/// <param name="neededPatching">Whether or not certain values need to be changed (for example due to upgrades to the format - it is advised that the current settings be saved again if this is set to true)</param>
		public void Restore(out bool neededPatching)
		{
			neededPatching = false;
			if (!File.Exists(_fileName))
				return;

			try
			{
				using (FileStream stream = File.OpenRead(_fileName))
				using (XmlReader reader = XmlReader.Create(stream))
				{
					while (reader.Read())
					{
						switch (reader.Name)
						{
							case "mainwindow":
								_mainWindow.Restore(reader);
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		private bool Save()
		{
			try
			{
				using (var stream = new MemoryStream())
				{
					var settings = new XmlWriterSettings
					{
						Indent = true,
						IndentChars = "  ",
						NewLineChars = "\r\n",
						NewLineHandling = NewLineHandling.Replace
					};
					using (var writer = XmlWriter.Create(stream, settings))
					{
						writer.WriteStartElement("xml");

						writer.WriteStartElement("mainwindow");
						_mainWindow.Save(writer);
						writer.WriteEndElement();

						writer.WriteEndElement();
					}

					if (!Directory.Exists(_fileFolder))
						Directory.CreateDirectory(_fileFolder);

					using (var file =
						new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
					{
						var length = (int) stream.Position;
						file.Write(stream.GetBuffer(), offset: 0, count: length);
						file.SetLength(length);
					}

					return true;
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return false;
			}
		}

		#region Implementation of ICloneable

		public ApplicationSettings Clone()
		{
			return new ApplicationSettings(this);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}