using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

namespace wpfmenu.Types
{
    public class BindingResultsList : BindingList<Model.Result>
    {
        public new void Add(Model.Result value)
        {
            value.Index = Count;
            base.Add(value);
        }
    }
}
