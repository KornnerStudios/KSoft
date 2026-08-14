using System;
using System.Linq;
using System.Reflection;
using KSoft.SourceGeneration.Descriptors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.KSoft.SourceGeneration;

/// <summary>
/// Guards descriptor ownership boundaries so humans and agents do not move domain semantics into shared catalogs.
/// </summary>
[TestClass]
public sealed class DescriptorOwnershipTests
{
	/// <summary>
	/// Verifies that <see cref="PrimitiveCatalog" /> stays limited to canonical primitive facts.
	/// </summary>
	/// <remarks>
	/// This is intentionally a guardrail test for future refactors by people or agents: BitStream-specific streamable
	/// groups belong with the BitStream generator even though they are composed from shared primitive descriptors.
	/// </remarks>
	[TestMethod]
	public void PrimitiveCatalogDoesNotOwnBitStreamSemanticGroupsTest()
	{
		var propertyNames = typeof(PrimitiveCatalog)
			.GetProperties(BindingFlags.Public | BindingFlags.Static)
			.Select(static x => x.Name)
			.ToArray();

		Assert.IsFalse(
			propertyNames.Any(static x => x.Contains("BitStream", StringComparison.Ordinal)),
			"BitStream-specific streamable type groups belong in the BitStream generator, not PrimitiveCatalog.");
	}
}
