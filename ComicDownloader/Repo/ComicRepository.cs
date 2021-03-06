﻿using JMI.General.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownloader.Repo
{
    public class ComicRepository
    {
        #region constructors
        public ComicRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            path = filePath;
        }
        #endregion

        #region properties
        private readonly string path;
        private ComicContext context;
        private readonly Logger logger = SingletonLogger.Instance;
        #endregion

        #region methods

        #region logging
        private void SendLogMessage(string msg, LogMessageStatus msgStatus)
        {
            logger.Log(msgStatus, msg);
        }

        private void LogStorageEntriesCount(int count)
        {
            string msg = $"{count} item(s) affected in the database.";
            SendLogMessage(msg, LogMessageStatus.Normal);
        }

        private void LogException(Operation action, string type, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            switch (action)
            {
                case Operation.Insert:
                    sb.AppendLine($"Creating {type} to storage failed, exception:");
                    break;
                case Operation.Delete:
                    sb.AppendLine($"Deleting {type} from storage failed, exception:");
                    break;
                case Operation.Update:
                    sb.AppendLine($"Updating {type} to storage failed, exception:");
                    break;
                case Operation.Load:
                    sb.AppendLine($"Loading {type} from storage failed, exception:");
                    break;
                default:
                    break;
            }

            if (ex.InnerException != null)
            {
                sb.AppendLine($"{ex.Message}");
                sb.AppendLine($"Inner exception:");
                sb.Append($"{ex.InnerException.Message}");
            }
            else
            {
                sb.Append($"{ex.Message}");
            }
            SendLogMessage(sb.ToString(), LogMessageStatus.Error);
        }
        #endregion

        #region comic
        public async Task<IList<ComicDto>> LoadComicsAsync(bool includePhotos)
        {
            List<ComicDto> list = new List<ComicDto>();

            try
            {
                using (context = new ComicContext(path))
                {
                    if (includePhotos)
                    {
                        list = await context.Comics
                            .Include(item => item.Photos)
                            .AsNoTracking()
                            .ToListAsync();
                    }
                    else
                    {
                        list = await context.Comics
                            .AsNoTracking()
                            .ToListAsync();
                    }
                }
            }
            catch (Exception e)
            {
                LogException(Operation.Load, "comics", e);

            }

            return list;
        }

        public async Task<bool> InsertComicAsync(ComicDto comic)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    context.Comics.Add(comic);
                    i = await context.SaveChangesAsync();
                }
                LogStorageEntriesCount(i);
            }
            catch (Exception e)
            {
                LogException(Operation.Insert, "comic", e);
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateComicAsync(ComicDto comic)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    context.Entry(comic).State = EntityState.Modified;
                    i = await context.SaveChangesAsync();
                }
                LogStorageEntriesCount(i);
            }
            catch (Exception e)
            {
                LogException(Operation.Update, "comic", e);
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateComicsAsync(IList<ComicDto> items)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    context.Comics.UpdateRange(items);
                    i = await context.SaveChangesAsync();
                }
                LogStorageEntriesCount(i);
            }
            catch (Exception e)
            {
                LogException(Operation.Update, "comic", e);
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteComicAsync(string comicId)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    ComicDto comic = context.Comics
                            .Single(x => x.UniqueIdentifier.ToString().Equals(comicId));
                    if (comic != null)
                    {
                        context.Comics.Remove(comic);
                        i = await context.SaveChangesAsync();
                        LogStorageEntriesCount(i);
                    }
                    else
                    {
                        string msg = $"Comic not found, id: {comicId}";
                        SendLogMessage(msg, LogMessageStatus.Warning);
                    }
                }
            }
            catch (Exception e)
            {
                LogException(Operation.Delete, "comic", e);
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteComicsAsync(IList<ComicDto> items)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    context.Comics.RemoveRange(items);
                    i = await context.SaveChangesAsync();
                    LogStorageEntriesCount(i);
                }
            }
            catch (Exception e)
            {
                LogException(Operation.Delete, "comic(s)", e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if there is comic with given name or url.
        /// </summary>
        /// <param name="name">Name of the comic</param>
        /// <param name="url">Start url of the comic</param>
        /// <returns>True if no duplicates where found</returns>
        public async Task<DuplicateCheckResult> CheckForDuplicateComicAsync(string name, string url)
        {
            int count = 0;
            DuplicateCheckResult result;
            try
            {
                using (context = new ComicContext(path))
                {
                    count = await context.Comics
                                    .Where(c => c.Name.Equals(name) || c.StartUrl.Equals(url))
                                    .CountAsync();
                }
                if (count > 0)
                {
                    result = DuplicateCheckResult.DuplicatesFound;
                }
                else
                {
                    result = DuplicateCheckResult.DuplicatesNotFound;
                }
            }
            catch (Exception e)
            {
                LogException(Operation.Load, "comics", e);
                result = DuplicateCheckResult.DatabaseError;
            }
            return result;
        }

        public async Task<DuplicateCheckResult> CheckForUpdateDuplicatesAsync(ComicDto comic)
        {
            int count1 = 0;
            int count2 = 0;
            DuplicateCheckResult result;
            try
            {
                using (context = new ComicContext(path))
                {
                    count1 = await context.Comics
                                    .Where(c => c.Name.Equals(comic.Name) &&
                                           !c.UniqueIdentifier.Equals(comic.UniqueIdentifier))
                                    .CountAsync();

                    count1 = await context.Comics
                                    .Where(c => c.StartUrl.Equals(comic.StartUrl) &&
                                           !c.UniqueIdentifier.Equals(comic.UniqueIdentifier))
                                    .CountAsync();


                }
                if (count1 + count2 > 0)
                {
                    result = DuplicateCheckResult.DuplicatesFound;
                }
                else
                {
                    result = DuplicateCheckResult.DuplicatesNotFound;
                }
            }
            catch (Exception e)
            {
                LogException(Operation.Load, "comics", e);
                result = DuplicateCheckResult.DatabaseError;
            }
            return result;
        }
        #endregion

        #region photo
        public async Task<bool> InsertPhotosAsync(IList<ComicPhotoDto> items)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    context.Photos.AddRange(items);
                    i = await context.SaveChangesAsync();
                }
                LogStorageEntriesCount(i);
            }
            catch (Exception e)
            {
                LogException(Operation.Insert, "photo(s)", e);
                return false;
            }
            return true;
        }

        public async Task<bool> UpdatePhotosAsync(IList<ComicPhotoDto> items)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    context.Photos.UpdateRange(items);
                    i = await context.SaveChangesAsync();
                }
                LogStorageEntriesCount(i);
            }
            catch (Exception e)
            {
                LogException(Operation.Update, "photo(s)", e);
                return false;
            }
            return true;
        }

        public async Task<bool> DeletePhotoAsync(string photoId)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    ComicPhotoDto photo = context.Photos
                            .Single(x => x.UniqueIdentifier.ToString().Equals(photoId));
                    if (photo != null)
                    {
                        context.Photos.Remove(photo);
                        i = await context.SaveChangesAsync();
                        LogStorageEntriesCount(i);
                    }
                    else
                    {
                        string msg = $"Photo not found, id: {photoId}";
                        SendLogMessage(msg, LogMessageStatus.Warning);
                    }
                }
            }
            catch (Exception e)
            {
                LogException(Operation.Delete, "photo", e);
                return false;
            }
            return true;
        }

        public async Task<bool> DeletePhotosAsync(IList<ComicPhotoDto> items)
        {
            try
            {
                int i;
                using (context = new ComicContext(path))
                {
                    context.Photos.RemoveRange(items);
                    i = await context.SaveChangesAsync();
                    LogStorageEntriesCount(i);
                }
            }
            catch (Exception e)
            {
                LogException(Operation.Delete, "photo(s)", e);
                return false;
            }
            return true;
        }
        #endregion
        #endregion

        private enum Operation
        {
            Insert,
            Delete,
            Update,
            Load
        }
    }
}
