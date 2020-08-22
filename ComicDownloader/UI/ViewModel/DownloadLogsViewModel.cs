using ComicDownloader.Model;
using JMI.General;
using JMI.General.VM.Application;
using System;
using System.Collections.ObjectModel;

namespace ComicDownloader.UI.ViewModel
{
    class DownloadLogsViewModel : ObservableObject
    {
        #region constructors
        public DownloadLogsViewModel(ComicManager comicManager)
        {
            manager = comicManager ?? throw new ArgumentNullException(nameof(comicManager) + " can not be null");
            manager.DownloadLogs.CollectionChanged += OnDownloadLogsCollectionChanged;
            Tabs = new ObservableCollection<ITabViewModel>();
        }
        #endregion

        #region properties
        private readonly ComicManager manager;

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
        private void RemoveAllTabs()
        {
            foreach (ITabViewModel tab in tabs)
            {
                tab.Dispose();
                tab.CloseRequested -= OnTabCloseRequested;
            }
            tabs.Clear();
            SelectedTab = null;
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
                            DowloadLogTabViewModel tab = new DowloadLogTabViewModel(item);
                            tab.CloseRequested += OnTabCloseRequested;
                            Tabs.Add(tab);
                            SelectedTab = tab;
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
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
