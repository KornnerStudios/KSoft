using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	using BitSet = Collections.BitSet;

	public partial class TagElementStreamDefaultSerializer
	{
		public delegate int SerializeBitToTagElementStreamDelegate<TDoc, TCursor, TContext>(
			TagElementStream<TDoc, TCursor, string> s, BitSet bitset, int bitIndex, TContext ctxt)
			where TDoc : class
			where TCursor : class;

		public static void Serialize<TDoc, TCursor, TContext>(BitSet @this,
			TagElementStream<TDoc, TCursor, string> s,
			string elementName,
			TContext ctxt, SerializeBitToTagElementStreamDelegate<TDoc, TCursor, TContext> streamElement,
			int highestBitIndex = TypeExtensions.kNoneInt32)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(streamElement != null);
			Contract.Requires(highestBitIndex.IsNoneOrPositive());
			Contract.Requires(highestBitIndex < @this.Length);

			if (highestBitIndex.IsNone())
				highestBitIndex = @this.Length - 1;

			if (s.IsReading)
			{
				int bit_index = 0;
				foreach (var node in s.ElementsByName(elementName))
					using (s.EnterCursorBookmark(node))
					{
						bit_index = streamElement(s, @this, TypeExtensions.kNoneInt32, ctxt);
						if (bit_index.IsNone())
							s.ThrowReadException(new System.IO.InvalidDataException(string.Format(
								"Element is not a valid {0} value", elementName)));

						@this[bit_index] = true;
					}
			}
			else if (s.IsWriting)
			{
				foreach (int bit_index in @this.SetBitIndices)
				{
					if (bit_index > highestBitIndex)
						break;

					using (s.EnterCursorBookmark(elementName))
						streamElement(s, @this, bit_index, ctxt);
				}
			}
		}
	};
}