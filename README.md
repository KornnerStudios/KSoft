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

## Enum-indexed bit collections

`BitVector32<TBits>` and `BitVector64<TBits>` associate a bit-index enum with one 4/8-byte mutable value. Enum values are positions, not flags masks. Operations accept the associated enum; different enum types and different closed vector types are not implicitly interchangeable. KSoft emits the two generic facades with its existing generator; consuming them requires no consumer-side generator.

Use `Set`, `Toggle`, or the indexer on a local/field. For an ordinary value-returning property, assign a replacement with `Options = Options.With(bit)`: calling a mutator on the returned struct changes a copy. For `Clear`/`SetAll`, which return no replacement, modify a local copy and assign it back. `Set` and `Toggle` also return copies after changing their receiver, so chaining them without assigning the final result does not keep every change in the original variable. Do not combine bit-index enum values with `|`; set members individually or combine same-type vectors.

```csharp
using KSoft.Collections;

enum ToolBits { None = -1, Verbose, Trace, kNumberOf }

sealed class ToolOptions
{
	public BitVector32<ToolBits> Bits { get; set; }

	public void EnableVerbose()
	{
		Bits = Bits.With(ToolBits.Verbose);
	}
}
```

Usable members are declared nonnegative indices excluding `kNumberOf`/`kMax` exclusive bounds. Negative sentinel members may be present but cannot be passed as bits; `[Flags]` enum types, invalid bounds, and undefined bit arguments are rejected. Ordinary `[Obsolete]`, `[Browsable(false)]`, or `[XmlIgnore]` members are still usable by code. `EnumBitEncoderDisableAttribute` governs enum-value encoding and is not consulted by these typed index collections. Aliases share a bit; canonical names use the first ordinary declaration for that index.

`FromRaw` validates the enum domain and required width, then preserves every raw bit; `ToRaw` exports the stored word unchanged. Whole-word mutations/combinations affect physical storage but still validate the enum domain and required width. Raw state queries and equality read the stored word without those checks. Named enumeration and `ToFlagsString` include only declared members in ascending index order, once per index, without applying UI visibility filters. The default value is an all-clear word; creating `default` or using the implicit parameterless construction does not itself validate the enum.

`EnumBitSet<TBits>` supplies a mutable reference-backed set, including for domains larger than 64 bits; copying the reference shares mutations. Its logical extent is an exclusive bound or highest usable index plus one, not the bits required to encode one enum value. Enum-returning searches start inclusively and skip gaps; the `...BitIndex` searches return physical indices and can report gaps. Both require a usable declared starting member. Supply a distinct `invalidSentinelValue` for enum-returning searches when the default zero denotes a real member. Named collection count follows named enumeration; `Cardinality` counts stored set bits.

Corrected `EnumBitSet` extents can change `SerializeWords` word counts. Those methods stream the existing word layout without a logical-length prefix or enum definition, so both endpoints must agree on the extent/layout; historical undersized streams are not detected or migrated automatically. General `Serialize(EndianStream)` is still unimplemented.

`BitVectorControl` infers typed metadata unless an explicit presentation source is supplied; explicit enum hints must still match a typed value. Visibility and labels do not change bit validity: see `IEnumBitVector` for the adapter boundary and `BitVectorUserInterfaceData` for the display/visibility and canonical-alias rules.

Fixed vectors contain only their raw word. Metadata is cached per closed enum type, and metadata discovery and the existing enum-conversion setup have first-use costs. Converting a fixed vector to `IEnumBitVector` boxes a snapshot; its update method returns a replacement rather than mutating that snapshot. Ordinary typed access does not require that interface; formatting, setup, and interface use can allocate.

## Building
Before you try building any of the projects, first [read the requirements](https://bitbucket.org/KornnerStudios/ksoft/wiki/Requirements) you may need.
