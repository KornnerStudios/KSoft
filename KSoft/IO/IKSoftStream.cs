
namespace KSoft.IO
{
	/// <summary> </summary>
	/// <remarks>Currently we only want this to provide query members (no setters)</remarks>
	public interface IKSoftStream
	{
		/// <summary>Owner of this stream</summary>
		object Owner { get; set; }

		object UserData { get; set; }

		string StreamName { get; }
	};
}