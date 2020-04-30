using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WinForms = System.Windows.Forms;

namespace KSoft.WPF.Controls
{
	/// <summary>
	/// Interaction logic for SelectableFileControl.xaml
	/// </summary>
	public partial class SelectableFileControl : UserControl
	{
		#region Text
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string), typeof(SelectableFileControl),
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
			typeof(string), typeof(SelectableFileControl));
		#endregion

		#region InitialDirectory
		public string InitialDirectory
		{
			get { return (string)GetValue(InitialDirectoryProperty); }
			set { SetValue(InitialDirectoryProperty, value); }
		}
		public static readonly DependencyProperty InitialDirectoryProperty = DependencyProperty.Register(
			nameof(InitialDirectory),
			typeof(string), typeof(SelectableFileControl));
		#endregion

		#region FileFilter
		public string FileFilter
		{
			get { return (string)GetValue(FileFilterProperty); }
			set { SetValue(FileFilterProperty, value); }
		}
		public static readonly DependencyProperty FileFilterProperty = DependencyProperty.Register(
			nameof(FileFilter),
			typeof(string), typeof(SelectableFileControl),
			new PropertyMetadata("*.*"));
		#endregion

		#region CheckFileExists
		public bool CheckFileExists
		{
			get { return (bool)GetValue(CheckFileExistsProperty); }
			set { SetValue(CheckFileExistsProperty, value); }
		}
		public static readonly DependencyProperty CheckFileExistsProperty = DependencyProperty.Register(
			nameof(CheckFileExists),
			typeof(bool), typeof(SelectableFileControl));
		#endregion

		public SelectableFileControl()
		{
			InitializeComponent();
		}

		private void OnBrowseClick(object sender, RoutedEventArgs e)
		{
			using (var dlg = new WinForms.OpenFileDialog())
			{
				dlg.Title = Description;
				dlg.FileName = Text;
				dlg.CheckFileExists = CheckFileExists;
				dlg.Filter = FileFilter;
				var result = dlg.ShowDialog();
				if (result == WinForms.DialogResult.OK)
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
	};
}
