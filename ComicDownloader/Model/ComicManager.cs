using ComicDownloader.Model.DtoConvert;
using ComicDownloader.Model.Editors;
using ComicDownloader.Repo;
using JMI.General;
using JMI.General.ListSelection;
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
            DownloadLogs = new ObservableCollection<DownloadLogger>();
            ComicEditors = new ObservableCollection<ComicEditor>();
            ComicCreator = new ComicCreator(repository);
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
        public ObservableCollection<DownloadLogger> DownloadLogs { get; private set; }
        public ObservableCollection<ComicEditor> ComicEditors { get; private set; }
        public ComicCreator ComicCreator { get; private set; }

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
            DownloadLogs.Clear();
            ComicEditors.Clear();
            repository = new ComicRepository(pathToRepository);
            ComicCreator = new ComicCreator(repository);
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

        public async Task DownloadAll()
        {
            if (!InitializeDownload())
            {
                return;
            }
            if (DownloadComicData)
            {
                CreateComicWorkItems(ComicsCollection.GetAllItemsAsIEnumerable());
            }
            await Download();
        }

        public async Task DownloadChecked()
        {
            if (!InitializeDownload())
            {
                return;
            }
            if (DownloadComicData)
            {
                CreateComicWorkItems(ComicsCollection.GetCheckedItemsAsIEnumerable());
            }
            await Download();
        }

        private bool InitializeDownload()
        {
            //check if download is already running
            if (DownloadRunning)
            {
                return false;
            }
            DownloadRunning = true;
            DownloadLogs.Clear();
            return true;
        }

        private void CreateComicWorkItems(IEnumerable<Comic> collection)
        {
            workItems.Clear();
            foreach (Comic comic in collection)
            {
                ComicWorkItem workItem = new ComicWorkItem(comic);
                workItems.Add(workItem);
                DownloadLogs.Add(workItem.Log);
            }
        }

        private async Task Download()
        {
            if (DownloadComicData)
            {
                //Download information
                await Crawl();
            }

            //There are two options:
            //1. Download information AND images
            //2. Download only images

            //In option one, information download must be succesfull 
            //until images can be dowloaded
            if ((DownloadComicData && DownloadComicPhoto && crawlSucceeded) ||
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

        private async Task Crawl()
        {
            string msg = string.Empty;
            crawlSucceeded = false;

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

            #region Run crawlers
            try
            {
                List<Task> tasks = new List<Task>();
                foreach (ComicWorkItem item in workItems)
                {
                    tasks.Add(item.Crawler.DownloadDataAsync(cts));
                }
                //Wait until all crawlers has finished
                await Task.WhenAll(tasks);
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

            msg = $"Information downloaded.";
            logger.Log(LogFactory.CreateNormalMessage(msg));

            //Collect all photo items and insert data to repository
            foreach (ComicWorkItem cwi in workItems)
            {
                WorkPhotos.AddRange(cwi.Photos);
            }
            if (WorkPhotos.AllItems.Count < 1)
            {
                msg = $"There were no new image information to be saved.";
                logger.Log(LogFactory.CreateNormalMessage(msg));
            }
            else
            {
                msg = $"Saving to repository...";
                logger.Log(LogFactory.CreateNormalMessage(msg));
                //TODO: what should we do if this fails
                if (await InsertWorkPhotosToRepository())
                {
                    msg = $"Image information inserted to repository.";
                    logger.Log(LogFactory.CreateNormalMessage(msg));
                    //Add photos to collection
                    PhotosCollection.AddRange(WorkPhotos.GetAllItemsAsIEnumerable());
                    //Add photos to comic photos
                    workItems.ForEach(wi => wi.MoveDownloadedPhotoInfosToComic());

                    //TODO: what should we do if this fails
                    if (await UpdateWorkComicsToRepository())
                    {
                        msg = $"Comic information updated to repository.";
                        logger.Log(LogFactory.CreateNormalMessage(msg));
                    }
                    else
                    {
                        msg = $"Updating comic information to repository failed.";
                        logger.Log(LogFactory.CreateWarningMessage(msg));
                        return;
                    }
                }
                else
                {
                    msg = $"Adding image information to repository failed.";
                    logger.Log(LogFactory.CreateWarningMessage(msg));
                    return;
                }
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

            if (WorkPhotos.AllItems.Count == 0)
            {
                msg = $"There are no images to download.";
                logger.Log(LogFactory.CreateWarningMessage(msg));
                return;
            }

            cts = new CancellationTokenSource();
            ParallelOptions po = new ParallelOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            FinishedItem finishedItem = new FinishedItem();

            List<ComicPhotoDownloader> downloaders = new List<ComicPhotoDownloader>();
            DownloadLogger imageDownloadLog = new DownloadLogger(finishedItem) { Name = "Image Dowload" };
            DownloadLogs.Add(imageDownloadLog);

            foreach (ComicPhoto cp in WorkPhotos.AllItems)
            {
                if (!System.IO.File.Exists(cp.AbsoluteFilePath))
                {
                    ComicPhotoDownloader loader = new ComicPhotoDownloader(cp, imageDownloadLog.Progress);
                    downloaders.Add(loader);
                }
                else
                {
                    msg = $"Image file {cp.AbsoluteFilePath} exists, image not added to downloads.";
                    logger.Log(LogFactory.CreateWarningMessage(msg));
                }
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
                finishedItem.IsFinished = true;
                return;
            }
            catch (Exception ex)
            {
                msg = $"Downloading images failed:\n'{ex.Message}'.";
                logger.Log(LogFactory.CreateErrorMessage(msg));
                photoDownloadSucceeded = false;
                finishedItem.IsFinished = true;
                return;
            }
            logger.Log(LogFactory.CreateNormalMessage("Images downloaded."));
            photoDownloadSucceeded = true;
            finishedItem.IsFinished = true;
        }

        private async Task<bool> UpdateWorkPhotosToRepository()
        {
            bool result = await repository.UpdatePhotosAsync
                (
                    ComicPhotoDtoConverter.ConvertItemsToDtos(WorkPhotos.GetAllItemsAsIEnumerable())
                );

            return result;
        }

        public void AddCheckedPhotosToWorkPhotos()
        {
            WorkPhotos.AddRange(PhotosCollection.GetCheckedItemsAsIEnumerable());
        }

        public void EditSelectedComic()
        {
            foreach (Comic item in ComicsCollection.SelectedItems)
            {
                if (!ComicEditors.Any(ed => ed.ModelItemId.Equals(item.Id)))
                {
                    ComicEditor editor = new ComicEditor(item, repository);
                    editor.EndEditingRequested += OnEditorEndEditingRequested;
                    ComicEditors.Add(editor);
                }
                else
                {
                    string msg = $"Comic '{item.Name}' is already in editing.";
                    logger.Log(LogFactory.CreateNormalMessage(msg));
                }
            }
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        private void OnEditorEndEditingRequested(object sender, EventArgs e)
        {
            ComicEditor editor = (ComicEditor)sender;
            editor.EndEditingRequested -= OnEditorEndEditingRequested;
            ComicEditors.Remove(editor);
        }
        #endregion
    }
}
