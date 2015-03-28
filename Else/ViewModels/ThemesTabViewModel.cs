using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Else.Lib;
using Else.Model;
using Else.Services;
using Microsoft.Win32;

namespace Else.ViewModels
{
    public class ThemesTabViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The currently selected theme
        /// <remarks>ThemeList control binds to this</remarks>
        /// </summary>
        private Theme _selectedItem;
        public Theme SelectedItem
        {
            get {
                return _selectedItem;
            }
            set {
                if (value != null) {
                    // trigger _onThemeChanged mehtod
                    _onThemeChanged(value);
                    _selectedItem = value;
                    // trigger PropertyChanged
                    if (PropertyChanged != null) {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
                    }
                }
            }
        }

        /// <summary>
        /// Called when a new theme is selected.
        /// </summary>
        /// <param name="newTheme">The new theme.</param>
        private void _onThemeChanged(Theme newTheme)
        {
            // tell the themeEditor to begin editing the newly selected theme
            _editorViewModel.SetTheme(newTheme);
            // persist the theme change to settings
            _themeManager.SaveSettings();
        }

        /// <summary>
        /// Provides access to the currently loaded themes from the ThemeManager
        /// <remarks>ThemeList control binds to this</remarks>
        /// </summary>
        public BindingList<Theme> Items
        {
            get { return _themeManager.Themes; }
        }

        // commands that are bound to xaml buttons
        public ICommand DuplicateCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        private readonly ThemeManager _themeManager;
        private ThemeEditorViewModel _editorViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ThemesTabViewModel"/> class.
        /// </summary>
        public ThemesTabViewModel(ThemeEditorViewModel editorViewModel, ThemeManager themeManager)
        {
            _themeManager = themeManager;
            _editorViewModel = editorViewModel;
            
            // select the current theme
            if (themeManager.ActiveTheme != null) {
                SelectedItem = themeManager.ActiveTheme;
            }
            
            // connect the commands to methods
            DuplicateCommand = new RelayCommand(param => Duplicate());
            ExportCommand = new RelayCommand(param => Export());
            // delete command is only available if the ActiveTheme is editable
            DeleteCommand = new RelayCommand(param => Delete(), param => _themeManager.ActiveTheme != null && _themeManager.ActiveTheme.Editable);
        }

        /// <summary>
        /// Exports the selected theme to a .json file.
        /// </summary>
        private void Export()
        {
            var dialog = new SaveFileDialog {
                DefaultExt = ".json",
                Filter = "JSON themes|*.json",
                AddExtension = true
            };

            var result = dialog.ShowDialog();

            if (result == true) {
                _themeManager.ActiveTheme.Save(dialog.FileName);
            }
        }

        /// <summary>
        /// Duplicates the selected theme.
        /// </summary>
        private void Duplicate()
        {
            var clone = SelectedItem.Duplicate();
            _themeManager.RegisterTheme(clone);
            SelectedItem = clone;
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
                SelectedItem.Delete();
                
                // unregister the theme
                _themeManager.UnregisterTheme(SelectedItem);

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
