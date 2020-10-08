using ComicDownloader.Model;
using JMI.General.VM.CED;
using JMI.General.VM.IO.Picker;
using System;
using System.Threading.Tasks;

namespace ComicDownloader.UI.ViewModel
{
    public class CreateNewComicTabViewModel : CreateNewItemTabViewModel
    {
        #region constructors

        public CreateNewComicTabViewModel(ComicCreator comicCreator)
        {
            creator = comicCreator ?? throw new ArgumentNullException(nameof(comicCreator) + " can not be null");
            creator.ValidationResultsUpdated += OnValidationResultsUpdated;

            LastDownloadDate = DateTime.Today;
            SavingLocationPicker = DefaultPickers.DirectoryPicker;
            Title = "Create new comic";
            AllowClose = true;
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

        private DirectoryPickerViewModel savingLocationPicker;
        public DirectoryPickerViewModel SavingLocationPicker
        {
            get { return savingLocationPicker; }
            private set { SetProperty(ref savingLocationPicker, value); }
        }

        private string validationResult;
        public string ValidationResult
        {
            get { return validationResult; }
            private set { SetProperty(ref validationResult, value); }
        }

        protected override bool CreateEnabled => true;
        #endregion

        #region commands
        #endregion

        #region methods
        protected override async Task CreateItemAsync()
        {
            bool result = await creator.CreateComicAsync(
                Name, StartUrl, SavingLocationPicker.SelectedPath, LastDownloadDate);
            if (result)
            {
                RequestClose();
            }
        }

        public override void Dispose()
        {
            creator.ValidationResultsUpdated -= OnValidationResultsUpdated;
        }
        #endregion

        #region events
        private void OnValidationResultsUpdated(object sender, EventArgs e)
        {
            ValidationResult = creator.ValidationResults;
        }
        #endregion

        #region event handlers
        #endregion
    }
}
