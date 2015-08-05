namespace Else.DesignerData
{
    public class MockLauncherViewModel
    {
        public string QueryInputText { get; set; } = "Mock Query Text";
        public MockResultsListViewModel ResultsListViewModel { get; set; } = new MockResultsListViewModel();
    }
}