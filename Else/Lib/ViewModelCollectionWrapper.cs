using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Else.Lib
{
    /// <summary>
    /// Wraps a collection of models with viewmodels.  The underlying collection is updated automatically when 
    /// this collection is changed.  
    /// source: http://www.thesilvermethod.com/default.aspx?Id=VMCollectionWrapperSynchronizeaModelcollectionwithaViewModelcollection
    /// </summary>
    /// <typeparam name="T">The ViewModel type.  Must implment IViewModelModelProp"/></typeparam>
    /// <typeparam name="U">The model type</typeparam>
    public class ViewModelCollectionWrapper<T, U> : ObservableCollection<T> where T : IViewModelModelProp<U>, new()
    {
        private readonly IList<U> _collection;
        private readonly Func<U, T> _vmFactory;
        private bool _ignoreChanges;

        public ViewModelCollectionWrapper(IList<U> modelCollection, Func<U, T> vmFactory = null)
        {
            _vmFactory = vmFactory;
            CollectionChanged += VMCollectionWrapper_CollectionChanged;
            _collection = modelCollection;
            if (_collection is INotifyCollectionChanged) {
                var childWatch = _collection as INotifyCollectionChanged;
                childWatch.CollectionChanged += wrappedCollection_CollectionChanged;
            }
            _ignoreChanges = true;
            foreach (var model in _collection) {
                Add(CreateVm(model));
            }
            _ignoreChanges = false;
        }

        private T CreateVm(U model)
        {
            if (_vmFactory != null) {
                return _vmFactory(model);
            }
            return new T {Model = model};
        }

        /// <summary>
        /// Synchronizes chages from the Models collection to the ViewModels collection
        /// </summary>
        private void wrappedCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_ignoreChanges) {
                return;
            }
            _ignoreChanges = true;
            if (e.Action == NotifyCollectionChangedAction.Reset) {
                Clear();
                foreach (var model in _collection) {
                    Add(CreateVm(model));
                }
            }
            else {
                var toRemove = new List<T>();
                if (e.OldItems != null && e.OldItems.Count > 0) {
                    foreach (U model in e.OldItems) {
                        foreach (var viewModel in this) {
                            if (viewModel.Model.Equals(model)) {
                                toRemove.Add(viewModel);
                            }
                        }
                    }
                }
                foreach (var viewModel in toRemove) {
                    Remove(viewModel);
                }

                if (e.NewItems != null && e.NewItems.Count > 0) {
                    foreach (U model in e.NewItems) {
                        Add(CreateVm(model));
                    }
                }
            }
            _ignoreChanges = false;
        }

        /// <summary>
        /// Synchronizes changes from the ViewModels collection to the Models colleciton
        /// </summary>
        private void VMCollectionWrapper_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_ignoreChanges)
                return;

            _ignoreChanges = true;

            // If a reset, then e.OldItems is empty. Just clear and reload.
            if (e.Action == NotifyCollectionChangedAction.Reset) {
                _collection.Clear();

                foreach (var viewModel in this) {
                    _collection.Add(viewModel.Model);
                }
            }
            else {
                // Remove items from the models collection
                var toRemove = new List<U>();

                if (null != e.OldItems && e.OldItems.Count > 0)
                    foreach (T viewModel in e.OldItems) {
                        foreach (var model in _collection) {
                            if (viewModel.Model.Equals(model)) {
                                toRemove.Add(model);
                            }
                        }
                    }

                foreach (var model in toRemove) {
                    _collection.Remove(model);
                }

                // Add new viewmodel items to the models collection
                if (null != e.NewItems && e.NewItems.Count > 0) {
                    foreach (T viewModel in e.NewItems) {
                        _collection.Add(viewModel.Model);
                    }
                }
            }

            _ignoreChanges = false;
        }
    }


    public interface IViewModelModelProp<T>
    {
        T Model { get; set; }
    }
}