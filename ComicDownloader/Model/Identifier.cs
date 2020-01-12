using JMI.General.ListSelection;
using System;

namespace ComicDownloader.Model
{
    public abstract class Identifier : SelectionCollectionItem
    {
        #region constructors
        /// <summary>
        /// Creates new identifier (<see cref="Id"/>)
        /// </summary>
        public Identifier()
        {
            id = Guid.NewGuid();
            OnPropertyChanged(nameof(Id));
        }

        /// <summary>
        /// Constructor for existing item
        /// </summary>
        /// <param name="uniqueIdentifier">Unique identifier for object</param>
        public Identifier(Guid uniqueIdentifier)
        {
            id = uniqueIdentifier;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// unique id (<see cref="Guid"/>)
        /// </summary>
        protected Guid id;
        /// <summary>
        /// Returns the unique (<see cref="Guid"/>) id of the item as a string.
        /// </summary>
        public override string Id { get { return id.ToString(); } }
        #endregion properties

        #region methods
        /// <summary>
        /// Returns the unique id of the item.
        /// </summary>
        /// <returns><see cref="Guid"/></returns>
        public Guid GetGuid()
        {
            return id;
        }
        #endregion

    }
}
