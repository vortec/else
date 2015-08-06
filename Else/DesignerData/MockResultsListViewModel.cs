using Else.DataTypes;
using Else.Extensibility;

namespace Else.DesignerData
{
    public class MockResultsListViewModel
    {
        public BindingResultsList Items => new BindingResultsList
        {
            new Result
            {
                Title = "Search Yahoo (Title Only)"
            },
            new Result
            {
                Title = "Search google (Title + SubTitle)",
                SubTitle = "Select this result to open a browser at the google search page"
            },
            new Result
            {
                Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis vulputate.",
                SubTitle =
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean laoreet dolor id rutrum facilisis. Suspendisse lectus dui, bibendum non augue at, congue accumsan ante. Nam gravida, eros nec pharetra vestibulum."
            }
        };

        public object ResultsListViewModel
        {
            get { throw new System.NotImplementedException(); }
        }

        public object SelectedIndex
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}