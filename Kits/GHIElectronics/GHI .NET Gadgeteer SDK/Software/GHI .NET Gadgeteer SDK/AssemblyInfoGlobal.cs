// See the Kit template Readme.txt file for information on how to use this file to make 
// keeping version numbers of many .NET Gadgeteer modules/mainboards updated and in sync much easier.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// These numbers must be changed whenever a new version of ANY of the modules/mainboards using this AssemblyInfoGlobal.cs file is made.
// Suggestion: Use X.Y.Z.0 where X.Y.Z is the same as the installer version found in version.wxi
[assembly: AssemblyFileVersion("1.6.2.0")]
[assembly: AssemblyInformationalVersion("1.6.2.0")]

// NB the AssemblyVersion is deliberately not included in this file, since it should be kept constant unless the API of the module/mainboard
// changes significantly - otherwise assemblies relying on the mainboard/module assembly will need to be recompiled.