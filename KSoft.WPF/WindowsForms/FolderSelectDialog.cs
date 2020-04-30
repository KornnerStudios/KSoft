using System;
using System.Reflection;
using System.Windows.Forms;

namespace KSoft.WPF.WindowsForms
{
	// Based on https://stackoverflow.com/a/33836106/444977

	/// <summary>
	/// Present the Windows Vista-style open file dialog to select a folder. Fall back for older Windows Versions
	/// </summary>
	public class FolderSelectDialog
	{
		internal const string kFoldersFilter = "Folders|\n";

		string mInitialDirectory;
		string mTitle;
		string mFileName = "";

		public string InitialDirectory
		{
			get { return string.IsNullOrEmpty(mInitialDirectory) ? Environment.CurrentDirectory : mInitialDirectory; }
			set { mInitialDirectory = value; }
		}
		public string Title
		{
			get { return mTitle ?? "Select a folder"; }
			set { mTitle = value; }
		}
		public string FileName { get { return mFileName; } }

		public bool ShowDialog()
		{
			return ShowDialog(IntPtr.Zero);
		}

		/// <param name="hWndOwner">Handle of the control or window to be the parent of the file dialog</param>
		/// <returns>true if the user clicks OK</returns>
		public bool ShowDialog(IntPtr hWndOwner)
		{
			var result = Environment.OSVersion.Version.Major >= 6
				? VistaDialog.Show(hWndOwner, InitialDirectory, Title)
				: ShowXpDialog(hWndOwner, InitialDirectory, Title);
			mFileName = result.FileName;
			return result.Result;
		}

		struct ShowDialogResult
		{
			public bool Result { get; set; }
			public string FileName { get; set; }
		};

		static ShowDialogResult ShowXpDialog(IntPtr ownerHandle, string initialDirectory, string title)
		{
			var dialogResult = new ShowDialogResult();
			using (var folderBrowserDialog = new FolderBrowserDialog
				{
					Description = title,
					SelectedPath = initialDirectory,
					ShowNewFolderButton = true
				})
			{
				if (folderBrowserDialog.ShowDialog(new Win32WindowHandleWrapper(ownerHandle)) == DialogResult.OK)
				{
					dialogResult.Result = true;
					dialogResult.FileName = folderBrowserDialog.SelectedPath;
				}
			}
			return dialogResult;
		}

		[Flags]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1712:DoNotPrefixEnumValuesWithTypeName")]
		internal enum FOS : uint
		{
			FOS_PICKFOLDERS = 0x00000020,
			FOS_PATHMUSTEXIST = 0x00000800,
		};

		static class VistaDialog
		{
			const BindingFlags kBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			readonly static Assembly gWindowsFormsAssembly = typeof(FileDialog).Assembly;
			readonly static Type gIFileDialogType = gWindowsFormsAssembly.GetType("System.Windows.Forms.FileDialogNative+IFileDialog");
			readonly static MethodInfo gCreateVistaDialogMethodInfo = typeof(OpenFileDialog).GetMethod("CreateVistaDialog", kBindingFlags);
			readonly static MethodInfo gOnBeforeVistaDialogMethodInfo = typeof(OpenFileDialog).GetMethod("OnBeforeVistaDialog", kBindingFlags);
			readonly static MethodInfo gGetOptionsMethodInfo = typeof(FileDialog).GetMethod("GetOptions", kBindingFlags);
			static MethodInfo gSetOptionsMethodInfo = gIFileDialogType.GetMethod("SetOptions", kBindingFlags);
#if false
			readonly static uint gFosPickFoldersBitFlag = (uint) gWindowsFormsAssembly
				.GetType("System.Windows.Forms.FileDialogNative+FOS")
				.GetField("FOS_PICKFOLDERS")
				.GetValue(null);
#endif
			const uint kOptionFlags = (uint)(FOS.FOS_PICKFOLDERS | FOS.FOS_PATHMUSTEXIST);
			readonly static ConstructorInfo gVistaDialogEventsConstructorInfo = gWindowsFormsAssembly
				.GetType("System.Windows.Forms.FileDialog+VistaDialogEvents")
				.GetConstructor(kBindingFlags, null, new[] { typeof(FileDialog) }, null);
			readonly static MethodInfo gAdviseMethodInfo = gIFileDialogType.GetMethod("Advise");
			readonly static MethodInfo gUnAdviseMethodInfo = gIFileDialogType.GetMethod("Unadvise");
			readonly static MethodInfo gShowMethodInfo = gIFileDialogType.GetMethod("Show");

			public static ShowDialogResult Show(IntPtr ownerHandle, string initialDirectory, string title)
			{
				var openFileDialog = new OpenFileDialog
				{
					AddExtension = false,
					CheckFileExists = false,
					DereferenceLinks = true,
					Filter = kFoldersFilter,
					InitialDirectory = initialDirectory,
					Multiselect = false,
					Title = title
				};

				var iFileDialog = gCreateVistaDialogMethodInfo.Invoke(openFileDialog, Util.EmptyArray);
				gOnBeforeVistaDialogMethodInfo.Invoke(openFileDialog, new[] { iFileDialog });
				gSetOptionsMethodInfo.Invoke(iFileDialog, new object[] { (uint) gGetOptionsMethodInfo.Invoke(openFileDialog, Util.EmptyArray) | kOptionFlags });
				var adviseParametersWithOutputConnectionToken = new[] { gVistaDialogEventsConstructorInfo.Invoke(new object[] { openFileDialog }), 0U };
				gAdviseMethodInfo.Invoke(iFileDialog, adviseParametersWithOutputConnectionToken);

				try
				{
					int retVal = (int) gShowMethodInfo.Invoke(iFileDialog, new object[] { ownerHandle });
					return new ShowDialogResult
					{
						Result = retVal == 0,
						FileName = openFileDialog.FileName
					};
				}
				finally
				{
					gUnAdviseMethodInfo.Invoke(iFileDialog, new[] { adviseParametersWithOutputConnectionToken[1] });
				}
			}
		};
	};
}
