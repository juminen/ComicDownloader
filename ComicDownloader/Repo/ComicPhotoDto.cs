using System;

namespace ComicDownloader.Repo
{
    public class ComicPhotoDto : Identifier
    {
        public Guid ComicGuid { get; set; }
        public ComicDto Comic { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime DownloadDate { get; set; }
        public string Status { get; set; }
        public string RelativeFilePath { get; set; }
        public string Url { get; set; }
    }
}
