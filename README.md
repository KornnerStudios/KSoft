# What is Vita?
Vita is the logical amalgatmation of many related and dependent codebases, primarily written in C++ and C#.NET, which are produced and developed by Kornner Studios.

Vita began as a closed source project back in November of 2009 (so we predate the [PS Vita][WikiPSVita]). It wasn't until the beginning of 2014 when it was transitioned to an open source product. **This is presently an ongoing task**, not everything has been transition over to the open source repositories yet.

Etymologically speaking, "Vita" is the Latin word for "life". If you're reading this, then it is very likely that coding is a part of your life, as it is to Vita's developers.

[WikiPSVita]: http://en.wikipedia.org/wiki/PlayStation_Vita#Post-announcement

# What is KSoft?
KSoft is the codename of the C#/.NET-centric part of the Vita codebase.

Specifically, you're viewing the "KSoft.BCL" (Base Class Libraries) repository. This emboddies many generalized systems that we have developed to supplement our specialized projects (which are located in external repositories).

Some are open source (eg, [KSoft.Blam][KSoftBlam], for targeting the [Halo][WikiHaloFPS] engine), some closed (eg, KSoft.XDK, for targeting multi-generation [Xbox Development Kits][WikiXDK]). This is part of the reason for not messing with sub-modules or the other such souce-control mechanisms, but also because the developer writing this isn't experienced with such setups :o)

[KSoftBlam]: https://bitbucket.org/KornnerStudios/ksoft.blam
[WikiHaloFPS]: http://en.wikipedia.org/wiki/Halo_%28series%29#Original_trilogy
[WikiXDK]: http://en.wikipedia.org/wiki/Xbox_Development_Kit

# License?
The default license of Vita projects is the [MIT License][LicenseMIT]

[LicenseMIT]: http://www.linfo.org/mitlicense.html

# KSoft.BCL Goals
The BCL aims to provide a compartmentalized framework which provides functionality not present, or at least efficently, in the .NET framework proper.

Compartmentalized in that non-critical systems like KSoft.Security are outside the actual root assembly, just named "KSoft".

Example functionality which you can't find in the .NET framework is a BitStream class. We also provide utilities for treating and using Enums as actual bit flags, in an optimized and complete manner.

We're also very, very heavy on using [T4 Text Templates][MsdnT4] to generate most code which would otherwise be copy and pasted to fit the context of like-wise types. For example, our static KSoft.Bits (located in KSoft's _Bitwise_ folder) class uses a single T4 template to generate the BitCount() method for the usual unsigned integer types (UInt32, etc)

[MsdnT4]: http://msdn.microsoft.com/en-us/library/bb126445.aspx

## Our BitStream
You will actually be hard pressed to find a decent comprehensive BitStream class for .NET anywhere on The 'Net. The most comprehensive one that I know of is featured in [a CodeProject article][CodeProjectBitStream]. To compare the two:

* **They** require a complete, internal copy of the stream's bytes; **KSoft** supports actual streaming (using a BaseStream) and use a configurable (at compile time) cache 'word' (32 or 64 bits) where bits reside until being flushed to the BaseStream

* **They** were last updated in 2005 have unpatched bugs; **KSoft** has a tried and tested class. Tested in both regular use and with Unit Tests to validate core operations. A patched version of the article is used in our Unit Testing to check compatability

* **They** have a #region infested, monolithic .cs file; **KSoft** makes use of partial classes and code generation using T4 to keep the files _bite_-sized and copy&paste code to a minimum

[CodeProjectBitStream]: http://www.codeproject.com/Articles/12261/A-BitStream-Class-for-the-NET-Framework

## [Flags]Enum++
The .NET framework offers a [Enum.HasFlag()][EnumHasFlagMsdn] object method which you can use to test that an enum. Unfortuntely, that method is [far from efficent][EnumHasFlagSO].

Using code generation (via Linq Expressions), we're able to address the inefficentices found in .NET's Enum.HasFlag(). We don't perform any boxing and can operate on the underlying type (ie, integer) of the enum instead of using UInt64 as a catch-all.

We take this one step further still and provide functionality for Add, Remove, and Modify as methods. What's great about this is that you're able to add a new semantic level to bit flag operations in your code, instead of bleeding your enum values together with the normal bit-wise operators.

If we so desired (and we presently don't), we could even modify the code generator to add checks to **DEBUG** assemblies that validate the flags being used in bit operations (Test, Add, etc) have named members in the enum type.

If there are other .NET languages which don't (easily) offer bit flag operations on enum types, I'm sure our utilities would prove their use once again.

[EnumHasFlagMsdn]: http://msdn.microsoft.com/en-us/library/system.enum.hasflag%28v=vs.110%29.aspx
[EnumHasFlagSO]: http://stackoverflow.com/questions/7368652/what-is-it-that-makes-enum-hasflag-so-slow