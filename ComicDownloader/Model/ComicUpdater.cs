using ComicDownloader.Model.Validators;
using ComicDownloader.Repo;
using FluentValidation.Results;
using JMI.General;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownloader.Model
{
    class ComicUpdater : ObservableObject
    {
        #region constructors
        public ComicUpdater(ComicRepository comicRepository)
        {
            repository = comicRepository ?? throw new ArgumentNullException(nameof(comicRepository) + " can not be null");
            Status = string.Empty;
        }
        #endregion

        #region properties
        private readonly ComicRepository repository;
        public string Status { get; private set; }
        #endregion

        #region methods
        /// <summary>
        /// Validates given comic data and updates it to database if given data was ok.
        /// </summary>
        /// <returns>True if comic was uodated to the database</returns>
        public async Task<bool> UpdateComicAsync(Comic comicToUpdate)
        {
            //Convert to DTO
            ComicDto dto = DtoConvert.ComicDtoConverter.ConvertItemToDto(comicToUpdate);

            //Validate data
            SendStatusChangeEvent($"Validating data...");
            bool valid = await ValidateDataAsync(comicToUpdate, dto);
            if (!valid)
            {
                ComicUpdateFailed?.Invoke(this, EventArgs.Empty);
                return false;
            }
            SendStatusChangeEvent($"Validation succesfull.");
                       
            //Update to database
            SendStatusChangeEvent($"Updating to database...");
            bool result = await repository.UpdateComicAsync(dto);
            if (!result)
            {
                SendStatusChangeEvent($"Updating to database failed, see log.");
                ComicUpdateFailed?.Invoke(this, EventArgs.Empty);
                return false;
            }
            SendStatusChangeEvent($"Comic updated to database.");
            ComicUpdated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private async Task<bool> ValidateDataAsync(Comic comic, ComicDto dto)
        {
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
                SendStatusChangeEvent(sb.ToString());
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
                    SendStatusChangeEvent($"Loading data from database failed, see log.");
                    result = false;
                    break;
                case DuplicateCheckResult.DuplicatesFound:
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("There is comic with:");
                    sb.AppendLine("- same name but different indentifier and/or");
                    sb.Append("- same start url but different indentifier");
                    SendStatusChangeEvent(sb.ToString());
                    result = false;
                    break;
                case DuplicateCheckResult.DuplicatesNotFound:
                    result = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown result '{dupCheck.ToString()}'");
            }
            return result;
        }

        private void SendStatusChangeEvent(string status)
        {
            Status = status;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region events
        public event EventHandler StatusChanged;
        public event EventHandler ComicUpdated;
        public event EventHandler ComicUpdateFailed;
        #endregion

        #region event handlers
        #endregion
    }
}
