using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Else.Helpers;
using Else.Model;
using Else.ViewModels;

namespace Else.Views.Controls
{
    
    /// <summary>
    /// This control extends the Launcher view with editing behaviour (choosing colors etc)
    /// </summary>
    partial class ThemeEditor
    {
        public ThemeEditorViewModel ViewModel;

        public Theme ActiveTheme {
            get {
                return GetValue(ActiveThemeProperty) as Theme;
            }
            set{
                SetValue(ActiveThemeProperty, value);
            }
        }
        public static readonly DependencyProperty ActiveThemeProperty =
            DependencyProperty.Register("ActiveTheme", typeof(Theme), typeof(ThemeEditor), new PropertyMetadata(ActiveThemeChanged));

        private static void ActiveThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var obj = d as ThemeEditor;
            //if (obj != null) {
            //    var viewModel = obj.DataContext as ThemeEditorViewModel;
            //    viewModel.SetTheme(e.NewValue as Theme);
            //}
        }

        public ThemeEditor()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel = DataContext as ThemeEditorViewModel;
            SetupEditBehaviour();
        }

        /// <summary>
        /// Because we use the same Launcher as the main app, we must augment its functionality to allow editing.
        /// Adds click handlers to different components of the launcher, to allow editing. 
        /// (e.g. will make QueryInput clickable to change text color)
        /// Modifies some elements to make them suit our purpose.
        /// </summary>
        private void SetupEditBehaviour()
        {
            // shrink QueryInput (so we can detect clicks next to it, "QueryBoxBackground") and make it ReadOnly
            Launcher.QueryInput.Width = 230;
            Launcher.QueryInput.IsReadOnly = true;
            Launcher.QueryInput.HorizontalAlignment = HorizontalAlignment.Left;

            // setup interactive components of our wysiwyg style theme editor
            SetMouseHandlersForElement("QueryBoxTextColor", "Query Box Text Color", UI.FindChildByTypeName(Launcher.QueryInput, "TextBoxView"));
            SetMouseHandlersForElement("QueryBoxBackgroundColor", "Query Box Background Color", Launcher.QueryInputContainer);
            SetMouseHandlersForElement("WindowBorderColor", "Window Border Color", Launcher.WindowBorder);
            SetMouseHandlersForElement("WindowBackgroundColor", "Launcher Background Color", Launcher.Container);

            // because there are multiple instances of ResultTitle and ResultSubTitle (multiple results), we must add handlers to each one
            foreach (var element in UI.FindVisualChildren<TextBlock>(Launcher.ResultsList, "Title")) {
                SetMouseHandlersForElement("ResultTitleColor", "Result Title Color", element);
            }
            foreach (var element in UI.FindVisualChildren<TextBlock>(Launcher.ResultsList, "SubTitle")) {
                SetMouseHandlersForElement("ResultSubTitleColor", "Result SubTitle Color", element);
            }

            // add handlers for ResultContainer, detects if it is a selected result (because that uses different styles)
            foreach (var element in UI.FindVisualChildren<StackPanel>(Launcher.ResultsList, "ResultContainer")) {
                // check if this element is selected, by checking if the subtitle 
                if (element.Background.Equals(Application.Current.Resources.MergedDictionaries[1]["ResultSelectedBackgroundColor"])) {
                    SetMouseHandlersForElement("ResultSelectedBackgroundColor", "Result Selected Background Color", element);
                }
                else {
                    SetMouseHandlersForElement("ResultBackgroundColor", "Result Background Color", element);
                }
            }

            // seperators between results
            var separators = UI.FindVisualChildren<Border>(Launcher.ResultsList, "preResultSeparator").ToList();
            separators.AddRange(UI.FindVisualChildren<Border>(Launcher.ResultsList, "postResultSeparator").ToList());

            foreach (var element in separators) {
                SetMouseHandlersForElement("ResultSeparatorColor", "Result Separator Color", element);
            }
        }

        /// <summary>
        /// Sets the mouse handlers for element. Mouse hover handler is added to change the helpful text, and click handler is added to open the color picker.
        /// When the color is saved, it updates the theme, and applies it.
        /// </summary>
        /// <param name="themeKey">The theme key (as per the json theme).</param>
        /// <param name="hoverText">The hover text.</param>
        /// <param name="element">The element on which we add hover and click handlers.</param>
        private void SetMouseHandlersForElement(string themeKey, string hoverText, UIElement element)
        {
            // change the instruction text to hoverText
            element.IsMouseDirectlyOverChanged += (sender, e) => {
                if (ViewModel.Editable && (bool)e.NewValue) {
                    // mouse is now directly over this element, show the helpful text to the user so they know what they are hovering
                    HoveredElementInfo.Text = hoverText;
                    return;
                }
                // mouse is no longer directly over this element
                HoveredElementInfo.Text = "";
            };
            // handle onclick
            element.PreviewMouseDown += (sender, e) => {
                // the rhs of the OR is a hack because TextBox elements have child element of TextBoxView (internal), its ugly, needs improvement
                var isCorrectElement = e.OriginalSource.Equals(element) || (element.GetType() == typeof(TextBox) && e.OriginalSource.GetType().Name == "TextBoxView");
                if (ViewModel.Editable && isCorrectElement) {
                    // mouse is now directly over this element, show the helpful text to the user so they know what they are hovering
                    ViewModel.ShowColorPicker(Window.GetWindow(this), hoverText, themeKey);
                    e.Handled = true;
                }
            };
        }
    }
}
