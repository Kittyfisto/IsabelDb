namespace IsabelDb.Browser
{
	public sealed class ValueViewModel
		: IValueViewModel
	{
		private readonly object _value;
		private string _preview;

		public ValueViewModel(object value)
		{
			_value = value;
		}

		#region Implementation of IValueViewModel

		public string Preview
		{
			get
			{
				if (_preview == null)
					_preview = new ObjectFormatter().Preview(_value, maximumLength: 128);
				return _preview;
			}
		}

		#endregion
	}
}