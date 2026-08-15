using System;
using System.IO;
using System.Xml;

namespace KSoft.Xml
{
	// based on: http://g-m-a-c.blogspot.com/2013/11/determine-exact-position-of-xmlreader.html

	static class XmlReaderStreamOffsetCalculator
	{
		#region StreamReader util
		const string kStreamReader_BufferLengthPropName = "ByteLen_Prop";
		const string kStreamReader_BufferPositionPropName = "CharPos_Prop";
		const string kStreamReader_DefaultBufferSizeFieldName = "DefaultBufferSize";

		static readonly Lazy<Func<StreamReader, int>> kStreamReader_BufferLengthGet =
			new(() => Reflection.Util.GenerateMemberGetter<StreamReader, int>(kStreamReader_BufferLengthPropName));
		static readonly Lazy<Func<StreamReader, int>> kStreamReader_BufferPositionGet =
			new(() => Reflection.Util.GenerateMemberGetter<StreamReader, int>(kStreamReader_BufferPositionPropName));

		static readonly Lazy<int> kStreamReader_DefaultBufferSize =
			new(() => Reflection.Util.GenerateStaticFieldGetter<StreamReader, int>(kStreamReader_DefaultBufferSizeFieldName)());

		static int GetBufferLength(StreamReader s)
		{
			return kStreamReader_BufferLengthGet.Value(s);
		}
		static int GetBufferPosition(StreamReader s)
		{
			return kStreamReader_BufferPositionGet.Value(s);
		}
		static int GetPreambleLength(StreamReader s)
		{
			return s.CurrentEncoding.GetPreamble().Length;
		}
		#endregion

		#region XmlTextReaderImpl util
		const string kTextReaderImpl_BufferLengthPropName = "DtdParserProxy_ParsingBufferLength";
		const string kTextReaderImpl_BufferPositionPropName = "DtdParserProxy_CurrentPosition";

		static readonly Lazy<Func<XmlReader, int>> kTextReaderImpl_BufferLengthGet =
			new(() => Reflection.Util.GenerateMemberGetter<XmlReader, int>(kTextReaderImpl_BufferLengthPropName));
		static readonly Lazy<Func<XmlReader, int>> kTextReaderImpl_BufferPositionGet =
			new(() => Reflection.Util.GenerateMemberGetter<XmlReader, int>(kTextReaderImpl_BufferPositionPropName));

		static int GetBufferLength(XmlReader s)
		{
			return kTextReaderImpl_BufferLengthGet.Value(s);
		}
		static int GetBufferPosition(XmlReader s)
		{
			return kTextReaderImpl_BufferPositionGet.Value(s);
		}
		#endregion

		public static long GetPosition(this XmlReader xmlReader, StreamReader underlyingStreamReader)
		{
			ArgumentNullException.ThrowIfNull(xmlReader);
			ArgumentNullException.ThrowIfNull(underlyingStreamReader);
			if (xmlReader.GetType().Name != "XmlTextReaderImpl")
			{
				throw new InvalidOperationException();
			}

			// get the 'base' position from the root stream
			long stream_position = underlyingStreamReader.BaseStream.Position;

			// get the underlying stream's buffer state and text encoding preamble
			var stream_buffer_length = GetBufferLength(underlyingStreamReader);
			var stream_buffer_pos = GetBufferPosition(underlyingStreamReader);
			var stream_preamble_length = GetPreambleLength(underlyingStreamReader);

			// get the xml reader's buffer state
			var xml_buffer_length = GetBufferLength(xmlReader);
			var xml_buffer_pos = GetBufferPosition(xmlReader);

			// subtract the lengths of the buffers which the stream/xml readers cached
			// then add the 'cursor' positions the readers have in those buffers
			// plus the text encoding preamble length
			long pos = stream_position
				- (stream_buffer_length == kStreamReader_DefaultBufferSize.Value ? kStreamReader_DefaultBufferSize.Value : 0)
				- xml_buffer_length
				+ xml_buffer_pos + stream_buffer_pos + stream_preamble_length;

			return pos;
		}
	};
}