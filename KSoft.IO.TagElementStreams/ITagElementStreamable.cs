namespace KSoft.IO
{
	public interface ITagElementStreamable<TName>
	{
		void Serialize<TDoc, TCursor>(TagElementStream<TDoc, TCursor, TName> s)
			where TDoc : class
			where TCursor : class;
	};


	public interface ITagElementStringNameStreamable : ITagElementStreamable<string>
	{
	};
}