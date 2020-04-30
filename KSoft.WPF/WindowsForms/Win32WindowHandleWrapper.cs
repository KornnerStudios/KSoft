using System;
using System.Windows;
using System.Windows.Forms;

namespace KSoft.WPF.WindowsForms
{
	public sealed class Win32WindowHandleWrapper : IWin32Window
	{
		private readonly IntPtr mHandle;
		public Win32WindowHandleWrapper(IntPtr handle) { mHandle = handle; }
		public IntPtr Handle { get { return mHandle; } }

		public static Win32WindowHandleWrapper FromPointer(IntPtr handle)
		{
			return new Win32WindowHandleWrapper(handle);
		}

		public static Win32WindowHandleWrapper FromDependencyObject(DependencyObject theObj)
		{
			var parentWindow = Window.GetWindow(theObj);

			return FromWindow(parentWindow);
		}

		public static Win32WindowHandleWrapper FromWindow(Window window)
		{
			var windowInterop = new System.Windows.Interop.WindowInteropHelper(window);

			return new Win32WindowHandleWrapper(windowInterop.Handle);
		}
	};
}
