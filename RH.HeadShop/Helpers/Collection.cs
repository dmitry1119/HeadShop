using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RH.HeadShop.Helpers
{
    [Serializable]
    public class Collection<T> : CollectionBase, IList<T>, IList, IChangeable
    {
        public Collection()
        {
        }
        public Collection(int capacity)
            : base(capacity)
        {
        }

        public T this[int index]
        {
            get
            {
                return (T)List[index];
            }
            set
            {
                List[index] = value;
            }
        }
        public virtual void Add(T item)
        {
            try
            {
                List.Add(item);
            }
            catch
            {
            }
        }
        public void AddRange(IEnumerable<T> items)
        {
            var array = items.ToArray();
            InnerList.AddRange(array);
            RaiseItemsAdded(array);
        }
        public void Insert(int index, T item)
        {
            List.Insert(index, item);
        }
        public bool Remove(T item)
        {
            var index = List.IndexOf(item);
            if (index < 0)
                return false;
            List.RemoveAt(index);
            return true;
        }
        public void RemoveRange(IEnumerable<T> items)
        {
            var array = items.ToArray();
            if (array.Length > 0)
            {
                foreach (var item in array)
                    InnerList.Remove(item);
                RaiseItemsRemoved(array);
            }
        }
        public bool Contains(T item)
        {
            return List.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            InnerList.CopyTo(array, arrayIndex);
        }
        public int IndexOf(T item)
        {
            return List.IndexOf(item);
        }
        public int IndexOf(T item, int startIndex)
        {
            return InnerList.IndexOf(item, startIndex);
        }
        public int IndexOf(T item, int startIndex, int count)
        {
            return InnerList.IndexOf(item, startIndex, count);
        }
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        public T[] GetRange(int index, int count)
        {
            return InnerList.GetRange(index, count).Cast<T>().ToArray();
        }
        public T[] ToArray()
        {
            return (T[])InnerList.ToArray(typeof(T));
        }
        public static bool Equal(T[] c1, T[] c2)
        {
            var minTotal = Math.Min(c1.Length, c2.Length);
            for (var i = 0; i < minTotal; i++)
                if (!c1[i].Equals(c2[i]))
                    return false;
            return true;
        }

        public override string ToString()
        {
            return "Count = " + Count;
        }

        private EventHandler initHandler;
        /// <summary> Начать инициализацию. Отключает событие изменение коллекции.</summary>
        public void BeginInit()
        {
            initHandler = CollectionChanged;
            if (CollectionChanged != null)
                CollectionChanged -= CollectionChanged;
        }
        /// <summary> Закончить инициализацию. Включает событие изменения коллекции</summary>
        /// <param name="forceUpdate"> Вызвать событие изменения коллекции, после завершения инициализации.</param>
        public void EndInit(bool forceUpdate = false)
        {
            CollectionChanged += initHandler;
            if (forceUpdate)
                RaiseCollectionChanged();
        }

        #region Enumerator

        public new IEnumerator<T> GetEnumerator()
        {
            return new CollectionEnumerator(List.GetEnumerator());
        }
        private struct CollectionEnumerator : IEnumerator<T>
        {
            public CollectionEnumerator(IEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }
            private readonly IEnumerator enumerator;

            public T Current
            {
                get
                {
                    return (T)enumerator.Current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return enumerator.Current;
                }
            }
            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }
            public void Reset()
            {
                enumerator.Reset();
            }
            public void Dispose()
            {

            }
        }

        #endregion
        #region events

        protected sealed override void OnInsert(int index, object value)
        {
            var e = new CancelEventArgs(false);
            OnItemsAdding(index, (T)value, e);
            if (!e.Cancel)
                base.OnInsert(index, value);
        }
        protected sealed override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            RaiseItemsAdded(new[] { (T)value });
        }
        protected sealed override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            RaiseItemsRemoved(new[] { (T)value });
        }
        protected sealed override void OnClear()
        {
            if (BeforeClear != null)
                BeforeClear(null, EventArgs.Empty);

            itemsToClear = ToArray();
            base.OnClear();
        }
        protected sealed override void OnClearComplete()
        {
            if (itemsToClear != null && itemsToClear.Length > 0)
                RaiseItemsRemoved(itemsToClear);
            itemsToClear = null;
            base.OnClearComplete();
        }
        private T[] itemsToClear;

        public event EventHandler BeforeClear;
        /// <summary> Содержимое коллекции или один из элементов коллекции были изменены </summary>
        public event EventHandler Changed;
        /// <summary> Содержимое коллекции было изменено </summary>
        public event EventHandler CollectionChanged;
        /// <summary> В коллекцию были добавленны элементы </summary>
        public event ItemsChangedEventHandler ItemsAdded;
        /// <summary> Элементы коллекции были удалены </summary>
        public event ItemsChangedEventHandler ItemsRemoved;

        private bool isChanged;
        private bool isCollectionChanged;

        private void RaiseChanged()
        {
            //try
            //{
            OnChanged();
            isChanged = false;
            if (Changed != null)
            {
                if (IsLockUpdating)
                    isChanged = true;
                else
                    Changed(this, EventArgs.Empty);
            }
            //}
            //catch
            //{
            //}
        }
        private void RaiseCollectionChanged()
        {
            //try
            //{
            OnCollectionChanged();
            isCollectionChanged = false;
            if (CollectionChanged != null)
            {
                if (IsLockUpdating)
                    isCollectionChanged = true;
                else
                    CollectionChanged(this, EventArgs.Empty);
            }
            RaiseChanged();
            //}
            //catch
            //{
            //}
        }
        private void RaiseItemsAdded(T[] items)
        {
            OnItemsAdded(items);
            if (ItemsAdded != null)
            {
                if (IsLockUpdating)
                {
                    EnqueueEvent(ItemsAdded, new object[] { items });
                }
                else
                {
                    if (ItemsAdded != null)
                        ItemsAdded(items);
                }
            }
            RaiseCollectionChanged();
        }
        private void RaiseItemsRemoved(T[] items)
        {
            OnItemsRemoved(items);
            if (ItemsRemoved != null)
            {
                if (IsLockUpdating)
                    EnqueueEvent(ItemsRemoved, new object[] { items });
                else
                {
                    if (ItemsRemoved != null)
                        ItemsRemoved(items);
                }
            }
            RaiseCollectionChanged();
        }

        protected virtual void OnChanged()
        {
        }
        protected virtual void OnCollectionChanged()
        {
        }
        protected virtual void OnItemsAdded(T[] items)
        {
        }
        protected virtual void OnItemsRemoved(T[] items)
        {
        }
        protected virtual void OnItemsAdding(int index, T item, CancelEventArgs e)
        {
        }

        private void EnqueueEvent(Delegate handler, IEnumerable<object> args)
        {
            if (handler == null || args == null)
                return;
            EventInfo previousEvent;
            if (events.Count > 0 && (previousEvent = events.Peek()).Handler.Method == handler.Method)
            {
                var oldArgs = (T[])previousEvent.Args[0];
                var newArgs = (T[])args.First();
                T[] newArray;

                var newSize = previousEvent.ArraySize + newArgs.Length;
                if (newSize > oldArgs.Length)
                {
                    var num = oldArgs.Length == 0 ? 4 : oldArgs.Length * 2;
                    if (num < newSize)
                        num = newSize;
                    newArray = new T[num];
                    Array.Copy(oldArgs, 0, newArray, 0, oldArgs.Length);
                }
                else
                    newArray = oldArgs;

                Array.Copy(newArgs, 0, newArray, previousEvent.ArraySize, newArgs.Length);
                previousEvent.ArraySize = newSize;
                previousEvent.Args[0] = newArray;

                //var l = oldArgs.Length;
                //Array.Resize(ref oldArgs, oldArgs.Length + newArgs.Length);
                //for (int i = 0; i < newArgs.Length; i++)
                //    oldArgs[l + i] = newArgs[i];
                //previousEvent.Args[0] = oldArgs;
            }
            else
                events.Enqueue(new EventInfo(handler, args));
        }
        private readonly Queue<EventInfo> events = new Queue<EventInfo>();

        private int updateDepth;
        private bool IsLockUpdating
        {
            get
            {
                return updateDepth > 0;
            }
        }
        public void BeginUpdate()
        {
            updateDepth++;
        }
        public void EndUpdate()
        {
            updateDepth = updateDepth > 0 ? updateDepth - 1 : 0;
            if (updateDepth == 0)
                RaiseDelayedEvents();
        }
        private void RaiseDelayedEvents()
        {
            while (events.Count > 0)
                events.Dequeue().Invoke();

            if (isCollectionChanged && CollectionChanged != null)
            {
                CollectionChanged(this, EventArgs.Empty);
                isCollectionChanged = false;
            }
            if (isChanged && Changed != null)
            {
                Changed(this, EventArgs.Empty);
                isChanged = false;
            }
        }

        #endregion
        #region event structures

        public delegate void ItemsChangedEventHandler(T[] items);

        private class EventInfo
        {
            public EventInfo(Delegate handler, IEnumerable<object> args)
            {
                Handler = handler;
                Args = new List<object>(args);

                if (handler is ItemsChangedEventHandler)
                    ArraySize = ((T[])Args[0]).Length;
            }

            /// <summary> Делегат </summary>
            public Delegate Handler
            {
                get;
                private set;
            }

            /// <summary> Аргуметы </summary>
            public List<object> Args
            {
                get;
                private set;
            }

            internal int ArraySize
            {
                get;
                set;
            }

            public void Invoke()
            {
                //try
                //{
                if (Handler is ItemsChangedEventHandler)
                {
                    var arr = (T[])Args[0];
                    Array.Resize(ref arr, ArraySize);
                    Args[0] = arr;
                }
                if (Handler.Target != null)
                    Handler.DynamicInvoke(Args.ToArray());
                //}
                //catch
                //{ }
            }
        }

        #endregion


    }
    public interface IChangeable
    {
        event EventHandler Changed;
    }
}
