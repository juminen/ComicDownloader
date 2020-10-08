using ComicDownloader.Model;
using JMI.General;
using JMI.General.Selections;
using JMI.General.VM.Selections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ComicDownloader.UI.ViewModel
{
    class ComicPhotoListViewModel : SelectionCollectionViewModel<ComicPhoto, ComicPhotoListItemViewModel>
    {
        #region constructors
        public ComicPhotoListViewModel(ComicPhotoCollection selectionCollection) : base(selectionCollection)
        {
            SetSorting();
            BuildFilterLists();
            collection.CollectionChangeAdded += OnCollectionChangeAdded;
            collection.CollectionChangeCleared += OnCollectionChangeCleared;
            collection.CollectionChangeRemoved += OnCollectionChangeRemoved;
        }
        #endregion

        #region properties
        private DateTime? filterPublishDateStart;
        public DateTime? FilterPublishDateStart
        {
            get { return filterPublishDateStart; }
            set { SetProperty(ref filterPublishDateStart, value); }
        }

        private DateTime? filterPublishDateEnd;
        public DateTime? FilterPublishDateEnd
        {
            get { return filterPublishDateEnd; }
            set { SetProperty(ref filterPublishDateEnd, value); }
        }

        private bool filtersApplied;

        private string selectedNameFilter;
        public string SelectedNameFilter
        {
            get { return selectedNameFilter; }
            set { SetProperty(ref selectedNameFilter, value); }
        }

        private IList<string> nameFilters;
        public IList<string> NameFilters
        {
            get { return nameFilters; }
            private set { SetProperty(ref nameFilters, value); }
        }

        private string selectedStatusFilter;
        public string SelectedStatusFilter
        {
            get { return selectedStatusFilter; }
            set { SetProperty(ref selectedStatusFilter, value); }
        }

        private IList<string> statusFilters;
        public IList<string> StatusFilters
        {
            get { return statusFilters; }
            private set { SetProperty(ref statusFilters, value); }
        }
        #endregion

        #region commands
        private RelayCommand clearFiltersCommand;
        public RelayCommand ClearFiltersCommand
        {
            get
            {
                if (clearFiltersCommand == null)
                {
                    clearFiltersCommand =
                      new RelayCommand(
                          param => ClearFilters(),
                          param => filtersApplied);
                }
                return clearFiltersCommand;
            }
        }

        private RelayCommand applyFiltersCommand;
        public RelayCommand ApplyFiltersCommand
        {
            get
            {
                if (applyFiltersCommand == null)
                {
                    applyFiltersCommand =
                      new RelayCommand(
                          param => ApplyFilters(),
                          param => true);
                }
                return applyFiltersCommand;
            }
        }
        #endregion

        #region methods
        public override void Dispose()
        {
            base.Dispose();
            collection.CollectionChangeAdded -= OnCollectionChangeAdded;
            collection.CollectionChangeCleared -= OnCollectionChangeCleared;
            collection.CollectionChangeRemoved -= OnCollectionChangeRemoved;
        }

        protected override ComicPhotoListItemViewModel CreateViewModel(ISelectionItem<ComicPhoto> item)
        {
            return new ComicPhotoListItemViewModel(item);
        }

        private void SetSorting()
        {
            ClearSorting();
            AllItems.SortDescriptions.Add(
                new SortDescription(
                    nameof(ComicPhotoListItemViewModel.SortByComicNameAndPublishDate),
                    ListSortDirection.Ascending));
        }

        private void BuildFilterLists()
        {
            IList<string> namelist =
                collection.GetAllTargetItems()
                .Select(a => a.Parent.Name)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            if (namelist.Count > 0)
            {
                NameFilters = namelist;
            }
            else
            {
                NameFilters = new List<string>();
            }
            NameFilters.Insert(0, string.Empty);
            SelectedNameFilter = NameFilters[0];

            IList<string> statuslist =
                collection.GetAllTargetItems()
                .Select(a => a.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            if (statuslist.Count > 0)
            {
                StatusFilters = statuslist;
            }
            else
            {
                StatusFilters = new List<string>();
            }
            StatusFilters.Insert(0, string.Empty);
            SelectedStatusFilter = StatusFilters[0];
        }

        private void ClearFilters()
        {
            AllItems.Filter = null;
            SelectedNameFilter = NameFilters[0];
            FilterPublishDateStart = null;
            FilterPublishDateEnd = null;
            SelectedStatusFilter = StatusFilters[0];
            filtersApplied = false;
            SetSorting();
        }

        private void ApplyFilters()
        {
            AllItems.Filter = Filter;
            filtersApplied = true;
            SetSorting();
        }

        private bool Filter(object item)
        {
            ComicPhotoListItemViewModel listItem = item as ComicPhotoListItemViewModel;
            ComicPhoto photo = listItem.Photo;
            #region comic name
            bool result1 = false;
            if (SelectedNameFilter.Equals(string.Empty))
            {
                result1 = true;
            }
            else if (photo.Parent.Name.Equals(SelectedNameFilter))
            {
                result1 = true;
            }
            #endregion

            #region publish date
            bool result2 = false;
            //both dates are empty
            if (!FilterPublishDateStart.HasValue &&
                !FilterPublishDateEnd.HasValue)
            {
                result2 = true;
            }
            //both dates are set
            else if (FilterPublishDateStart.HasValue &&
                FilterPublishDateEnd.HasValue)
            {
                if (photo.PublishDate >= FilterPublishDateStart.Value &&
                photo.PublishDate <= FilterPublishDateEnd.Value)
                {
                    result2 = true;
                }
            }
            //start date is set and end date is empty
            else if (FilterPublishDateStart.HasValue &&
                !FilterPublishDateEnd.HasValue)
            {
                if (photo.PublishDate >= FilterPublishDateStart.Value)
                {
                    result2 = true;
                }
            }
            //start date is empty and end date is set
            else if (!FilterPublishDateStart.HasValue &&
                FilterPublishDateEnd.HasValue)
            {
                if (photo.PublishDate <= FilterPublishDateEnd.Value)
                {
                    result2 = true;
                }
            }
            #endregion

            #region comic status
            bool result3 = false;
            if (SelectedStatusFilter.Equals(string.Empty))
            {
                result3 = true;
            }
            if (photo.Status.Equals(SelectedStatusFilter))
            {
                result3 = true;
            }
            #endregion
            return result1 && result2 && result3;
        }
        #endregion

        #region events
        #endregion

        #region event handlers
        private void OnCollectionChangeRemoved(object sender, SelectionCollectionRemoveEventArgs e)
        {
            BuildFilterLists();
        }

        private void OnCollectionChangeCleared(object sender, EventArgs e)
        {
            BuildFilterLists();
        }

        private void OnCollectionChangeAdded(object sender, SelectionCollectionAddEventArgs<ComicPhoto> e)
        {
            BuildFilterLists();
        }
        #endregion
    }
}
