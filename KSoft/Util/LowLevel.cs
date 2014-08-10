using System;
using System.Runtime.InteropServices;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.LowLevel.Util
{
	/// <summary>Utility functions related to unmanaged operations</summary>
	public static class Unmanaged
	{
		/// <summary>Convert a handle to object</summary>
		/// <param name="ptr">Handle</param>
		/// <returns>Managed object</returns>
		public static object IntPtrToStructure(IntPtr ptr, Type t)
		{
			Contract.Requires(ptr != IntPtr.Zero);
			Contract.Requires(t != null);

			return Marshal.PtrToStructure(ptr, t);
		}
		/// <summary>Convert a handle to object</summary>
		/// <param name="ptr">Handle</param>
		/// <typeparam name="T">Type to covert <paramref name="ptr"/> to</typeparam>
		/// <returns>Managed object</returns>
		public static T IntPtrToStructure<T>(IntPtr ptr)
		{
			Contract.Requires(ptr != IntPtr.Zero);

			return (T)Marshal.PtrToStructure(ptr, typeof(T));
		}

		/// <summary>Copy an object into unmanaged memory</summary>
		/// <typeparam name="T">Type of the object we'll be copying. Fields should all be bittable</typeparam>
		/// <param name="obj">Object to copy</param>
		/// <param name="ptr">Address of the unmanaged memory</param>
		/// <remarks>
		/// Doesn't destroy any pre-existing memory or objects inside <paramref name="ptr"/> 
		/// before the copy takes place
		/// </remarks>
		public static void StructureToPtr<T>(T obj, IntPtr ptr)
		{
			Contract.Requires(ptr != IntPtr.Zero);

			Marshal.StructureToPtr(obj, ptr, false);
		}
		
		/// <summary>Allocate unmanaged memory for an object of type <paramref name="t"/></summary>
		/// <param name="t">Type to allocate memory for</param>
		/// <returns>Handle to allocated memory</returns>
		public static IntPtr New(Type t)
		{
			Contract.Requires(t != null);

			return Marshal.AllocHGlobal(Marshal.SizeOf(t));
		}
		/// <summary>Allocate unmanaged memory for an object</summary>
		/// <typeparam name="T">Type to allocate memory for</typeparam>
		/// <returns>Handle to allocated memory</returns>
		public static IntPtr New<T>()
		{
			return Marshal.AllocHGlobal(SizeOf<T>());
		}
		
		/// <summary>Free unmanaged memory for an existing object</summary>
		/// <param name="ptr">Memory allocated by <c>New</c></param>
		public static void Delete(IntPtr ptr)
		{
			Contract.Requires(ptr != IntPtr.Zero);

			Marshal.FreeHGlobal(ptr);
		}

		/// <summary>Calculate the byte size of an object</summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static int SizeOf<T>()
		{
			return
#if __MonoCS__
				Marshal.SizeOf(typeof(T));
#else
				Marshal.SizeOf<T>();
#endif
		}
	};
}