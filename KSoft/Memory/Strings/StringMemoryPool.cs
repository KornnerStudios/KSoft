using System;
using System.Collections.Generic;
using System.Text;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Memory.Strings
{
	/// <summary>Builds representations of unmanaged string pools</summary>
	/// <remarks>
	/// Equal (case-sensitive) strings will only ever appear once.
	/// 
	/// While this builds a representation of an unmanaged string pool, 
	/// the implementation is entirely "safe" and managed in .NET.
	/// 
	/// Call the explicit Read\Write methods to fragment where the respected 
	/// information is streamed. Otherwise use the default 
	/// <see cref="IO.IEndianStreamable"/> implementation to stream this class
	/// </remarks>
	public partial class StringMemoryPool : IO.IEndianStreamable, IO.IEndianStreamSerializable,
		ICollection<string>, IEnumerable<string>
	{
		/// <summary>Default amount of entry memory allocated for use</summary>
		const int kEntryStartCount = 64;
		/// <summary>Sentinel value of an invalid string address reference</summary>
		public static readonly Values.PtrHandle kInvalidReference = new Values.PtrHandle(ulong.MaxValue);


		/// <summary>Configuration instance data for this pool</summary>
		public StringMemoryPoolSettings Settings { get; private set; }

		List<string> mPool;
		List<Values.PtrHandle> mReferences;
		/// <remarks>Only created when <see cref="UseStringToIndex"/> is true</remarks>
		Dictionary<string, int> mStringToIndex;
		// null string offset starts off null in case the user doesn't want an implicit null
		Values.PtrHandle mNullReference = kInvalidReference;
		Text.StringStorageEncoding mEncoding;

		#region Count
		/// <summary>Get the number of strings in the pool</summary>
		public int Count { get { return mPool.Count; } }
		#endregion

		#region Size
		/// <summary>Total size in bytes of the pool</summary>
		public uint Size { get; private set; }

		/// <summary>Calculate how many bytes of storage <paramref name="value"/> will consume in the pool</summary>
		/// <param name="value"></param>
		/// <returns>Number of bytes <paramref name="value"/> will consume</returns>
		int CalculateStringByteLength(string value)
		{
			return mEncoding.GetByteCount(value);
		}
		#endregion

		bool UseStringToIndex { get { return !Settings.AllowDuplicates; } }

		void InitializeCollections(int capacity)
		{
			mPool = new List<string>(capacity);
			mReferences = new List<Values.PtrHandle>(capacity);
			if (UseStringToIndex)
				mStringToIndex = new Dictionary<string, int>(capacity, StringComparer.Ordinal);
		}
		/// <summary>Create a <see cref="StringMemoryPool"/> from a <see cref="StringMemoryPoolSettings"/> definition</summary>
		/// <param name="definition"></param>
		public StringMemoryPool(StringMemoryPoolSettings definition)
		{
			Settings = definition;
			InitializeCollections(kEntryStartCount);
			mEncoding = new Text.StringStorageEncoding(definition.Storage);
		}

		#region Add
		/// <summary>Takes a string and adds it to the pool</summary>
		/// <param name="str">value to add</param>
		/// <returns>address reference of the string</returns>
		/// <remarks>
		/// If <see cref="Configuration.AllowDuplicates"/> is NOT true, this will return an address 
		/// of a string which is equal to <paramref name="str"/>
		/// </remarks>
		public Values.PtrHandle Add(string str)
		{
			int index;

			if (string.IsNullOrEmpty(str))
			{
				if (Settings.ImplicitNull) // if we're setup to use a implicit null string, its always the first string in the pool
					return Settings.BaseAddress;

				if (mNullReference == kInvalidReference) // if not, check to see if a null string has been added yet
				{
					index = Count;
					this.AddInternal("");
					mNullReference = Settings.BaseAddress + mReferences[index];
				}
				return mNullReference;
			}

			// If we allow dups, we won't try to find a matching entry, we'll immediately add it.
			if (Settings.AllowDuplicates || !mStringToIndex.TryGetValue(str, out index))
			{
				index = Count;
				this.AddInternal(str);
			}

			return mReferences[index];
		}

		void AddInternal(string value)
		{
			if (UseStringToIndex)
				mStringToIndex.Add(value, mPool.Count);
			// the PtrHandle created will implicitly take after [BaseAddress]'s address size
			mReferences.Add(Settings.BaseAddress + Size);
			mPool.Add(value);

			Size += (uint)CalculateStringByteLength(value);
		}

		void AddFromRead(int index, string value)
		{
			if (UseStringToIndex)
				mStringToIndex.Add(value, index);
			mPool[index] = value;
		}
		#endregion

		#region Get
		/// <summary>
		/// Takes a string and gets the address it would have if the pool was located at
		/// <see cref="Configuration.BaseAddress"/> in memory
		/// </summary>
		/// <param name="value"></param>
		/// <returns>
		/// Address of <paramref name="value"/> in the pool or <see cref="kInvalidReference"/> 
		/// if there is no matching string
		/// </returns>
		/// <remarks>
		/// This method is not written to support configurations whose <see cref="Config.AllowDuplicates"/> 
		/// is set to true. The first instance will ALWAYS be returned.
		/// </remarks>
		[Contracts.Pure]
		public Values.PtrHandle GetAddress(string value)
		{
			int index;
			if (UseStringToIndex)
				mStringToIndex.TryGetValue(value, out index);
			else
				index = mPool.IndexOf(value);

			if (index.IsNone())
				return Settings.BaseAddress + mReferences[index];

			return kInvalidReference;
		}

		/// <summary>Get the reference index of the string at <paramref name="address"/></summary>
		/// <param name="address"></param>
		/// <returns>Index of <paramref name="address"/> or -1 if not found</returns>
		/// <remarks>Actually returns the index used internally tracking strings\offsets</remarks>
		[Contracts.Pure]
		/*public*/ int GetIndex(Values.PtrHandle address)
		{
			return mReferences.FindIndex(x => x == address);
		}

		/// <summary>Get the address of the 'null' string</summary>
		/// <returns></returns>
		public Values.PtrHandle GetNull() { return mNullReference; }

		/// <summary>Get the string thats located at <paramref name="address"/></summary>
		/// <param name="address"></param>
		/// <returns></returns>
		/// <remarks>
		/// Code contracts will cause an assert if the address doesn't start a new string
		/// </remarks>
		[Contracts.Pure]
		public string Get(Values.PtrHandle address)	{ return mPool[GetIndex(address)]; }
		/// <summary>Get the string thats located at <paramref name="address"/></summary>
		/// <param name="address"></param>
		/// <returns></returns>
		/// <remarks>
		/// Code contracts will cause an assert if the address doesn't start a new string
		/// </remarks>
		public string this[Values.PtrHandle address]	{ get { return Get(address); } }
		#endregion

		#region IEnumerable Members
		/// <summary>Get an enumerator that iterates through this pool's stored string values</summary>
		/// <returns></returns>
		public IEnumerator<string> GetEnumerator()										{ return mPool.GetEnumerator(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()	{ return mPool.GetEnumerator(); }
		/// <summary>Get an enumerator that iterates through this pool using address/string pairs</summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair<Values.PtrHandle, string>> GetKeyValueEnumerator()		{ return new KeyValueEnumerator(this); }
		#endregion

		#region IEndianStreamable/IEndianStreamSerializable Members
		/// <summary>Read the header for the pool from a stream to properly initialize this pool's configuration, counts, etc</summary>
		/// <param name="s"></param>
		public void ReadHeader(IO.EndianReader s)
		{
			Contract.Requires(s != null);
			// TODO: test to see if the config is a built-in, else we could overwrite an existing config
//			Configuration.Read(s);
			int count = s.ReadInt32();
			Size = s.ReadUInt32();

			InitializeCollections(count);
			for (int x = 0; x < mReferences.Count; x++)
				mReferences[x] = new Values.PtrHandle(Settings.AddressSize);
		}
		/// <summary>
		/// Write the header for this pool to a stream for future re-initializing 
		/// and usage, storing things such as the configuration, count, etc
		/// </summary>
		/// <param name="s"></param>
		public void WriteHeader(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

//			Configuration.Write(s);
			s.Write(Count);
			s.Write(Size);
		}

		int[] ioStringLengths;
		/// <summary>Read the character count for the string values from a stream</summary>
		/// <param name="s"></param>
		/// <remarks>
		/// Obviously, this needs to be called before <see cref="ReadStrings(IO.EndianReader s)"/> 
		/// if you're going to even use this (due to performance reasons or due to the storage definition)
		/// </remarks>
		public void ReadStringCharacterLengths(IO.EndianReader s)
		{
			Contract.Requires(s != null);

			ioStringLengths = new int[Count];
			for (int x = 0; x < ioStringLengths.Length; x++)
				ioStringLengths[x] = s.ReadInt32();
		}
		/// <summary>Write the character count for the string values to a stream</summary>
		/// <param name="s"></param>
		public void WriteStringCharacterLengths(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

			foreach (string str in mPool)
				s.Write(str.Length);
		}
		/// <summary>Only used for Interop situations where explicit lengths are needed for cases (like enumeration) in unmanaged code</summary>
		/// <param name="s"></param>
		public void WriteStringByteLengths(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

			foreach (string str in mPool)
				s.Write(CalculateStringByteLength(str));
		}

		/// <summary>Read the string addresses from a stream</summary>
		/// <param name="s"></param>
		public void ReadReferences(IO.EndianReader s)
		{
			Contract.Requires(s != null);

			for (int x = 0; x < mReferences.Count; x++)
				mReferences[x].Read(s);
		}
		/// <summary>Write the string addresses to a stream</summary>
		/// <param name="s"></param>
		public void WriteReferences(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

			foreach (Values.PtrHandle r in mReferences)
				r.Write(s);
		}

		/// <summary>Read the string values from a stream</summary>
		/// <param name="s"></param>
		public void ReadStrings(IO.EndianReader s)
		{
			Contract.Requires(s != null);

			if (ioStringLengths == null)
				for (int x = 0; x < mPool.Count; x++)
					AddFromRead(x, s.ReadString(mEncoding));
			else
			{
				for (int x = 0; x < Count; x++)
					AddFromRead(x, s.ReadString(mEncoding, ioStringLengths[x]));
				ioStringLengths = null;
			}
		}
		/// <summary>Write the string values to a stream</summary>
		/// <param name="s"></param>
		public void WriteStrings(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

			foreach (string str in mPool)
				s.Write(str, mEncoding);
		}

		/// <summary>Read a <see cref="StringMemoryPool"/> from a stream</summary>
		/// <param name="s"></param>
		/// <remarks>
		/// Stream order:
		/// <see cref="ReadHeader(IO.EndianReader)"/>
		/// <see cref="ReadReferences(IO.EndianReader)"/>
		/// <see cref="ReadStrings(IO.EndianReader)"/>
		/// </remarks>
		public void Read(IO.EndianReader s)
		{
			ReadHeader(s);
			ReadReferences(s);
			ReadStrings(s);
		}
		/// <summary>Write this <see cref="StringMemoryPool"/> to a stream</summary>
		/// <param name="s"></param>
		/// <remarks>
		/// Stream order:
		/// <see cref="WriteHeader(IO.EndianWriter)"/>
		/// <see cref="WriteReferences(IO.EndianWriter)"/>
		/// <see cref="WriteStrings(IO.EndianWriter)"/>
		/// </remarks>
		public void Write(IO.EndianWriter s)
		{
			WriteHeader(s);
			WriteReferences(s);
			WriteStrings(s);
		}

		public void SerializeHeader(IO.EndianStream s)
		{
				 if (s.IsReading) ReadHeader(s.Reader);
			else if (s.IsWriting) WriteHeader(s.Writer);
		}
		public void SerializeStringCharacterLengths(IO.EndianStream s)
		{
				 if (s.IsReading) ReadStringCharacterLengths(s.Reader);
			else if (s.IsWriting) WriteStringCharacterLengths(s.Writer);
		}
		public void SerializeReferences(IO.EndianStream s)
		{
				 if (s.IsReading) ReadReferences(s.Reader);
			else if (s.IsWriting) WriteReferences(s.Writer);
		}
		public void SerializeStrings(IO.EndianStream s)
		{
				 if (s.IsReading) ReadStrings(s.Reader);
			else if (s.IsWriting) WriteStrings(s.Writer);
		}
		public void Serialize(IO.EndianStream s)
		{
				 if (s.IsReading) Read(s.Reader);
			else if (s.IsWriting) Write(s.Writer);
		}
		#endregion

		#region ICollection<string> Members
		void ICollection<string>.Add(string item)						{ var handle = Add(item); }
		void ICollection<string>.Clear()								{ throw new NotSupportedException("Can't clear items from a StringMemoryPool"); }
		public bool Contains(string item)								{ return UseStringToIndex ? mStringToIndex.ContainsKey(item) : mPool.Contains(item); }
		void ICollection<string>.CopyTo(string[] array, int arrayIndex)	{ mPool.CopyTo(array, arrayIndex); }
		bool ICollection<string>.IsReadOnly								{ get { return false; } }

		bool ICollection<string>.Remove(string item)					{ throw new NotSupportedException("Can't remove items from a StringMemoryPool"); }
		#endregion
	};
}