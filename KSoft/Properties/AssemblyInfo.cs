using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a275205d-87c9-4fbb-bbea-04a90aea06cf")]

[assembly: InternalsVisibleTo("KSoft.IO.TagElementStreams")]
[assembly: InternalsVisibleTo("KSoft.Security")]
[assembly: InternalsVisibleTo("Test.KSoft.BCL")]

[assembly: SuppressMessage("Microsoft.Design",
	"CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1028:EnumStorageShouldBeInt32")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1034:NestedTypesShouldNotBeVisible",
	Justification = "Because I do this all over the place")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1062:ValidateArgumentsOfPublicMethods",
	Justification = "Explicit guard clauses validate public inputs where needed")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1303:DoNotPassLiteralsAsLocalizedParameters")]
[assembly: SuppressMessage("Design",
	"CA1033:Interface methods should be callable by child types",
	Justification = "Explicit interface implementations intentionally preserve the StringMemoryPool surface")]

[assembly: SuppressMessage("Style",
	"IDE1005:Delegate invocation can be simplified.",
	Justification = "Can't breakpoint simplification")]
