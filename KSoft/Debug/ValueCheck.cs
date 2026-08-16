using System;
using System.Collections.Generic;
using System.IO;

namespace KSoft.Debug
{
	public static class ValueCheck
	{
		public static void AreEqual<T>(string description, T expected, T actual,
			string expectedDisplayValue = null, string actualDisplayValue = null)
		{
			ArgumentException.ThrowIfNullOrEmpty(description);

			if (!EqualityComparer<T>.Default.Equals(expected, actual))
			{
				if (expectedDisplayValue == null)
				{
					expectedDisplayValue = expected.ToString();
				}
				if (actualDisplayValue == null)
				{
					actualDisplayValue = actual.ToString();
				}

				throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"{0}. Expected '{1}' but got '{2}'",
					description, expectedDisplayValue, actualDisplayValue));
			}
		}

		public static void IsLessThanEqualTo(string description, int expectedMax, int actualMax)
		{
			ArgumentException.ThrowIfNullOrEmpty(description);

			if (actualMax > expectedMax)
			{
				throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"{0}. Expected at most {1}, got {2}",
					description, expectedMax, actualMax));
			}
		}

		public static void IsGreaterThanEqualTo(string description, int expectedMin, int actualMin)
		{
			ArgumentException.ThrowIfNullOrEmpty(description);

			if (actualMin < expectedMin)
			{
				throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"{0}. Expected at least {1}, got {2}",
					description, expectedMin, actualMin));
			}
		}

		public static void IsDistinct<T>(string description, string valueName, IEnumerable<T> seq)
		{
			ArgumentException.ThrowIfNullOrEmpty(description);
			ArgumentException.ThrowIfNullOrEmpty(valueName);
			ArgumentNullException.ThrowIfNull(seq);

			if (seq.ContainsDuplicates())
			{
				throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"{0}. Expected all '{1}' values to be distinct",
					description, valueName));
			}
		}
	};
}
