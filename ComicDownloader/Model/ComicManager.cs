using ComicDownloader.Model.DtoConvert;
using ComicDownloader.Repo;
using JMI.General;
using JMI.General.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComicDownloader.Model
{
    class ComicManager : ObservableObject
    {
        #region constructors
        public ComicManager(string pathToDatabase)
        {
            repository = new ComicRepository(pathToDatabase);

            ComicsCollection = new ComicCollection(repository);
            PhotosCollection = new ComicPhotoCollection();
            WorkPhotos = new ComicPhotoCollection();
            DownloadLogs = new ObservableCollection<Logger>();
            //LoadSettings();
            //TODO: testiä varten
            //GetComicsFromDatabase();
            //TODO: testiä varten
            //ComicsCollection.SelectAll();
        }
        #endregion

        #region properties
        private readonly Logger logger = SingletonLogger.Instance;
        private ComicRepository repository;
        private CancellationTokenSource cts;
        private bool crawlSucceeded;
        private bool photoDownloadSucceeded;
        List<ComicWorkItem> workItems = new List<ComicWorkItem>();

        /// <summary>
        /// Collection to hold all comic information
        /// </summary>
        public ComicCollection ComicsCollection { get; private set; }
        /// <summary>
        /// Collection to hold all comic photo information
        /// </summary>
        public ComicPhotoCollection PhotosCollection { get; private set; }
        /// <summary>
        /// Collection to hold all photo information that has been dowloaded
        /// </summary>
        public ComicPhotoCollection WorkPhotos { get; private set; }
        /// <summary>
        /// Collection to hold all logs used during download
        /// </summary>
        public ObservableCollection<Logger> DownloadLogs { get; private set; }

        private bool downloadComicData;
        /// <summary>
        /// Set to true if you want to download information of comic photos
        /// </summary>
        public bool DownloadComicData
        {
            get { return downloadComicData; }
            set { SetProperty(ref downloadComicData, value); }
        }

        private bool downloadComicPhoto;
        /// <summary>
        /// Set to true if you want to download actual comic photos
        /// </summary>
        public bool DownloadComicPhoto
        {
            get { return downloadComicPhoto; }
            set { SetProperty(ref downloadComicPhoto, value); }
        }

        private bool downloadRunning;
        /// <summary>
        /// Indicates if information or image download is ongoing.
        /// </summary>
        public bool DownloadRunning
        {
            get { return downloadRunning; }
            private set { SetProperty(ref downloadRunning, value); }
        }
        #endregion

        #region methods
        /// <summary>
        /// Loads all comic and photos from repository. 
        /// </summary>
        public async Task GetComicsFromRepositoryAsync()
        {
            IList<ComicDto> dtos;
            try
            {
                dtos = await repository.LoadComicsAsync(true);
            }
            catch (Exception ex)
            {
                string msg = $"Failed to load comics from repository:\n'{ex.Message}'.";
                logger.Log(LogFactory.CreateErrorMessage(msg));
                return;
            }

            //TODO: seuraava rivi poistaa kaikki tiedot myös tietokannasta
            ComicsCollection.RemoveAll();
            PhotosCollection.RemoveAll();
            WorkPhotos.RemoveAll();

            //Add all comics to collection
            ComicsCollection.AddRange(ComicDtoConverter.ConvertDtosToItems(dtos, true));
            //Add all photos to collection
            ComicsCollection.GetAllItemsAsIEnumerable().ToList().ForEach(
                (p) => PhotosCollection.AddRange(p.Photos));
        }

        /// <summary>
        /// Changes path to repository and loads all comics from repository (<see cref="GetComicsFromRepositoryAsync"/>)
        /// </summary>
        /// <param name="pathToRepository">Full path to repository</param>
        /// <returns>True if change was succesfull</returns>
        public async Task<bool> ConnectToRepository(string pathToRepository)
        {
            if (!System.IO.File.Exists(pathToRepository))
            {
                string msg = $"File '{pathToRepository}' not found.";
                logger.Log(LogFactory.CreateWarningMessage(msg));
                return false;
            }

            repository = new ComicRepository(pathToRepository);
            await GetComicsFromRepositoryAsync();
            return true;
        }

        /// <summary>
        /// Cancels download
        /// </summary>
        public void CancelDownload()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
            DownloadRunning = false;
        }

        public async Task Download()
        {
            //check if dowload is already running
            if (DownloadRunning)
            {
                return;
            }
            DownloadRunning = true;
            DownloadLogs.Clear();
            if (DownloadComicData)
            {
                //Download information
                await Crawl();
            }

            //There are two options:
            //1. Download information AND images
            //2. Download only images

            //In option one, information download must be succesfull until
            //images can be dowloaded
            if ((DownloadComicData && crawlSucceeded && DownloadComicPhoto) ||
                (!DownloadComicData && DownloadComicPhoto))
            {
                //Download images
                await DownloadPhotos();
                if (photoDownloadSucceeded)
                {
                    //Download images
                    await UpdateWorkPhotosToRepository();
                }
            }

            DownloadRunning = false;
        }

        private void CreateComicWorkItems()
        {
            workItems.Clear();
            foreach (Comic comic in ComicsCollection.CheckedItems)
            {
                ComicWorkItem workItem = new ComicWorkItem(comic);
                workItems.Add(workItem);
                DownloadLogs.Add(workItem.Log);
            }
        }

        private async Task Crawl()
        {
            string msg = string.Empty;
            crawlSucceeded = false;

            CreateComicWorkItems();
            if (workItems.Count < 1)
            {
                msg = $"No comics checked for downloading.";
                logger.Log(LogFactory.CreateNormalMessage(msg));
                return;
            }

            cts = new CancellationTokenSource();
            ParallelOptions po = new ParallelOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            //List<ComicDataCrawler> crawlers = new List<ComicDataCrawler>();
            //BlockingCollection<ComicPhoto> comicPhotos = new BlockingCollection<ComicPhoto>();
            //foreach (Comic comic in ComicsCollection.CheckedItems)
            //{
            //    //Comic c = comic;
            //    //Progress<ILogMessage> progress = new Progress<ILogMessage>();
            //    //TODO: ei voi tehdä näin, koska kaikki viestit tulee yhteen logiin
            //    //progress.ProgressChanged += OnProgressChanged;
            //    //crawlers.Add(new ComicDataCrawler(ref c, comicPhotos, progress));
            //}

            #region Run crawlers
            try
            {
                await Task.Run(() =>
                {
                    Parallel.ForEach(workItems, po, async (workitem) =>
                     await workitem.Crawler.DownloadDataAsync());
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                msg = $"Downloading data canceled.";
                logger.Log(LogFactory.CreateNormalMessage(msg));
                return;
            }
            catch (Exception ex)
            {
                msg = $"Downloading data failed:\n'{ex.Message}'.";
                logger.Log(LogFactory.CreateErrorMessage(msg));
                return;
            }
            #endregion

            msg = $"Information downloaded, saving to repository...";
            logger.Log(LogFactory.CreateNormalMessage(msg));

            //Collect all photo items and insert data to repository
            foreach (ComicWorkItem cwi in workItems)
            {
                WorkPhotos.AddRange(cwi.Photos.GetAllItemsAsIEnumerable());
            }
            //TODO: what should we do if this fails
            if (await InsertWorkPhotosToRepository())
            {
                msg = $"Image information inserted to repository.";
                logger.Log(LogFactory.CreateNormalMessage(msg));
                PhotosCollection.AddRange(WorkPhotos.GetAllItemsAsIEnumerable());
                //TODO: what should we do if this fails
                //Update comic data to repository
                if (await UpdateWorkComicsToRepository())
                {
                    //"Sarjakuvatiedot päivitetty tietokantaan.");
                    msg = $"Comic information updated to repository.";
                    logger.Log(LogFactory.CreateNormalMessage(msg));
                }
                else
                {
                    // "Sarjakuvatietojen päivitys tietokantaan epäonnistui.");
                    msg = $"Updating comic information to repository failed.";
                    logger.Log(LogFactory.CreateWarningMessage(msg));
                }
            }
            else
            {
                msg = $"Adding image information to repository failed.";
                logger.Log(LogFactory.CreateWarningMessage(msg));
            }
            crawlSucceeded = true;
        }

        private async Task<bool> InsertWorkPhotosToRepository()
        {
            bool result = await repository.InsertPhotosAsync
                (
                    ComicPhotoDtoConverter.ConvertItemsToDtos(WorkPhotos.GetAllItemsAsIEnumerable())
                );

            //Task<bool> result = Task.FromResult
            //(
            //    repository.InsertPhotosAsync
            //    (
            //        ComicPhotoDtoConverter.ConvertItemsToDtos(WorkPhotos.GetAllItemsAsIEnumerable())
            //    )
            //);
            return result;
        }

        private async Task<bool> UpdateWorkComicsToRepository()
        {
            List<Comic> comics = new List<Comic>();
            workItems.ForEach(wi => comics.Add(wi.Comic));

            bool result = await repository.UpdateComicsAsync
                (
                    ComicDtoConverter.ConvertItemsToDtos(comics)
                );

            return result;
            //Task<bool> result = Task.FromResult
            //(
            //    repository.UpdateComics
            //    (
            //        ComicDtoConverter.ConvertItemsToDtos(ComicsCollection.SelectedItems)
            //    )
            //);
            //return await result;
        }

        private async Task DownloadPhotos()
        {
            string msg = string.Empty;
            photoDownloadSucceeded = false;
            cts = new CancellationTokenSource();
            ParallelOptions po = new ParallelOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            List<ComicPhotoDownloader> downloaders = new List<ComicPhotoDownloader>();
            ProgressLogger imageDownloadLog = new ProgressLogger() { Name = "Image Dowload" };
            DownloadLogs.Add(imageDownloadLog);

            foreach (ComicPhoto cp in WorkPhotos.AllItems)
            {
                ComicPhotoDownloader loader = new ComicPhotoDownloader(cp, imageDownloadLog.Progress);
                downloaders.Add(loader);
            }

            logger.Log(LogFactory.CreateNormalMessage("Starting to download images..."));

            try
            {
                await Task.Run(() =>
                {
                    Parallel.ForEach(downloaders, po, async (loader) =>
                     await loader.GetComicPhoto());
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                msg = $"Downloading images canceled.";
                logger.Log(LogFactory.CreateNormalMessage(msg));
                photoDownloadSucceeded = true;
                return;
            }
            catch (Exception ex)
            {
                msg = $"Downloading images failed:\n'{ex.Message}'.";
                logger.Log(LogFactory.CreateErrorMessage(msg));
                photoDownloadSucceeded = false;
                return;
            }
            logger.Log(LogFactory.CreateNormalMessage("Images downloaded."));
            photoDownloadSucceeded = true;
        }

        private async Task<bool> UpdateWorkPhotosToRepository()
        {
            bool result = await repository.UpdatePhotosAsync
                (
                    ComicPhotoDtoConverter.ConvertItemsToDtos(WorkPhotos.GetAllItemsAsIEnumerable())
                );

            return result;
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion
    }
}
