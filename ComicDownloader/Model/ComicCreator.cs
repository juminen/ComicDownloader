using ComicDownloader.Model.Validators;
using ComicDownloader.Repo;
using FluentValidation.Results;
using JMI.General;
using JMI.General.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownloader.Model
{
    public class ComicCreator : ObservableObject
    {
        #region constructors
        public ComicCreator(ComicRepository comicRepository)
        {
            repository = comicRepository ?? throw new ArgumentNullException(nameof(comicRepository) + " can not be null");
            ValidationResults = string.Empty;
        }
        #endregion

        #region properties
        private SingletonLogger logger = SingletonLogger.Instance;
        private readonly ComicRepository repository;
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
        #endregion

        #region methods
        private void CreateLogEntry(ILogMessage message)
        {
            logger.Log(message);
        }

        /// <summary>
        /// Validates given data and creates new comic and saves it to database if given data was ok.
        /// </summary>
        /// <param name="name">Comic name</param>
        /// <param name="startUrl">Start url for download</param>
        /// <param name="savingLocation">Directory path where comic photos are saved.</param>
        /// <param name="lastDate">Last download date</param>
        /// <returns>True if comic was created and saved to the database</returns>
        public async Task<bool> CreateComicAsync(string name, string startUrl, string savingLocation, DateTime lastDate)
        {
            Comic comic = new Comic()
            {
                Name = name,
                StartUrl = startUrl,
                LastDownloadDate = lastDate,
                SavingLocation = savingLocation
            };

            //Validate data
            bool valid = await ValidateDataAsync(comic);
            if (!valid)
            {
                CreateLogEntry(LogFactory.CreateWarningMessage("Comic not created."));
                return false;
            }

            //convert to DTO
            ComicDto dto = DtoConvert.ComicDtoConverter.ConvertItemToDto(comic);

            //Save to database
            CreateLogEntry(LogFactory.CreateNormalMessage("Saving to database..."));
            bool result = await repository.InsertComicAsync(dto);
            if (!result)
            {
                CreateLogEntry(LogFactory.CreateWarningMessage("Saving to database failed."));
                return false;
            }
            return true;
        }

        private async Task<bool> ValidateDataAsync(Comic comic)
        {
            CreateLogEntry(LogFactory.CreateNormalMessage("Validating data..."));
            //Validate comic basic details
            ComicValidator validator = new ComicValidator();
            ValidationResult results = validator.Validate(comic);

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

            //Check that there are no comics in the database with same name and/or start url
            DuplicateCheckResult dupCheck = await repository.CheckForDuplicateComicAsync(comic.Name, comic.StartUrl);
            bool result = false;
            switch (dupCheck)
            {
                case DuplicateCheckResult.DatabaseError:
                    ValidationResults = "Validation failed: Loading data from database failed, see log.";
                    result = false;
                    break;
                case DuplicateCheckResult.DuplicatesFound:
                    ValidationResults = "There is comic with same name and/or start url.";
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
        #endregion

        #region events
        public event EventHandler ValidationResultsUpdated;
        #endregion

        #region event handlers
        #endregion
    }
}
