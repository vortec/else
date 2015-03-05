using System.ComponentModel;

namespace wpfmenu.Types
{
    /// <summary>
    /// BindingList of <see cref="Model.Result"/>, but automatically increments Model.Result.Index (for xaml usage).
    /// </summary>
    public class BindingResultsList : BindingList<Model.Result>
    {
        public new void Add(Model.Result value)
        {
            value.Index = Count;
            base.Add(value);
        }
    }
}
