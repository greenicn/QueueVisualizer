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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Win32;

using Network;
using Common;

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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, NodeProfile> nodes = new Dictionary<string, NodeProfile>();

        public Func<ISerializable, Tuple<Brush, Pen>> IPPacketDrawer = (packet) =>
        {
            float h = 0, s = 0, b = 0.5f;

            if (packet is IPPacket)
            {
                IPPacket pkt = packet as IPPacket;
                h = (float)((pkt.SRC.GetHashCode() + pkt.DST.GetHashCode()) % 360);
                s = 1.0f;
                if (pkt.Payload is Payload)
                    b = ((pkt.Payload as Payload).SegID % 2) / 1.0f * 0.3f + 0.53f;
            }

            Brush brush = new LinearGradientBrush
                            {
                                StartPoint = new Point(0.5, 0),
                                EndPoint = new Point(0.5, 1),
                                GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Helper.ColorFromAhsb(255, h, s, b-0.2f), 0),
                        new GradientStop(Helper.ColorFromAhsb(255, h, s, b), 0.5),
                        new GradientStop(Helper.ColorFromAhsb(255, h, s, b-0.2f), 1)
                    }
                            };
            Pen p = new Pen(new SolidColorBrush(Helper.ColorFromAhsb(255, h, s, 1 - b)), 1.0);
            return new Tuple<Brush, Pen>(brush, p);
        };

        private bool Running = false;
        private double Step = 1;
        public MainWindow()
        {
            InitializeComponent();
            viewer.Nodes = nodes;
            viewer.PacketDrawer = IPPacketDrawer;

            UpdateViewAction = (l) =>
            {
                LabelTime.Content = l.ToString("0.000");
                viewer.InvalidateVisual();
            };
            Thread t = new Thread(RunThread);
            t.IsBackground = true;
            t.Start();
        }

        void RunThread()
        {
            EventQueue.AddEvent(EventQueue.Now, UpdateView);
            EventQueue.Run();
        }

        Action<double> UpdateViewAction;

        void UpdateView(params object[] objs)
        {
            while (!Running)
                Thread.Sleep(1000 / 36);

            Dispatcher.BeginInvoke(UpdateViewAction, EventQueue.Now * 1.0 / Network.ANode.MS);
            Thread.Sleep(1000 / 36);
            EventQueue.AddEvent(EventQueue.Now + (long)(Step * Network.ANode.MIUS), UpdateView);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Running)
            {
                Running = false;
                (sender as Button).Content = "Start";
            }
            else
            {
                Running = true;
                (sender as Button).Content = "Pause";
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Step = (sender as Slider).Value;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //LabelTime.Content = e.Key.ToString();
            switch (e.Key)
            {
                case Key.F5:
                case Key.Escape:
                case Key.Space:
                    Button_Click(ButtonStart, null);
                    break;
                case Key.Next:
                case Key.Right:
                    SliderSpeed.Value = Math.Min(SliderSpeed.Maximum, SliderSpeed.Value + 1);
                    break;
                case Key.End:
                    SliderSpeed.Value = SliderSpeed.Maximum;
                    break;
                case Key.Home:
                    SliderSpeed.Value = SliderSpeed.Minimum;
                    break;
                case Key.PageUp:
                case Key.Left:
                    SliderSpeed.Value = Math.Max(SliderSpeed.Minimum, SliderSpeed.Value - 1);
                    break;
                case Key.Delete:
                case Key.ImeProcessed:
                case Key.OemPeriod:
                    this.Close();
                    break;
            }
        }

        private OpenFileDialog ofd = new OpenFileDialog();

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            string profileName;
            if (args.Length > 1)
                profileName = args[1];
            else
            {
                ofd.CheckFileExists = true;
                ofd.DefaultExt = ".txt";
                ofd.Title = "Select a simulation file";
                ofd.Filter = "Text documents (.txt)|*.txt"; 
                bool? file = ofd.ShowDialog(this);
                if (file == true)
                {
                    profileName = ofd.FileName;
                }
                else
                {
                    this.Close();
                    return;
                }
            }
            Action<double> SetSpeedDispatcherAction = (speed) => SliderSpeed.Value = speed;
            ProfileReader.ReadProfile(profileName, nodes,
                (speed) =>
                {
                    Dispatcher.BeginInvoke(SetSpeedDispatcherAction, speed);
                });
            viewer.InvalidateVisual();
        }

    }
}
