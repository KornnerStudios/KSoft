using System.Windows.Input;
using KSoft.PropertyChanged.SourceGeneration;

namespace KSoft.WPF.ViewModels
{
	public partial class DockWindowViewModel
		: ObjectModel.BasicViewModel
	{
		#region Title
		[GeneratedPropertyChanged]
		public partial string? Title { get; set; }
		#endregion

		#region CanClose
		[GeneratedPropertyChanged]
		public partial bool CanClose { get; set; } = true;
		#endregion

		#region IsClosed
		[GeneratedPropertyChanged]
		public partial bool IsClosed { get; set; }
		#endregion

		#region CloseCommand
		ICommand? mCloseCommand;
		public ICommand CloseCommand { get {
			if (mCloseCommand == null)
			{
				mCloseCommand = new RelayCommand(_ => this.Close());
			}

			return mCloseCommand;
		} }
		#endregion

		public void Close()
		{
			if (CanClose)
			{
				IsClosed = true;
			}
		}
	};
}