using ComicDownloader.Model;
using JMI.General;
using JMI.General.VM.Commands;
using System;

namespace ComicDownloader.UI.ViewModel
{
    class DownloadCommandsViewModel
    {
        #region constructors
        public DownloadCommandsViewModel(ComicManager comicManager)
        {
            manager = comicManager ?? throw new ArgumentNullException(nameof(comicManager) + " can not be null");
            CreateCommandGroup();
        }
        #endregion

        #region properties
        private readonly ComicManager manager;

        public CommandGroupViewModel DownloadImageCommandGroup { get; private set; }
        #endregion

        #region commands
        private RelayCommand downloadAllCommand;
        public RelayCommand DownloadAllCommand
        {
            get
            {
                if (downloadAllCommand == null)
                {
                    downloadAllCommand =
                      new RelayCommand(
                          async param => await manager.DownloadAll(),
                          param => !manager.DownloadRunning &&
                          manager.ComicsCollection.AllItems.Count > 0);
                }
                return downloadAllCommand;
            }
        }

        private RelayCommand downloadCheckedCommand;
        public RelayCommand DownloadCheckedCommand
        {
            get
            {
                if (downloadCheckedCommand == null)
                {
                    downloadCheckedCommand =
                      new RelayCommand(
                          async param => await manager.DownloadChecked(),
                          param => !manager.DownloadRunning &&
                          manager.ComicsCollection.CheckedItems.Count > 0);
                }
                return downloadCheckedCommand;
            }
        }

        private RelayCommand cancelDownloadCommand;
        public RelayCommand CancelDownloadCommand
        {
            get
            {
                if (cancelDownloadCommand == null)
                {
                    cancelDownloadCommand =
                      new RelayCommand(
                          param => manager.CancelDownload(),
                          param => manager.DownloadRunning);
                }
                return cancelDownloadCommand;
            }
        }

        private RelayCommand downloadImagesCommand;
        public RelayCommand DownloadImagesCommand
        {
            get
            {
                if (downloadImagesCommand == null)
                {
                    downloadImagesCommand =
                      new RelayCommand(
                          async param => await manager.DownloadImages(),
                          param => !manager.DownloadRunning &&
                          manager.WorkPhotos.AllItems.Count > 0);
                }
                return downloadImagesCommand;
            }
        }
        #endregion

        #region methods
        private void CreateCommandGroup()
        {
            CommandViewModel downloadImageCvm = new CommandViewModel(
                "Dowload images", DownloadImagesCommand);
            DownloadImageCommandGroup = new CommandGroupViewModel("Download");
            DownloadImageCommandGroup.Commands.Add(downloadImageCvm);
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion

    }
}
