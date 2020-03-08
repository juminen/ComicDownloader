using JMI.General.Logging;
using System;

namespace ComicDownloader.Model
{
    class DownloadLogger : ProgressLogger
    {
        #region constructors
        public DownloadLogger(IIsFinished isFinished)
        {
            Status = isFinished ?? throw new ArgumentNullException(nameof(isFinished) + " can not be null");
        }
        #endregion

        #region properties
        public IIsFinished Status { get; }
        #endregion

        #region methods
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion


    }
}
