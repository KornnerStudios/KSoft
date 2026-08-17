namespace KSoft.IO
{
	public interface IEndianStreamSerializable
	{
		void Serialize(EndianStream s);
	};
}