using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft
{
	[TestClass] // required for AssemblyInitialize & AssemblyCleanup to work
	public // VS2017 this started: UTA001: TestClass attribute defined on non-public class KSoft.TestLibrary
	static partial class TestLibrary
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			KSoft.Program.Initialize();
		}
		[AssemblyCleanup]
		public static void AssemblyDispose()
		{
			KSoft.Program.Dispose();
		}
	};

	[TestClass]
	public abstract class BaseTestClass
	{
		/// <summary>
		/// Gets or sets the test context which provides information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }
	};

	static class TestExtentions
	{
		public static bool NextBoolean(this Random rand)
		{
			return rand.Next(1) == 1;
		}
	};
}