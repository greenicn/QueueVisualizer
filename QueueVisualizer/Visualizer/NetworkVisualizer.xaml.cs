using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Common;
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
    /// Interaction logic for NetworkVisualizer.xaml
    /// </summary>
    public partial class NetworkVisualizer : UserControl
    {
        public static Func<ISerializable, Tuple<Brush, Pen>> DEFAULT_PACKET_DRAWER = (pkt) =>
        {
            return new Tuple<Brush, Pen>(Brushes.White, PEN_EMPTY);
        };

        public Func<ISerializable, Tuple<Brush, Pen>> PacketDrawer { set; get; }

        public Dictionary<string, NodeProfile> Nodes { set; get; }

        public double Ratio { get { return Math.Min(ActualWidth, ActualHeight); } }

        public NetworkVisualizer()
        {
            InitializeComponent();
            TYPE_FACE = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
            PacketDrawer = DEFAULT_PACKET_DRAWER;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            foreach (NodeProfile node in Nodes.Values)
            {
                DrawNode(drawingContext, node);
                foreach (ANode.Link link in node.Node.Links)
                    DrawLink(drawingContext, link);
            }
            foreach (NodeProfile node in Nodes.Values)
            {
                foreach (ANode.Link link in node.Node.Links)
                    DrawOnWirePacket(drawingContext, link);
            }
            foreach (NodeProfile node in Nodes.Values)
            {
                foreach (ANode.Link link in node.Node.Links)
                    DrawStackPacket(drawingContext, link);
            }
        }

        #region DrawNode
        public const double NODE_SIZE = 60 / 800.0;
        public const double BLUR_SIZE = 2 / 800.0;
        public static readonly Pen PEN_EMPTY = new Pen(Brushes.Transparent, 0);
        public static readonly BlurBitmapEffect BLUR = new BlurBitmapEffect { KernelType = KernelType.Gaussian };
        public readonly Typeface TYPE_FACE;

        public void DrawNode(DrawingContext context, NodeProfile node)
        {
            BLUR.Radius = BLUR_SIZE * Ratio;
            double size = NODE_SIZE / 2 * Ratio;
            context.PushEffect(BLUR, null);
            Point center = Helper.Zoom(node.Center, ActualWidth, ActualHeight);
            Color fill = node.Fill;
            context.DrawEllipse(
                new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop{Color=Helper.ModifyBright(fill, 0.9f), Offset=0.0},
                        new GradientStop{Color=Helper.ModifyBright(fill, 0.7f), Offset=0.2},
                        new GradientStop{Color=Helper.ModifyBright(fill, 0.45f), Offset=0.6},
                        new GradientStop{Color=Helper.ModifyBright(fill, 0.35f), Offset=1},
                    },
                    RelativeTransform = new ScaleTransform { ScaleX = 0.9, ScaleY = 0.8 },
                },
                PEN_EMPTY,
                center,
                size,
                size);
            context.Pop();

            FormattedText ft = new FormattedText(node.Node.Name, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                            new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch), size * .6, Brushes.Black);

            context.DrawText(ft, new Point(center.X - ft.Width / 2, center.Y - ft.Height / 2));
        }
        #endregion


        #region DrawLink

        public const double LINK_BANDWIDTH_RATIO = 1 / 20.0 / ANode.Mb;
        public static readonly Brush PEN_LINK = Brushes.LightGray;

        public void DrawLink(DrawingContext context, ANode.Link link)
        {
            NodeProfile fromNode = Nodes[link.From.Name];
            NodeProfile toNode = Nodes[link.To.Name];

            double diffX, diffY, angle, width, height, rpx, rpy;

            diffX = (toNode.Center.X - fromNode.Center.X) * ActualWidth;
            diffY = (toNode.Center.Y - fromNode.Center.Y) * ActualHeight;

            angle = Math.Atan2(diffY, diffX);
            width = Math.Sqrt(diffY * diffY + diffX * diffX) - NODE_SIZE * Ratio;
            height = link.Bandwidth * LINK_BANDWIDTH_RATIO;

            rpx = fromNode.Center.X * ActualWidth + NODE_SIZE * Ratio / 2 * Math.Cos(angle) + height / 2 * Math.Sin(angle);
            rpy = fromNode.Center.Y * ActualHeight + NODE_SIZE * Ratio / 2 * Math.Sin(angle) - height / 2 * Math.Cos(angle);

            context.PushTransform(new TranslateTransform(rpx, rpy));

            context.PushTransform(new RotateTransform(angle / Math.PI * 180));
            context.DrawRectangle(
                PEN_LINK,
                PEN_EMPTY,
                new Rect(0, -height / 2, width, height)
                );
            context.Pop();
            context.Pop();
        }
        #endregion

        #region DrawPacket
        public const double STACK_WIDTH = 20 / 800.0;
        public const double STACK_PACKET_HEIGHT = 10 / 800.0;
        public const double PACKET_BANDWIDTH_RATIO = 1 / 1.0 / ANode.Mb;

        public void DrawOnWirePacket(DrawingContext context, ANode.Link link)
        {
            NodeProfile fromNode = Nodes[link.From.Name];
            NodeProfile toNode = Nodes[link.To.Name];

            double diffX, diffY, angle, width, height, rpx, rpy;
            double now = EventQueue.Now;


            diffX = (toNode.Center.X - fromNode.Center.X) * ActualWidth;
            diffY = (toNode.Center.Y - fromNode.Center.Y) * ActualHeight;

            angle = Math.Atan2(diffY, diffX);
            width = Math.Sqrt(diffY * diffY + diffX * diffX) - NODE_SIZE * Ratio;
            height = link.Bandwidth * PACKET_BANDWIDTH_RATIO;

            rpx = fromNode.Center.X * ActualWidth + NODE_SIZE * Ratio / 2 * Math.Cos(angle) + height / 2 * Math.Sin(angle);
            rpy = fromNode.Center.Y * ActualHeight + NODE_SIZE * Ratio / 2 * Math.Sin(angle) - height / 2 * Math.Cos(angle);

            context.PushTransform(new TranslateTransform(rpx, rpy));
            context.PushTransform(new RotateTransform(angle / Math.PI * 180));

            try
            {
                foreach (KeyValuePair<ISerializable, long> pair in link.PacketsOnWire)
                {
                    long start = pair.Value;
                    double front = (double)(now - start) / link.Delay;
                    double tail = (now - start - (double)pair.Key.Length / link.Bandwidth * ANode.S) / link.Delay;

                    if (front > 1) front = 1;
                    if (tail < 0) tail = 0;

                    Tuple<Brush, Pen> t = PacketDrawer(pair.Key);

                    context.DrawRectangle(
                        t.Item1,
                        t.Item2,
                        new Rect(tail * width, -height / 2 - link.Bandwidth * LINK_BANDWIDTH_RATIO, Math.Max(0, (front - tail) * width), height)
                        );
                }
            }
            catch { }

            context.Pop();
            context.Pop();

        }

        public void DrawStackPacket(DrawingContext context, ANode.Link link)
        {
            NodeProfile fromNode = Nodes[link.From.Name];
            NodeProfile toNode = Nodes[link.To.Name];

            double diffX, diffY, angle, width, height, rpx, rpy, stackBottomX, stackBottomY;
            double now = EventQueue.Now;


            diffX = (toNode.Center.X - fromNode.Center.X) * ActualWidth;
            diffY = (toNode.Center.Y - fromNode.Center.Y) * ActualHeight;

            angle = Math.Atan2(diffY, diffX);
            width = Math.Sqrt(diffY * diffY + diffX * diffX) - NODE_SIZE * Ratio;
            height = link.Bandwidth * PACKET_BANDWIDTH_RATIO + link.Bandwidth * LINK_BANDWIDTH_RATIO;
            stackBottomX = fromNode.Center.X + NODE_SIZE * Ratio / 2 * Math.Cos(angle) + (height + STACK_PACKET_HEIGHT * Ratio) * Math.Sin(angle);
            stackBottomY = fromNode.Center.Y + NODE_SIZE * Ratio / 2 * Math.Sin(angle) - (height + STACK_PACKET_HEIGHT * Ratio) * Math.Cos(angle);

            rpx = fromNode.Center.X * ActualWidth + NODE_SIZE * Ratio / 2 * Math.Cos(angle) + height * Math.Sin(angle);
            rpy = fromNode.Center.Y * ActualHeight + NODE_SIZE * Ratio / 2 * Math.Sin(angle) - height * Math.Cos(angle);

            context.PushTransform(new TranslateTransform(rpx, rpy));
            context.PushTransform(new RotateTransform(angle / Math.PI * 180 - 90));

            int pos = 1;
            try
            {
                foreach (ISerializable packet in link.PacketsInQueue)
                {
                    Tuple<Brush, Pen> t = PacketDrawer(packet);

                    context.DrawRectangle(
                        t.Item1,
                        t.Item2,
                        new Rect(pos * STACK_PACKET_HEIGHT * Ratio, 0, STACK_PACKET_HEIGHT * Ratio, STACK_WIDTH * Ratio)
                        );
                    pos++;
                }
            }
            catch { }

            context.Pop();
            context.Pop();

        }


        #endregion
    }


}
