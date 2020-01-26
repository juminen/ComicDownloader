using JMI.General.Logging;
using System;

namespace ComicDownloader.Model
{
    /// <summary>
    /// Work item is used only to dowload comic photo information
    /// </summary>
    class ComicWorkItem
    {
        public ComicWorkItem(Comic comic)
        {

            Comic = comic ?? throw new ArgumentNullException(nameof(comic) + " can not be null");
            Photos = new ComicPhotoCollection();
            Log = new ProgressLogger() { Name = comic.Name };
            //Progress<ILogMessage> progress = new Progress<ILogMessage>();
            //progress.ProgressChanged += OnProgressChanged;
            Crawler = new ComicDataCrawler(ref comic, Photos, Log.Progress);
            Photos = new ComicPhotoCollection();
        }

        public Comic Comic { get; private set; }
        public ComicDataCrawler Crawler { get; private set; }
        public ProgressLogger Log { get; private set; }
        public ComicPhotoCollection Photos { get; private set; }

        //private void OnProgressChanged(object sender, ILogMessage e)
        //{
        //    Log.Log(e);
        //}
    }
}
