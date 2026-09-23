namespace KSoft.WPF
{
	/// <summary>Indexed presentation metadata for bit controls, independent of stored bit values.</summary>
	/// <remarks>Implementations must supply consistent data for indices from zero to <see cref="NumberOfBits"/> exclusive. This interface does not imply immutability, change notifications, or complete validation of custom implementations by a control.</remarks>
	public interface IBitVectorUserInterfaceData
	{
		/// <summary>Gets the exclusive metadata-index bound, not the visible-item count or vector width.</summary>
		/// <remarks>Built-in factories can trim trailing absent slots; hidden slots may remain, and the bound can be zero.</remarks>
		int NumberOfBits { get; }

		/// <summary>Gets the presentation label at an index inside the metadata bound.</summary>
		/// <param name="bitIndex">An index from zero to <see cref="NumberOfBits"/> exclusive.</param>
		/// <returns>The presentation label.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Built-in implementations reject an index outside their metadata bound.</exception>
		string GetDisplayName(int bitIndex);

		/// <summary>Gets the description at an index inside the metadata bound.</summary>
		/// <param name="bitIndex">An index from zero to <see cref="NumberOfBits"/> exclusive.</param>
		/// <returns>The description, or empty text when none is supplied by the source.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Built-in implementations reject an index outside their metadata bound.</exception>
		string GetDescription(int bitIndex);

		/// <summary>Reports presentation visibility, not enum membership or the bit's stored state.</summary>
		/// <param name="bitIndex">An index from zero to <see cref="NumberOfBits"/> exclusive.</param>
		/// <returns>Whether the source requests a visible item.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Built-in implementations reject an index outside their metadata bound.</exception>
		bool IsVisible(int bitIndex);
	};
}
