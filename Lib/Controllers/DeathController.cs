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
        private DeathManager _deathManager;

        public DeathControllerImpl(
            Logger logger,
            SettingsManager settingsManager,
            DeathManager deathManager,
            LoginSessionManager loginSessionManager)
        {
            _logger = logger.Scope("DeathController");
            _loginSessionManager = loginSessionManager;
            _deathManager = deathManager;
            _settingsManager = settingsManager;
        }

        public void Init(object sender, DeathEventArgs e)
        {
            try
            {
                _deathManager.ProcessDeathFromOther(e.Text);
            } catch (Exception ex) { _logger.Error(ex); }
        }
    }
}
