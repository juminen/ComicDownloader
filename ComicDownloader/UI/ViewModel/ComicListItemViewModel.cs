using ComicDownloader.Model;
using JMI.General.VM.ListSelection;
namespace ComicDownloader.UI.ViewModel
{
    class ComicListItemViewModel : SelectionCollectionItemViewModel
    {
        public ComicListItemViewModel(Comic comic) : base(comic)
        {
            this.comic = comic;
            this.comic.PropertyChanged += OnComicPropertyChanged;
        }

        private void OnComicPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Comic.LastDownloadDate)))
            {
                OnPropertyChanged(nameof(LastDownloadDate));
            }
        }

        private readonly Comic comic;

        public  string LastDownloadDate
        {
            get { return comic.LastDownloadDate.ToString("dd.MM.yyyy"); }
        }
    }
}
