using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

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
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private List<MainWindow> windows = new List<MainWindow>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //System.Windows.Forms.Screen lastScreen = null;
            //foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            //{
            //    lastScreen = screen;
            //}
            //MainWindow window = new MainWindow
            //{
            //    WindowStartupLocation = WindowStartupLocation.Manual,
            //    WindowState = WindowState.Normal,
            //    WindowStyle = WindowStyle.None,
            //    Width = lastScreen.Bounds.Width,
            //    Height = lastScreen.Bounds.Height,
            //    Left = lastScreen.Bounds.Left,
            //    Top = lastScreen.Bounds.Top,
            //};
            //window.Closed += window_Closed;
            //window.Show();
            //windows.Add(window);

        }

        void window_Closed(object sender, EventArgs e)
        {
            //foreach (MainWindow window in windows)
            //    if (window != sender)
            //        window.Close();
        }

    }
}
