using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Abstraction of a network node.
    /// </summary>
    public abstract class ANode
    {
        public const int b = 1;
        public const int Kb = b * 1024;
        public const int Mb = Kb * 1024;
        public const int B = 8;
        public const int KB = B * 1024;
        public const int MB = KB * 1024;
        public const long MIUS = 1;
        public const long MS = 1000 * MIUS;
        public const long S = 1000 * MS;

        public static void LinkNodes(ANode n1, ANode n2, Func<int, Tuple<IQueue<ISerializable>, IQueue<ISerializable>>> queueGenerator, int queueCapacity, int bandwidth, long delay)
        {
            Tuple<IQueue<ISerializable>, IQueue<ISerializable>> tuple = queueGenerator(queueCapacity);
            n1.AddNode(n2, tuple.Item1, bandwidth, delay);
            n2.AddNode(n1, tuple.Item2, bandwidth, delay);
        }

        public static void DitachNodes(ANode n1, ANode n2)
        {
            Trace.Assert(n1._Links.ContainsKey(n2));
            n1._Links.Remove(n2);
            n2._Links.Remove(n1);
        }

        public string Name { private set; get; }
        protected Dictionary<ANode, Link> _Links = new Dictionary<ANode, Link>();

        public IEnumerable<Link> Links { get { foreach (Link l in _Links.Values) yield return l; } }

        public ANode(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        private void AddNode(ANode another, IQueue<ISerializable> queue, int bandwidth, long delay)
        {
            _Links.Add(another, new Link(this, another, queue, bandwidth, delay));
        }

        public abstract void HandlePacket(ANode from, ISerializable packet);

        public void SendPacket(ANode target, ISerializable packet, bool urgent)
        {
            Link link = null;
            if (!_Links.TryGetValue(target, out link))
                Console.WriteLine("[{0}]\t>D\t{1}\t{2}\t{3}", EventQueue.Now, this, null, packet);
            else
                link.SendPacket(packet, urgent);
        }

        public bool IsLinkedTo(ANode another)
        {
            return _Links.ContainsKey(another);
        }

        public class Link
        {
            public ANode From { private set; get; }
            public ANode To { private set; get; }

            private Dictionary<ISerializable, long> _PacketsOnWIre = new Dictionary<ISerializable, long>();

            private IQueue<ISerializable> Queue;
            public int Bandwidth { private set; get; }
            public long Delay { private set; get; }
            private bool Busy = false;

            public IEnumerable<ISerializable> PacketsInQueue { get { return Queue.Content; } }
            public IEnumerable<KeyValuePair<ISerializable, long>> PacketsOnWire { get { foreach (KeyValuePair<ISerializable, long> p in _PacketsOnWIre) yield return p; } }

            /// <summary>
            /// Create a link.
            /// </summary>
            /// <param name="from">Source Node</param>
            /// <param name="to">Destination Node</param>
            /// <param name="queue">Queue on the outgoing face</param>
            /// <param name="bandwidth">Bandwidth in bit on the link</param>
            /// <param name="delay">Delay in micro-second on the link</param>
            internal Link(ANode from, ANode to, IQueue<ISerializable> queue, int bandwidth, long delay)
            {
                From = from;
                To = to;
                queue.Name = string.Format("{0}->{1}", from, to);
                Queue = queue;
                queue.ItemDropped += (q, p) => { Console.WriteLine("QUEUEDROP {0} {2} {1}", q.Name, p, EventQueue.Now); };
                Bandwidth = bandwidth;
                Delay = delay;
            }

            internal void SendPacket(ISerializable packet, bool urgent)
            {
                Queue.AddData(packet, urgent);
                if (!Busy)
                {
                    Busy = true;
                    EventQueue.AddEvent(EventQueue.Now, SendPacket);
                }
            }

            /// <summary>
            /// Get send delay in microsecond for object
            /// </summary>
            /// <param name="obj">specified object</param>
            /// <returns>send delay in microsecond</returns>
            private long GetSendDelay(ISerializable obj)
            {
                return obj.Length * 1000000L / Bandwidth;
            }

            private void SendPacket(params object[] objs)
            {
                if (Queue.Size == 0)
                {
                    Busy = false;
                    return;
                }
                ISerializable obj = Queue.GetData();
                long sendDelay = GetSendDelay(obj);
                _PacketsOnWIre.Add(obj, EventQueue.Now);
                EventQueue.AddEvent(EventQueue.Now + sendDelay, SendPacket);
                EventQueue.AddEvent(EventQueue.Now + sendDelay + Delay, ReceivePacket, obj);
            }

            private void ReceivePacket(params object[] objs)
            {
                ISerializable obj = (ISerializable)objs[0];
                _PacketsOnWIre.Remove(obj);
                To.HandlePacket(From, obj);
            }
        }
    }
}

