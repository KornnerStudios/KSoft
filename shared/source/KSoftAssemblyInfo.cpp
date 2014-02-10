#include "Precompile.hpp"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

[assembly:AssemblyConfiguration(
#if DEBUG
	"Debug"
#else
	"Release"
#endif
)];
[assembly:AssemblyCompany("Kornner Studios ©  2005 - 2014")];

[assembly:AssemblyCopyright("Copyright (c)  2014")];
[assembly:AssemblyTrademark("")];
[assembly:AssemblyCulture("")];

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the value or you can default the Revision and Build Numbers
// by using the '*' as shown below:

[assembly:AssemblyVersion("0.0.*")];

[assembly:CLSCompliant(true)];

[assembly:SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];