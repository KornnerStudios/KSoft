using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class AssemblyTraceSourcesCollectorTest : BaseTestClass
{
	static readonly TraceSource kMainTraceSource = new("Main");
	static readonly TraceSource kSecondaryTraceSource = new("Secondary");

	static class SampleDebugTraceSources
	{
		public static TraceSource Main => kMainTraceSource;
		public static TraceSource Secondary => kSecondaryTraceSource;
		public static string NotATraceSource => "ignored";
	}

	[TestMethod]
	public void FromClass_CollectsPublicStaticTraceSources()
	{
		var sources = AssemblyTraceSourcesCollector.FromClass(typeof(SampleDebugTraceSources));

		Assert.AreEqual(2, sources.Count);
		Assert.IsTrue(sources.Contains(kMainTraceSource));
		Assert.IsTrue(sources.Contains(kSecondaryTraceSource));
	}

	[TestMethod]
	public void FromClasses_NullArgumentsThrowExpectedExceptions()
	{
		var classException = Assert.ThrowsExactly<ArgumentNullException>(
			() => AssemblyTraceSourcesCollector.FromClass(null!));
		Assert.AreEqual("debugTraceClass", classException.ParamName);

		var classesException = Assert.ThrowsExactly<ArgumentNullException>(
			() => AssemblyTraceSourcesCollector.FromClasses(null, (Type[])null!));
		Assert.AreEqual("debugTraceClasses", classesException.ParamName);
	}
}
