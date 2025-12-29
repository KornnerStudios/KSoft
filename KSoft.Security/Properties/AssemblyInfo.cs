using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("579b04cd-a819-422a-bc8e-f78afae580f8")]

[assembly: SuppressMessage("Microsoft.Design",
	"CA1028:EnumStorageShouldBeInt32")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1034:NestedTypesShouldNotBeVisible",
	Justification = "Because I do this all over the place")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1062:ValidateArgumentsOfPublicMethods",
	Justification = "CodeContracts generally handle this already")]
