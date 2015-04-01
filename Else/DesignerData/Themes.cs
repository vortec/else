using System.ComponentModel;
using Else.Model;

namespace Else.DesignerData
{
    /// <summary>
    /// Provides test data for xaml Design View.
    /// </summary>
    public class Themes
    {
        public BindingList<Theme> Items => new BindingList<Theme>() {
            new Theme{
                Author = "James Hutchby",
                Name = "Test Theme 1",
            },
            new Theme{
                Author = "James Hutchby",
                Name = "Test Theme 2",
            },
            new Theme{
                Author = "James Hutchby James Hutchby James Hutchby James Hutchby",
                Name = "Test Theme Test Theme Test Theme Test Theme Test Theme Test Theme Test Theme Test Theme ",
            },
        };

        public Theme SelectedItem => Items[0];
    }
}
