namespace KSoft.IO
{
	/// <summary>Interfaces object serialization with the endian streams</summary>
	public interface IEndianStreamable
	{
		/// <summary>Reads the object from the endian stream object</summary>
		/// <param name="s"></param>
		void Read(EndianReader s);
		/// <summary>Writes the object to the endian stream object</summary>
		/// <param name="s"></param>
		void Write(EndianWriter s);
	};
}