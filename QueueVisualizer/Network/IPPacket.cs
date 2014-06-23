using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network
{
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
    /// A simple IP packet.
    public class IPPacket : ISerializable
    {
        public string SRC { private set; get; }
        public int SRCPORT { private set; get; }
        public string DST { private set; get; }
        public int DSTPORT { private set; get; }
        public ISerializable Payload { private set; get; }

        public IPPacket(string src, int srcPort, string dst, int dstPort, ISerializable payload)
        {
            SRC = src;
            SRCPORT = srcPort;
            DST = dst;
            DSTPORT = dstPort;
            Payload = payload;
            Length = Payload.Length + 12 * 8;
        }
        public override ISerializable Clone()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("IP{{SRC={0}:{1},DST={2}:{3},PLD={4}}}", SRC, SRCPORT, DST, DSTPORT, Payload);
        }
    }
}
