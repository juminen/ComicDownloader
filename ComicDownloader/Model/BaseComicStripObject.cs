using JMI.General;
using JMI.General.Identifiers;
using JMI.General.Selections;
using System;

namespace ComicDownloader.Model
{
    public abstract class BaseComicStripObject : 
        ObservableObject, ISelectionTarget, IDisposable
    {
        #region constructors
        public BaseComicStripObject()
        {
            Identifier = new Identifier();
        }

        public BaseComicStripObject(Guid guid)
        {
            Identifier = new Identifier(guid);
        }
        #endregion

        #region properties
        public IIdentifier Identifier { get; }

        public abstract string DisplayText { get; }
        #endregion

        #region methods
        public virtual void Dispose()
        {
            
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        #endregion
    }
}
