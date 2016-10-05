using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace OsrmRoutesEditor.Helpers
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        protected enum ProcessRangeAction
        {
            Add,
            Replace,
            Remove
        };

        public ObservableRangeCollection() : base()
        {
        }

        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public ObservableRangeCollection(List<T> list) : base(list)
        {
        }

        protected virtual void ProcessRange(IEnumerable<T> collection, ProcessRangeAction action)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            var items = collection as IList<T> ?? collection.ToList();
            if (!items.Any()) return;

            this.CheckReentrancy();

            if (action == ProcessRangeAction.Replace) this.Items.Clear();
            foreach (var item in items)
            {
                if (action == ProcessRangeAction.Remove) this.Items.Remove(item);
                else this.Items.Add(item);
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs(CountString));
            this.OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void AddRange(IEnumerable<T> collection)
        {
            this.ProcessRange(collection, ProcessRangeAction.Add);
        }

        public void ReplaceRange(IEnumerable<T> collection)
        {
            this.ProcessRange(collection, ProcessRangeAction.Replace);
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            this.ProcessRange(collection, ProcessRangeAction.Remove);
        }
    }
}
