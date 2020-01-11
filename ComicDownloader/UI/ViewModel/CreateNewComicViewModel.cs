using ComicDownloader.Model;
using JMI.General.VM.CED;
using JMI.General.VM.IO.Picker;
using System;
using System.Threading.Tasks;

namespace ComicDownloader.UI.ViewModel
{
    public class CreateNewComicViewModel : CreateNewItemViewModel, IDisposable
    {
        #region constructors

        public CreateNewComicViewModel(ComicCreator comicCreator)
        {
            creator = comicCreator ?? throw new ArgumentNullException(nameof(comicCreator) + " can not be null");
            creator.StatusChanged += OnCreatorStatusChanged;

            LastDownloadDate = DateTime.Today;
            SavingLocation = DefaultPickers.DirectoryPicker;
            Title = "Create new comic";
            //TODO: poista testauksen jälkeen
            SavingLocation.SelectedPath = @"F:\Kuvat\0_testi\testi";
        }
        #endregion

        #region properties
        private readonly ComicCreator creator;

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private DateTime lastDownloadDate;
        public DateTime LastDownloadDate
        {
            get { return lastDownloadDate; }
            set { SetProperty(ref lastDownloadDate, value); }
        }

        private string startUrl;
        public string StartUrl
        {
            get { return startUrl; }
            set { SetProperty(ref startUrl, value); }
        }

        private DirectoryPickerViewModel savingLocation;
        public DirectoryPickerViewModel SavingLocation
        {
            get { return savingLocation; }
            private set { SetProperty(ref savingLocation, value); }
        }

        public string Status { get { return creator.Status; } }
        protected override bool CreateEnabled => true;
        #endregion

        #region commands
        //private RelayCommand createCommand;
        //public RelayCommand CreateCommand
        //{
        //    get
        //    {
        //        if (createCommand == null)
        //        {
        //            createCommand =
        //              new RelayCommand(
        //                  async param => await CreateComicAsync(),
        //                  param => true);
        //        }
        //        return createCommand;
        //    }
        //}

        //private RelayCommand cancelCommand;
        //public RelayCommand CancelCommand
        //{
        //    get
        //    {
        //        if (cancelCommand == null)
        //        {
        //            cancelCommand =
        //              new RelayCommand(
        //                  param => Cancel(),
        //                  param => true);
        //        }
        //        return cancelCommand;
        //    }
        //}
        #endregion

        #region methods
        protected override async Task CreateItemAsync()
        {
            await creator.CreateComicAsync(
                Name, StartUrl, SavingLocation.SelectedPath, LastDownloadDate);
        }

        public override void Dispose()
        {
            creator.StatusChanged -= OnCreatorStatusChanged;
        }
        #endregion

        #region events
        private void OnCreatorStatusChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Status));
        }
        #endregion

        #region event handlers
        #endregion
    }
}
