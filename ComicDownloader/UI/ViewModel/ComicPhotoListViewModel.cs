using ComicDownloader.Model;
using JMI.General.VM.ListSelection;

namespace ComicDownloader.UI.ViewModel
{
    class ComicPhotoListViewModel : SelectionCollectionViewModel<ComicPhoto, ComicPhotoListItemViewModel>
    {
        public ComicPhotoListViewModel(ComicPhotoCollection selectionCollection) : base(selectionCollection)
        {
            SetSorting();
        }

        protected override ComicPhotoListItemViewModel CreateViewModel(ComicPhoto item)
        {
            return new ComicPhotoListItemViewModel(item);
        }
    }
}
