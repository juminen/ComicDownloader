using ComicDownloader.Model;
using JMI.General.VM.Logging;
using System;

namespace ComicDownloader.UI.ViewModel
{
    class DowloadLogTabViewModel : LogTabViewModel
    {
        #region constructors
        public DowloadLogTabViewModel(DownloadLogger log) : base(log)
        {
            dLogger = log;
            Status = dLogger.Status;
            Status.Finished += OnStatusFinished;
            DisplayText = log.Name;
            AllowClose = false;
            if (!Status.IsFinished)
            {
                CloseFailReason = $"Download '{ DisplayText }' is running.";
            }
        }
        #endregion

        #region properties
        private DownloadLogger dLogger;
        public IIsFinished Status { get; }
        #endregion

        #region commands
        #endregion

        #region methods
        public override void Dispose()
        {
            Status.Finished -= OnStatusFinished;
            base.Dispose();
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        private void OnStatusFinished(object sender, EventArgs e)
        {
            AllowClose = true;
            CloseFailReason = "";
        }
        #endregion
    }
}
