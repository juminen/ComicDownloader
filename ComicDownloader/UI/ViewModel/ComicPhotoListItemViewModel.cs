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
            this.photo = photo.Target;
            this.photo.PropertyChanged += OnComicPropertyChanged;
        }
        #endregion

        #region properties
        private readonly ComicPhoto photo;

        public string Comic
        {
            get { return photo.Parent.Name; }
        }

        public string PublishDate
        {
            get { return photo.PublishDate.ToString("dd.MM.yyyy"); }
        }

        public string DownloadDate
        {
            get { return photo.DownloadDate.ToString("dd.MM.yyyy"); }
        }

        public string Status
        {
            get { return photo.Status; }
        }

        public string AbsoluteFilePath
        {
            get { return photo.AbsoluteFilePath; }
        }

        public string Url
        {
            get { return photo.Url; }
        }

        public string SortByComicNameAndPublishDate
        {
            get { return $"{ photo.DisplayText }"; }
        }
        #endregion

        #region methods
        public override void Dispose()
        {
            base.Dispose();
            photo.PropertyChanged -= OnComicPropertyChanged;
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
