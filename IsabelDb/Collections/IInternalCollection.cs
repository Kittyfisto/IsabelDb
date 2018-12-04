namespace IsabelDb.Collections
{
	internal interface IInternalCollection
		: ICollection
	{
		string TableName { get; }

		void MarkAsDropped();
		void UnnmarkAsDropped();
	}
}