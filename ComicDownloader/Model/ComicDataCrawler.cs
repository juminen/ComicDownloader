using JMI.General.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ComicDownloader.Model
{
    /// <summary>
    /// Class for crawling data from web page
    /// </summary>
    class ComicDataCrawler : IIsFinished
    {
        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="comic"><see cref="Comic"/> which data will be downloaded</param>
        /// <param name="containerForPhotos">Container for <see cref="ComicPhoto"/> constructed from downloaded data</param>
        /// <param name="progressReporter">For progress reporting</param>
        public ComicDataCrawler(
            ref Comic comic,
            BlockingCollection<ComicPhoto> containerForPhotos,
            //ComicPhotoCollection containerForPhotos,
            IProgress<ILogMessage> progress)
        {
            this.comic = comic ?? throw new ArgumentNullException(nameof(comic));
            comicPhotos = containerForPhotos ?? throw new ArgumentNullException(nameof(containerForPhotos));
            progressReporter = progress ?? throw new ArgumentNullException(nameof(progress) + " can not be null");
            IsFinished = false;
        }
        #endregion

        #region properties
        private Comic comic;
        private string siteContent = string.Empty;
        private string currentAddress = string.Empty;
        private BlockingCollection<ComicPhoto> comicPhotos;
        //private ComicPhotoCollection comicPhotos;
        private IProgress<ILogMessage> progressReporter;
        private CancellationTokenSource cancelTokenSource;

        private bool isFinished;
        public bool IsFinished
        {
            get { return isFinished; }
            private set
            {
                isFinished = value;
                if (isFinished)
                {
                    Finished?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region methods
        private void ReportProgress(ILogMessage message)
        {
            progressReporter.Report(message);
        }

        public async Task DownloadDataAsync(CancellationTokenSource cts)
        {
            cancelTokenSource = cts;
            IsFinished = false;
            //Download main site
            bool mainsiteDownloaded = await GetSiteContentAsync(comic.StartUrl);
            if (!mainsiteDownloaded)
            {
                IsFinished = true;
                return;
            }

            //Search for starting web page address
            bool startUriFound = GetStartUri(siteContent);
            if (!startUriFound)
            {
                IsFinished = true;
                return;
            }

            //while (true)
            while (!cancelTokenSource.IsCancellationRequested)
            {
                bool siteDownloaded = await GetSiteContentAsync(currentAddress);
                if (!siteDownloaded)
                {
                    IsFinished = true;
                    return;
                }
                if (!GetComicPhotoInfo())
                {
                    IsFinished = true;
                    return;
                }
            }
            IsFinished = true;
        }

        private async Task<bool> GetSiteContentAsync(string url)
        {
            string s = string.Empty;
            if (string.IsNullOrWhiteSpace(url))
            {
                s = "Url was empty.";
                ReportProgress(LogFactory.CreateErrorMessage(s));
                return false;
            }

            //WebClient wc = new WebClient();
            try
            {
                s = $"Downloading page '{url}'...";
                ReportProgress(LogFactory.CreateNormalMessage(s));
                //siteContent = await wc.DownloadStringTaskAsync(url);
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url, cancelTokenSource.Token))
                    {
                        siteContent = await response.Content.ReadAsStringAsync();
                    }
                }
            }

            catch (Exception ex)
            {
                s = $"Page {url} download failed.\n{ex.Message}";
                ReportProgress(LogFactory.CreateErrorMessage(s));
                return false;
            }
            s = $"Page '{url}' downloaded.";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            return true;
        }

        private bool GetStartUri(string siteContent)
        {
            string s = $"Searching for web page address for first image...";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            //<meta itemprop="contentUrl" content="/fingerpori/car-2000005237070.html">
            string find = $"{comic.GetUrlName()}/car.+"; //returns: fingerpori/car-2000005948557.html">
            Regex regu = new Regex(find);

            MatchCollection matches = regu.Matches(siteContent);
            if (matches.Count < 1)
            {
                s = "Did not found address from main page.";
                ReportProgress(LogFactory.CreateWarningMessage(s));
                return false;
            }

            //remove double quote and greater-than from the end
            currentAddress = matches[0].Value.Replace("\">", string.Empty);
            //address should be: 
            //fingerpori /car-2000005948557.html 

            //comic start url shoud be:
            //http://www.hs.fi/fingerpori/

            //search longest common substring from current and start address
            string common = FindLongestCommonSubstring(comic.StartUrl, currentAddress);

            //common string should be:
            //fingerpori/

            // substitute start url common string with current address
            // --> get image page url
            //http://www.hs.fi/fingerpori/car-2000004894832.html
            currentAddress = comic.StartUrl.Replace(common, currentAddress);
            s = $"Found address '{currentAddress}'.";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            return true;
        }

        private bool GetComicPhotoInfo()
        {
            //TODO: publishDate kusahtaa, on nollana
            if (!GetDate(out DateTime publishDate))
            {
                return false;
            }

            if (publishDate <= comic.LastDownloadDate)
            {
                comic.LastDownloadDate = DateTime.Today;
                return false;
            }

            string photoUrl = string.Empty;
            if (!GetPhotoUrl(out photoUrl))
            {
                return false;
            }

            ComicPhoto cp = new ComicPhoto(comic)
            {
                PublishDate = publishDate,
                Url = photoUrl,
                Status = "information downloaded, image not downloaded"
            };

            CreateFilePathForPhoto(ref cp);

            string s = $"Created comic photo {cp.DisplayText}";
            ReportProgress(LogFactory.CreateNormalMessage(s));

            //Check that there are no duplicate photos
            if (!comic.Photos.Any(p => p.PublishDate == cp.PublishDate && p.Url == cp.Url))
            {
                //comic.Photos.Add(cp);
                comicPhotos.Add(cp);
            }
            else
            {
                s = $"Comic {cp.DisplayText} was duplicate, not added.";
                ReportProgress(LogFactory.CreateWarningMessage(s));
            }

            if (!GetNextUrl())
            {
                return false;
            }
            return true;
        }

        private bool GetDate(out DateTime date)
        {
            date = DateTime.Now;
            string s = string.Empty;
            //<meta itemprop="datePublished" content="2016-12-06">
            Regex regu = new Regex("datePublished.+content.+");
            MatchCollection matches = regu.Matches(siteContent);
            if (matches.Count < 1)
            {
                s = $"Did not found publishing date from page {currentAddress}.";
                ReportProgress(LogFactory.CreateWarningMessage(s));
                return false;
            }

            regu = new Regex(@"(\d{4})-(\d{2})-(\d{2})");
            string dateString = regu.Match(matches[0].Value).Value;

            DateTime day = DateTime.ParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            date = day;

            s = $"Found date {date.ToString("dd.MM.yyyy")}";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            return true;
        }

        private bool GetPhotoUrl(out string url)
        {
            url = string.Empty;
            string s = string.Empty;

            // change 26.08.2017, image example url:
            //<img  src="//hs.mediadelivery.io/img/978/17fc6e100d9146b88a34b779bf2a833e.jpg" alt="" />
            //change 23.09.2017, image example url:
            //<img  src="//hs.mediadelivery.fi/img/978/6827963b40cd48ecb2161f96fbb259e3.jpg" alt="" />
            //regu = new Regex(@"data-original.+hs\.mediadelivery.+");
            //regu = new Regex(@"src=.+hs\.mediadelivery\.io.+"); //löytää: src="//hs.mediadelivery.io/img/978/17fc6e100d9146b88a34b779bf2a833e.jpg" alt="" />
            //regu = new Regex(@"src=.+hs\.mediadelivery.+"); //löytää: src="//hs.mediadelivery.io/img/978/17fc6e100d9146b88a34b779bf2a833e.jpg" alt="" />

            //28.12.2018 big image address in form: data - srcset = "//hs.mediadelivery.fi/img/1920/6961d628031f4df4953637ead46a66c5.jpg 1920w"
            Regex regu = new Regex(@"srcset=.+hs\.mediadelivery.+");
            //finds: srcset="//hs.mediadelivery.fi/img/1920/6961d628031f4df4953637ead46a66c5.jpg 1920w"
            if (!regu.IsMatch(siteContent))
            {
                s = $"Did not found image address from page {currentAddress}.";
                ReportProgress(LogFactory.CreateWarningMessage(s));
                return false;
            }
            url = regu.Match(siteContent).Value;
            //change 26.08.2017
            //regu = new Regex(@"hs\.mediadelivery.+");
            regu = new Regex(@"hs\.mediadelivery.+\.\w{3,4}");
            //finds: hs.mediadelivery.io/img/978/17fc6e100d9146b88a34b779bf2a833e.jpg
            url = regu.Match(url).Value;
            url = "http://" + url;
            s = $"Found image address {url}";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            return true;
        }

        private bool GetNextUrl()
        {
            string s = $"Searching for link to previous days image page...";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            Regex regu = new Regex("href.+Edellinen");
            MatchCollection matches = regu.Matches(siteContent);
            if (matches.Count < 1)
            {
                s = $" Did not found link to previous day from page {currentAddress}.";
                ReportProgress(LogFactory.CreateWarningMessage(s));
                return false;
            }

            //If link is disable, text is in form:
            //<a class="article-navlink prev disabled" href="#">Edellinen</a>
            if (regu.Match(matches[0].Value).Value.Contains("#"))
            {
                s = $"No link to previous day (last page)";
                ReportProgress(LogFactory.CreateNormalMessage(s));
                return false;
            }

            regu = new Regex("/.+html");
            currentAddress = "http://www.hs.fi" + regu.Match(matches[0].Value).Value;
            s = $"Found link to next page {currentAddress}";
            ReportProgress(LogFactory.CreateNormalMessage(s));
            return true;
        }

        private void CreateFilePathForPhoto(ref ComicPhoto cp)
        {
            StringBuilder sb = new StringBuilder();
            int count = comic.GetPhotoCountByRealeaseDate(cp.PublishDate);
            if (count > 0)
            {
                cp.SetDefaultRelativePath(count);
            }
            else
            {
                cp.SetDefaultRelativePath();
            }
        }

        private string FindLongestCommonSubstring(string first, string second)
        {
            //http://www.datavoila.com/projects/text/longest-common-substring-of-two-strings.html
            //string first = "complexity";
            //string second = "flexibility";

            int[,] num = new int[first.Length, second.Length];
            int maxLen = 0;
            int lastSubsBegin = 0;
            StringBuilder sequenceBuilder = new StringBuilder();

            for (int i = 0; i < first.Length; i++)
            {
                for (int j = 0; j < second.Length; j++)
                {
                    if (first[i] != second[j])
                        num[i, j] = 0;
                    else
                    {
                        if (i == 0 || j == 0)
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxLen)
                        {
                            maxLen = num[i, j];
                            int thisSubsBegin = i - num[i, j] + 1;
                            if (lastSubsBegin == thisSubsBegin)
                            {
                                // If the current LCS is the same as the last time this block ran
                                sequenceBuilder.Append(first[i]);
                            }
                            else
                            {
                                // Reset the string builder if a different LCS is found
                                lastSubsBegin = thisSubsBegin;
                                sequenceBuilder.Length = 0;
                                sequenceBuilder.Append(first.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
                            }
                        }
                    }
                }
            }
            return sequenceBuilder.ToString();
        }
        #endregion

        #region events
        public event EventHandler Finished;
        #endregion

        #region event handlers
        #endregion
    }
}
