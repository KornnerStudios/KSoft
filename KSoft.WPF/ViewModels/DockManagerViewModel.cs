using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.ComponentModel;

namespace KSoft.WPF.ViewModels
{
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class DockManagerViewModel
		: ObjectModel.BasicViewModel
		, IEnumerable<DockWindowViewModel>
	{
		#region Documents
		ObservableCollection<DockWindowViewModel> mDocuments;
		[SuppressMessage("Microsoft.Design", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public ObservableCollection<DockWindowViewModel> Documents
		{
			get { return mDocuments; }
			set { SetField(ref mDocuments, value, overrideChecks: true); }
		}
		#endregion

		#region Anchorables
		ObservableCollection<object> mAnchorables;
		[SuppressMessage("Microsoft.Design", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public ObservableCollection<object> Anchorables
		{
			get { return mAnchorables; }
			set { SetField(ref mAnchorables, value, overrideChecks: true); }
		}
		#endregion

		public void InitializeObservableCollections()
		{
			if (Documents == null)
				Documents = new ObservableCollection<DockWindowViewModel>();

			if (Anchorables == null)
				Anchorables = new ObservableCollection<object>();
		}

		private void DockWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			DockWindowViewModel document = sender as DockWindowViewModel;

			if (e.PropertyName == nameof(DockWindowViewModel.IsClosed))
			{
				if (document.IsClosed)
					CloseDocument(document);
				else
					OpenDocument(document);
			}
		}

		/// <summary>Add the window to this manager</summary>
		/// <param name="doc"></param>
		/// <returns>False if the document is null or wasn't also added to Documents because IsClosed</returns>
		public bool AddDocument(DockWindowViewModel doc)
		{
			if (doc == null)
				return false;

			if (Documents == null)
				Documents = new ObservableCollection<DockWindowViewModel>();

			doc.PropertyChanged += DockWindowViewModel_PropertyChanged;

			if (doc.IsClosed)
				return false;

			Documents.Add(doc);
			return true;
		}

		/// <summary>Remove the window from this manager</summary>
		/// <param name="doc"></param>
		/// <returns>False if the document is null, or not in the manager</returns>
		public bool RemoveDocument(DockWindowViewModel doc)
		{
			if (doc == null)
				return false;

			if (Documents == null)
				return false;

			doc.PropertyChanged -= DockWindowViewModel_PropertyChanged;
			return Documents.Remove(doc);
		}

		private void OpenDocument(DockWindowViewModel doc)
		{
			if (doc == null)
				return;

			Documents.Add(doc);
		}

		private void CloseDocument(DockWindowViewModel doc)
		{
			if (doc == null)
				return;

			Documents.Remove(doc);
		}

		public void Clear()
		{
			if (Documents == null)
				return;

			Documents.Clear();
		}

		public bool ContainsInstanceOf<TViewModel>(out TViewModel viewModel)
			where TViewModel : DockWindowViewModel
		{
			viewModel = null;

			if (Documents == null)
				return false;

			foreach (var obj in Documents)
			{
				if (!(obj is TViewModel vm))
					continue;

				viewModel = vm;
			}

			return viewModel != null;
		}

		public IEnumerator<DockWindowViewModel> GetEnumerator()
		{
			if (Documents == null)
				return Enumerable.Empty<DockWindowViewModel>().GetEnumerator();

			return ((IEnumerable<DockWindowViewModel>)Documents).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (Documents == null)
				return Enumerable.Empty<DockWindowViewModel>().GetEnumerator();

			return Documents.GetEnumerator();
		}
	};
}
