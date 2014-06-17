using System;
using System.Collections.Generic;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Debug
{
	public static class ValueCheck
	{
		[Contracts.Pure]
		public static void IsLessThanEqualTo(string description, int expectedMax, int actualMax)
		{
			Contract.Requires(!string.IsNullOrEmpty(description));

			if (actualMax > expectedMax)
				throw new InvalidDataException(string.Format("{0}. Expected at most {1}, got {2}",
					description, expectedMax, actualMax));
		}

		[Contracts.Pure]
		public static void IsDistinct<T>(string description, string valueName, IEnumerable<T> seq)
		{
			Contract.Requires(!string.IsNullOrEmpty(description));
			Contract.Requires(!string.IsNullOrEmpty(valueName));
			Contract.Requires(seq != null);

			if (seq.ContainsDuplicates())
				throw new InvalidDataException(string.Format("{0}. Expected all '{1}' values to be distinct",
					description, valueName));
		}
	};
}