using Commander.Lib.Models;
using Commander.Lib.Services;
using Commander.Lib.Views;
using Commander.Models;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Runtime.InteropServices;
using System.Timers;

namespace Commander.Lib.Controllers
{
    public interface LoginCompleteController
    {
        void Init(object sender, EventArgs e);
        void Clear();
    }

    public class LoginCompleteControllerImpl : LoginCompleteController
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hhwnd, uint msg, IntPtr wparam, UIntPtr lparam);

        private Logger _logger;
        private MainView _mainView;
        private LoginSessionManager _loginSessionManager;
        private ServerDispatchController _serverDispatchController;
        private SettingsManager _settingsManager;
        private PlayerManager _playerManager;
        private DebuffManager _debuffManager;
        private Debugger _debugger;
        private Timer _vitaeTimer;
        private DebuffInformation.Factory _debuffInformationFactory;
        private Player.Factory _playerFactory;


        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const byte VK_PAUSE = 0x13;

        public LoginCompleteControllerImpl(
            Logger logger,
            LoginSessionManager loginSessionManager,
            SettingsManager settingsManager,
            DebuffManager debuffManager,
            PlayerManager playerManager,
            ServerDispatchController serverDispatchController,
            Debugger debugger,
            DebuffInformation.Factory debuffInformationFactory,
            Player.Factory playerFactory,
            MainView mainView)
        {
            _logger = logger.Scope("LoginCompleteController");
            _settingsManager = settingsManager;
            _loginSessionManager = loginSessionManager;
            _debuffInformationFactory = debuffInformationFactory;
            _debugger = debugger;
            _mainView = mainView;
            _playerManager = playerManager;
            _playerFactory = playerFactory;
            _serverDispatchController = serverDispatchController;
            _debuffManager = debuffManager;
            _vitaeTimer = new Timer();
            _vitaeTimer.Elapsed += new ElapsedEventHandler(VitaeTimerChecker);
            _vitaeTimer.Interval = 10000;
            _vitaeTimer.AutoReset = false;
        }

        public void Init(object sender, EventArgs e)
        {
            try
            {
                _logger.Info("Init()");

                LoginSession session = _loginSessionManager.Init();
                string server = session.Server;
                string account = session.AccountName;
                string name = session.Name;
                _settingsManager.Init(server, account, name);
                _logger.WriteToWindow($"{server}-{account}-{name}");

                _handleDebugger();
                _handleRelogger();
                _mainView.Init();
                _vitaeTimer.Start();
                _handleCachedPlayers();
            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _handleCachedPlayers()
        {
            LoginSession session = _loginSessionManager.Session;
            foreach (int id in _playerManager.GetCache())
            {
                if (!WorldObjectService.IsValidObject(id))
                    return;

                WorldObject wo = WorldObjectService.GetWorldObject(id);
                int woMonarch = wo.Values(LongValueKey.Monarch);

                bool enemy = session.Monarch != woMonarch;
                bool self = session.Id == id;
                if (!self)
                {
                    _playerManager.Add(_playerFactory(wo, enemy));
                }
            }

            _playerManager.ClearCache();
        }

        private void _handleRelogger()
        {
            Settings settings = _settingsManager.Settings;

            if (settings.Relog)
            {
                _sendPause();
            }
        }

        private void _handleDebugger()
        {
            Settings settings = _settingsManager.Settings;

            if (settings.Debug)
                _debugger.Show();
            else
                _debugger.Hide();
        }

        private void VitaeTimerChecker(object sender, ElapsedEventArgs e)
        {
            _logger.Info("VitaeTimerChecker[EVENT]");
            Settings settings = _settingsManager.Settings;
            bool logOnVit = settings.LogOnVitae;
            int limit = settings.VitaeLimit;
            int vit = WorldObjectService.GetVitae();

            if (logOnVit && (vit >= limit))
            {
                string message = $"Logging off, due to vitae limit of {limit.ToString()} being reached";
                _logger.WriteToChat(message);
                _logger.WriteToWindow(message);
                WorldObjectService.Logout();
                _vitaeTimer.Stop();
            }
        }

        private void _sendPause()
        {
            Console.WriteLine("Enabling Vtank");
            PostMessage(CoreManager.Current.Decal.Hwnd, WM_KEYDOWN, (IntPtr)VK_PAUSE, (UIntPtr)0x00450001);
            PostMessage(CoreManager.Current.Decal.Hwnd, WM_KEYUP, (IntPtr)VK_PAUSE, (UIntPtr)0xC0450001);
        }

        public void Clear()
        {
            _logger.Info("Clear()");
            _playerManager.Clear();
            _vitaeTimer.Stop();
        }

    }
}
