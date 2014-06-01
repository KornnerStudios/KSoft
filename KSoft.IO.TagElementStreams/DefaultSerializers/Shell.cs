using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class TagElementStreamDefaultSerializer
	{
		public static void Stream<TDoc, TCursor>(TagElementStream<TDoc, TCursor, string> s,
			ref Shell.Processor value)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var inst_set = reading
				? 0
				: value.InstructionSet;
			var proc_size = reading
				? 0
				: value.ProcessorSize;
			var byte_order = reading
				? 0
				: value.ByteOrder;

			s.StreamAttributeEnum("instructionSet", ref inst_set);
			s.StreamAttributeEnum("processorSize", ref proc_size);
			s.StreamAttributeEnum("byteOrder", ref byte_order);

			if (reading)
			{
				value = new Shell.Processor(proc_size, byte_order, inst_set);
			}
		}

		public static void Stream<TDoc, TCursor>(TagElementStream<TDoc, TCursor, string> s,
			ref Shell.Platform value)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var platform_type = reading
				? 0
				: value.Type;
			var processor = reading
				? new Shell.Processor()
				: value.ProcessorType;

			s.StreamAttributeEnum("platformType", ref platform_type);
			Stream(s, ref processor);

			if (reading)
			{
				value = new Shell.Platform(platform_type, processor);
			}
		}
	};
}