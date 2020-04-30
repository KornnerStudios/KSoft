using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WinForms = System.Windows.Forms;

namespace KSoft.WPF.Controls
{
	// #TODO: use CommonOpenFileDialog (need to defer to a nuget) https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/source/WindowsAPICodePack/Shell/CommonFileDialogs/CommonOpenFileDialog.cs

	/// <summary>
	/// Interaction logic for SelectableFolderControl.xaml
	/// </summary>
	public partial class SelectableFolderControl : UserControl
	{
		#region Text
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string), typeof(SelectableFolderControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		#endregion

		#region Description
		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}
		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
			nameof(Description),
			typeof(string), typeof(SelectableFolderControl));
		#endregion

		public SelectableFolderControl()
		{
			InitializeComponent();
		}

		private void OnBrowseClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			OnBrowseClickUsingFolderSelectDialog(sender, e);
		}

		private void OnBrowseClickUsingFolderBrowserDialog(object sender, RoutedEventArgs e)
		{
			var parentWindowHandleWrapper = WindowsForms.Win32WindowHandleWrapper.FromDependencyObject(this);

			using (var dlg = new WinForms.FolderBrowserDialog())
			{
				dlg.Description = Description;
				dlg.SelectedPath = Text;
				dlg.ShowNewFolderButton = true;
				var result = dlg.ShowDialog(parentWindowHandleWrapper);
				if (result == WinForms.DialogResult.OK)
				{
					Text = dlg.SelectedPath;
					BindingExpression be = GetBindingExpression(TextProperty);
					if (be != null)
					{
						// Textbox bindings are only updated on the lostfocus event.
						be.UpdateSource();
					}
				}
			}
		}

		private void OnBrowseClickUsingFolderSelectDialog(object sender, RoutedEventArgs e)
		{
			var parentWindowHandleWrapper = WindowsForms.Win32WindowHandleWrapper.FromDependencyObject(this);

			var dlg = new WindowsForms.FolderSelectDialog();
			{
				dlg.Title = Description;
				dlg.InitialDirectory = Text;
				if (dlg.ShowDialog(parentWindowHandleWrapper.Handle))
				{
					Text = dlg.FileName;
					BindingExpression be = GetBindingExpression(TextProperty);
					if (be != null)
					{
						// Textbox bindings are only updated on the lostfocus event.
						be.UpdateSource();
					}
				}
			}
		}

		private void OnClearClick(object sender, RoutedEventArgs e)
		{
			Text = "";
		}
	};
}
