namespace IsabelDb
{
	/// <summary>
	/// 
	/// </summary>
	public struct Rectangle2D
	{
		/// <summary>
		/// 
		/// </summary>
		public double MinX;

		/// <summary>
		/// 
		/// </summary>
		public double MinY;

		/// <summary>
		/// 
		/// </summary>
		public double MaxX;

		/// <summary>
		/// 
		/// </summary>
		public double MaxY;

		#region Overrides of ValueType

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("MinX: {0}, MinY: {1}, MaxX: {2}, MaxY: {3}", MinX, MinY, MaxX, MaxY);
		}

		#endregion
	}
}