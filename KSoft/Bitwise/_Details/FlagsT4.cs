using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	partial class Flags
	{
		#region Add
		/// <summary>Adds <paramref name="rhs"/> to <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to add to <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> != <paramref name="rhs"/></returns>
		public static uint Add(uint lhs, uint rhs)
		{
			return lhs |= rhs;
		}
		/// <summary>Adds <paramref name="rhs"/> to <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector reference</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to add to <paramref name="lhs"/></param>
		public static void Add(ref uint lhs, uint rhs)
		{
			lhs |= rhs;
		}

		/// <summary>Adds <paramref name="rhs"/> to <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to add to <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> != <paramref name="rhs"/></returns>
		public static ulong Add(ulong lhs, ulong rhs)
		{
			return lhs |= rhs;
		}
		/// <summary>Adds <paramref name="rhs"/> to <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector reference</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to add to <paramref name="lhs"/></param>
		public static void Add(ref ulong lhs, ulong rhs)
		{
			lhs |= rhs;
		}

		#endregion

		#region Remove
		/// <summary>Removes <paramref name="rhs"/> from <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to remove from <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> AND-EQUALS <paramref name="rhs"/></returns>
		public static uint Remove(uint lhs, uint rhs)
		{
			return lhs &= ~rhs;
		}
		/// <summary>Removes <paramref name="rhs"/> from <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to remove from <paramref name="lhs"/></param>
		public static void Remove(ref uint lhs, uint rhs)
		{
			lhs &= ~rhs;
		}

		/// <summary>Removes <paramref name="rhs"/> from <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to remove from <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> AND-EQUALS <paramref name="rhs"/></returns>
		public static ulong Remove(ulong lhs, ulong rhs)
		{
			return lhs &= ~rhs;
		}
		/// <summary>Removes <paramref name="rhs"/> from <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to remove from <paramref name="lhs"/></param>
		public static void Remove(ref ulong lhs, ulong rhs)
		{
			lhs &= ~rhs;
		}

		#endregion

		#region Modify
		#region 8-bit
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns>
		/// If <paramref name="addOrRemove"/> is True:
		/// <paramref name="lhs"/> |= <paramref name="rhs"/>
		/// 
		/// Else:
		/// <paramref name="lhs"/> &amp;= <paramref name="rhs"/>
		/// </returns>
		public static byte Modify(bool addOrRemove, byte lhs, byte rhs)
		{
			return (addOrRemove == true ?
				lhs |= rhs : 
				lhs &= (byte)~rhs);
		}
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns><paramref name="addOrRemove"/></returns>
		public static bool Modify(bool addOrRemove, ref byte lhs, byte rhs)
		{
			if (addOrRemove == true)
				lhs |= rhs;
			else
				lhs &= (byte)~rhs;

			return addOrRemove;
		}
		#endregion
		#region 16-bit
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns>
		/// If <paramref name="addOrRemove"/> is True:
		/// <paramref name="lhs"/> |= <paramref name="rhs"/>
		/// 
		/// Else:
		/// <paramref name="lhs"/> &amp;= <paramref name="rhs"/>
		/// </returns>
		public static ushort Modify(bool addOrRemove, ushort lhs, ushort rhs)
		{
			return (addOrRemove == true ?
				lhs |= rhs : 
				lhs &= (ushort)~rhs);
		}
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns><paramref name="addOrRemove"/></returns>
		public static bool Modify(bool addOrRemove, ref ushort lhs, ushort rhs)
		{
			if (addOrRemove == true)
				lhs |= rhs;
			else
				lhs &= (ushort)~rhs;

			return addOrRemove;
		}
		#endregion
		#region 32-bit
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns>
		/// If <paramref name="addOrRemove"/> is True:
		/// <paramref name="lhs"/> |= <paramref name="rhs"/>
		/// 
		/// Else:
		/// <paramref name="lhs"/> &amp;= <paramref name="rhs"/>
		/// </returns>
		public static uint Modify(bool addOrRemove, uint lhs, uint rhs)
		{
			return (addOrRemove == true ?
				lhs |= rhs : 
				lhs &= (uint)~rhs);
		}
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns><paramref name="addOrRemove"/></returns>
		public static bool Modify(bool addOrRemove, ref uint lhs, uint rhs)
		{
			if (addOrRemove == true)
				lhs |= rhs;
			else
				lhs &= (uint)~rhs;

			return addOrRemove;
		}
		#endregion
		#region 64-bit
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns>
		/// If <paramref name="addOrRemove"/> is True:
		/// <paramref name="lhs"/> |= <paramref name="rhs"/>
		/// 
		/// Else:
		/// <paramref name="lhs"/> &amp;= <paramref name="rhs"/>
		/// </returns>
		public static ulong Modify(bool addOrRemove, ulong lhs, ulong rhs)
		{
			return (addOrRemove == true ?
				lhs |= rhs : 
				lhs &= (ulong)~rhs);
		}
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns><paramref name="addOrRemove"/></returns>
		public static bool Modify(bool addOrRemove, ref ulong lhs, ulong rhs)
		{
			if (addOrRemove == true)
				lhs |= rhs;
			else
				lhs &= (ulong)~rhs;

			return addOrRemove;
		}
		#endregion
		#endregion
	};
}