using ComicDownloader.Model;
using JMI.General.VM.ListSelection;

namespace ComicDownloader.UI.ViewModel
{
    class ComicListViewModel : SelectionCollectionViewModel<Comic, ComicListItemViewModel>
    {
        public ComicListViewModel(ComicCollection selectionCollection) : base(selectionCollection)
        {
        }

        protected override ComicListItemViewModel CreateViewModel(Comic item)
        {
            return new ComicListItemViewModel(item);
        }
    }
}
