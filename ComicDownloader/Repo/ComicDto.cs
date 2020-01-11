using System;
using System.Collections.Generic;

namespace ComicDownloader.Repo
{
    public class ComicDto : Identifier
    {
        public string Name { get; set; }
        public string StartUrl { get; set; }
        public DateTime LastDownloadDate { get; set; }
        public string SavingLocation { get; set; }
        public List<ComicPhotoDto> Photos { get; set; }

        public ComicDto()
        {
            Photos = new List<ComicPhotoDto>();
        }
    }
}