﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("IsabelDb")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("IsabelDb")]
[assembly: AssemblyCopyright("Copyright © Simon Mießler 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(visibility: false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b413ad3c-5410-4b84-9218-7db81fc95213")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.6.0.0")]
[assembly: AssemblyFileVersion("0.6.0.0")]


#if DEBUG
[assembly: InternalsVisibleTo("IsabelDb.Test")]
[assembly: InternalsVisibleTo("IsabelDb.Browser")]
#else
[assembly: InternalsVisibleTo("IsabelDb.Test,PublicKey=00240000048000009400000006020000002400005253413100040000010001006d873a2f2f5d54" +
	"280b91e8a2b6997fbe287f0631db99675716fbd9ded5ae79276ec77851fbe7be4e975bae1bc1d6" +
	"dcc76d4e00ab7dbba236f2c2e842310cc6b842ae0785afd969bf0b2fc79b5a902cf0e7278dbf33" +
	"00e9158b2693d209dfda4670b3ef8f660b7bc7be6028bcef1665f4aaaa8cc6851d36968210ea77" +
	"1db7ebdb")]
[assembly: InternalsVisibleTo("IsabelDb.Browser,PublicKey=00240000048000009400000006020000002400005253413100040000010001006d873a2f2f5d54" +
	"280b91e8a2b6997fbe287f0631db99675716fbd9ded5ae79276ec77851fbe7be4e975bae1bc1d6" +
	"dcc76d4e00ab7dbba236f2c2e842310cc6b842ae0785afd969bf0b2fc79b5a902cf0e7278dbf33" +
	"00e9158b2693d209dfda4670b3ef8f660b7bc7be6028bcef1665f4aaaa8cc6851d36968210ea77" +
	"1db7ebdb")]
#endif