namespace Else.DesignerData
{
    public class MockThemeEditorViewModel
    {
        public MockLauncherViewModel LauncherViewModel { get; set; } = new MockLauncherViewModel();

        public object UnloadedCommand
        {
            get { throw new System.NotImplementedException(); }
        }

        public object HasChanged
        {
            get { throw new System.NotImplementedException(); }
        }

        public object SaveCommand
        {
            get { throw new System.NotImplementedException(); }
        }

        public object RevertCommand
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}