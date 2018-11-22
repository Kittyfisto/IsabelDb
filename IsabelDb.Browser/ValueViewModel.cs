namespace IsabelDb.Browser
{
	public sealed class ValueViewModel<T>
		: IValueViewModel
	{
		private readonly T _value;
		private readonly string _preview;

		public ValueViewModel(T value)
		{
			_value = value;
			_preview = value.ToString();
		}

		#region Implementation of IValueViewModel

		public string Preview => _preview;

		#endregion
	}
}