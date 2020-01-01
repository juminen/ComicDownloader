namespace ComicDownloader.Repo
{
    static class TableNames
    {
        private static class Common
        {
            public static string Guid { get { return "Guid"; } }
        }

        public static class ComicTable
        {
            public static string Name { get { return "Comic"; } }
            public static class Columns
            {
                public static string Guid { get { return Common.Guid; } }
                public static string Name { get { return "Name"; } }
                public static string Url { get { return "Url"; } }
                public static string LastDownloadDate { get { return "LastDownloadDate"; } }
                public static string SavingLocation { get { return "SavingLocation"; } }

            }
        }

        public static class ComicPhotoTable
        {
            public static string Name { get { return "ComicPhoto"; } }
            public static class Columns
            {
                public static string Guid { get { return Common.Guid; } }
                public static string ReleaseDate { get { return "ReleaseDate"; } }
                public static string DownloadDate { get { return "DownloadDate"; } }
                public static string Status { get { return "Status"; } }
                public static string FilePath { get { return "FilePath"; } }
                public static string Url { get { return "Url"; } }
                public static string ComicGuid { get { return "ComicGuid"; } }
            }
        }
    }
}
