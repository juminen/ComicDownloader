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
            Identifier = new GuidIdentifier();
        }

        public BaseComicStripObject(Guid guid)
        {
            Identifier = new GuidIdentifier(guid);
        }
        #endregion

        #region properties
        public IGuidIdentifier Identifier { get; }

        public abstract string DisplayText { get; }

        IIdentifier ISelectionTarget.Identifier => Identifier;
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
