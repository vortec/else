using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Else.DataTypes;
using Else.Helpers;
using Else.Lib;
using Else.Model;
using Else.Services;
using Else.Services.Interfaces;

namespace Else.ViewModels
{
    public class ThemeEditorViewModel : DependencyObject
    {
        /// <summary>
        /// Has the user actually made any changes to the edited theme?
        /// </summary>
        public static readonly DependencyProperty HasChangedProperty =
            DependencyProperty.Register("HasChanged", typeof(bool), typeof(ThemeEditorViewModel), new PropertyMetadata(false));

        /// <summary>
        /// Sample data used when a preview of the launcher is displayed.
        /// todo: add more examples here
        /// </summary>
        private static BindingResultsList _items = new BindingResultsList{
            new Result{
                Title = "Google Chrome",
                Icon = IconTools.GetBitmapForFile(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"),
                SubTitle = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            },
            new Result{
                Title = "Notepad",
                Icon = IconTools.GetBitmapForFile(@"C:\Windows\system32\notepad.exe"),
                SubTitle = @"C:\Windows\system32\notepad.exe",
            },
            new Result{
                Title = "Internet Explorer",
                Icon = IconTools.GetBitmapForFile(@"C:\Program Files\Internet Explorer\iexplore.exe"),
                SubTitle = @"C:\Program Files\Internet Explorer\iexplore.exe",
            },
        };
        public static BindingResultsList Items
        {
            get {
                return _items;
            }
        }
        
        public ICommand SaveCommand { get; set; }
        public ICommand RevertCommand { get; set; }

        
        /// <summary>
        /// Gets a value indicating whether this <see cref="Theme"/> is editable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if editable; otherwise, <c>false</c>.
        /// </value>
        public bool Editable
        {
            get {
                return _editedTheme.Editable;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the theme has been edited.
        /// </summary>
        /// <value>
        /// <c>true</c> if the theme has changed; otherwise, <c>false</c>.
        /// </value>
        public bool HasChanged
        {
            get { return (bool)GetValue(HasChangedProperty); }
            set { SetValue(HasChangedProperty, value); }
        }

        /// <summary>
        /// Reference to the theme we are editing
        /// </summary>
        private Theme _originalTheme;
        /// <summary>
        /// Copy of the original theme that is currently being edited (we need this clone so that we can offer Revert to the original later)
        /// </summary>
        private Theme _editedTheme;
        
        private readonly IPickerWindow _colorPickerWindow;
        /// <summary>
        /// The picker window that is currently open.
        /// </summary>
        private IPickerWindow _activePickerWindow;

        private readonly ThemeManager _themeManager;

        public ThemeEditorViewModel(ThemeManager themeManager, IPickerWindow colorPickerWindow)
        {
            _themeManager = themeManager;
            _colorPickerWindow = colorPickerWindow;
            SaveCommand = new RelayCommand(param => Save());
            RevertCommand = new RelayCommand(param => Revert());
        }

        /// <summary>
        /// Sets a config parameter on the currently editedtheme and then applies it so the user can see the changes.
        /// </summary>
        /// <remarks>This method is called many times when the color picker is used.</remarks>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetConfigParam(string key, string value)
        {
            if (_editedTheme.Config.ContainsKey(key) && value == _editedTheme.Config[key]) {
                // config is unchanged, so we return early
                return;
            }

            // update config
            _editedTheme.Config[key] = value;

            // apply the theme
            _themeManager.ApplyTheme(_editedTheme);
            // theme is now dirty
            HasChanged = true;
        }

        /// <summary>
        /// Sets the theme to be edited.
        /// We store a reference to the original theme, and create a clone for our editing purposes.
        /// </summary>
        public void SetTheme(Theme originalTheme)
        {
            // close any popups
            HidePickerWindow();
            // store reference to original theme
            _originalTheme = originalTheme;
            // create a clone
            _editedTheme = originalTheme.Clone();
            // apply the clone
            _themeManager.ApplyTheme(_editedTheme);
            _themeManager.SaveSettings();
            // editor state is unchanged (no cancel or save buttons shown)
            HasChanged = false;
        }

        /// <summary>
        /// Save the current _editedTheme (replace original with our temporary clone)
        /// </summary>
        public void Save()
        {
            // ensure popup is closed
            HidePickerWindow();

            // replace _originalTheme config with the config from _editedTheme
            _originalTheme.CopyFrom(_editedTheme);

            // save the theme to disk
            _originalTheme.Save();

            // apply the theme
            _themeManager.ApplyTheme(_originalTheme);
            _themeManager.SaveSettings();

            // editor state is unchanged (no cancel or save buttons shown)
            HasChanged = false;
        }

        /// <summary>
        /// User has decided to revert their edits to this theme, so we restore the _originalTheme and discard the _editedTheme.
        /// </summary>
        public void Revert()
        {
            // ensure popup is closed
            HidePickerWindow();
            // restore _originalTheme
            SetTheme(_originalTheme);

            // apply the theme
            _themeManager.ApplyTheme(_originalTheme);
            _themeManager.SaveSettings();
            HasChanged = false;
        }
        
        public void HidePickerWindow()
        {
            if (_activePickerWindow != null) {
                _activePickerWindow.Close();
                _activePickerWindow = null;
            }
        }

        /// <summary>
        /// Shows the color picker, automatically updates _editedTheme.Config[themeKey] whenever a new color is chosen.
        /// </summary>
        /// <param name="parentWindow">The parent window (we need this so that our picker window can be owned by the parent.</param>
        /// <param name="windowTitle">The window title.</param>
        /// <param name="themeKey">The theme key.</param>
        public void ShowColorPicker(Window parentWindow, string windowTitle, string themeKey)
        {
            // update the theme when the color is changed
            _colorPickerWindow.PropertyChanged += (sender, e) => {
                var newBrush = new SolidColorBrush((Color)e.NewValue).ToString();
                SetConfigParam(themeKey, newBrush);
            };
            // hide any currently open picker window
            HidePickerWindow();
            // show the color picker window
            _colorPickerWindow.Show(parentWindow, windowTitle);
            // mark is active
            _activePickerWindow = _colorPickerWindow;
        }
    }
}
