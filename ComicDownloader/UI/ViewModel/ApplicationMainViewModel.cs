using ComicDownloader.Model;
using JMI.General;
using JMI.General.Logging;
using JMI.General.VM.Application;
using JMI.General.VM.IO.Picker;
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
            ReadSettings();
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

        private void ReadSettings()
        {
            #region window place
            if (Properties.Settings.Default.WindowTop < 0 ||
                Properties.Settings.Default.WindowTop > SystemParameters.VirtualScreenHeight)
            {
                WindowTop = 0;
            }
            else
            {
                WindowTop = Properties.Settings.Default.WindowTop;
            }

            if (Properties.Settings.Default.WindowLeft < 0 ||
                Properties.Settings.Default.WindowLeft > SystemParameters.VirtualScreenWidth)
            {
                WindowLeft = 0;
            }
            else
            {
                WindowLeft = Properties.Settings.Default.WindowLeft;
            }
            #endregion window place

            #region window size
            if (Properties.Settings.Default.WindowHeight < 300)
            {
                WindowHeight = SystemParameters.PrimaryScreenHeight;
            }
            else
            {
                WindowHeight = Properties.Settings.Default.WindowHeight;
            }

            if (Properties.Settings.Default.WindowWidht < 300)
            {
                WindowWidht = SystemParameters.PrimaryScreenWidth;
            }
            else
            {
                WindowWidht = Properties.Settings.Default.WindowWidht;
            }

            if (Properties.Settings.Default.WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = Properties.Settings.Default.WindowState;
            }
            #endregion window size

            RowHeightTop = new GridLength(Properties.Settings.Default.GridRowHeightTop, GridUnitType.Star);
            RowHeightBottom = new GridLength(Properties.Settings.Default.GridRowHeightBottom, GridUnitType.Star);

            FilePicker.SelectedPath = Properties.Settings.Default.LastDatabaseFilePath;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.WindowTop = WindowTop;
            Properties.Settings.Default.WindowLeft = WindowLeft;
            Properties.Settings.Default.WindowHeight = WindowHeight;
            Properties.Settings.Default.WindowWidht = WindowWidht;
            Properties.Settings.Default.WindowState = WindowState;
            Properties.Settings.Default.GridRowHeightTop = RowHeightTop.Value;
            Properties.Settings.Default.GridRowHeightBottom = RowHeightBottom.Value;
            Properties.Settings.Default.LastDatabaseFilePath = FilePicker.SelectedPath;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion       
    }
}
