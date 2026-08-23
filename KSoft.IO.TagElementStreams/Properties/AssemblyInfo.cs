using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("bfbc32b7-e2e1-4560-93ff-d86baf216b0c")]

[assembly: SuppressMessage("Microsoft.Design",
	"CA1062:ValidateArgumentsOfPublicMethods",
	Justification = "Explicit guard clauses validate public inputs where needed")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1303:DoNotPassLiteralsAsLocalizedParameters")]
[assembly: SuppressMessage("Design",
	"CA1033:Interface methods should be callable by child types",
	Justification = "Explicit interface implementations intentionally preserve stream serialization contracts")]
