using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Shell.Test
{
	[TestClass]
	public partial class PlatformTest : BaseTestClass
	{
		[TestMethod]
		public void Shell_PlatformApiTest()
		{
			Assert.AreEqual(6, Processor.BitCount, 
				"Expected Processor bit count size has changed. Was this intentional?");
			Assert.AreEqual(6+4, Platform.BitCount,
				"Expected Platform bit count size has changed. Was this intentional?");

			const InstructionSet k_xenon_instruction_set = InstructionSet.PPC;
			const ProcessorSize k_xenon_processor_size = ProcessorSize.x32;
			const EndianFormat k_xenon_byte_order = EndianFormat.Big;

			var xenon_processor = new Processor(k_xenon_processor_size, k_xenon_byte_order, k_xenon_instruction_set);
			Assert.AreEqual(Processor.PowerPcXenon, xenon_processor);

			var xenon_platform = new Platform(PlatformType.Xbox, xenon_processor);
			Assert.AreEqual(Platform.Xbox360, xenon_platform);

			Assert.AreEqual(Processor.PowerPcXenon, xenon_platform.ProcessorType);
			Assert.AreEqual(k_xenon_instruction_set, xenon_platform.ProcessorType.InstructionSet);
			Assert.AreEqual(k_xenon_processor_size, xenon_platform.ProcessorType.ProcessorSize);
			Assert.AreEqual(k_xenon_byte_order, xenon_platform.ProcessorType.ByteOrder);

			Assert.IsTrue(Platform.Win32.CompareTo(Platform.Win64) < 0,
				"ProcessorSize's x32 member should come before x64, yet Win32 is not less-than Win64");
		}
	};
}