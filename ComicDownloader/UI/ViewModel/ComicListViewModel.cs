using ComicDownloader.Model;
using JMI.General.Selections;
using JMI.General.VM.Selections;
using System.ComponentModel;

namespace ComicDownloader.UI.ViewModel
{
    class ComicListViewModel : SelectionCollectionViewModel<Comic, ComicListItemViewModel>
    {
        public ComicListViewModel(ComicCollection selectionCollection) : base(selectionCollection)
        {
            SetSorting();
        }

        protected override ComicListItemViewModel CreateViewModel(ISelectionItem<Comic> item)
        {
            return new ComicListItemViewModel(item);
        }

        private void SetSorting()
        {
            ClearSorting();
            AllItems.SortDescriptions.Add(new SortDescription(nameof(ComicListItemViewModel.DisplayText), ListSortDirection.Ascending));

        }
    }
}
