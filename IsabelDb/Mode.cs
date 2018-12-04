using System;

namespace IsabelDb
{
	[Flags]
	internal enum Mode
	{
		Get     = 0x01,
		Create  = 0x02
	}
}