using System;
using System.Collections.Generic;
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
    /// Abstraction of a data stream on wire.
    /// </summary>
    public abstract class ISerializable
    {
        public abstract ISerializable Clone();
        public int Length { protected set; get; }
    }

    public class Payload : ISerializable
    {
        public int SegID { private set; get; }
        public int PayloadSize { private set; get; }
        public Payload(int segId, int payloadSize)
        {
            SegID = segId;
            PayloadSize = payloadSize;
            Length = 32 + payloadSize;
        }

        public override ISerializable Clone()
        {
            return new Payload(SegID, PayloadSize);
        }


        public override string ToString()
        {
            return string.Format("[P Seg={0} Size={1}]", SegID, PayloadSize);
        }
    }

}
