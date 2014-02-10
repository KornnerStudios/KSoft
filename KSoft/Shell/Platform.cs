using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Shell
{
	/// <summary>Represents a platform definition</summary>
	/// <remarks>Currently only this class can define the various platform definitions</remarks>
	public struct Platform :
		IComparer<Platform>, System.Collections.IComparer,
		IComparable<Platform>, IComparable,
		IEquatable<Platform>
	{
		#region Type
		PlatformType mType;
		/// <summary>This platform's type</summary>
		public PlatformType Type { get { return mType; } }
		#endregion

		#region ProcessorType
		Processor mProcessorType;
		/// <summary>This platform's normal processor type</summary>
		public Processor ProcessorType { get { return mProcessorType; } }
		#endregion

		/// <summary>Construct a platform definition</summary>
		/// <param name="platformType">Operating system of the platform</param>
		/// <param name="processorType">Processor definition of the platform</param>
		Platform(PlatformType platformType, Processor processorType)
		{
			mType = platformType;
			mProcessorType = processorType;
		}

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is Platform)
				return Platform.StaticEquals(this, (Platform)obj);

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode()
		{
			const int kTypeShift = 24; // NOTE: This depends on how Procesor.GetHashCode works
			const int kProcessorTypeShift = 0;

			uint code = (
					(((uint)mType) & 0xFF) << kTypeShift |
					(((uint)mProcessorType.GetHashCode()) & 0x00FFFFFF) << kProcessorTypeShift
				);

			return unchecked((int)code);
		}
		/// <summary>Returns a string representation of this object</summary>
		/// <returns>"[<see cref="Type"/>\t<see cref="ProcessorType.ToString()"/>]"</returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return string.Format("[{0}\t{1}]",
				mType.ToString(),
				mProcessorType.ToString()
				);
		}
		#endregion

		#region IComparer<Platform> Members
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(Platform x, Platform y)
		{
			return Platform.StaticCompare(x, y);
		}
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		int System.Collections.IComparer.Compare(object x, object y)
		{
			Contract.Assume(x != null && y != null);

			return Platform.StaticCompare((Platform)x, (Platform)y);
		}
		#endregion

		#region IComparable<Platform> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(Platform other)
		{
			return Platform.StaticCompare(this, other);
		}
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(object obj)
		{
			Contract.Assume(obj != null);

			return Platform.StaticCompare(this, (Platform)obj);
		}
		#endregion

		#region IEquatable<Platform> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Platform other)
		{
			return Platform.Equals(this, other);
		}
		#endregion

		/// <summary></summary>
		/// <param name="encoder"></param>
		/// <remarks>10 bits</remarks>
		public void HandleEncode(ref Bitwise.HandleBitEncoder encoder)
		{
			encoder.Encode32(mType, TypeExtensions.BitEncoders.PlatformType);
			mProcessorType.HandleEncode(ref encoder);
		}

		public void HandleDecode(ref Bitwise.HandleBitEncoder encoder)
		{
			encoder.Decode32(out mType, TypeExtensions.BitEncoders.PlatformType);
			mProcessorType.HandleDecode(ref encoder);
		}


		#region Util
		static bool StaticEquals(Platform lhs, Platform rhs)
		{
			return lhs.mType == rhs.mType &&
				lhs.mProcessorType.Equals(rhs.mProcessorType);
		}

		static int StaticCompare(Platform lhs, Platform rhs)
		{
			int lhs_data = (int)lhs.mType, rhs_data = (int)rhs.mType;
			int result = lhs_data - rhs_data;

			if (result == 0)
				result = lhs.CompareTo(rhs);

			return result;
		}
		#endregion

		/*
		 Environment Signatures:
			'MS32' - Windows 32bit
			'MS64' - Windows 64bit
			'MPPC' - Mac PowerPC
			'Mx86' - Mac Intel x86
			'Mx64' - Mac Intel x64
			'PING' - Linux (aka Ping1)
			
			'XBOX' - Xbox 1
			'X360' - Xbox 360
			'X720' - Xbox Durango
			'SPS2' - Playstation 2
			'SPS3' - Playstation 3
			'WIWI' - Nintendo Wii
			'CUBE' - GameCube
		*/
		#region Windows
		static readonly Platform kWin32 = new Platform(PlatformType.Windows, Processor.Intelx86);
		/// <summary>Microsoft Windows 32-bit platform</summary>
		public static Platform Win32 { get { return kWin32; } }

		static readonly Platform kWin64 = new Platform(PlatformType.Windows, Processor.Intelx64);
		/// <summary>Microsoft Windows 64-bit platform</summary>
		public static Platform Win64 { get { return kWin64; } }
		#endregion

		#region Xbox
		static readonly Platform kXbox1 = new Platform(PlatformType.Xbox, Processor.Intelx86);
		/// <summary>Microsoft Xbox (Original) platform</summary>
		public static Platform Xbox1 { get { return kXbox1; } }

		static readonly Platform kXbox360 = new Platform(PlatformType.Xbox, Processor.PowerPcXenon);
		/// <summary>Microsoft Xbox 360 platform</summary>
		public static Platform Xbox360 { get { return kXbox360; } }

		static readonly Platform kXboxDurango = new Platform(PlatformType.Xbox, Processor.Intelx64);
		/// <summary>Microsoft Xbox One platform</summary>
		public static Platform XboxDurango { get { return kXboxDurango; } }
		#endregion

		#region Mac
		static readonly Platform kMac32 = new Platform(PlatformType.Mac, Processor.PowerPc32);
		/// <summary>Apple's Macintosh PowerPC 32-bit platform</summary>
		public static Platform Mac32 { get { return kMac32; } }

		static readonly Platform kMac64 = new Platform(PlatformType.Mac, Processor.PowerPc64);
		/// <summary>Apple's Macintosh PowerPC 64-bit platform</summary>
		public static Platform Mac64 { get { return kMac64; } }

		static readonly Platform kMacIntel32 = new Platform(PlatformType.Mac, Processor.Intelx86);
		/// <summary>Apple's Macintosh for Intel 32-bit platform</summary>
		public static Platform MacIntel32 { get { return kMacIntel32; } }

		static readonly Platform kMacIntel64 = new Platform(PlatformType.Mac, Processor.Intelx64);
		/// <summary>Apple's Macintosh for Intel 64-bit platform</summary>
		public static Platform MacIntel64 { get { return kMacIntel64; } }
		#endregion

		#region Operating Environment
		static class OperatingEnvironment
		{
			internal static readonly Platform kEnvironment;

			static OperatingEnvironment()
			{
				// TODO: .NET 4 upgrade:
				// System.Environment.Is64BitProcess and Is64BitOperatingSystem
				ProcessorSize size;
				switch (IntPtr.Size) // HACK: the only way I've read on how to detect the processor size (assuming you compile with AnyCPU)
				{
					case 4: size = ProcessorSize.x32; break;
					case 8: size = ProcessorSize.x64; break;

					// TODO: use UnreachableCaseException
					default:
						throw new PlatformNotSupportedException(string.Format("Pointer Size: {0}",
							IntPtr.Size.ToString()));
				}

				Contract.Assume(System.Environment.OSVersion != null);
				switch (System.Environment.OSVersion.Platform)
				{
					case PlatformID.Win32NT:
						switch (size)
						{
							case ProcessorSize.x32: kEnvironment = Win32; break;
							case ProcessorSize.x64: kEnvironment = Win64; break;
						}
						break;

					// TODO: Somehow use Environment.OSVersion.Version to detect if this is 
					// a Mac Intel (I think PPC was discontinued after 10.5?)
					case PlatformID.MacOSX:
						switch (size)
						{
							case ProcessorSize.x32: kEnvironment = Mac32; break;
							case ProcessorSize.x64: kEnvironment = Mac64; break;
						}
						break;

					case PlatformID.Xbox:
						switch (size)
						{
							case ProcessorSize.x32: kEnvironment = Xbox360; break;
							case ProcessorSize.x64: kEnvironment = XboxDurango; break;
						}
						break;

					// TODO: iPod maybe?

					// TODO: use UnreachableCaseException
					default:
						throw new PlatformNotSupportedException(string.Format("PlatformID: {0}",
							System.Environment.OSVersion.Platform.ToString()));
				}
			}
		};

		/// <summary>Get the library's platform definition for the current operating environment</summary>
		public static Platform Environment { get {
			return OperatingEnvironment.kEnvironment;
		} }
		#endregion
	};
}