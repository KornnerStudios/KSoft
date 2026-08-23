using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2e3f7664-9f4a-413b-9bc1-39401af0c665")]

[assembly: SuppressMessage("Microsoft.Design",
	"CA1303:DoNotPassLiteralsAsLocalizedParameters",
	Justification = "I'm not localizing this stuff, go away")]
[assembly: SuppressMessage("Microsoft.Design",
	"CA1707:IdentifiersShouldNotContainUnderscores",
	Justification="Because I do this all over the place in unit tests")]
[assembly: SuppressMessage("Performance",
	"CA1806:Do not ignore method results",
	Justification = "Tests intentionally invoke constructors and parsers through assertion delegates")]
[assembly: SuppressMessage("Performance",
	"CA1861:Avoid constant arrays as arguments",
	Justification = "Tests intentionally keep expectation arrays local to each test")]
