using ComicDownloader.Model;
using JMI.General;
using JMI.General.Logging;
using JMI.General.VM.Application;
using JMI.General.VM.Commands;
using JMI.General.VM.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownloader.UI.ViewModel
{
    class ComicWorkspaceViewModel : CloseViewModel
    {
        #region constructors
        public ComicWorkspaceViewModel(ComicManager comicManager)
        {
            manager = comicManager ?? throw new ArgumentNullException(nameof(comicManager) + " can not be null");
            manager.DownloadLogs.CollectionChanged += OnDownloadLogsCollectionChanged;
            manager.PropertyChanged += OnManagerPropertyChanged;
            Tabs = new ObservableCollection<ITabViewModel>();
            //logTabs = new Dictionary<Logger, Tuple<LogViewModel, DowloadLogTabViewModel>>();
            Comics = new ComicListViewModel(manager.ComicsCollection);
            WorkPhotos = new ComicPhotoListViewModel(manager.WorkPhotos);
            CreateCommands();
        }
        #endregion

        #region properties
        private readonly ComicManager manager;
        //private Dictionary<Logger, Tuple<LogViewModel, DowloadLogTabViewModel>> logTabs;

        public ComicListViewModel Comics { get; private set; }
        public ReadOnlyCollection<CommandGroupViewModel> CommandGroups { get; private set; }

        private ObservableCollection<ITabViewModel> tabs;
        public ObservableCollection<ITabViewModel> Tabs
        {
            get { return tabs; }
            private set { SetProperty(ref tabs, value); }
        }

        private ITabViewModel selectedTab;
        public ITabViewModel SelectedTab
        {
            get { return selectedTab; }
            set { SetProperty(ref selectedTab, value); }
        }

        //private bool downloadComicInfos;
        //public bool DownloadComicInfos
        //{
        //    get { return downloadComicInfos; }
        //    set { SetProperty(ref downloadComicInfos, value); }
        //}

        //private bool downloadImageFiles;
        //public bool DownloadImageFiles
        //{
        //    get { return downloadImageFiles; }
        //    set { SetProperty(ref downloadImageFiles, value); }
        //}

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

        public ComicPhotoListViewModel WorkPhotos { get; private set; }
        #endregion

        #region commands
        private RelayCommand downloadCheckedCommand;
        public RelayCommand DownloadCheckedCommand
        {
            get
            {
                if (downloadCheckedCommand == null)
                {
                    downloadCheckedCommand =
                      new RelayCommand(
                          async param => await DownloadChecked(),
                          param => !manager.DownloadRunning && 
                          manager.ComicsCollection.CheckedItems.Count > 0 &&
                          (manager.DownloadComicData || manager.DownloadComicPhoto));  //TODO: mikä enabloi/disabloi?
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
        private void TestDownload()
        {
            Task t = manager.Download();

        }

        public override void Dispose()
        {
            RemoveAllTabs();
            manager.DownloadLogs.CollectionChanged -= OnDownloadLogsCollectionChanged;
            manager.PropertyChanged -= OnManagerPropertyChanged;
            base.Dispose();
        }

        private void CreateCommands()
        {
            #region Actions
            List<CommandViewModel> actionsList = new List<CommandViewModel>();

            RelayCommand createNewRelay =
                new RelayCommand(
                    param => CreateNewComic(),
                    param => true);     //TODO: jos lataus käynnissä, disabloi nappi
            CommandViewModel createNewCvm =
                new CommandViewModel("Create new", createNewRelay);
            actionsList.Add(createNewCvm);

            RelayCommand editSelectedRelay =
                new RelayCommand(
                    param => EditSelected(),
                    param => true);    //TODO: jos lataus käynnissä tai ei valittuja sarjoja, niin disabloi nappi
            CommandViewModel editSelectedCvm =
                new CommandViewModel("Edit selected", editSelectedRelay);
            actionsList.Add(editSelectedCvm);

            List<CommandGroupViewModel> commandGroupsList = new List<CommandGroupViewModel>();
            CommandGroupViewModel group = new CommandGroupViewModel("Actions");
            foreach (CommandViewModel item in actionsList)
            {
                group.Commands.Add(item);
            }
            commandGroupsList.Add(group);
            #endregion

            #region Download
            //CommandGroupViewModel groupDownload = new CommandGroupViewModel("Download");

            //RelayCommand downloadAllRelay =
            //    new RelayCommand(
            //        param => DownloadAll(),
            //        param => true);     //TODO: mikä enabloi/disabloi?
            //CommandViewModel downloadAllCvm =
            //    new CommandViewModel("Download all", downloadAllRelay);
            //groupDownload.Commands.Add(downloadAllCvm);

            //RelayCommand downloadCheckedRelay =
            //    new RelayCommand(
            //        param => DownloadChecked(),
            //        param => true);     //TODO: mikä enabloi/disabloi?
            //CommandViewModel downloadCheckedCvm =
            //    new CommandViewModel("Download checked", downloadCheckedRelay);
            //groupDownload.Commands.Add(downloadCheckedCvm);

            //commandGroupsList.Add(groupDownload);
            #endregion
            CommandGroups = new ReadOnlyCollection<CommandGroupViewModel>(commandGroupsList);
        }

        private void RemoveAllTabs()
        {
            foreach (ITabViewModel tab in tabs)
            {
                tab.Dispose();
                tab.CloseRequested -= OnTabCloseRequested;
                tabs.Remove(tab);
            }
        }

        private void EditSelected()
        {
            //foreach (Comic item in manager.ComicsCollection.SelectedItems)
            //{
            //    EditComicTabViewModel vm = new EditComicTabViewModel(manager.ComicUpdater, item);
            //    vm.CloseRequested += OnTabCloseRequested;
            //    Tabs.Add(vm);
            //    SelectedTab = vm;
            //}
        }

        private void CreateNewComic()
        {
            CreateNewComicTabViewModel vm = new CreateNewComicTabViewModel(manager.ComicCreator);
            vm.CloseRequested += OnTabCloseRequested;
            Tabs.Add(vm);
            SelectedTab = vm;
        }

        //private void DownloadAll()
        //{
        //    //TODO DownloadAll
        //}

        private async Task DownloadChecked()
        {
            await manager.Download();
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        private void OnDownloadLogsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (DownloadLogger item in e.NewItems)
                        {
                            //LogViewModel lvm = new LogViewModel(item);
                            //DowloadLogTabViewModel tab = new DowloadLogTabViewModel(lvm, item.Status);
                            DowloadLogTabViewModel tab = new DowloadLogTabViewModel(item);
                            tab.CloseRequested += OnTabCloseRequested;
                            Tabs.Add(tab);
                            SelectedTab = tab;
                            //logTabs.Add(item, new Tuple<LogViewModel, DowloadLogTabViewModel>(lvm, tab));
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    //if (e.OldItems != null)
                    //{
                    //    foreach (Logger item in e.OldItems)
                    //    {
                    //        if (logTabs.ContainsKey(item))
                    //        {
                    //            Tabs.Remove(logTabs[item].Item2);
                    //        }
                    //    }
                    //}
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RemoveAllTabs();
                    break;
                default:
                    break;
            }
        }

        private void OnManagerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(manager.DownloadRunning))
            {
                AllowClose = !manager.DownloadRunning;
            }
        }

        private void OnTabCloseRequested(object sender, EventArgs e)
        {
            ITabViewModel tab = sender as TabViewModel;
            if (tab.AllowClose)
            {
                tab.Dispose();
                tab.CloseRequested -= OnTabCloseRequested;
                tabs.Remove(tab);
            }
        }
        #endregion
    }
}
