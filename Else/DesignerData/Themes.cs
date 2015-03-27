using System.ComponentModel;
using Else.Lib;

namespace Else.DesignerData
{
    /// <summary>
    /// Provides test data for xaml Design View.
    /// </summary>
    public class Themes
    {
        public BindingList<Theme> Items
        {
            get {
                return new BindingList<Theme>() {
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
            }
        }
        public Theme SelectedItem
        {
            get { return Items[0]; }
        }
    }
}
