using System;

namespace KSoft
{
	public static partial class TypeExtensionsTagElementStreams
	{
		/// <summary>Does the requested type require an associated name in the element stream?</summary>
		/// <param name="nodeType"></param>
		/// <returns></returns>
		public static bool RequiresName(this IO.TagElementNodeType nodeType)
		{
			return nodeType != IO.TagElementNodeType.Text; // aka, Cursor
		}

		#region TagElementStreamFormat
		public static IO.TagElementStreamFormat GetBaseFormat(this IO.TagElementStreamFormat format)
		{
			return format & ~IO.TagElementStreamFormat.kTypeFlags;
		}
		public static IO.TagElementStreamFormat GetTypeFlags(this IO.TagElementStreamFormat format)
		{
			return format & IO.TagElementStreamFormat.kTypeFlags;
		}
		public static bool IsText(this IO.TagElementStreamFormat format)
		{
			return (format & IO.TagElementStreamFormat.Binary) == 0;
		}
		public static bool IsBinary(this IO.TagElementStreamFormat format)
		{
			return (format & IO.TagElementStreamFormat.Binary) != 0;
		}
		#endregion

		public static void Serialize<TDoc, TCursor, TContext>(this Collections.BitSet @this,
			IO.TagElementStream<TDoc, TCursor, string> s,
			string elementName,
			TContext ctxt,
			IO.TagElementStreamDefaultSerializer.SerializeBitToTagElementStreamDelegate<TDoc, TCursor, TContext> streamElement,
			int highestBitIndex = TypeExtensions.kNoneInt32)
			where TDoc : class
			where TCursor : class
		{
			ArgumentNullException.ThrowIfNull(@this);
			ArgumentNullException.ThrowIfNull(s);
			ArgumentNullException.ThrowIfNull(streamElement);
			if (!highestBitIndex.IsNoneOrPositive())
			{
				throw new ArgumentOutOfRangeException(nameof(highestBitIndex), highestBitIndex,
					"Highest bit index must be None or positive.");
			}
			if (highestBitIndex >= @this.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(highestBitIndex), highestBitIndex,
					"Highest bit index must be less than the bit set length.");
			}

			IO.TagElementStreamDefaultSerializer.Serialize(@this, s, elementName,
				ctxt, streamElement,
				highestBitIndex);
		}
	};
};

namespace KSoft.IO
{
	public static partial class TagElementStreamDefaultSerializer
	{
	};
};
