using JMI.General.Logging;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ComicDownloader.Model
{
    class ComicPhotoDownloader
    {
        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="comicPhoto">Photo details for dowload</param>
        /// /// <param name="progressReporter">For progress reporting</param>
        public ComicPhotoDownloader(ComicPhoto comicPhoto, IProgress<ILogMessage> progress)
        {
            photo = comicPhoto ?? throw new ArgumentNullException(nameof(comicPhoto));
            progressReporter = progress ?? throw new ArgumentNullException(nameof(progress) + " can not be null");
        }
        #endregion

        #region properties
        private readonly Logger logger = SingletonLogger.Instance;
        private ComicPhoto photo;
        private byte[] imageData;
        private IProgress<ILogMessage> progressReporter;
        #endregion

        #region methods
        private void ReportProgress(ILogMessage message)
        {
            progressReporter.Report(message);
        }

        private void CheckCreateDirectory()
        {
            string dir = Path.GetDirectoryName(photo.AbsoluteFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                string msg = $"Created directory '{dir}'.";
                logger.Log(LogFactory.CreateNormalMessage(msg));
            }
        }

        private async Task<bool> DownloadImageDataAsync()
        {
            string s = $"Downloading '{ photo.DisplayText }'...";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            using (WebClient wc = new WebClient())
            {
                try
                {
                    imageData = await wc.DownloadDataTaskAsync(photo.Url);
                }
                catch (Exception ex)
                {
                    s = $"Download '{ photo.DisplayText }' failed: {ex.Message}";
                    photo.SetDownloadDateToNow();
                    ReportProgress(LogFactory.CreateWarningMessage(s));
                    return false;
                }
            }
            s = $"Image '{ photo.DisplayText }' downloaded for saving to file.";
            photo.SetDownloadDateToNow();
            ReportProgress(LogFactory.CreateNormalMessage(s));
            return true;
        }

        private void SaveImageData()
        {
            string s = $"Saving to '{ photo.AbsoluteFilePath }'...";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            using (MemoryStream stream = new MemoryStream(imageData))
            {
                try
                {
                    Image image = Image.FromStream(stream);
                    image.Save(photo.AbsoluteFilePath, System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    s = $"Saving to '{ photo.AbsoluteFilePath }' failed: {ex.Message}";
                    ReportProgress(LogFactory.CreateWarningMessage(s));
                    return;
                }
            }
            s = $"Image '{ photo.AbsoluteFilePath }' saved.";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            photo.Status = "image downloaded";
        }

        /// <summary>
        /// Downloads comic photo
        /// </summary>
        public async Task GetComicPhoto()
        {
            if (File.Exists(photo.AbsoluteFilePath))
            {
                string s = $"Image file '{ photo.AbsoluteFilePath }' was already in the directory.";
                ReportProgress(LogFactory.CreateWarningMessage(s));
                return;
            }

            CheckCreateDirectory();

            bool result = await DownloadImageDataAsync();
            if (result)
            {
                SaveImageData();
            }
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion
    }
}
