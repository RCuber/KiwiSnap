using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KiwiSnap
{
    public partial class MainWindow : Form
    {
        #region Imports

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion Imports

        #region LLConsts

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        #endregion LLConsts

        #region HookHandlers

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        #endregion HookHandlers

        public static int x, y, width, height;

        public static MainWindow Instance { get; private set; }
        public Bitmap bitmap { get; set; }

        private static CaptureForm captureForm;

        public MainWindow()
        {
            _hookID = SetHook(_proc);
            InitializeComponent();
            Instance = this;
            InvokeCapture();
        }

        private static void InvokeCapture()
        {
            captureForm = new CaptureForm();
            captureForm.InstanceRef = Instance;
            captureForm.Show();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.window_MouseWheel);
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            this.BackgroundImage = Clipboard.GetImage();
        }

        private void MainWindow_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.Hide();
            }
        }

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if ((Keys)vkCode == Keys.F10)
                {
                    GrabScreen();
                }
                else if ((Keys)vkCode == Keys.F9)
                {
                    InvokeCapture();
                }
                Console.WriteLine((Keys)vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void MainWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!this.Visible)
                Show();
            else
                Hide();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void captureAgainF11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvokeCapture();
        }

        private void captureBehindWindowF12ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GrabScreen();
        }

        private void window_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            Console.WriteLine(numberOfTextLinesToMove);

            if (numberOfTextLinesToMove > 0)
            {
                this.Width += 50;
                this.Height += 50;
            }
            else
            {
                this.Width -= 50;
                this.Height -= 50;
            }
        }

        private static void GrabScreen()

        {
            try

            {
                var x = Instance.Location.X;
                var y = Instance.Location.Y;
                var width = Instance.Width;
                var height = Instance.Height;

                Point StartPoint = new Point(x, y);
                Rectangle bounds = new Rectangle(x, y, width, height);

                Instance.Visible = false;
                ScreenShot.CaptureImage(StartPoint, Point.Empty, bounds);

                Instance.BackgroundImage = Clipboard.GetImage();

                Instance.Visible = true;
                Instance.Opacity = 100;
            }
            catch (Exception ex)

            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }
    }
}