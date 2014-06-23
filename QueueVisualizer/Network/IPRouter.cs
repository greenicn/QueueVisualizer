using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
    /// An IP router. Forward packets purely based on FIB that can manually added.
    /// </summary>
    public class IPRouter : ANode
    {
        private Dictionary<string, ANode> FIB = new Dictionary<string, ANode>();

        public IPRouter(string name) : base(name) { }

        public void AddFIB(string name, ANode nextHop)
        {
            FIB.Add(name, nextHop);
        }

        public override void HandlePacket(ANode from, ISerializable packet)
        {
            IPPacket pkt = packet as IPPacket;
            Trace.Assert(pkt != null);
            ANode nextHop;
            bool known = FIB.TryGetValue(pkt.DST, out nextHop);
            Trace.Assert(known);
            SendPacket(nextHop, pkt, false);
        }
    }
}
