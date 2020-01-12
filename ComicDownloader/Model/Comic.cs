using System;
using System.Collections.Generic;

namespace ComicDownloader.Model
{
    public class Comic : Identifier
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

        public override void Dispose()
        {
            Photos.ForEach(p => p.Dispose());
        }

        ///// <summary>
        ///// Validates <see cref="Name"/>, <see cref="SavingLocation"/> and <see cref="StartUrl"/>.
        ///// </summary>
        //public ActionResult<bool> ValidateData()
        //{
        //    ActionResult<bool> ar = new ActionResult<bool>();
        //    int lenght = 2;
        //    if (name.Length < lenght)
        //    {
        //        ar.AddFailReason($"Name must be longer than {lenght.ToString()}.");
        //    }

        //    if (!System.IO.Directory.Exists(SavingLocation))
        //    {
        //        ar.AddFailReason($"Given saving location does not exist.");
        //    }

        //    bool result = Uri.TryCreate(StartUrl, UriKind.Absolute, out Uri uriResult)
        //        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        //    if (!result)
        //    {
        //        ar.AddFailReason($"Given start url is not valid http address.");
        //    }
        //    return ar;
        //}
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion
    }
}
