using Commander.Lib.Services;
using Commander.Models;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;

namespace Commander.Lib.Controllers
{
    public interface DeathController
    {
        void Init(object sender, DeathEventArgs e);
    }

    public class DeathControllerImpl : DeathController
    {
        private Logger _logger;
        private LoginSessionManager _loginSessionManager;
        private SettingsManager _settingsManager;

        public DeathControllerImpl(
            Logger logger,
            SettingsManager settingsManager,
            LoginSessionManager loginSessionManager)
        {
            _logger = logger.Scope("DeathController");
            _loginSessionManager = loginSessionManager;
            _settingsManager = settingsManager;
        }

        public void Init(object sender, DeathEventArgs e)
        {
            try
            {
                LoginSession session = _loginSessionManager.Session;
                Settings settings = _settingsManager.Settings;

                _logger.Info("DeathController.Init");

                if (session == null || settings == null)
                {
                    _logger.WriteToChat("Session or Settings not loaded before death occured, logging out");
                    CoreManager.Current.Actions.Logout();
                    return;
                }

                if (settings.LogOnDeath)
                {
                    _logger.WriteToChat("Logging off, due to LogOnDeath enabled");
                    _logger.WriteToWindow(e.Text);
                    CoreManager.Current.Actions.Logout();
                }

                if (settings.LogOnVitae && session.Vitae >= settings.VitaeLimit)
                {
                    string message = $"Logging off, due to vitae limit of {settings.VitaeLimit.ToString()} being reached";
                    _logger.WriteToChat(message);
                    _logger.WriteToWindow(message);
                    CoreManager.Current.Actions.Logout();
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }
    }
}
