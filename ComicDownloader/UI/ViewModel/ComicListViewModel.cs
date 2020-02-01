using ComicDownloader.Model;
using JMI.General.ListSelection;
using JMI.General.VM.ListSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
