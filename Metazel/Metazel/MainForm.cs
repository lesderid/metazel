using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Metazel.NES
{
    public partial class MainForm : Form
    {
        private NESEngine _engine;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ToggleState(object sender, EventArgs e)
        {
            if (_engine == null || !_engine.Running)
            {
                new Thread(() =>
                           {
                               var cartridge = new NESCartridge("nestress.nes");

                               if (_engine == null)
                               {
                                   _engine = new NESEngine();
                                   _engine.Load(cartridge);
                                   _engine.NewFrame += SetNewFrame;
                               }

                               _engine.Run();
                           }) { Priority = ThreadPriority.Highest }.Start();

                resetMenuItem.Enabled = true;

                toggleStateMenuItem.Text = "Pause Emulator";
            }
            else
            {
                _engine.Stop();

                toggleStateMenuItem.Text = "Resume Emulator";
            }
        }

        private void SetNewFrame(Bitmap frame)
        {
            if (pictureBox != null)
                BeginInvoke((Action)(() => { pictureBox.Image = frame; }));
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void PreParse(object sender, EventArgs e)
        {
            _engine.CPU.PreParse();
        }

        private void Reset(object sender, EventArgs e)
        {
            _engine.Reset();
        }
    }
}