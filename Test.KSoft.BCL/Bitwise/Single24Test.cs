using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Bitwise.Test
{
	[TestClass]
	public class Single24Test : BaseTestClass
	{
		static readonly uint[] kTestConversionsList = [
			Single24.kMinInt,  0xCFFFFFC0,
			Single24.kMaxInt,  0x4FFFFFC0,

			0x39C000, 0x3E700000,
			0x407921, 0x401E4840,
			0x5870F0, 0x461C3C00,
		];

		[TestMethod]
		public void Single24_TestConversions()
		{
			for (int x = 0; x < kTestConversionsList.Length; x += 2)
			{
				uint input = kTestConversionsList[x + 0];
				uint expected = kTestConversionsList[x + 1];

				float single = Single24.ToSingle(input);
				bool was_encoded = Single24.TryFromSingle(single, out uint output);

				Assert.IsTrue(was_encoded);
				Assert.AreEqual(ByteSwap.SingleFromUInt32(expected), single); // Test ToSingle
				Assert.AreEqual(input, output); // Test FromSingle
			}
		}

		#region Halo Wars Definitive Edition tests
		[TestMethod]
		public void Single24_TestHaloWarsDefinitiveEditionValues()
		{
			foreach (var kvp in kHaloWarsDefinitiveEditionValues)
			{
				uint expectedSingle24Bits = kvp.Key;
				float expectedFloat = kvp.Value;

				float actualFloat = Single24.ToSingle(expectedSingle24Bits);
				bool expectedFloatWasEncoded = Single24.TryFromSingle(expectedFloat, out uint actualSingle24Bits);

				Assert.AreEqual(expectedFloat, actualFloat);
				Assert.IsTrue(expectedFloatWasEncoded);
				Assert.AreEqual(expectedSingle24Bits, actualSingle24Bits);
			}
		}

		static readonly KeyValuePair<uint, float>[] kHaloWarsDefinitiveEditionValues =
		[
			new(0x000000, 0f),             // data\tactics\cov_air_banshee_01.tactics.xmb
			new(0x1618DF, 1.0000003E-06f), // art\effects\cleansing\cleansing_air_impact_large_a.vis.xmb
			new(0x30C5F9, 0.010833323f),   // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x341B4F, 0.032916784f),   // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x342222, 0.0333333f),     // art\effects\shield_dome\shield_flicker_animation_01.xml.xmb
			new(0x35B281, 0.057770014f),   // art\ui\flash\hud\hud_minimap\hud_minimap.xml.xmb
			new(0x3645A2, 0.0710001f),     // art\campaign\scn02\rocketwarthog_01\rocketwarthog_01.vis.xmb
			new(0x39B3C4, 0.23138809f),    // art\ui\flash\hud\hud_minimap\hud_minimap.xml.xmb
			new(0x3BF47C, 0.49437714f),    // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x3D6222, 0.8458328f),     // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x3D83D7, 0.87874985f),    // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x3D9A74, 0.90083313f),    // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x3DBC29, 0.93375015f),    // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x3DDDDE, 0.9666672f),     // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x3DFFC9, 0.9997902f),     // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x3EAE30, 1.34021f),       // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x3F333F, 1.60009f),       // physics\cov_bldg_temple_02.shp.xmb
			new(0x3F7777, 1.7333298f),     // art\effects\shield_dome\shield_flicker_animation_01.xml.xmb
			new(0x3F8889, 1.7666702f),     // art\effects\shield_dome\shield_flicker_animation_01.xml.xmb
			new(0x407921, 2.4731598f),     // physics\cov_bldg_temple_02.shp.xmb
			new(0x44633F, 9.55072f),       // art\effects\shield_dome\shield_fade_fast_01.xml.xmb
			new(0x515200, 850f),           // data\objects.xml.xmb
			new(0x515400, 852f),           // data\objects.xml.xmb
			new(0x51E800, 1000f),          // data\tactics\cov_air_vampire_01.tactics.xmb
			new(0x521C00, 1080f),          // data\objects.xml.xmb
			new(0x522600, 1100f),          // data\tactics\cpgn_npc_forgewarthog_01.tactics.xmb
			new(0x522D00, 1114f),          // data\objects.xml.xmb
			new(0x524E80, 1181f),          // data\objects.xml.xmb
			new(0x525800, 1200f),          // data\objects.xml.xmb
			new(0x527100, 1250f),          // data\tactics\for_air_enforcer_01.tactics.xmb
			new(0x528000, 1280f),          // data\objects.xml.xmb
			new(0x528600, 1292f),          // data\objects.xml.xmb
			new(0x529A80, 1333f),          // data\objects.xml.xmb
			new(0x52BC00, 1400f),          // data\tactics\cov_inf_suicidegrunt_01.tactics.xmb
			new(0x52EA00, 1492f),          // data\objects.xml.xmb
			new(0x52EE00, 1500f),          // data\tactics\cov_air_banshee_01.tactics.xmb
			new(0x533980, 1651f),          // data\objects.xml.xmb
			new(0x533B80, 1655f),          // data\tactics\pow_gp_rage_impact.tactics.xmb
			new(0x536600, 1740f),          // data\objects.xml.xmb
			new(0x53E800, 2000f),          // data\objects.xml.xmb
			new(0x543A00, 2280f),          // data\objects.xml.xmb
			new(0x5461C0, 2439f),          // data\objects.xml.xmb
			new(0x547100, 2500f),          // data\aidifficultysettings.xml.xmb
			new(0x54EE00, 3000f),          // data\tactics\cpgn_npc_forge_01.tactics.xmb
			new(0x54FA40, 3049f),          // data\techs.xml.xmb
			new(0x5518C0, 3171f),          // data\objects.xml.xmb
			new(0x552000, 3200f),          // data\tactics\unsc_air_vulture_01.tactics.xmb
			new(0x552E80, 3258f),          // data\objects.xml.xmb
			new(0x556B00, 3500f),          // data\powers.xml.xmb
			new(0x558400, 3600f),          // data\objects.xml.xmb
			new(0x558E80, 3642f),          // data\objects.xml.xmb
			new(0x55B300, 3788f),          // data\objects.xml.xmb
			new(0x55CFC0, 3903f),          // data\objects.xml.xmb
			new(0x55E700, 3996f),          // data\objects.xml.xmb
			new(0x55E800, 4000f),          // data\objects.xml.xmb
			new(0x561340, 4250f),          // data\objects.xml.xmb
			new(0x5624E0, 4391f),          // data\objects.xml.xmb
			new(0x567100, 5000f),          // data\tactics\cov_bldg_megaturret_01.tactics.xmb
			new(0x5692A0, 5269f),          // data\objects.xml.xmb
			new(0x56EE00, 6000f),          // data\objects.xml.xmb
			new(0x56F1C0, 6030f),          // data\objects.xml.xmb
			new(0x56FF20, 6137f),          // data\objects.xml.xmb
			new(0x570940, 6218f),          // data\objects.xml.xmb
			new(0x5711A0, 6285f),          // data\objects.xml.xmb
			new(0x572000, 6400f),          // data\objects.xml.xmb
			new(0x572C80, 6500f),          // data\tactics\cpgn_scn07_scarabboss_01.tactics.xmb
			new(0x57A980, 7500f),          // data\objects.xml.xmb
			new(0x57E800, 8000f),          // data\objects.xml.xmb
			new(0x57EF20, 8057f),          // data\objects.xml.xmb
			new(0x580830, 8323f),          // data\objects.xml.xmb
			new(0x581660, 8550f),          // data\objects.xml.xmb
			new(0x581C00, 8640f),          // data\objects.xml.xmb
			new(0x5870F0, 9999f),          // data\aidifficultysettings.xml.xmb
			new(0x587100, 10000f),         // data\aidifficultysettings.xml.xmb
			new(0x589040, 10500f),         // data\objects.xml.xmb
			new(0x58E1F0, 11807f),         // data\objects.xml.xmb
			new(0x58EE00, 12000f),         // data\objects.xml.xmb
			new(0x58FFC0, 12284f),         // data\objects.xml.xmb
			new(0x596F60, 14070f),         // data\objects.xml.xmb
			new(0x59A980, 15000f),         // data\objects.xml.xmb
			new(0x5A1340, 17000f),         // data\objects.xml.xmb
			new(0x5A3280, 18000f),         // data\tactics\pow_cp_largeexplode_01.tactics.xmb
			new(0x5A7100, 20000f),         // data\objects.xml.xmb
			new(0x5A8230, 20550f),         // data\objects.xml.xmb
			new(0x5AEE00, 24000f),         // data\objects.xml.xmb
			new(0x5B0D40, 25000f),         // data\objects.xml.xmb
			new(0x5B3C20, 26500f),         // data\objects.xml.xmb
			new(0x5BA980, 30000f),         // data\objects.xml.xmb
			new(0x5BAB58, 30059f),         // data\objects.xml.xmb
			new(0x5C3280, 36000f),         // data\objects.xml.xmb
			new(0x5C7100, 40000f),         // data\tactics\cov_veh_brutechopper_01.tactics.xmb
			new(0x5D0D40, 50000f),         // data\objects.xml.xmb
			new(0x5D48F0, 53820f),         // data\objects.xml.xmb
			new(0x5DA980, 60000f),         // data\tactics\cov_bldg_megaturret_01.tactics.xmb
			new(0x5DB8A8, 60970f),         // data\objects.xml.xmb
			new(0x5E22E0, 70000f),         // data\tactics\hook_bldg_megaturret_01.tactics.xmb
			new(0x5E49F0, 75000f),         // data\objects.xml.xmb
			new(0x5E7100, 80000f),         // data\objects.xml.xmb
			new(0xDD0D40, -50000f),        // data\techs.xml.xmb
		];
		#endregion
	};
}
