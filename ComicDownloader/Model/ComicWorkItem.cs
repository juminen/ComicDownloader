using System;
using System.Collections.Concurrent;

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
            Photos = new BlockingCollection<ComicPhoto>();
            Log = new DownloadLogger() { Name = comic.Name };
            Crawler = new ComicDataCrawler(ref comic, Photos, Log.Progress);
            Log.Status = Crawler;
        }

        public Comic Comic { get; private set; }
        public ComicDataCrawler Crawler { get; private set; }
        public DownloadLogger Log { get; private set; }
        public BlockingCollection<ComicPhoto> Photos { get; private set; }

        public void MoveDownloadedPhotoInfosToComic()
        {
            Comic.Photos.AddRange(Photos);
        }
    }
}
