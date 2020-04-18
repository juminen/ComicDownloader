using ComicDownloader.Model;
using ComicDownloader.Model.Editors;
using JMI.General;
using JMI.General.VM.Application;
using JMI.General.VM.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownloader.UI.ViewModel
{
    class ComicsViewModel : ObservableObject
    {
        #region constructors
        public ComicsViewModel(ComicManager comicManager)
        {
            manager = comicManager ?? throw new ArgumentNullException(nameof(comicManager) + " can not be null");
            manager.ComicEditors.CollectionChanged += OnComicEditorsCollectionChanged;
            Comics = new ComicListViewModel(manager.ComicsCollection);
            Tabs = new ObservableCollection<ITabViewModel>();
            CreateCommands();
        }
        #endregion

        #region properties
        private readonly ComicManager manager;
        public ComicListViewModel Comics { get; private set; }

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
        #endregion

        #region commands
        #endregion

        #region methods
        private void CreateCommands()
        {
            List<CommandViewModel> actionsList = new List<CommandViewModel>();

            RelayCommand createNewRelay =
                new RelayCommand(
                    param => CreateNewComic(),
                    param => !manager.DownloadRunning);
            CommandViewModel createNewCvm =
                new CommandViewModel("Create new", createNewRelay);
            actionsList.Add(createNewCvm);

            RelayCommand editSelectedRelay =
                new RelayCommand(
                    param => EditSelected(),
                    param => !manager.DownloadRunning &&
                    manager.ComicsCollection.SelectedItems.Count > 0);
            CommandViewModel editSelectedCvm =
                new CommandViewModel("Edit selected", editSelectedRelay);
            actionsList.Add(editSelectedCvm);

            CommandGroupViewModel group = new CommandGroupViewModel("Actions");
            foreach (CommandViewModel item in actionsList)
            {
                group.Commands.Add(item);
            }
            Comics.AddCommandGroup(group);
            Comics.RemoveCommand(Comics.ClearListCommand);
            Comics.RemoveCommand(Comics.RemoveCheckedCommand);
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
            manager.EditSelectedComic();
        }

        private void CreateNewComic()
        {
            CreateNewComicTabViewModel vm = new CreateNewComicTabViewModel(manager.ComicCreator);
            vm.CloseRequested += OnTabCloseRequested;
            Tabs.Add(vm);
            SelectedTab = vm;
        }
        #endregion

        #region events
        #endregion

        #region event handlers
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

        private void OnComicEditorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems!=null)
                    {
                        foreach (ComicEditor item in e.NewItems)
                        {
                            EditComicTabViewModel vm = new EditComicTabViewModel(item);
                            vm.CloseRequested += OnTabCloseRequested;
                            Tabs.Add(vm);
                            SelectedTab = vm;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
