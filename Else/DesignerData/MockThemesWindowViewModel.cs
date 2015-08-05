using System.ComponentModel;
using System.Windows.Input;
using Else.Model;

namespace Else.DesignerData
{
    public class MockThemesWindowViewModel
    {
        public BindingList<Theme> Items => new BindingList<Theme>
        {
            new Theme
            {
                Author = "James Hutchby",
                Name = "Test Theme 1"
            },
            new Theme
            {
                Author = "James Hutchby",
                Name = "Test Theme 2"
            },
            new Theme
            {
                Author = "James Hutchby James Hutchby James Hutchby James Hutchby",
                Name = "Test Theme Test Theme Test Theme Test Theme Test Theme Test Theme Test Theme Test Theme "
            }
        };

        public MockThemeEditorViewModel ThemeEditorViewModel { get; set; } = new MockThemeEditorViewModel();
        public Theme SelectedItem => Items[0];
        public ICommand DuplicateCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
    }
}