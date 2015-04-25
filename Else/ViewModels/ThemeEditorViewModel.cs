using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Else.DataTypes;
using Else.Extensibility;
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
            DependencyProperty.Register("HasChanged", typeof (bool), typeof (ThemeEditorViewModel),
                new PropertyMetadata(false));

        private readonly IColorPickerWindow _colorPickerWindow;

        /// <summary>
        /// Color picker used when changing element colors.
        /// </summary>
        //        private readonly IPickerWindow _colorPickerWindow;
        private readonly ThemeManager _themeManager;

        /// <summary>
        /// Copy of the original theme that is currently being edited (we need this clone so that we can offer Revert to the original later)
        /// </summary>
        private Theme _editedTheme;

        /// <summary>
        /// Reference to the theme we are editing
        /// </summary>
        private Theme _originalTheme;

        public ThemeEditorViewModel(ThemeManager themeManager, ThemeEditorLauncherViewModel launcherViewModel,
            IColorPickerWindow colorPickerWindow)
        {
            LauncherViewModel = launcherViewModel;
            _themeManager = themeManager;
            _colorPickerWindow = colorPickerWindow;
            SaveCommand = new RelayCommand(param => Save());
            RevertCommand = new RelayCommand(param => Revert());
            UnloadedCommand = new RelayCommand(param => Unloaded());
        }

        public ThemeEditorLauncherViewModel LauncherViewModel { get; set; }

        /// <summary>
        /// Example data for showing off the launcher styles.
        /// </summary>
        public static BindingResultsList Items { get; } = new BindingResultsList
        {
            new Result
            {
                Title = "Google Chrome",
                Icon = @"GetFileIcon://C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                SubTitle = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
            },
            new Result
            {
                Title = "Notepad",
                Icon = @"GetFileIcon://C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                SubTitle = @"C:\Windows\system32\notepad.exe"
            },
            new Result
            {
                Title = "Internet Explorer",
                Icon = @"GetFileIcon://C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                SubTitle = @"C:\Program Files\Internet Explorer\iexplore.exe"
            }
        };

        public ICommand SaveCommand { get; set; }
        public ICommand RevertCommand { get; set; }
        public ICommand UnloadedCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Theme"/> is editable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if editable; otherwise, <c>false</c>.
        /// </value>
        public bool Editable => _editedTheme.Editable;

        /// <summary>
        /// Gets or sets a value indicating whether the theme has been edited.
        /// </summary>
        /// <value>
        /// <c>true</c> if the theme has changed; otherwise, <c>false</c>.
        /// </value>
        public bool HasChanged
        {
            get { return (bool) GetValue(HasChangedProperty); }
            set { SetValue(HasChangedProperty, value); }
        }

        private void Unloaded()
        {
            if (_themeManager.ActiveTheme != _originalTheme) {
                _themeManager.ApplyTheme(_originalTheme);
            }
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
            // ensure existing window is closed
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
            _colorPickerWindow.Close();
        }

        /// <summary>
        /// Shows the color picker, automatically updates _editedTheme.Config[themeKey] whenever a new color is chosen.
        /// </summary>
        /// <param name="parentWindow">The parent window (we need this so that our picker window can be owned by the parent.</param>
        /// <param name="windowTitle">The window title.</param>
        /// <param name="themeKey">The theme key.</param>
        public void ShowColorPicker(Window parentWindow, string windowTitle, string themeKey)
        {
            Color? currentColor = null;
            if (_editedTheme.Config.ContainsKey(themeKey)) {
                var existingColor = ColorConverter.ConvertFromString(_editedTheme.Config[themeKey]);
                if (existingColor is Color) {
                    currentColor = (Color)existingColor;
                }
            }

            // color not found in existing theme, use default
            if (currentColor == null) {
                currentColor = Colors.DarkSlateGray;
            }

            HidePickerWindow();
            _colorPickerWindow.ColorChanged += (sender, color) => { SetConfigParam(themeKey, color); };
            _colorPickerWindow.Show(parentWindow, windowTitle, currentColor.Value);
        }
    }
}