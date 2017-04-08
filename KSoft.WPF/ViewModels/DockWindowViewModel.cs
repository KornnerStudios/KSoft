using System.Windows.Input;

namespace KSoft.WPF.ViewModels
{
	public class DockWindowViewModel
		: ObjectModel.BasicViewModel
	{
		#region Title
		string mTitle;
		public string Title
		{
			get { return mTitle; }
			set { SetFieldObj(ref mTitle, value); }
		}
		#endregion

		#region CanClose
		bool mCanClose = true;
		public bool CanClose
		{
			get { return mCanClose; }
			set { SetFieldVal(ref mCanClose, value); }
		}
		#endregion

		#region IsClosed
		bool mIsClosed;
		public bool IsClosed
		{
			get { return mIsClosed; }
			set { SetFieldVal(ref mIsClosed, value); }
		}
		#endregion

		#region CloseCommand
		ICommand mCloseCommand;
		public ICommand CloseCommand { get {
			if (mCloseCommand == null)
				mCloseCommand = new RelayCommand(_ => this.Close());

			return mCloseCommand;
		} }
		#endregion

		public void Close()
		{
			if (CanClose)
				IsClosed = true;
		}
	};
}