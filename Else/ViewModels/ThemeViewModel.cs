using System.ComponentModel;
using Else.Lib;
using Else.Model;

namespace Else.ViewModels
{
    public class ThemeViewModel : ObservableObject, IViewModelModelProp<Theme>
    {
        public string Name => Model.Name;

        public string Author
        {
            get { return "by " + Model.Author; }
            set { Model.Author = value; }
        }

        public string GUID => Model.GUID;
        public bool Editable => Model.Editable;
        public Theme Model { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}