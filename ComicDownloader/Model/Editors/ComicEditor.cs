using ComicDownloader.Model.Validators;
using ComicDownloader.Repo;
using FluentValidation.Results;
using JMI.General;
using JMI.General.ChangeTracking;
using JMI.General.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownloader.Model.Editors
{
    class ComicEditor : ObservableObject
    {
        #region constructors
        public ComicEditor(Comic comicToModify, ComicRepository comicRepository)
        {
            comicToEdit = comicToModify ?? throw new ArgumentNullException(nameof(comicToModify) + " can not be null");
            repository = comicRepository ?? throw new ArgumentNullException(nameof(comicRepository) + " can not be null");
            ReadValuesFromModel();
            dto = DtoConvert.ComicDtoConverter.ConvertItemToDto(comicToEdit);
            copyOfComic = DtoConvert.ComicDtoConverter.ConvertDtoToItem(dto, false);
            ValidationResults = string.Empty;

            tracker = new CustomStateTracker();
            tracker.AddTracking(
                comicToEdit, nameof(comicToEdit.Name),
                this, nameof(Name));
            tracker.AddTracking(
                comicToEdit, nameof(comicToEdit.LastDownloadDate),
                this, nameof(LastDownloadDate));
            tracker.AddTracking(
                comicToEdit, nameof(comicToEdit.StartUrl),
                this, nameof(StartUrl));
            tracker.AddTracking(
                comicToEdit, nameof(comicToEdit.SavingLocation),
                this, nameof(SavingLocation));
        }
        #endregion

        #region properties
        private SingletonLogger logger = SingletonLogger.Instance;
        private readonly Comic comicToEdit;
        private readonly Comic copyOfComic;
        private ComicDto dto;
        private readonly ComicRepository repository;
        private readonly ICustomStateTracker tracker;

        public bool HasChanges { get { return !tracker.SameState; } }

        private string validationResults;
        public string ValidationResults
        {
            get { return validationResults; }
            private set
            {
                validationResults = value;
                ValidationResultsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ModelItemId { get { return comicToEdit.Id; } }

        #region editable properties
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

        private string savingLocation;
        public string SavingLocation
        {
            get { return savingLocation; }
            set { SetProperty(ref savingLocation, value); }
        }
        #endregion editable properties
        #endregion

        #region methods
        private void ReadValuesFromModel()
        {
            Name = comicToEdit.Name;
            LastDownloadDate = comicToEdit.LastDownloadDate;
            StartUrl = comicToEdit.StartUrl;
            SavingLocation = comicToEdit.SavingLocation;
        }

        private void WriteValuesToCopy()
        {
            copyOfComic.Name = Name;
            copyOfComic.LastDownloadDate = LastDownloadDate;
            copyOfComic.StartUrl = StartUrl;
            copyOfComic.SavingLocation = SavingLocation;
        }

        private void ChangeModel()
        {
            comicToEdit.Name = Name;
            comicToEdit.LastDownloadDate = LastDownloadDate;
            comicToEdit.StartUrl = StartUrl;
            comicToEdit.SavingLocation = SavingLocation;
        }

        private void CreateLogEntry(ILogMessage message)
        {
            logger.Log(message);
        }

        private async Task<bool> ValidateDataAsync()
        {
            CreateLogEntry(LogFactory.CreateNormalMessage("Validating data..."));
            //Validate comic basic details
            ComicValidator validator = new ComicValidator();
            ValidationResult results = validator.Validate(copyOfComic);

            if (results.IsValid == false)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Validation failed:");
                foreach (ValidationFailure failure in results.Errors)
                {
                    sb.AppendLine(failure.ErrorMessage);
                }
                ValidationResults = sb.ToString();
                return false;
            }

            //Check that there are no comics in the database with 
            //- same name and different identifier
            //- same start url and different identifier
            DuplicateCheckResult dupCheck = await repository.CheckForUpdateDuplicatesAsync(dto);
            bool result = false;
            switch (dupCheck)
            {
                case DuplicateCheckResult.DatabaseError:
                    ValidationResults = "Validation failed: Loading data from database failed, see log.";
                    result = false;
                    break;
                case DuplicateCheckResult.DuplicatesFound:
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("There is comic with:");
                    sb.AppendLine("- same name but different indentifier and/or");
                    sb.Append("- same start url but different indentifier");
                    ValidationResults = sb.ToString();
                    result = false;
                    break;
                case DuplicateCheckResult.DuplicatesNotFound:
                    result = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown result '{dupCheck.ToString()}'");
            }

            if (result)
            {
                ValidationResults = string.Empty;
                CreateLogEntry(LogFactory.CreateNormalMessage("Validation succesfull."));
            }
            return result;
        }

        public async Task UpdateChangesAsync()
        {
            WriteValuesToCopy();
            dto = DtoConvert.ComicDtoConverter.ConvertItemToDto(comicToEdit);
            bool valid = await ValidateDataAsync();
            if (!valid)
            {
                CreateLogEntry(LogFactory.CreateWarningMessage("Comic not saved."));
                return;
            }
            CreateLogEntry(LogFactory.CreateNormalMessage("Updating to database..."));
            bool saved = await repository.UpdateComicAsync(dto);
            if (!saved)
            {
                CreateLogEntry(LogFactory.CreateWarningMessage("Updating to database failed."));
                return;
            }
            ChangeModel();
            ChangesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void CancelChanges()
        {
            ReadValuesFromModel();
            ChangesCanceled?.Invoke(this, EventArgs.Empty);
        }

        public void EndEditing()
        {
            EndEditingRequested?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region events
        public event EventHandler ValidationResultsUpdated;
        public event EventHandler ChangesUpdated;
        public event EventHandler ChangesCanceled;
        public event EventHandler EndEditingRequested;
        #endregion

        #region event handlers
        #endregion
    }
}
