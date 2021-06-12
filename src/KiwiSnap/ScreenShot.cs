using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KiwiSnap
{
    internal class ScreenShot
    {
        public static bool saveToClipboard = true;

        public static Bitmap CaptureImage(Point SourcePoint, Point DestinationPoint, Rectangle SelectionRectangle)
        {
            using (Bitmap bitmap = new Bitmap(SelectionRectangle.Width, SelectionRectangle.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(SourcePoint, DestinationPoint, SelectionRectangle.Size);
                }

                if (saveToClipboard)
                {
                    Image img = (Image)bitmap;
                    Clipboard.SetImage(img);
                    return bitmap;
                }

                return null;
            }
        }
    }
}