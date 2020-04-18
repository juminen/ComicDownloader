﻿using ComicDownloader.Model;
using JMI.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownloader.UI.ViewModel
{
    class DownloadViewModel : ObservableObject
    {
        #region constructors
        public DownloadViewModel(ComicManager comicManager)
        {
            manager = comicManager ?? throw new ArgumentNullException(nameof(comicManager) + " can not be null");
            DownloadComics = new ComicListViewModel(manager.ComicsCollection);
            DownloadCommands = new DownloadCommandsViewModel(manager);
            DownloadLogs = new DownloadLogsViewModel(manager);
            WorkPhotos = new ComicPhotoListViewModel(manager.WorkPhotos);
        }
        #endregion

        #region properties
        private readonly ComicManager manager;

        public ComicListViewModel DownloadComics { get; private set; }
        public DownloadCommandsViewModel DownloadCommands { get; private set; }
        public DownloadLogsViewModel DownloadLogs { get; private set; }

        //private DownloadCommandsViewModel downloadCommands;
        //public DownloadCommandsViewModel DownloadCommands
        //{
        //    get { return downloadCommands; }
        //    private set { SetProperty(ref downloadCommands, value); }
        //}

        //private DownloadLogsViewModel downloadLogs;
        //public DownloadLogsViewModel DownloadLogs
        //{
        //    get { return downloadLogs; }
        //    private set { SetProperty(ref downloadLogs, value); }
        //}

        public ComicPhotoListViewModel WorkPhotos { get; private set; }
        #endregion

        #region commands
        #endregion

        #region methods
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion

    }
}
