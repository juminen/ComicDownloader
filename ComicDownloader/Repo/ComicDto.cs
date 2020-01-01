using System;
using System.Collections.Generic;

namespace ComicDownloader.Repo
{
    class ComicDto : Identifier
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime LastDownloadDate { get; set; }
        public string SavingLocation { get; set; }
        public List<ComicPhotoDto> Photos { get; set; }

        public ComicDto()
        {
            Photos = new List<ComicPhotoDto>();
        }
    }
}