using ComicDownloader.Model.DtoConvert;
using ComicDownloader.Repo;
using JMI.General.ListSelection;
using JMI.General.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComicDownloader.Model
{
    public class ComicCollection : SelectionCollection<Comic>
    {
        #region constructors
        public ComicCollection(ComicRepository comicRepository)
        {
            repository = comicRepository ?? throw new ArgumentNullException(nameof(comicRepository) + " can not be null");
        }
        #endregion

        #region properties
        private ComicRepository repository;
        private readonly Logger logger = Logger.Instance;
        #endregion

        #region methods
        /// <summary>
        /// Checks if collection contains item with given Id
        /// </summary>
        /// <param name="identifier">Unique id of the item</param>
        /// <returns>True if item was in collection</returns>
        public bool ContainsComic(string identifier)
        {
            return allItems.Any(x => x.Id.Equals(identifier));
        }

        /// <summary>
        /// Get comic from the collection using item's unique id
        /// </summary>
        /// <param name="identifier">Unique id of the item</param>
        /// <returns><see cref="Comic"/> if collection contains item, otherwise return null</returns>
        public Comic GetComic(string identifier)
        {
            if (ContainsComic(identifier))
            {
                return null;
            }
            return allItems.First(x => x.Id.Equals(identifier));
        }

        /// <summary>
        /// Deletes from the repository checked comics that has no photos.
        /// </summary>
        public async Task DeleteCheckedAsync()
        {
            List<Comic> toBeDeleted = new List<Comic>();
            //Check that comic does not have no photos
            foreach (Comic item in CheckedItems)
            {
                if (item.Photos.Count > 0)
                {
                    string msg = $"Comic {item.Name} can not be deleted because it has photos.";
                    logger.Log(LogFactory.CreateWarningMessage(msg));
                }
                else
                {
                    toBeDeleted.Add(item);
                }
            }
            //Convert to dto
            List<ComicDto> dtos = ComicDtoConverter.ConvertItemsToDtos(toBeDeleted);
            //Delete from repo
            bool result = await repository.DeleteComicsAsync(dtos);
            //Remove items from collection
            if (result)
            {
                RemoveChecked();
            }
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion
    }
}
