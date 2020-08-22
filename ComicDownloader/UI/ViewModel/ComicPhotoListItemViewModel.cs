using ComicDownloader.Model;
using JMI.General.Selections;
using JMI.General.VM.Selections;

namespace ComicDownloader.UI.ViewModel
{
    class ComicPhotoListItemViewModel : SelectionItemViewModel<ComicPhoto>
    {
        #region constructors
        public ComicPhotoListItemViewModel(ISelectionItem<ComicPhoto> photo) : base(photo)
        {
            Photo = photo.Target;
            Photo.PropertyChanged += OnComicPropertyChanged;
        }
        #endregion

        #region properties
        public readonly ComicPhoto Photo;

        public string Comic
        {
            get { return Photo.Parent.Name; }
        }

        public string PublishDate
        {
            get { return Photo.PublishDate.ToString("dd.MM.yyyy"); }
        }

        public string DownloadDate
        {
            get { return Photo.DownloadDate.ToString("dd.MM.yyyy"); }
        }

        public string Status
        {
            get { return Photo.Status; }
        }

        public string AbsoluteFilePath
        {
            get { return Photo.AbsoluteFilePath; }
        }

        public string Url
        {
            get { return Photo.Url; }
        }

        public string SortByComicNameAndPublishDate
        {
            get { return $"{ Photo.DisplayText }"; }
        }
        #endregion

        #region methods
        public override void Dispose()
        {
            base.Dispose();
            Photo.PropertyChanged -= OnComicPropertyChanged;
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        private void OnComicPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("");
        }
        #endregion
    }
}
