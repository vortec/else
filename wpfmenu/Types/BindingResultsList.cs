using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace wpfmenu.Types
{
    /// <summary>
    /// BindingList of <see cref="Model.Result" />, but automatically increments Model.Result.Index (for xaml usage).
    /// </summary>
    public class BindingResultsList : List<Model.Result>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new void Add(Model.Result value)
        {
            value.Index = Count;
            base.Add(value);
            NotifyChanged();
        }
        public void AddRange(List<Model.Result> collection)
        {
            if (collection != null) {
                var i = 0;
                foreach (var r in collection) {
                    r.Index = Count + i++;
                }
                base.AddRange(collection);
                NotifyChanged();
            }
        }
        public new void Clear()
        {
            base.Clear();
            NotifyChanged();
        }
        /// <summary>
        /// trigger reset always, since we don't need support for partial changes or such.
        /// </summary>
        private void NotifyChanged()
        {
            if (CollectionChanged != null) {
                var notification = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged(this, notification);
            }
        }
    }
}
