using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft
{
	[TestClass] // required for AssemblyInitialize & AssemblyCleanup to work
	public // VS2017 this started: UTA001: TestClass attribute defined on non-public class KSoft.TestLibrary
	static partial class TestLibrary
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			[SuppressMessage("Microsoft.Design", "IDE0060:ReviewUnusedParameters")]
			TestContext context)
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

		protected static TException AssertThrowsExactly<TException>(
			string parameterName,
			Action action)
			where TException : ArgumentException
		{
			var exception = Assert.ThrowsExactly<TException>(action);
			Assert.AreEqual(parameterName, exception.ParamName);
			return exception;
		}

		protected static TException AssertThrows<TException>(
			string parameterName,
			Action action)
			where TException : ArgumentException
		{
			var exception = Assert.Throws<TException>(action);
			Assert.AreEqual(parameterName, exception.ParamName);
			return exception;
		}

		protected static ArgumentNullException AssertThrowsArgumentNull(
			string parameterName,
			Action action) =>
			AssertThrowsExactly<ArgumentNullException>(parameterName, action);

		protected static ArgumentOutOfRangeException AssertThrowsArgumentOutOfRange(
			string parameterName,
			Action action) =>
			AssertThrowsExactly<ArgumentOutOfRangeException>(parameterName, action);

		protected static ArgumentException AssertThrowsArgument(
			string parameterName,
			Action action) =>
			AssertThrowsExactly<ArgumentException>(parameterName, action);

		protected static InvalidOperationException AssertThrowsInvalidStreamMode(Action action)
		{
			var exception = Assert.ThrowsExactly<InvalidOperationException>(action);
			Assert.AreEqual("Stream doesn't support the requested access mode", exception.Message);
			return exception;
		}
	};

	static class TestExtentions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Test helper randomness does not require cryptographic security.")]
		public static bool NextBoolean(this Random rand)
		{
			return rand.Next(1) == 1;
		}
	};
}
