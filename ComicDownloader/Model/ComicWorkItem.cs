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
            Crawler = new ComicDataCrawler(ref comic, Photos, Log.Progress);
            Log = new DownloadLogger(Crawler) { Name = comic.Name };
            Photos = new ComicPhotoCollection();
        }

        public Comic Comic { get; private set; }
        public ComicDataCrawler Crawler { get; private set; }
        public DownloadLogger Log { get; private set; }
        public ComicPhotoCollection Photos { get; private set; }
    }
}
