using ComicDownloader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        #endregion

        #region properties
        private readonly ComicManager manager;

        public DownloadViewModel Downloads { get; private set; }
        public ComicsViewModel Comics { get; private set; }
        //public DownloadViewModel Comics { get; private set; }
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
