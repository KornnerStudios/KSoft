using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("KSoft.Security")]
[assembly: AssemblyDescription("")]

[assembly: AssemblyProduct("KSoft.Security")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("579b04cd-a819-422a-bc8e-f78afae580f8")]

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
[assembly: AssemblyVersion("0.0.*")]

[assembly: SuppressMessage("Microsoft.Design",
	"CA1028:EnumStorageShouldBeInt32")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1034:NestedTypesShouldNotBeVisible",
	Justification = "Because I do this all over the place")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1062:ValidateArgumentsOfPublicMethods",
	Justification = "CodeContracts generally handle this already")]
