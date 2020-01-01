using System;

namespace ComicDownloader.Repo
{
    class ComicPhotoDto : Identifier
    {
        public Guid ComicGuid { get; set; }
        public ComicDto Comic { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime DownloadDate { get; set; }
        public string Status { get; set; }
        public string FilePath { get; set; }
        public string Url { get; set; }
    }
}
