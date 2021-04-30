using Commander.Lib.Services;
using Decal.Adapter.Wrappers;
using System;

namespace Commander.Lib.Controllers
{
    public interface LoginController
    {
        void Init(object sender, LoginEventArgs e);
    }

    public class LoginControllerImpl : LoginController
    {
        private Logger _logger;
        private GlobalProvider _globals;
        private RelogManager _relogManager;
        private SettingsManager _settingsManager;

        public LoginControllerImpl(
            Logger logger, 
            RelogManager relogManager,
            SettingsManager settingsManager,
            GlobalProvider globals)
        {
            _logger = logger.Scope("LoginController");
            _globals = globals;
            _relogManager = relogManager;
            _settingsManager = settingsManager;
        }

        public void Init(object sender, LoginEventArgs e)
        {
            try
            {
                _logger.Info("Init()");
                _logger.WriteToWindow("Asheron's Call");
                
                if (_globals.Relogging)
                {
                    _logger.Info("Relogging");
                    _relogManager.Stop();
                    _settingsManager.Settings.Relog = false;
                    _settingsManager.Write();
                }
            } catch (Exception ex) { _logger.Error(ex); }
        }

    }
}
