using ComicDownloader.Model;
using JMI.General;
using JMI.General.VM.Commands;
using System;
using System.Collections.Generic;

namespace ComicDownloader.UI.ViewModel
{
    class ComicPhotosViewModel : ObservableObject
    {
        #region constructors
        public ComicPhotosViewModel(ComicManager comicManager)
        {
            manager = comicManager ?? throw new ArgumentNullException(nameof(comicManager) + " can not be null");
            ComicPhotos = new ComicPhotoListViewModel(manager.PhotosCollection);
            WorkPhotos = new ComicPhotoListViewModel(manager.WorkPhotos);
            CreateCommands();
        }
        #endregion

        #region properties
        private readonly ComicManager manager;
        public ComicPhotoListViewModel ComicPhotos { get; private set; }
        public ComicPhotoListViewModel WorkPhotos { get; private set; }
        #endregion

        #region commands
        #endregion

        #region methods
        private void CreateCommands()
        {
            List<CommandViewModel> actionsList = new List<CommandViewModel>();

            RelayCommand addCheckedRelay =
                new RelayCommand(
                    param => manager.AddCheckedPhotosToWorkPhotos(),
                    param => !manager.DownloadRunning && manager.PhotosCollection.CheckedItems.Count > 0);
            CommandViewModel addCheckedCvm =
                new CommandViewModel("Add checked to work items", addCheckedRelay);
            actionsList.Add(addCheckedCvm);

            CommandGroupViewModel group = new CommandGroupViewModel("Actions");
            foreach (CommandViewModel item in actionsList)
            {
                group.Commands.Add(item);
            }
            ComicPhotos.AddCommandGroup(group);
            ComicPhotos.RemoveCommand(ComicPhotos.ClearListCommand);
            ComicPhotos.RemoveCommand(ComicPhotos.RemoveCheckedCommand);
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion
    }
}
