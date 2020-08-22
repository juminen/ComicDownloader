using ComicDownloader.Model;
using JMI.General.Selections;
using JMI.General.VM.Selections;
namespace ComicDownloader.UI.ViewModel
{
    class ComicListItemViewModel : SelectionItemViewModel<Comic>
    {
        public ComicListItemViewModel(ISelectionItem<Comic> comic)
            : base(comic)
        {
            item.Target.PropertyChanged += OnComicPropertyChanged;
        }

        private void OnComicPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Comic.LastDownloadDate)))
            {
                OnPropertyChanged(nameof(LastDownloadDate));
            }
            else if (e.PropertyName.Equals(nameof(Comic.DisplayText)))
            {
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public string DisplayText => item.Target.DisplayText;

        public  string LastDownloadDate
        {
            get { return item.Target.LastDownloadDate.ToString("dd.MM.yyyy"); }
        }
    }
}
