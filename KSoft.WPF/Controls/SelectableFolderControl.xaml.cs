﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace KSoft.WPF.Controls
{
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
		public static DependencyProperty TextProperty = DependencyProperty.Register(
			"Text",
			typeof(string), typeof(SelectableFolderControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		#endregion

		#region Description
		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}
		public static DependencyProperty DescriptionProperty = DependencyProperty.Register(
			"Description",
			typeof(string), typeof(SelectableFolderControl));
		#endregion

		public SelectableFolderControl()
		{
			InitializeComponent();
		}

		private void OnBrowseClick(object sender, RoutedEventArgs e)
		{
			using (var dlg = new WinForms.FolderBrowserDialog())
			{
				dlg.Description = Description;
				dlg.SelectedPath = Text;
				dlg.ShowNewFolderButton = true;
				var result = dlg.ShowDialog();
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
	};
}