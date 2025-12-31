using System;
using System.Windows.Input;

namespace KSoft.WPF
{
	public class RelayCommand
		: ICommand
	{
		private readonly Predicate<object> mCanExecute;
		private readonly Action<object> mExecute;

		public RelayCommand(Predicate<object> canExecute, Action<object> execute)
		{
			this.mCanExecute = canExecute;
			this.mExecute = execute;
		}

		/// <summary>Create a command that can always execute</summary>
		/// <param name="execute"></param>
		public RelayCommand(Action<object> execute)
			: this(null, execute)
		{
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute(object parameter)
		{
			if (mCanExecute == null)
			{
				return true;
			}

			return mCanExecute(parameter);
		}

		public void Execute(object parameter)
		{
			if (mExecute != null)
			{
#pragma warning disable IDE1005 // Delegate invocation can be simplified.
				mExecute(parameter);
#pragma warning restore IDE1005 // Delegate invocation can be simplified.
			}
		}
	}
}
