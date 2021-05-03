using Commander.Models;
using Decal.Adapter;
using System;
using System.Runtime.InteropServices;
using System.Timers;

namespace Commander.Lib.Services
{
    public interface RelogManager
    {
        void Init(string loggedBy);
        void Start();
        void Stop();
    }

    public class RelogManagerImpl : RelogManager
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hhwnd, uint msg, IntPtr wparam, UIntPtr lparam);

        private Logger _logger;
        private SettingsManager _settingsManager;
        private LoginSessionManager _loginSessionManager;
        private GlobalProvider _globals;
        private Timer _relogTimer;
        private TimeSpan _remaining;
        private DateTime _startTime;
        private string _loggedBy;
        private bool _active = false;
        private uint WM_MOUSEMOVE = 0x0200;
        private uint WM_LBUTTONDOWN = 0x0201;
        private uint WM_LBUTTONUP = 0x0202;

        public RelogManagerImpl(
            Logger logger,
            GlobalProvider globals,
            LoginSessionManager loginSessionManager,
            SettingsManager settingsMangager)
        {
            _logger = logger;
            _settingsManager = settingsMangager;
            _globals = globals;
            _loginSessionManager = loginSessionManager;
        }

        public void Init(string loggedBy)
        {
            _loggedBy = loggedBy;
            _relogTimer = new Timer();
            _relogTimer.Interval = 1000;
            _relogTimer.Elapsed += new ElapsedEventHandler(RelogTimerChecker);
            _relogTimer.AutoReset = true;
            _relogTimer.Stop();
        }
        
        public void Stop()
        {
            LoginSession session = _loginSessionManager.Session;
            _globals.Relogging = false;
            _relogTimer.Stop();
            _logger.WriteToWindow($"{session.Server}-{session.AccountName}-{session.Name}");
        }

        public void Start()
        {
            _startTime = DateTime.Now;
            _relogTimer.Start();
        }

        private void RelogTimerChecker(object sender, ElapsedEventArgs e)
        {
            try
            {
                int relogDuration = _settingsManager.Settings.RelogDuration;

                _remaining = (TimeSpan.FromMinutes(Convert.ToDouble(relogDuration)) - (DateTime.Now - _startTime));
                string countdown = string.Format("{0:00}:{1:00}", (int)_remaining.Minutes, _remaining.Seconds);

                if (_remaining.Minutes <= 0 && _remaining.Seconds <= 0)
                {
                    Stop();
                    SendMouseClick(300, 407);
                    return;
                }

                _logger.WriteToWindow("Logged by: " + _loggedBy + " relogging in " + countdown);
            } catch(Exception ex) { _logger.Error(ex); }
        }

        private void SendMouseClick(int x, int y)
        {
            int loc = (y * 0x10000) + x;

            PostMessage(CoreManager.Current.Decal.Hwnd, WM_MOUSEMOVE, (IntPtr)0x00000000, (UIntPtr)loc);
            PostMessage(CoreManager.Current.Decal.Hwnd, WM_LBUTTONDOWN, (IntPtr)0x00000001, (UIntPtr)loc);
            PostMessage(CoreManager.Current.Decal.Hwnd, WM_LBUTTONUP, (IntPtr)0x00000000, (UIntPtr)loc);
        }
    }
}
