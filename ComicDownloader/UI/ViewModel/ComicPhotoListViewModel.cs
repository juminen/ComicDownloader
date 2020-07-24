using ComicDownloader.Model;
using JMI.General.Selections;
using JMI.General.Sorting;
using JMI.General.VM.Selections;
using System.Collections;
using System.ComponentModel;

namespace ComicDownloader.UI.ViewModel
{
    class ComicPhotoListViewModel : SelectionCollectionViewModel<ComicPhoto, ComicPhotoListItemViewModel>
    {
        public ComicPhotoListViewModel(ComicPhotoCollection selectionCollection) : base(selectionCollection)
        {
            SetSorting();
        }

        protected override ComicPhotoListItemViewModel CreateViewModel(ISelectionItem<ComicPhoto> item)
        {
            return new ComicPhotoListItemViewModel(item);
        }

        private void SetSorting()
        {
            ClearSorting();
            AllItems.SortDescriptions.Add(new SortDescription(nameof(ComicPhotoListItemViewModel.SortByComicNameAndPublishDate), ListSortDirection.Ascending));
        }

        //TODO: poista
        private class ComicSorting : IComparer
        {
            public int Compare(object x, object y)
            {
                if (!(x is ComicPhotoListItemViewModel item1))
                {
                    return 0;
                }
                if (!(y is ComicPhotoListItemViewModel item2))
                {
                    return 0;
                }
                AlphanumComparatorFast comp = new AlphanumComparatorFast();
                return comp.Compare(item1.Comic, item2.Comic);
            }
        }
    }
}
