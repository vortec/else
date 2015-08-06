using System.Linq;
using System.Windows;
using System.Windows.Input;
using Else.Lib;
using Else.Model;
using Else.Services;
using Microsoft.Win32;

namespace Else.ViewModels
{
    public class ThemesWindowViewModel : ObservableObject
    {
        private readonly ThemeManager _themeManager;

        /// <summary>
        /// The currently selected theme
        /// <remarks>ThemeList control binds to this</remarks>
        /// </summary>
        private ThemeViewModel _selectedItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ThemesWindowViewModel" /> class.
        /// </summary>
        public ThemesWindowViewModel(ThemeEditorViewModel themeEditorViewModel, ThemeManager themeManager)
        {
            ThemeEditorViewModel = themeEditorViewModel;
            _themeManager = themeManager;

            Items = new ViewModelCollectionWrapper<ThemeViewModel, Theme>(_themeManager.Themes);

            // select the current theme
            if (themeManager.ActiveTheme != null) {
                SelectedItem = Items.First(t => t.Model == themeManager.ActiveTheme);
            }

            // connect the commands to methods
            DuplicateCommand = new RelayCommand(param => Duplicate());
            ExportCommand = new RelayCommand(param => Export());
            // delete command is only available if the ActiveTheme is editable
            DeleteCommand = new RelayCommand(param => Delete(),
                param => _themeManager.ActiveTheme != null && _themeManager.ActiveTheme.Editable);
        }

        public ThemeEditorViewModel ThemeEditorViewModel { get; set; }

        /// <summary>
        /// Selected theme
        /// </summary>
        public ThemeViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value != null) {
                    SetProperty(ref _selectedItem, value);
                    // notify ThemeEditorViewModel
                    ThemeEditorViewModel.SetTheme(_selectedItem.Model);
                }
            }
        }

        /// <summary>
        /// Provides access to the currently loaded themes from the ThemeManager
        /// <remarks>ThemeList control binds to this</remarks>
        /// </summary>
        public ViewModelCollectionWrapper<ThemeViewModel, Theme> Items { get; set; }

        /// <summary>
        /// Duplicate the currently selected theme.
        /// </summary>
        public ICommand DuplicateCommand { get; set; }

        /// <summary>
        /// Export the currently selected theme.
        /// </summary>
        public ICommand ExportCommand { get; set; }

        /// <summary>
        /// Delete the currently selected theme.
        /// </summary>
        public ICommand DeleteCommand { get; set; }

        /// <summary>
        /// Show a save file dialog that will export the currently selected theme to a .json file.
        /// </summary>
        private void Export()
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON themes|*.json",
                AddExtension = true
            };

            var result = dialog.ShowDialog();

            // if 'save' button was pressed
            if (result == true) {
                _themeManager.ActiveTheme.Save(dialog.FileName);
            }
        }

        /// <summary>
        /// Duplicates the selected theme.
        /// </summary>
        private void Duplicate()
        {
            var clone = SelectedItem.Model.Duplicate();
            clone.Save();
            _themeManager.RegisterTheme(clone);
            SelectedItem = Items.First(model => model.Model == clone);
        }

        /// <summary>
        /// Deletes the selected theme.
        /// </summary>
        private void Delete()
        {
            var result = MessageBox.Show("Remove currently selected theme?", "Delete Theme", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes) {
                var idx = Items.IndexOf(SelectedItem);
                // delete the theme
                SelectedItem.Model.Delete();

                // unregister the theme
                _themeManager.UnregisterTheme(SelectedItem.Model);

                // select another theme
                if (Items.Any()) {
                    if (idx >= Items.Count) {
                        idx = Items.Count - 1;
                    }
                    SelectedItem = Items[idx];
                }
                else {
                    SelectedItem = null;
                }
            }
        }
    }
}