using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace wpfmenu.Types
{
    /// <summary>
    /// BindingList of <see cref="Model.Result" />, but automatically increments Model.Result.Index (for xaml usage).
    // todo: add support for Add()
    /// </summary>
    public class BindingResultsList : List<Model.Result>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new void Add(Model.Result value)
        {
            throw new NotImplementedException();
            //value.Index = Count;
            //base.Add(value);
        }
        public void NotifyChanged(NotifyCollectionChangedEventArgs e)
        {
            
            if (CollectionChanged != null) {
                var alternate = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged(this, alternate);
            }
        }
        public void AddRange(List<Model.Result> collection)
        {
            var i = 0;
            foreach (var r in collection) {
                r.Index = Count + i++;
            }
            base.AddRange(collection);
            NotifyChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
        }
    }
}
