namespace IsabelDb.TypeModels
{
	internal sealed class PropertyDescription
	{
		private readonly TypeDescription _typeDescription;
		private readonly string _name;

		public PropertyDescription(TypeDescription typeDescription, string name)
		{
			_typeDescription = typeDescription;
			_name = name;
		}

		public TypeDescription TypeDescription => _typeDescription;

		public string Name => _name;
	}
}