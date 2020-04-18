using ComicDownloader.Model;
using JMI.General;
using JMI.General.Logging;
using JMI.General.VM.Application;
using JMI.General.VM.IO.Picker;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ComicDownloader.UI.ViewModel
{
    class ApplicationMainViewModel : BaseApplicationViewModel
    {
        #region constructors
        public ApplicationMainViewModel()
        {
            WindowTitle = "Comic downloader";
            FilePicker = new OpenSingleFilePickerViewModel("Current database", "...")
            {
                FileFilters = JMI.General.IO.FileFilters.SQLite.Filter
            };
            LoadSettingsFromDisk();

            //if (ConnectToDatabaseCommand.CanExecute(CurrentDatabaseExist()))
            //{
            //    //ConnectToDatabaseCommand.Execute(await ConnectToDatabase());
            //}

            //ConnectToDatabase();
            //TODO: testing
            //ForTesting();
        }
        #endregion

        #region properties
        Logger logger = SingletonLogger.Instance;
        ComicManager manager;

        public ComicManagerViewModel workspace;
        public ComicManagerViewModel Workspace
        {
            get { return workspace; }
            private set { SetProperty(ref workspace, value); }
        }

        private OpenSingleFilePickerViewModel filePicker;
        public OpenSingleFilePickerViewModel FilePicker
        {
            get { return filePicker; }
            private set { SetProperty(ref filePicker, value); }
        }
        #endregion

        #region commands
        private RelayCommand connectToDatabaseCommand;
        public RelayCommand ConnectToDatabaseCommand
        {
            get
            {
                if (connectToDatabaseCommand == null)
                {
                    connectToDatabaseCommand =
                      new RelayCommand(
                          async param => await ConnectToDatabase(),
                          param => CurrentDatabaseExist());
                }
                return connectToDatabaseCommand;
            }
        }
        #endregion

        #region methods
        private void LoadSettingsFromDisk()
        {
            FilePicker.SelectedPath = @"F:\Kuvat\0_testi\Sarjakuvat_testi.sqlite"; //TODO: testiä varten, tähän myöhemmin oikea toteutus
            //TODO: ikkunan asetuksien lataus muistista
            WindowHeight = SystemParameters.PrimaryScreenHeight / 2;
            WindowWidht = SystemParameters.PrimaryScreenWidth / 2;

            WindowTop = WindowHeight / 2;
            WindowLeft = WindowWidht / 2;
        }

        //TODO: asetuksien tallennus muistiin
        private bool CurrentDatabaseExist()
        {
            return System.IO.File.Exists(FilePicker.SelectedPath);
        }

        private async Task ConnectToDatabase()
        {
            if (!CurrentDatabaseExist())
            {
                logger.Log(LogFactory.CreateWarningMessage($"Selected database '{ FilePicker.SelectedPath }' does not exist."));
                return;
            }
            if (manager == null)
            {
                manager = new ComicManager(FilePicker.SelectedPath);
            }
            //else
            //{
            //    await manager.ConnectToRepository(FilePicker.SelectedPath);
            //}
            ComicManagerViewModel vm = new ComicManagerViewModel(manager);
            await manager.ConnectToRepository(FilePicker.SelectedPath);
            //await manager.GetComicsFromRepositoryAsync();
            Workspace = vm;
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion

        #region For Testing

        private void ForTesting()
        {
            //WindowState = WindowState.Maximized;
            //TestCreateNewComic();
            //TestComicListView();
            //TestComicWorkspace();
            //TestComicManagerView();
        }

        //private void TestCreateNewComic()
        //{
        //    ComicCreator comicCreator = new ComicCreator(new Repo.ComicRepository(FilePicker.SelectedPath));
        //    CreateNewComicTabViewModel vm = new CreateNewComicTabViewModel(comicCreator)
        //    {
        //        Name = "Fingerpori",
        //        //SavingLocation = @"F:\Kuvat\0_testi\testi",
        //        StartUrl = @"http://www.hs.fi/fingerpori/"
        //    };

        //    Workspace = vm;
        //}

        //private void TestComicListView()
        //{
        //    ComicManager manager = new ComicManager(FilePicker.SelectedPath);
        //    ComicListViewModel vm = new ComicListViewModel(manager.ComicsCollection);
        //    manager.GetComicsFromRepositoryAsync();
        //    Workspace = vm;
        //}

        //private void TestComicWorkspace()
        //{
        //    ComicManager manager = new ComicManager(FilePicker.SelectedPath);
        //    ComicWorkspaceViewModel vm = new ComicWorkspaceViewModel(manager);
        //    manager.GetComicsFromRepositoryAsync();
        //    Workspace = vm;
        //}

        private void TestComicManagerView()
        {
            ComicManager manager = new ComicManager(FilePicker.SelectedPath);
            ComicManagerViewModel vm = new ComicManagerViewModel(manager);
            manager.GetComicsFromRepositoryAsync();
            Workspace = vm;
        }
        #endregion
    }
}
