using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Debug.Test;

[TestClass]
public sealed class TraceSourceRegistryTest
{
	[TestMethod]
	public void Register_SameInstanceIsIdempotent()
	{
		var registry = new TraceSourceRegistry();
		var source = new TraceSource("Registry.Idempotent", SourceLevels.All);
		try
		{
			registry.Register(source);
			registry.Register(source);

			Assert.AreSame(source, registry.Resolve(source.Name));
		}
		finally
		{
			source.Close();
		}
	}

	[TestMethod]
	public void Register_DifferentInstanceWithSameNameThrows()
	{
		var registry = new TraceSourceRegistry();
		var first = new TraceSource("Registry.Collision", SourceLevels.All);
		var second = new TraceSource("Registry.Collision", SourceLevels.All);
		try
		{
			registry.Register(first);
			var exception = Assert.ThrowsExactly<InvalidOperationException>(() => registry.Register(second));
			StringAssert.Contains(exception.Message, first.Name);
		}
		finally
		{
			first.Close();
			second.Close();
		}
	}

	[TestMethod]
	public void Register_BatchCollisionIsRejectedAtomically()
	{
		var registry = new TraceSourceRegistry();
		var first = new TraceSource("Registry.BatchCollision", SourceLevels.All);
		var second = new TraceSource("Registry.BatchCollision", SourceLevels.All);
		try
		{
			Assert.ThrowsExactly<InvalidOperationException>(() => registry.Register(first, second));
			Assert.ThrowsExactly<InvalidOperationException>(() => registry.Resolve(first.Name));
		}
		finally
		{
			first.Close();
			second.Close();
		}
	}

	[TestMethod]
	public void Resolve_UsesExactThenLongestDotDelimitedPrefix()
	{
		var registry = new TraceSourceRegistry();
		var root = new TraceSource("Registry", SourceLevels.All);
		var child = new TraceSource("Registry.Child", SourceLevels.All);
		try
		{
			registry.Register(root, child);

			Assert.AreSame(child, registry.Resolve("Registry.Child"));
			Assert.AreSame(child, registry.Resolve("Registry.Child.Grandchild"));
			Assert.AreSame(root, registry.Resolve("Registry.Other"));
			Assert.ThrowsExactly<InvalidOperationException>(() => registry.Resolve("RegistryChild"));
		}
		finally
		{
			root.Close();
			child.Close();
		}
	}
}
