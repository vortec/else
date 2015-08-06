namespace Else.DesignerData
{
    public class MockLauncherViewModel
    {
        public string QueryInputText { get; set; } = "Mock Query Text";
        public MockResultsListViewModel ResultsListViewModel { get; set; } = new MockResultsListViewModel();

        public object VisibilityChangedCommand
        {
            get { throw new System.NotImplementedException(); }
        }

        public object IsQueryInputFocused
        {
            get { throw new System.NotImplementedException(); }
        }

        public object QueryInputPreviewKeyDown
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}