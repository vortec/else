
namespace Else.Views
{
    public partial class SettingsWindow
    {
        public App App;
        public SettingsWindow(App app)
        {
            App = app;
            InitializeComponent();
            ThemesTab.Init(app.ThemeManager);
        }
    }
}
