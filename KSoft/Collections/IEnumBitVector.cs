using System;

namespace KSoft.Collections;

/// <summary>A type-erased adapter for a fixed vector's enum metadata and stored bits.</summary>
/// <remarks>KSoft's fixed-vector implementations are boxed snapshots through this interface. <see cref="WithBit"/> returns a replacement; it does not update the original box or a property holding it. Enum membership and physical bit reads are intentionally distinct.</remarks>
public interface IEnumBitVector
{
	/// <summary>Gets the associated bit-index enum type.</summary>
	/// <remarks>KSoft's implementations validate the enum domain and its fit in the vector width.</remarks>
	/// <exception cref="ArgumentException">The enum domain is invalid or exceeds the vector width.</exception>
	Type BitsEnumType { get; }

	/// <summary>Gets the physical vector width, not the number of declared members.</summary>
	/// <remarks>Also exposed as the fixed vector's public <c>Length</c> property; no enum validation is performed.</remarks>
	int Length { get; }

	/// <summary>Reports whether an index is a usable member of the associated enum domain.</summary>
	/// <param name="bitIndex">The numeric index to query.</param>
	/// <returns><see langword="true"/> for a usable declared index; otherwise <see langword="false"/>.</returns>
	/// <remarks>This is an enum-membership query, not a physical-width or UI-visibility check.</remarks>
	/// <exception cref="ArgumentException">The enum domain is invalid.</exception>
	bool IsDefinedIndex(int bitIndex);

	/// <summary>Gets the canonical C# name for a usable declared enum index.</summary>
	/// <param name="bitIndex">The declared numeric index.</param>
	/// <returns>The first ordinary declared name for the index.</returns>
	/// <remarks>This does not return a display label or validate the vector's physical width.</remarks>
	/// <exception cref="ArgumentException">The enum domain is invalid.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is not a usable declared index.</exception>
	string GetBitName(int bitIndex);

	/// <summary>Reads a physical bit from zero to <see cref="Length"/> exclusive.</summary>
	/// <param name="bitIndex">The physical bit position.</param>
	/// <returns>The stored state of the bit.</returns>
	/// <remarks>A declaration for this position is not required. This operation does not validate the enum domain.</remarks>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is outside the physical vector width.</exception>
	bool GetBit(int bitIndex);

	/// <summary>Returns a replacement snapshot with one usable declared bit updated.</summary>
	/// <param name="bitIndex">The usable declared bit position.</param>
	/// <param name="value">The new bit state.</param>
	/// <returns>An updated boxed value, leaving this snapshot unchanged.</returns>
	/// <remarks>Validates the enum domain, vector width, and declared index before updating a copy. Reading an unnamed position with <see cref="GetBit"/> does not make it writable through this method.</remarks>
	/// <exception cref="ArgumentException">The enum domain is invalid or exceeds the vector width.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bitIndex"/> is not a usable declared index.</exception>
	IEnumBitVector WithBit(int bitIndex, bool value);
}
