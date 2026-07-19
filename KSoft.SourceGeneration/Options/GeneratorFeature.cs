namespace KSoft.SourceGeneration.Options;

// Keep this enum grouped by generated-domain namespace, not by implementation packet chronology. GeneratorRegistry owns
// the property/output metadata and tests enforce that every enum value has a registration.
internal enum GeneratorFeature
{
	// Bitwise domains.
	BitsBitCount,
	BitsEncoding,
	BitsRotate,

	// Collections domains.
	BitVectors,

	// IO domains.
	BitStream,
	EndianStreamsNumbers,
	TagElementStreams,

	// Math domains.
	IntegerMath,
};
