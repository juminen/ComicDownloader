using ComicDownloader.Model;
using JMI.General;
using JMI.General.Logging;
using JMI.General.VM.Application;
using JMI.General.VM.IO.Picker;
using System.Windows;

namespace ComicDownloader.UI.ViewModel
{
    class ApplicationMainViewModel : BaseApplicationViewModel
    {
        #region constructors
        public ApplicationMainViewModel()
        {
            WindowTitle = "Comic downloader";
            //TODO: ikkunan asetuksien lataus muistista
            Logger logger = Logger.Instance;
            logger.Log(LogFactory.CreateNormalMessage("Application started."));
            DatabaseSelector = new OpenSingleFilePickerViewModel("Current database", "...");
            ConnectToDatabase();
            //TODO: testing
            ForTesting();
        }
        #endregion

        #region properties
        public object workspace;
        public object Workspace
        {
            get { return workspace; }
            private set { SetProperty(ref workspace, value); }
        }

        private OpenSingleFilePickerViewModel databaseSelector;
        public OpenSingleFilePickerViewModel DatabaseSelector
        {
            get { return databaseSelector; }
            private set { SetProperty(ref databaseSelector, value); }
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
                          param => ConnectToDatabase(),
                          param => true);
                }
                return connectToDatabaseCommand;
            }
        }
        #endregion

        #region methods
        //TODO: ikkunan asetuksien tallennus muistiin
        private void ConnectToDatabase()
        {
            //TODO: tähän oikea toteutus
            DatabaseSelector.SelectedPath = @"F:\Kuvat\0_testi\Sarjakuvat_testi.sqlite";
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion

        #region For Testing
        
        private void ForTesting()
        {
            WindowHeight = SystemParameters.PrimaryScreenHeight / 2;
            WindowWidht = SystemParameters.PrimaryScreenWidth / 2;

            WindowTop = WindowHeight / 2;
            WindowLeft = WindowWidht / 2;

            //WindowState = WindowState.Maximized;
            ComicCreator comicCreator = new ComicCreator(new Repo.ComicRepository(DatabaseSelector.SelectedPath));
            CreateNewComicViewModel vm = new CreateNewComicViewModel(comicCreator)
            {
                Name = "Fingerpori",
                //SavingLocation = @"F:\Kuvat\0_testi\testi",
                StartUrl = @"http://www.hs.fi/fingerpori/"
            };

            Workspace = vm;
        }
        #endregion
    }
}
