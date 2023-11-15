using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LeagueK1LL3R {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        private static readonly string saveFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LeagueK1LL3R");
        private static readonly string savePath = System.IO.Path.Combine(saveFolder, "killedCounter.txt");
        private static readonly string autoStart = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Start Menu\\Programs\\Startup", System.AppDomain.CurrentDomain.FriendlyName);
        public static int killedCounter { get; set; } = 0;
        public static NotifyIcon notifyIcon { get; set; } = new NotifyIcon();
        public MainWindow() {
            InitializeComponent();
            Init();
        }

        private void Init() {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            notifyIcon.Icon = Properties.Resources.leagueoflegends;
            notifyIcon.Click += new System.EventHandler(ShowApp);

            if (Process.GetCurrentProcess().MainModule.FileName == autoStart) {
                HideApp();
            }

            try {
                //generate appdata saveFolder
                Directory.CreateDirectory(saveFolder);
            } catch { }

            try {
                //create killCounterSaveLocation txt and read it in
                if (!File.Exists(savePath))
                    File.Create(savePath).Close();
                using (var sr = new StreamReader(savePath)) {
                    killedCounter = Convert.ToInt32(sr.ReadToEnd());
                }
            } catch { }
            UpdateKillCounter();

            var listener = RunListener();
        }

        protected void HideApp() {
            this.ShowInTaskbar = false;
            notifyIcon.Visible = true;
            this.Hide();
        }

        protected void ShowApp(object sender, System.EventArgs e) {
            this.ShowInTaskbar = true;
            notifyIcon.Visible = false;
            this.Show();
        }

        private Task RunListener() {
            return Task.Run(() => {
                int detectDelay = 0;
                int presses = 0;
                bool unpressed = true;
                while (true) {
                    Thread.Sleep(40);
                    Dispatcher.Invoke(() => {
                        if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) > 0 &&
                            (Keyboard.GetKeyStates(Key.F4) & KeyStates.Down) > 0 && unpressed) {
                            presses++;
                            unpressed = false;
                        }
                        if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) == 0 ||
                            (Keyboard.GetKeyStates(Key.F4) & KeyStates.Down) == 0) unpressed = true;
                    });
                    if (presses == 0) detectDelay = 0;
                    if (presses == 1) detectDelay++;
                    if (detectDelay > 10) {
                        detectDelay = 0;
                        presses = 0;
                    }
                    if (presses > 1) {
                        foreach (var process in Process.GetProcessesByName("League of Legends")) {
                            var foo = GetForegroundWindow();
                            if (foo != process.MainWindowHandle) continue;
                            process.Kill();
                            killedCounter++;
                            File.WriteAllText(savePath, killedCounter.ToString());
                            UpdateKillCounter();
                        }
                        detectDelay = 0;
                        presses = 0;
                    }
                }
            });
        }

        public void UpdateKillCounter() {
            Dispatcher.Invoke(() => {
                txt_KilledCounter.Text = killedCounter.ToString();
            });
        }

        private void Button_HideApp_Click(object sender, RoutedEventArgs e)
            => HideApp();

        private void Button_EnableAutoStart_Click(object sender, RoutedEventArgs e) {
            try {
                File.Copy(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, autoStart, true);
            } catch { }
        }

        private void Button_DisableAutoStart_Click(object sender, RoutedEventArgs e) {
            try {
                File.Delete(autoStart);
            } catch { }
        }

        static void OnProcessExit(object sender, EventArgs e)
            => notifyIcon.Visible = false;
    }
}
