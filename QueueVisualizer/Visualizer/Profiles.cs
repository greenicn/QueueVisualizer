using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

using Network;

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
    /// Profile of a network node.
    /// </summary>
    public class NodeProfile
    {
        public ANode Node { private set; get; }
        public Point Center { private set; get; }
        public Color Fill { private set; get; }

        public NodeProfile(ANode node, Point center, Color fill)
        {
            Node = node;
            Center = center;
            Fill = fill;
        }

    }

}
