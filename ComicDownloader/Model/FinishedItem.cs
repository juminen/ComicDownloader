using System;

namespace ComicDownloader.Model
{
    class FinishedItem : IIsFinished
    {
        #region constructors
        public FinishedItem()
        {
            IsFinished = false;
        }
        #endregion

        #region properties
        private bool isFinished;
        public bool IsFinished
        {
            get { return isFinished; }
            set
            {
                isFinished = value;
                if (isFinished)
                {
                    Finished?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region methods
        #endregion

        #region events
        public event EventHandler Finished;
        #endregion

        #region event handlers
        #endregion

    }
}
