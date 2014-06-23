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
    /// IP endhost
    /// </summary>
    public class IPEndHost : AEndHost
    {
        private Dictionary<int, Action<ANode, IPPacket>> listeners = new Dictionary<int, Action<ANode, IPPacket>>();

        public IPEndHost(string name, ANode firstHop, Func<int, Tuple<IQueue<ISerializable>, IQueue<ISerializable>>> queueGenerator, int capacity, int bandwidth, long delay)
            : base(name, firstHop, queueGenerator, capacity, bandwidth, delay)
        { 
        }

        public void ListenOn(int port, Action<ANode, IPPacket> listener)
        {
            listeners.Add(port, listener);
        }

        public ConstantWindowACKer ACK(int port)
        {
            ConstantWindowACKer acker = new ConstantWindowACKer(SendPacket);
            ListenOn(port, acker.HandlePacket);
            return acker;
        }

        public ConstantWindowSender Send(int port, string dst, int dstPort, int window)
        {
            ConstantWindowSender sender = new ConstantWindowSender(SendPacket, window, Name, port, dst, dstPort);
            ListenOn(port, sender.HandlePacket);
            return sender;
        }

        public TCPSender SendTCP(int port, string dst, int dstPort)
        {
            TCPSender sender = new TCPSender(SendPacket, Name, port, dst, dstPort);
            ListenOn(port, sender.HandlePacket);
            return sender;
        }

        public TCPACKer ACKTCP(int port)
        {
            TCPACKer acker = new TCPACKer(SendPacket);
            ListenOn(port, acker.HandlePacket);
            return acker;
        }


        public override void HandlePacket(ANode from, ISerializable packet)
        {
            IPPacket pkt = packet as IPPacket;
            Trace.Assert(pkt != null);
            Action<ANode, IPPacket> listener;
            if (listeners.TryGetValue(pkt.DSTPORT, out listener))
                listener(from, pkt);
        }
    }

    public class TCPSender
    {
        public Action<ISerializable> SendPacket;
        private string Src, Dst;
        private int SrcPort, DstPort;

        private List<int> OutstandingPackets = new List<int>();
        private double Window = 1;
        private int DuplicateACK = 0;
        private int RightEdge = 0;

        Action<ANode, int> HandlePacketAction, CongestionAvoidAction, FastRecoveryAction;


        public TCPSender(Action<ISerializable> sendPacket, string src, int srcPort, string dst, int dstPort)
        {
            SendPacket = sendPacket;
            Src = src;
            Dst = dst;
            SrcPort = srcPort;
            DstPort = dstPort;

            CongestionAvoidAction = CongestionAvoid;
            FastRecoveryAction = FastRecovery;

            HandlePacketAction = CongestionAvoidAction;
        }

        public void Start()
        {
            FillWindow();
        }

        public void HandlePacket(ANode from, IPPacket packet)
        {
            Payload pld = packet.Payload as Payload;
            Console.WriteLine("TCPGETACK {0}:{1}->{2}:{3} {4} {5}", Src, SrcPort, Dst, DstPort, Common.EventQueue.Now, pld.SegID);
            HandlePacketAction(from, pld.SegID);
        }

        private double sthresh;
        private int safeCount = 0;

        private void FastRecovery(ANode from, int ackSegment)
        {
            int removeCount = OutstandingPackets.RemoveAll(i => i <= ackSegment);
            if (removeCount > 0)
            {
                HandlePacketAction = CongestionAvoidAction;
                Window = sthresh;
                FillWindow();
                DuplicateACK = 0;
                safeCount = (int)Window;
            }
            else
            {
                Window++;
                Console.WriteLine("TCPFASTRECOVERY {0}:{1}->{2}:{3} {4} {5} {6}", Src, SrcPort, Dst, DstPort, Common.EventQueue.Now, Window, OutstandingPackets.Count);
                FillWindow();
            }

        }

        private void CongestionAvoid(ANode from, int ackSegment)
        {
            int removeCount = OutstandingPackets.RemoveAll(i => i <= ackSegment);
            safeCount -= removeCount;
            if (removeCount == 0)
            {
                DuplicateACK++;
                if (DuplicateACK == 3)
                {
                    if (safeCount <= 0)
                        sthresh = Window / 2;
                    Window = sthresh + 3;
                    SendPacket(new IPPacket(Src, SrcPort, Dst, DstPort, new Payload(ackSegment + 1, 11872)));
                    Console.WriteLine("TCPRESEND {0}:{1}->{2}:{3} {4} {5}", Src, SrcPort, Dst, DstPort, Common.EventQueue.Now, ackSegment + 1);
                    HandlePacketAction = FastRecoveryAction;
                }
                else
                {
                    OutstandingPackets.Add(RightEdge);
                    Console.WriteLine("TCPSEND {0}:{1}->{2}:{3} {4} {5}", Src, SrcPort, Dst, DstPort, Common.EventQueue.Now, RightEdge);
                    SendPacket(new IPPacket(Src, SrcPort, Dst, DstPort, new Payload(RightEdge++, 11872)));
                }
            }
            else
            {
                DuplicateACK = 0;
                for (int i = 0; i < removeCount; i++)
                    Window += 1 / Window;
            }
            Console.WriteLine("TCPWINDOW {0}:{1}->{2}:{3} {4} {5}", Src, SrcPort, Dst, DstPort, Common.EventQueue.Now, Window);
            FillWindow();
        }



        private void FillWindow()
        {
            while (OutstandingPackets.Count < Window)
            {
                OutstandingPackets.Add(RightEdge);
                Console.WriteLine("TCPSEND {0}:{1}->{2}:{3} {4} {5}", Src, SrcPort, Dst, DstPort, Common.EventQueue.Now, RightEdge);
                SendPacket(new IPPacket(Src, SrcPort, Dst, DstPort, new Payload(RightEdge++, 11872)));
            }
        }


    }

    public class TCPACKer
    {
        private Action<ISerializable> SendPacket;

        private int ReceiveLeftEdge = 0;
        private List<int> OutstandingReceives = new List<int>();

        public TCPACKer(Action<ISerializable> sendPacket)
        {
            SendPacket = sendPacket;
        }

        public void HandlePacket(ANode from, IPPacket packet)
        {
            Payload pld = packet.Payload as Payload;
            Trace.Assert(pld != null);
            OutstandingReceives.Add(pld.SegID);
            while (OutstandingReceives.Remove(ReceiveLeftEdge))
            {
                ReceiveLeftEdge++;
            }
            OutstandingReceives.RemoveAll(i => i <= ReceiveLeftEdge);
            Console.WriteLine("TCPSENDACK {0}:{1}->{2}:{3} {4} {5}", packet.SRC, packet.SRCPORT, packet.DST, packet.DSTPORT, Common.EventQueue.Now, ReceiveLeftEdge - 1);
            SendPacket(new IPPacket(packet.DST, packet.DSTPORT, packet.SRC, packet.SRCPORT, new Payload(ReceiveLeftEdge - 1, 500)));
        }

    }

    public class ConstantWindowSender
    {
        public Action<ISerializable> SendPacket;
        public int Window { private set; get; }
        private int RightEdge = 0;
        private int OutstandingCount = 0;
        private string Src, Dst;
        private int SrcPort, DstPort;
        public ConstantWindowSender(Action<ISerializable> sendPacket, int window, string src, int srcPort, string dst, int dstPort)
        {
            SendPacket = sendPacket;
            Window = window;
            Src = src;
            Dst = dst;
            SrcPort = srcPort;
            DstPort = dstPort;
        }

        public void HandlePacket(ANode from, IPPacket packet)
        {
            OutstandingCount--;
            FillWindow();
        }

        public void Start()
        {
            FillWindow();
        }

        private void FillWindow()
        {
            while (OutstandingCount < Window)
            {
                SendPacket(new IPPacket(Src, SrcPort, Dst, DstPort, new Payload(RightEdge++, 11872)));
                OutstandingCount++;
            }
        }

    }

    public class ConstantWindowACKer
    {
        private Action<ISerializable> SendPacket;
        public ConstantWindowACKer(Action<ISerializable> sendPacket)
        {
            SendPacket = sendPacket;
        }

        public void HandlePacket(ANode from, IPPacket packet)
        {
            Payload pld = packet.Payload as Payload;
            Trace.Assert(pld != null);
            SendPacket(new IPPacket(packet.DST, packet.DSTPORT, packet.SRC, packet.SRCPORT, new Payload(pld.SegID, 500)));
        }
    }
}
