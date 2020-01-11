using ComicDownloader.Model.Validators;
using ComicDownloader.Repo;
using FluentValidation.Results;
using JMI.General;
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
            Status = string.Empty;
        }
        #endregion

        #region properties
        private readonly ComicRepository repository;
        public string Status { get; private set; }

        #endregion

        #region methods
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
            SendStatusChangeEvent($"Validating data...");
            bool valid = await ValidateDataAsync(comic);
            if (!valid)
            {
                ComicCreationFailed?.Invoke(this, EventArgs.Empty);
                return false;
            }
            SendStatusChangeEvent($"Validation succesfull.");
            
            //convert to DTO
            ComicDto dto = DtoConvert.ComicDtoConverter.ConvertItemToDto(comic);

            //Save to database
            SendStatusChangeEvent($"Saving to database...");
            bool result = await repository.InsertComicAsync(dto);
            if (!result)
            {
                SendStatusChangeEvent($"Saving to database failed, see log.");
                ComicCreationFailed?.Invoke(this, EventArgs.Empty);
                return false;
            }
            SendStatusChangeEvent($"Comic saved to database.");
            ComicCreated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private async Task<bool> ValidateDataAsync(Comic comic)
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

            //Check that there are no comics in the database with same name and/or start url
            DuplicateCheckResult dupCheck = await repository.CheckForDuplicateComicAsync(comic.Name, comic.StartUrl);
            bool result = false;
            switch (dupCheck)
            {
                case DuplicateCheckResult.DatabaseError:
                    SendStatusChangeEvent($"Loading data from database failed, see log.");
                    result= false;
                    break;
                case DuplicateCheckResult.DuplicatesFound:
                    SendStatusChangeEvent($"There is comic with same name and/or start url.");
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
        public event EventHandler ComicCreated;
        public event EventHandler ComicCreationFailed;
        #endregion

        #region event handlers
        #endregion
    }
}
