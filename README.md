# What is Vita?
Vita is the logical amalgatmation of many related and dependent codebases, primarily written in C++ and C#.NET, which are produced and developed by Kornner Studios.

Vita began as a closed source project back in November of 2009 (so we predate the [PS Vita][WikiPSVita]). It wasn't until the beginning of 2014 when it was transitioned to an open source product. **This is currently an ongoing task**, not everything has been transition over to the open source repositories yet.

Etymologically speaking, "Vita" is the Latin word for "life". If you're reading this, then it is very likely that coding is a part of your life, as it is to Vita's developers.

[WikiPSVita]: http://en.wikipedia.org/wiki/PlayStation_Vita#Post-announcement

# What is KSoft?
KSoft is the codename of the C#/.NET-centric part of the Vita codebase.

Specifically, you're viewing the "KSoft.BCL" (Base Class Libraries) repository. This embodies many generalized systems that we have developed to supplement our specialized projects (which are located in external repositories).

Some are open source (eg, [KSoft.Blam][KSoftBlam], for targeting the [Halo][WikiHaloFPS] engine), some closed (eg, KSoft.XDK, for targeting multi-generation [Xbox Development Kits][WikiXDK]). This is part of the reason for not messing with sub-modules or the other such source-control mechanisms, but also because the developer writing this isn't experienced with such setups :o)

[KSoftBlam]: https://github.com/KornnerStudios/KSoft.Blam
[WikiHaloFPS]: http://en.wikipedia.org/wiki/Halo_%28series%29#Original_trilogy
[WikiXDK]: http://en.wikipedia.org/wiki/Xbox_Development_Kit

# License?
The default license of Vita projects is the [MIT License][LicenseMIT]

[LicenseMIT]: http://www.linfo.org/mitlicense.html

# KSoft.BCL Goals
The BCL aims to provide a compartmentalized framework which provides functionality not present, or at least efficiently, in the .NET framework proper.

Compartmentalized in that non-critical systems like KSoft.Security are outside the actual root assembly, just named "KSoft".

Examples include stream-based bit-level I/O and readable helpers for generic enum flag mutation.

KSoft uses Roslyn source generators for repeated C# surfaces such as numeric overload matrices. The former T4 assets are preserved in Git history and the `pre-t4-removal` tag.

## Our BitStream
You will actually be hard pressed to find a decent, comprehensive BitStream class for .NET anywhere on The 'Net. The most comprehensive one that I know of is featured in [a CodeProject article][CodeProjectBitStream]. To compare the two:

* **They** require a complete, internal copy of the stream's bytes; **KSoft** supports actual streaming (using a BaseStream) and use a configurable (at compile time) cache 'word' (32 or 64 bits) where bits reside until being flushed to the BaseStream

* **They** were last updated in 2005 and have unpatched bugs; **KSoft** has a tried and tested class. Tested in both regular use and with Unit Tests to validate core operations. A patched version of the article is used in our Unit Testing to check compatibility

* **They** have a #region infested, monolithic .cs file; **KSoft** makes use of partial classes and Roslyn source generators to keep the files _bite_-sized and copy&paste code to a minimum

[CodeProjectBitStream]: http://www.codeproject.com/Articles/12261/A-BitStream-Class-for-the-NET-Framework

## [Flags]Enum++
Use `Enum.HasFlag` for all-bits flag tests and .NET's `EqualityComparer<TEnum>.Default` / `Comparer<TEnum>.Default` for enum equality, hashing, and ordering.

`EnumFlags.Add`, `Remove`, and `Modify` remain readability helpers for `[Flags]` enums, with value-returning and `ref` forms. They support generic enum mutation by operating on the backing type through shared `EnumValue<TEnum>` conversions, rather than a separate expression-compiled flags implementation. See `KSoft\Enum\EnumFlags.cs` for the contract.

## Building
Before you try building any of the projects, first [read the requirements](https://bitbucket.org/KornnerStudios/ksoft/wiki/Requirements) you may need.
