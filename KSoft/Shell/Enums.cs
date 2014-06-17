using System;

namespace KSoft.Shell
{
	/// <summary>The ordering which bytes are interpreted on the processor</summary>
	/// <remarks>
	/// See http://en.wikipedia.org/wiki/Endian for more information.
	/// 
	/// "Middle-endian" is unsupported.
	/// 
	/// Most people do not realize that the terms 'big-endian' and 'little-endian' come 
	/// from <a href="http://www.jaffebros.com/lee/gulliver/index.html">Gulliver's Travels</a>. 
	/// The nations of <a href="http://www.jaffebros.com/lee/gulliver/bk1/chap1-4.html">Lilliput 
	/// and Blefuscu</a> were waging a terrible and bloody war over which end one should cut 
	/// open on a boiled egg - the little end or the big end.
	/// </remarks>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum EndianFormat : byte
	{
		/// <summary>Least Significant Bit order (lsb)</summary>
		/// <remarks>Right-to-Left</remarks>
		/// <see cref="BitConverter.IsLittleEndian"/>
		Little,
		/// <summary>Most Significant Bit order (msb)</summary>
		/// <remarks>Left-to-Right</remarks>
		Big,

		/// <remarks>1 bit</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	/// <summary>Supported processor instruction set sizes</summary>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum ProcessorSize : byte
	{
		/// <summary>Processor size used is determined during runtime. Special for managed frameworks like .NET</summary>
		AnyCPU,

		/// <summary>32-bit processor</summary>
		x32,
		/// <summary>64-bit processor</summary>
		x64,

		#region Reserved
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused3,

#if false
		/// <summary>128-bit processor</summary>
		/// <remarks>http://en.wikipedia.org/wiki/128-bit</remarks>
		[Obsolete(KSoftConstants.kUnsupportedMsg)] x128,
		/// <summary>256-bit processor</summary>
		/// <remarks>http://en.wikipedia.org/wiki/256-bit</remarks>
		[Obsolete(KSoftConstants.kUnsupportedMsg)] x256,
#endif
		#endregion

		/// <remarks>2 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	/// <summary>Supported sizes of a processor's logical word</summary>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum ProcessorWordSize : byte
	{
		x8,
		x16,
		x32,
		x64,

		#region Reserved
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused4,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused5,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused6,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused7,

#if false
		[Obsolete(KSoftConstants.kUnsupportedMsg)] x128,
		[Obsolete(KSoftConstants.kUnsupportedMsg)] x256,
#endif
		#endregion

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	/// <summary>Supported processor instruction set types</summary>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum InstructionSet : byte
	{
		/// <summary>Intel based</summary>
		Intel,
		/// <summary>PowerPC based</summary>
		PPC,

		/// <summary>Common Intermediate Language (.NET)</summary>
		/// <remarks>http://en.wikipedia.org/wiki/Common_Intermediate_Language</remarks>
		CIL,

		/// <summary></summary>
		/// <remarks>http://en.wikipedia.org/wiki/ARM_architecture
		/// Used for:
		///  * iOS (iPhone OS - http://en.wikipedia.org/wiki/IPhone_OS)
		///  * MS Windows Phone
		///  * MS Zune
		///  * MS Windows CE
		/// </remarks>
		ARM,

		/// <summary>Microprocessor without Interlocked Pipeline Stages</summary>
		/// <remarks>http://en.wikipedia.org/wiki/MIPS_architecture</remarks>
		[Obsolete(KSoftConstants.kUnsupportedMsg)]
		MIPS,

		#region Reserved
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused5,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused6,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused7,
		#endregion

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};


	/// <summary>Supported Operating System platform types</summary>
	[System.Reflection.Obfuscation(Exclude=true)]
	public enum PlatformType : uint
	{
		/// <summary>Undefined platform!</summary>
		Undefined,

		/// <summary>Microsoft Windows</summary>
		Windows,
		
		/// <summary>Linux based</summary>
		Linux,
		Android,

		/// <summary>Apple's Macintosh</summary>
		Mac,
		/// <summary>Apple's iPod (a.k.a iPhone OS)</summary>
		/// <remarks>Currently unsupported</remarks>
		iOS,

		/// <summary>Nintendo non-hand held (e.g., Wii)</summary>
		/// <remarks>Currently unsupported</remarks>
		NintendoConsole,
		
		/// <summary>Sony's Playstation</summary>
		/// <remarks>Currently unsupported</remarks>
		Playstation,
		
		/// <summary>Microsoft's Xbox (Original, 360, Durango)</summary>
		Xbox,

		#region Reserved
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused9,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused10,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused11,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused12,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused13,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused14,
		[Obsolete(KSoftConstants.kReservedMsg)] zUnused15,
		#endregion

		/// <remarks>4 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};
}