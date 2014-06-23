using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Common;
using Network;
using System.Windows.Controls;
using System.Windows.Media;

namespace Visualizer
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
    /// Reads a profile and creates simulation topology, events.
    /// </summary>
    public static class ProfileReader
    {
        public static void ReadProfile(string profile,
            Dictionary<string, NodeProfile> nodes,
            Action<double> setSpeedAction)
        {
            using (StreamReader reader = new StreamReader(profile))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("#")) continue;
                    if (line.Equals("")) break;
                    CreateRouter(line, nodes);
                }
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("#")) continue;
                    if (line.Equals("")) break;
                    LinkNodes(line, nodes);
                }
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("#")) continue;
                    if (line.Equals("")) break;
                    CreateEndHost(line, nodes);
                }
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("#")) continue;
                    if (line.Equals("")) break;
                    AddFIB(line, nodes);
                }
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("#")) continue;
                    if (line.Equals("")) break;
                    CreateEvent(line, nodes, setSpeedAction);
                }
            }
        }

        private static void CreateEvent(string line,
            Dictionary<string, NodeProfile> nodes,
            Action<double> setSpeedAction)
        {
            string[] parts = line.Split(' ');
            switch (parts[1])
            {
                case "SEND":
                    CreateSendEvent(parts, nodes);
                    break;
                case "SENDTCP":
                    CreateSendTCPEvent(parts, nodes);
                    break;
                case "SETSPEED":
                    CreateSetSpeedEvent(parts, setSpeedAction);
                    break;
            }
        }

        private static void CreateSendTCPEvent(string[] parts,
            Dictionary<string, NodeProfile> nodes)
        {
            long time = Convert.ToInt64(parts[0]);
            string nsrc = parts[2];
            int srcPort = Convert.ToInt32(parts[3]);
            string ndst = parts[4];
            int dstPort = Convert.ToInt32(parts[5]);

            IPEndHost src = nodes[nsrc].Node as IPEndHost, dst = nodes[ndst].Node as IPEndHost;

            dst.ACKTCP(dstPort);
            TCPSender sender = src.SendTCP(srcPort, ndst, dstPort);
            EventQueue.AddEvent(time, (objs) => { sender.Start(); });
        }

        private static void CreateSetSpeedEvent(string[] parts, Action<double> setSpeedAction)
        {
            long time = Convert.ToInt64(parts[0]);
            double speed = Convert.ToDouble(parts[2]);
            EventQueue.AddEvent(time, (objs) => setSpeedAction((double)objs[0]), speed);
        }

        private static void CreateSendEvent(string[] parts,
             Dictionary<string, NodeProfile> nodes)
        {
            long time = Convert.ToInt64(parts[0]);
            string nsrc = parts[2];
            int srcPort = Convert.ToInt32(parts[3]);
            string ndst = parts[4];
            int dstPort = Convert.ToInt32(parts[5]);
            int windowSize = Convert.ToInt32(parts[6]);

            IPEndHost src = nodes[nsrc].Node as IPEndHost, dst = nodes[ndst].Node as IPEndHost;
            dst.ACK(dstPort);
            ConstantWindowSender sender = src.Send(srcPort, ndst, dstPort, windowSize);
            EventQueue.AddEvent(time, (objs) => { sender.Start(); });


        }

        private static void CreateRouter(string line,
            Dictionary<string, NodeProfile> nodes)
        {
            string[] parts = line.Split(' ');
            string name = parts[0];
            double x = Convert.ToDouble(parts[1]);
            double y = Convert.ToDouble(parts[2]);
            byte r = Convert.ToByte(parts[3]);
            byte g = Convert.ToByte(parts[4]);
            byte b = Convert.ToByte(parts[5]);

            IPRouter router = new IPRouter(name);
            nodes.Add(name, new NodeProfile(router, new System.Windows.Point(x, y), Color.FromRgb(r, g, b)));

        }

        private static void LinkNodes(string line,
            Dictionary<string, NodeProfile> nodes)
        {
            string[] parts = line.Split(' ');
            string nr1 = parts[0];
            string nr2 = parts[1];
            int b = Convert.ToInt32(parts[2]);
            long d = Convert.ToInt64(parts[3]);
            int c = Convert.ToInt32(parts[4]);
            ANode r1 = nodes[nr1].Node;
            ANode r2 = nodes[nr2].Node;
            ANode.LinkNodes(r1, r2, FIFOQueue<ISerializable>.QueueGenerator, c, b, d);
        }

        private static void CreateEndHost(string line,
            Dictionary<string, NodeProfile> nodes)
        {
            string[] parts = line.Split(' ');
            string name = parts[0];
            string nfirstHop = parts[1];
            int bandwidth = Convert.ToInt32(parts[2]);
            long delay = Convert.ToInt64(parts[3]);
            int capacity = Convert.ToInt32(parts[4]);
            double x = Convert.ToDouble(parts[5]);
            double y = Convert.ToDouble(parts[6]);
            byte r = Convert.ToByte(parts[7]);
            byte g = Convert.ToByte(parts[8]);
            byte b = Convert.ToByte(parts[9]);

            NodeProfile firstHop = nodes[nfirstHop];

            IPEndHost eh = new IPEndHost(name, firstHop.Node, FIFOQueue<ISerializable>.QueueGenerator, capacity, bandwidth, delay);
            nodes.Add(name, new NodeProfile(eh, new System.Windows.Point(x, y), Color.FromRgb(r, g, b)));
        }

        private static void AddFIB(string line, Dictionary<string, NodeProfile> nodes)
        {
            string[] parts = line.Split(' ');
            string nr = parts[0];
            string dst = parts[1];
            string nnext = parts[2];

            ANode r = nodes[nr].Node, next = nodes[nnext].Node;
            (r as IPRouter).AddFIB(dst, next);
        }
    }
}
