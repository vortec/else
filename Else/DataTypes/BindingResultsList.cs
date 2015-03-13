using System.Collections.Generic;
using System.Collections.Specialized;

namespace Else.DataTypes
{
    /// <summary>
    /// BindingList of <see cref="Model.Result" />, but automatically increments Model.Result.Index (for xaml usage).
    /// <remarks>
    /// BindingList does not trigger data-binding change events, so the UI will not update.  Instead you must call BindingRefresh to trigger the databinding event.
    /// Only "Reset" event is triggered, so the UI will redraw every element.
    /// </remarks>
    /// </summary>
    public class BindingResultsList : List<Model.Result>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Trigger CollectionChanged Reset event, so the UI element bound to this data source is refreshed.
        /// </summary>
        public void BindingRefresh()
        {
            if (CollectionChanged != null) {
                var notification = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged(this, notification);
            }
        }
        public new void Add(Model.Result value)
        {
            value.Index = Count;
            base.Add(value);
        }
        public void AddRange(List<Model.Result> collection)
        {
            if (collection != null) {
                var i = 0;
                foreach (var r in collection) {
                    r.Index = Count + i++;
                }
                base.AddRange(collection);
            }
        }
    }
}
