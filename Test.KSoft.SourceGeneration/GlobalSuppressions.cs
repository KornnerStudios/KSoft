using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance",
	"CA1861:Avoid constant arrays as arguments",
	Justification = "Generator tests intentionally keep expectation arrays local to each test")]
