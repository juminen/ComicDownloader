using ComicDownloader.Model.Editors;
using JMI.General;
using JMI.General.VM.Application;
using JMI.General.VM.IO.Picker;
using System;

namespace ComicDownloader.UI.ViewModel
{
    class EditComicTabViewModel : TabViewModel
    {
        #region constructors
        public EditComicTabViewModel(ComicEditor comicEditor)
        {
            editor = comicEditor ?? throw new ArgumentNullException(nameof(comicEditor) + " can not be null");
            editor.ValidationResultsUpdated += OnValidationResultsUpdated;
            editor.ChangesUpdated += OnEditorChanges;
            editor.ChangesCanceled += OnEditorChanges;

            SavingLocationPicker = DefaultPickers.DirectoryPicker;
            SavingLocationPicker.SelectedPath = editor.SavingLocation;
            SavingLocationPicker.PropertyChanged += OnSavingLocationPropertyChanged;
            AllowClose = true;
            SetDisplayText();
        }
        #endregion

        #region properties
        private readonly ComicEditor editor;

        private string name;
        public string Name
        {
            get { return editor.Name; }
            set { SetProperty(ref name, value); editor.Name = value; }
        }

        private DateTime lastDownloadDate;
        public DateTime LastDownloadDate
        {
            get { return editor.LastDownloadDate; }
            set { SetProperty(ref lastDownloadDate, value); editor.LastDownloadDate = value; }
        }

        private string startUrl;
        public string StartUrl
        {
            get { return editor.StartUrl; }
            set { SetProperty(ref startUrl, value); editor.StartUrl = value; }
        }

        private DirectoryPickerViewModel savingLocationPicker;
        public DirectoryPickerViewModel SavingLocationPicker
        {
            get { return savingLocationPicker; }
            private set { SetProperty(ref savingLocationPicker, value); }
        }

        private string validationResult;
        public string ValidationResult
        {
            get { return validationResult; }
            private set { SetProperty(ref validationResult, value); }
        }
        #endregion

        #region commands
        private RelayCommand updateCommand;
        public RelayCommand UpdateCommand
        {
            get
            {
                if (updateCommand == null)
                {
                    updateCommand =
                      new RelayCommand(
                          async param => await editor.UpdateChangesAsync(),
                          param => editor.HasChanges);
                }
                return updateCommand;
            }
        }

        private RelayCommand cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand =
                      new RelayCommand(
                          param => editor.CancelChanges(),
                          param => editor.HasChanges);
                }
                return cancelCommand;
            }
        }
        #endregion

        #region methods
        public override void Dispose()
        {
            SavingLocationPicker.PropertyChanged -= OnSavingLocationPropertyChanged;
            editor.ValidationResultsUpdated -= OnValidationResultsUpdated;
            editor.ChangesUpdated -= OnEditorChanges;
            editor.ChangesCanceled -= OnEditorChanges;
            editor.EndEditing();
        }
        private void SetDisplayText()
        {
            DisplayText = $"Edit item { Name }";
        }

        #endregion

        #region events
        #endregion

        #region event handlers
        private void OnValidationResultsUpdated(object sender, EventArgs e)
        {
            ValidationResult = editor.ValidationResults;
        }

        private void OnEditorChanges(object sender, EventArgs e)
        {
            SetDisplayText();
            SavingLocationPicker.SelectedPath = editor.SavingLocation;
            OnPropertyChanged("");
        }

        private void OnSavingLocationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(DirectoryPickerViewModel.SelectedPath)))
            {
                editor.SavingLocation = SavingLocationPicker.SelectedPath;
            }
        }
        #endregion
    }
}



//class EditComicTabViewModel : TabViewModel
//{
//    #region constructors
//    public EditComicTabViewModel(ComicUpdater comicUpdater, Comic comicToModify)
//    {
//        comic = comicToModify ?? throw new ArgumentNullException(nameof(comicToModify) + " can not be null");
//        updater = comicUpdater ?? throw new ArgumentNullException(nameof(comicUpdater) + " can not be null");
//        updater.StatusChanged += OnCreatorStatusChanged;
//        SavingLocation = DefaultPickers.DirectoryPicker;
//        AllowClose = true;
//        ReadValuesFromItem();
//        SetDisplayText();

//        tracker = new CustomStateTracker();
//        tracker.AddTracking(
//            comic, nameof(comic.Name),
//            this, nameof(Name));
//        tracker.AddTracking(
//            comic, nameof(comic.LastDownloadDate),
//            this, nameof(LastDownloadDate));
//        tracker.AddTracking(
//            comic, nameof(comic.StartUrl),
//            this, nameof(StartUrl));
//        tracker.AddTracking(
//            comic, nameof(comic.SavingLocation),
//            SavingLocation, nameof(SavingLocation.SelectedPath));
//    }
//    #endregion

//    #region properties
//    private readonly Comic comic;
//    private readonly ComicUpdater updater;
//    private readonly ICustomStateTracker tracker;

//    private string name;
//    public string Name
//    {
//        get { return name; }
//        set { SetProperty(ref name, value); }
//    }

//    private DateTime lastDownloadDate;
//    public DateTime LastDownloadDate
//    {
//        get { return lastDownloadDate; }
//        set { SetProperty(ref lastDownloadDate, value); }
//    }

//    private string startUrl;
//    public string StartUrl
//    {
//        get { return startUrl; }
//        set { SetProperty(ref startUrl, value); }
//    }

//    private DirectoryPickerViewModel savingLocation;
//    public DirectoryPickerViewModel SavingLocation
//    {
//        get { return savingLocation; }
//        private set { SetProperty(ref savingLocation, value); }
//    }

//    public string Status { get { return updater.Status; } }
//    #endregion

//    #region commands
//    private RelayCommand updateCommand;
//    public RelayCommand UpdateCommand
//    {
//        get
//        {
//            if (updateCommand == null)
//            {
//                updateCommand =
//                  new RelayCommand(
//                      async param => await UpdateComicAsync(),
//                      param => !tracker.SameState);
//            }
//            return updateCommand;
//        }
//    }

//    private RelayCommand cancelCommand;
//    public RelayCommand CancelCommand
//    {
//        get
//        {
//            if (cancelCommand == null)
//            {
//                cancelCommand =
//                  new RelayCommand(
//                      param => Cancel(),
//                      param => !tracker.SameState);
//            }
//            return cancelCommand;
//        }
//    }
//    #endregion

//    #region methods
//    public override void Dispose()
//    {
//        tracker.Dispose();
//    }

//    private void ReadValuesFromItem()
//    {
//        Name = comic.Name;
//        LastDownloadDate = comic.LastDownloadDate;
//        StartUrl = comic.StartUrl;
//        SavingLocation.SelectedPath = comic.SavingLocation;
//    }

//    private void SetDisplayText()
//    {
//        DisplayText = $"Edit item { Name }";
//    }

//    protected async Task UpdateComicAsync()
//    {
//        comic.CreateMemento();
//        comic.Name = Name;
//        comic.LastDownloadDate = LastDownloadDate;
//        comic.StartUrl = StartUrl;
//        comic.SavingLocation = SavingLocation.SelectedPath;
//        bool result = await updater.UpdateComicAsync(comic);
//        if (result)
//        {
//            SetDisplayText();
//        }
//        else
//        {
//            comic.BackupFromMemento();
//        }
//    }

//    private void Cancel()
//    {
//        ReadValuesFromItem();
//    }
//    #endregion

//    #region events
//    #endregion

//    #region event handlers
//    private void OnCreatorStatusChanged(object sender, EventArgs e)
//    {
//        OnPropertyChanged(nameof(Status));
//    }
//    #endregion
//}