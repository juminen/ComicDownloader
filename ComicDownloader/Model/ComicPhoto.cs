using System;

namespace ComicDownloader.Model
{
    public class ComicPhoto : BaseComicStripObject
    {
        #region constructors
        public ComicPhoto(Comic comic) : base()
        {
            SetParent(comic);
        }
        public ComicPhoto(Guid guid, Comic comic) : base(guid)
        {
            SetParent(comic);
        }
        #endregion

        #region properties
        public Comic Parent { get; private set; }

        private DateTime publishDate;
        public DateTime PublishDate
        {
            get { return publishDate; }
            set
            {
                SetProperty(ref publishDate, value);
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        private DateTime downloadDate;
        public DateTime DownloadDate
        {
            get { return downloadDate; }
            set { SetProperty(ref downloadDate, value); }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        private string relativeFilePath;
        public string RelativeFilePath
        {
            get { return relativeFilePath; }
            set
            {
                SetProperty(ref relativeFilePath, value);
                OnPropertyChanged(nameof(AbsoluteFilePath));
            }
        }

        public string AbsoluteFilePath
        {
            get
            {
                return $"{ System.IO.Path.Combine(Parent.SavingLocation, RelativeFilePath) }";
            }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { SetProperty(ref url, value); }
        }

        public override string DisplayText
        {
            get
            {
                return $"{ Parent.DisplayText } { publishDate.ToString("yyyy-MM-dd") }";
            }
        }
        #endregion

        #region methods
        private void SetParent(Comic comic)
        {
            Parent = comic ?? throw new ArgumentNullException(nameof(comic) + " can not be null");
            Parent.PropertyChanged += OnParentPropertyChanged;
        }

        public override void Dispose()
        {
            if (Parent != null)
            {
                Parent.PropertyChanged -= OnParentPropertyChanged;
            }
        }

        public void SetDownloadDateToNow()
        {
            DownloadDate = DateTime.Now;
        }

        public void SetDefaultRelativePath()
        {
            string path = $"{ CreateDefaultRelativePathWithoutExtension() }{ GetFileExtension() }";
            RelativeFilePath = path;
        }

        public void SetDefaultRelativePath(int count)
        {
            string path = $"{ CreateDefaultRelativePathWithoutExtension() }_{ count.ToString() }{ GetFileExtension() }";
            RelativeFilePath = path;
        }

        private string CreateDefaultRelativePathWithoutExtension()
        {
            string separator = System.IO.Path.DirectorySeparatorChar.ToString();
            string comicName = Parent.GetFileName();
            string year = PublishDate.Year.ToString();
            string filename = comicName + "_" + PublishDate.ToString("yyyy-MM-dd");
            string path = $"{ comicName }{ separator }{ year }{ separator }{ filename }";
            return path;
        }

        private string GetFileExtension()
        {
            return ".png";
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        private void OnParentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Comic.SavingLocation)))
            {
                OnPropertyChanged(nameof(AbsoluteFilePath));
            }
            else if (e.PropertyName.Equals(nameof(Comic.DisplayText)))
            {
                OnPropertyChanged(nameof(DisplayText));
            }
        }
        #endregion
    }
}
