using System;
using System.Collections.Generic;
using System.Linq;

namespace ComicDownloader.Model
{
    public class Comic : BaseComicStripObject
    {
        #region constructors
        public Comic() : base()
        {

        }

        public Comic(Guid guid) : base(guid)
        {
            
        }
        #endregion

        #region properties
        private string name;
        /// <summary>
        /// Name of the comic
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                SetProperty(ref name, value);
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        private string startUrl;
        /// <summary>
        /// Start url for download
        /// </summary>
        public string StartUrl
        {
            get { return startUrl; }
            set { SetProperty(ref startUrl, value); }
        }

        private DateTime lastDownloadDate;
        /// <summary>
        /// Date when comic photos was last downloaded
        /// </summary>
        public DateTime LastDownloadDate
        {
            get { return lastDownloadDate; }
            set { SetProperty(ref lastDownloadDate, value); }
        }

        private string savingLocation;
        /// <summary>
        /// Directory path where comic photos are saved.
        /// </summary>
        public string SavingLocation
        {
            get { return savingLocation; }
            set { SetProperty(ref savingLocation, value); }
        }

        /// <summary>
        /// Returns <see cref="Name"/>
        /// </summary>
        public override string DisplayText => Name;

        public List<ComicPhoto> Photos { get; set; } = new List<ComicPhoto>();
        #endregion

        #region methods
        /// <summary>
        /// Returns <see cref="Name"/> in lowercase for url. Scands are replaced with letter without umlaut and spaces are removed.
        /// </summary>
        /// <returns></returns>
        public string GetUrlName()
        {
            string s = Name.ToLower();
            s = s.Replace('å', 'a');
            s = s.Replace('ä', 'a');
            s = s.Replace('ö', 'o');
            s = s.Replace(" ", "");
            return s;
        }

        /// <summary>
        /// Returns <see cref="Name"/> for file name. Scands are replaced with letter without umlaut and 
        /// spaces are replaced with underscore.
        /// </summary>
        /// <returns></returns>
        public string GetFileName()
        {
            string s = Name.Replace('å', 'a');
            s = s.Replace('Å', 'A');
            s = s.Replace('ä', 'a');
            s = s.Replace('Ä', 'A');
            s = s.Replace('ö', 'o');
            s = s.Replace('Ö', 'O');
            s = s.Replace(" ", "_");
            return s;
        }

        /// <summary>
        /// Returns count of photos for given date (used to check ih there are more than one photo per day)
        /// </summary>
        /// <param name="publishDate">Publish date of <see cref="ComicPhoto"/></param>
        /// <returns>integer</returns>
        public int GetPhotoCountByRealeaseDate(DateTime publishDate)
        {
            int count = Photos.Count(x => x.PublishDate.Equals(publishDate));
            return count;
        }

        public override void Dispose()
        {
            Photos.ForEach(p => p.Dispose());
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion
    }
}
