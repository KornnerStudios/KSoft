namespace KSoft.SourceGeneration.Options;

// Keep this enum grouped by generated-domain namespace, not by implementation packet chronology. GeneratorRegistry owns
// the property/output metadata and tests enforce that every enum value has a registration.
internal enum GeneratorFeature
{
	// Bitwise domains.
	BitsBitCount,
	BitsCore,
	BitsEncoding,
	BitsRotate,
	ByteSwap,
	Flags,
	HandleBitEncoder,

	// Collections domains.
	BitSet,
	BitVectors,

	// Enum domains.
	Enums,

	// IO domains.
	BitStream,
	EndianStreamsNumbers,
	IOExceptions,
	TagElementStreams,

	// Math domains.
	IntegerMath,
};
