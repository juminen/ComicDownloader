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