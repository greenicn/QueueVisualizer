using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;

namespace Network
{
    /// <summary>
    /// Author: Jiachen Chen, University of Goettingen
    /// Acknowledgment:
    /// This document has been supported by the GreenICN project 
    /// (GreenICN: Architecture and Applications of Green Information Centric Networking ), 
    /// a research project supported jointly by the European Commission under its 
    /// 7th Framework Program (contract no. 608518) 
    /// and the National Institute of Information and Communications Technology (NICT) 
    /// in Japan (contract no. 167). The views and conclusions contained herein are 
    /// those of the authors and should not be interpreted as necessarily 
    /// representing the official policies or endorsements, either expressed 
    /// or implied, of the GreenICN project, the European Commission, or NICT.
    /// 
    /// Abstraction of a buffer on the router.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class IQueue<T>
    {
        public event Action<IQueue<T>, T> ItemDropped;

        public string Name { set; get; }
        public int Capacity { protected set; get; }
        public abstract int Size { get; }
        public abstract IEnumerable<T> Content { get; }
        public abstract void AddData(T item, bool urgent);
        public abstract T GetData();

        protected void FireItemDrop(T t) { if (ItemDropped != null) ItemDropped(this, t); }
    }

    public class FIFOQueue<T> : IQueue<T>
    {
        public static Func<int, Tuple<IQueue<T>, IQueue<T>>> QueueGenerator = (capacity) => new Tuple<IQueue<T>, IQueue<T>>(new FIFOQueue<T>(capacity), new FIFOQueue<T>(capacity));

        private LinkedList<T> innerQueue = new LinkedList<T>();

        public override IEnumerable<T> Content { get { return innerQueue.AsEnumerable(); } }

        public FIFOQueue(int capacity) { Capacity = capacity; }

        public override void AddData(T item, bool urgent)
        {
            if (urgent)
                innerQueue.AddFirst(item);
            else
                innerQueue.AddLast(item);
            DropTail();
        }

        private void DropTail()
        {
            while (innerQueue.Count > Capacity)
            {
                var last = innerQueue.Last;
                innerQueue.Remove(last);
                FireItemDrop(last.Value);
            }
        }

        public override T GetData()
        {
            var first = innerQueue.First;
            innerQueue.Remove(first);
            return first.Value;
        }

        public override int Size { get { return innerQueue.Count; } }
    }
}
