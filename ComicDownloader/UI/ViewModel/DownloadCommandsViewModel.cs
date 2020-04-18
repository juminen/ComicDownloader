using ComicDownloader.Model;
using JMI.General;
using JMI.General.VM.Commands;
using System;
using System.Collections.ObjectModel;

namespace ComicDownloader.UI.ViewModel
{
    class DownloadCommandsViewModel
    {
        #region constructors
        public DownloadCommandsViewModel(ComicManager comicManager)
        {
            manager = comicManager ?? throw new ArgumentNullException(nameof(comicManager) + " can not be null");
        }
        #endregion

        #region properties
        private readonly ComicManager manager;

        public ReadOnlyCollection<CommandGroupViewModel> CommandGroups { get; private set; }

        public bool DownloadComicInfos
        {
            get { return manager.DownloadComicData; }
            set { manager.DownloadComicData = value; }
        }

        public bool DownloadImageFiles
        {
            get { return manager.DownloadComicPhoto; }
            set { manager.DownloadComicPhoto = value; }
        }
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
                          manager.ComicsCollection.AllItems.Count > 0 &&
                          (manager.DownloadComicData || manager.DownloadComicPhoto));
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
                          manager.ComicsCollection.CheckedItems.Count > 0 &&
                          (manager.DownloadComicData || manager.DownloadComicPhoto));
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
        #endregion

        #region methods
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion

    }
}
