namespace IsabelDb.Browser.Inspectors
{
	public sealed class ObjectInspectorViewModel
	{
		private readonly object _value;
		private string _formattedValue;

		public ObjectInspectorViewModel(object value)
		{
			if (value is ValueViewModel valueViewModel)
			{
				_value = valueViewModel.Value;
			}
			else
			{
				_value = value;
			}
		}

		public string FormattedValue
		{
			get
			{
				if (_formattedValue == null)
				{
					_formattedValue = new ObjectFormatter().Format(_value);
				}

				return _formattedValue;
			}
		}
	}
}