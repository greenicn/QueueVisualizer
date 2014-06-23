using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

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
    /// Helper functions
    /// </summary>
    public class Helper
    {
        public static System.Windows.Point Zoom(System.Windows.Point normalizedPoint, double actualWidth, double actualHeight)
        {
            return new System.Windows.Point(normalizedPoint.X * actualWidth, normalizedPoint.Y * actualHeight);
        }

        public static System.Windows.Media.Color ModifyBright(System.Windows.Media.Color original, float newBright)
        {
            System.Drawing.Color c = System.Drawing.Color.FromArgb(original.A, original.R, original.G, original.B);
            float hue = c.GetHue();
            float sat = c.GetSaturation();
            return ColorFromAhsb(original.A, hue, sat, newBright);
        }

        public static System.Windows.Media.Color ColorFromAhsb(int a, float h, float s, float b)
        {
            // h: 0 - 360
            // s: 0 - 1
            // b: 0 - 1
            if (0 == s) return System.Windows.Media.Color.FromArgb((byte)a, (byte)(b * 255), (byte)(b * 255), (byte)(b * 255));

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < b)
            {
                fMax = b - (b * s) + s;
                fMin = b + (b * s) - s;
            }
            else
            {
                fMax = b + (b * s);
                fMin = b - (b * s);
            }

            iSextant = (int)Math.Floor(h / 60f);
            if (300f <= h)
            {
                h -= 360f;
            }
            h /= 60f;
            h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = h * (fMax - fMin) + fMin;
            }
            else
            {
                fMid = fMin - h * (fMax - fMin);
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return System.Windows.Media.Color.FromArgb((byte)a, (byte)iMid, (byte)iMax, (byte)iMin);
                case 2:
                    return System.Windows.Media.Color.FromArgb((byte)a, (byte)iMin, (byte)iMax, (byte)iMid);
                case 3:
                    return System.Windows.Media.Color.FromArgb((byte)a, (byte)iMin, (byte)iMid, (byte)iMax);
                case 4:
                    return System.Windows.Media.Color.FromArgb((byte)a, (byte)iMid, (byte)iMin, (byte)iMax);
                case 5:
                    return System.Windows.Media.Color.FromArgb((byte)a, (byte)iMax, (byte)iMin, (byte)iMid);
                default:
                    return System.Windows.Media.Color.FromArgb((byte)a, (byte)iMax, (byte)iMid, (byte)iMin);
            }
        }

    }
}
