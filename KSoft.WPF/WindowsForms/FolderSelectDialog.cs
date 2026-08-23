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

		string? mInitialDirectory;
		string? mTitle;
		string? mFileName = "";

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
		public string? FileName { get { return mFileName; } }

		public bool ShowDialog()
		{
			return ShowDialog(IntPtr.Zero);
		}

		/// <param name="hWndOwner">Handle of the control or window to be the parent of the file dialog</param>
		/// <returns>true if the user clicks OK</returns>
		public bool ShowDialog(IntPtr hWndOwner)
		{
			var result = Environment.OSVersion.Version.Major >= 6 && gVistaDialogIsEnabled
				? VistaDialog.Show(hWndOwner, InitialDirectory, Title)
				: ShowXpDialog(hWndOwner, InitialDirectory, Title);
			mFileName = result.FileName;
			return result.Result;
		}

		struct ShowDialogResult
		{
			public bool Result { get; set; }
			public string? FileName { get; set; }
		};

		static ShowDialogResult ShowXpDialog(IntPtr ownerHandle, string initialDirectory, string title)
		{
			var dialogResult = new ShowDialogResult();
			using (var folderBrowserDialog = new FolderBrowserDialog
				{
					Description = title,
					SelectedPath = initialDirectory,
					ShowNewFolderButton = true,
					AutoUpgradeEnabled = true, // to get Vista style
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

		// Nope! This code is not 1:1 compatible with post-.netframework runtimes
		static bool gVistaDialogIsEnabled = false;
		static class VistaDialog
		{
			const BindingFlags kBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			readonly static Assembly gWindowsFormsAssembly = typeof(FileDialog).Assembly;
			// .netframework: System.Windows.Forms.FileDialogNative+IFileDialog
			// .net9:
			//	Assembly: System.Windows.Forms.Primitives
			//	Class:Windows.Win32.UI.Shell.IFileDialog
			readonly static Type? gIFileDialogType = gWindowsFormsAssembly.GetType("System.Windows.Forms.FileDialogNative+IFileDialog");
			// .net9 this returns ComScope<IFileDialog>, which is a ref struct
			readonly static MethodInfo? gCreateVistaDialogMethodInfo = typeof(OpenFileDialog).GetMethod("CreateVistaDialog", kBindingFlags);
			readonly static MethodInfo? gOnBeforeVistaDialogMethodInfo = typeof(OpenFileDialog).GetMethod("OnBeforeVistaDialog", kBindingFlags);
			readonly static MethodInfo? gGetOptionsMethodInfo = typeof(FileDialog).GetMethod("GetOptions", kBindingFlags);
			readonly static MethodInfo? gSetOptionsMethodInfo = gIFileDialogType?.GetMethod("SetOptions", kBindingFlags);
#if false
			readonly static uint gFosPickFoldersBitFlag = (uint) gWindowsFormsAssembly
				.GetType("System.Windows.Forms.FileDialogNative+FOS")
				.GetField("FOS_PICKFOLDERS")
				.GetValue(null);
#endif
			const uint kOptionFlags = (uint)(FOS.FOS_PICKFOLDERS | FOS.FOS_PATHMUSTEXIST);
			readonly static ConstructorInfo? gVistaDialogEventsConstructorInfo = gWindowsFormsAssembly
				.GetType("System.Windows.Forms.FileDialog+VistaDialogEvents")
				?.GetConstructor(kBindingFlags, null, [typeof(FileDialog)], null);
			readonly static MethodInfo? gAdviseMethodInfo = gIFileDialogType?.GetMethod("Advise");
			readonly static MethodInfo? gUnAdviseMethodInfo = gIFileDialogType?.GetMethod("Unadvise");
			readonly static MethodInfo? gShowMethodInfo = gIFileDialogType?.GetMethod("Show");

			private static T GetRequiredReflectionMember<T>(T? member, string memberName)
				where T : class
			{
				return member ?? throw new PlatformNotSupportedException(
					$"The Windows Forms member '{memberName}' is unavailable on this runtime.");
			}

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

				var createVistaDialog = GetRequiredReflectionMember(gCreateVistaDialogMethodInfo, nameof(gCreateVistaDialogMethodInfo));
				var onBeforeVistaDialog = GetRequiredReflectionMember(gOnBeforeVistaDialogMethodInfo, nameof(gOnBeforeVistaDialogMethodInfo));
				var getOptions = GetRequiredReflectionMember(gGetOptionsMethodInfo, nameof(gGetOptionsMethodInfo));
				var setOptions = GetRequiredReflectionMember(gSetOptionsMethodInfo, nameof(gSetOptionsMethodInfo));
				var createVistaDialogEvents = GetRequiredReflectionMember(gVistaDialogEventsConstructorInfo, nameof(gVistaDialogEventsConstructorInfo));
				var advise = GetRequiredReflectionMember(gAdviseMethodInfo, nameof(gAdviseMethodInfo));
				var unadvise = GetRequiredReflectionMember(gUnAdviseMethodInfo, nameof(gUnAdviseMethodInfo));
				var show = GetRequiredReflectionMember(gShowMethodInfo, nameof(gShowMethodInfo));
				var iFileDialog = createVistaDialog.Invoke(openFileDialog, Util.EmptyArray)
					?? throw new PlatformNotSupportedException("The Windows Forms Vista dialog could not be created.");
				onBeforeVistaDialog.Invoke(openFileDialog, [iFileDialog]);
				var options = getOptions.Invoke(openFileDialog, Util.EmptyArray)
					?? throw new PlatformNotSupportedException("The Windows Forms Vista dialog options could not be read.");
				setOptions.Invoke(iFileDialog, [(uint)options | kOptionFlags]);
				var dialogEvents = createVistaDialogEvents.Invoke([openFileDialog])
					?? throw new PlatformNotSupportedException("The Windows Forms Vista dialog event handler could not be created.");
				var adviseParametersWithOutputConnectionToken = new object[] { dialogEvents, 0U };
				advise.Invoke(iFileDialog, adviseParametersWithOutputConnectionToken);

				try
				{
					int retVal = (int)(show.Invoke(iFileDialog, [ownerHandle])
						?? throw new PlatformNotSupportedException("The Windows Forms Vista dialog could not be shown."));
					return new ShowDialogResult
					{
						Result = retVal == 0,
						FileName = openFileDialog.FileName
					};
				}
				finally
				{
					unadvise.Invoke(iFileDialog, [adviseParametersWithOutputConnectionToken[1]]);
				}
			}
		};
	};
}
