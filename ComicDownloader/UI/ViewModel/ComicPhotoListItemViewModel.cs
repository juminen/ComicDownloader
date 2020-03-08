using ComicDownloader.Model;
using JMI.General.VM.ListSelection;

namespace ComicDownloader.UI.ViewModel
{
    class ComicPhotoListItemViewModel : SelectionCollectionItemViewModel
    {
        #region constructors
        public ComicPhotoListItemViewModel(ComicPhoto photo) : base(photo)
        {
            this.photo = photo;
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
