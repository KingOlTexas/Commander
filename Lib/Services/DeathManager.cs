using Commander.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commander.Lib.Services
{
    public interface DeathManager
    {
        void ProcessPkDeath(int killerId, int killedId, string deathMessage);
        void ProcessDeathFromOther(string deathMessage);
    }

    public class DeathManagerImpl : DeathManager
    {
        private Logger _logger;
        private SettingsManager _settingsManager;

        public DeathManagerImpl(
            Logger logger,
            SettingsManager settingsManager)
        {
            _logger = logger.Scope("DeathManager");
            _settingsManager = settingsManager;
        }

        public void ProcessPkDeath(int killerId, int killedId, string deathMessage)
        {
            _logger.Info("ProcessPkDeath()");
            int self = WorldObjectService.GetSelf().Id;

            if (killerId == self)
            {
                _logger.Info($"You killed: {WorldObjectService.GetWorldObject(killedId).Name}");
                _logger.Info(deathMessage);
            }

            if (killedId == self)
            {
                _processDeath(deathMessage);
            }
        }

        public void ProcessDeathFromOther(string deathMessage)
        {
            _logger.Info("ProcessDeathFromOther()");
            _processDeath(deathMessage);
        }

        private void _processDeath(string deathMessage)
        {
            _logger.Info("_processDeath");
            Settings settings = _settingsManager.Settings;

            if (settings == null)
            {
                _processDeathSettingsUnknown();
            }

            if (settings.LogOnDeath)
            {
                _processLogOnDeath(deathMessage);
            }

            if (settings.LogOnVitae)
            {
                _processLogOnVit();
            }
        }
        private void _processDeathSettingsUnknown()
        {
            _logger.Info("_processDeathSettingsUnknown()");
            string message = "Settings not loaded before death occured, logging out";
            _logger.WriteToChat(message);
            _logger.WriteToWindow(message);
            WorldObjectService.Logout();
            return;
        }

        private void _processLogOnDeath(string deathMessage)
        {
            _logger.Info("_processLogOnDeath()");
            _logger.WriteToChat(deathMessage);
            _logger.WriteToWindow(deathMessage);
            WorldObjectService.Logout();
            return;
        }

        private void _processLogOnVit()
        {
            _logger.Info("_processLogOnVit()");
            Settings settings = _settingsManager.Settings;

            if (settings.LogOnVitae && WorldObjectService.GetVitae() >= settings.VitaeLimit)
            {
                string message = $"Logging off, due to vitae limit of {settings.VitaeLimit.ToString()} being reached";
                _logger.WriteToChat(message);
                _logger.WriteToWindow(message);
                WorldObjectService.Logout();
                return;
            }
        }
    }
}
