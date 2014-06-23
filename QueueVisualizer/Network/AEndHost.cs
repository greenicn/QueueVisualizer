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
    /// Abstraction of an endhost.
    /// </summary>
    public abstract class AEndHost : ANode
    {

        public ANode FirstHop { private set; get; }

        public bool Busy { private set; get; }

        public AEndHost(string name, ANode firstHop, Func<int, Tuple<IQueue<ISerializable>, IQueue<ISerializable>>> queueGenerator, int capacity, int bandwidth, long delay)
            : base(name)
        {
            FirstHop = firstHop;
            LinkNodes(this, firstHop, queueGenerator, capacity, bandwidth, delay);
        }

        public void SendPacket(ISerializable packet)
        {
            base.SendPacket(FirstHop, packet, false);
        }

        //public override sealed void HandlePacket(ANode from, ISerializable packet)
        //{
        //    //IncomingQueue.AddData(packet, packet is IControlMessage);
        //    if (!Busy)
        //    {
        //        InnerHandleData();
        //    }
        //}

        //private void InnerHandleData(List<ISerializable> packetsToSend = null)
        //{
        //    if (IncomingQueue.Size == 0)
        //    {
        //        Busy = false;
        //        return;
        //    }
        //    ISerializable packet = IncomingQueue.GetData();
        //    if (packetsToSend == null) packetsToSend = new List<ISerializable>();
        //    long delay = ProcessPacket(packet, packetsToSend);

        //    EventQueue.AddEvent(EventQueue.Now + delay, EndProcessData, packetsToSend);
        //}

        //private void EndProcessData(params object[] prams)
        //{
        //    List<ISerializable> packetsToSend = prams[0] as List<ISerializable>;
        //    foreach (ISerializable packet in packetsToSend)
        //        SendPacket(packet);
        //    packetsToSend.Clear();
        //    InnerHandleData(packetsToSend);
        //}

        //public abstract long ProcessPacket(ISerializable packet, List<ISerializable> packetsToSend);
    }
}
