using ComicDownloader.Model;
using System;

namespace ComicDownloader.UI.ViewModel
{
    class ComicManagerViewModel
    {
        #region constructors
        public ComicManagerViewModel(ComicManager comicManager)
        {
            manager = comicManager ?? throw new ArgumentNullException(nameof(comicManager) + " can not be null");
            Downloads = new DownloadViewModel(manager);
            Comics = new ComicsViewModel(manager);
            Photos = new ComicPhotosViewModel(manager);
        }
        #endregion

        #region properties
        private readonly ComicManager manager;

        public DownloadViewModel Downloads { get; private set; }
        public ComicsViewModel Comics { get; private set; }
        public ComicPhotosViewModel Photos { get; private set; }
        #endregion

        #region commands
        #endregion

        #region methods
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion

    }
}
