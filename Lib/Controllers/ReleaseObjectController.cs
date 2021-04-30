using Commander.Lib.Models;
using Commander.Lib.Services;
using Decal.Adapter.Wrappers;
using System;

namespace Commander.Lib.Controllers
{
    public interface ReleaseObjectController
    {
        void Init(object sender, ReleaseObjectEventArgs e);
    }

    public class ReleaseObjectControllerImpl : ReleaseObjectController
    {
        private Logger _logger;
        private PlayerManager _PlayerManager;

        public ReleaseObjectControllerImpl(
            Logger logger, 
            PlayerManager PlayerManager,
            SettingsManager settingsManager,
            LoginSessionManager loginSessionManager)          {
            _logger = logger.Scope("ReleaseObjectController");
            _PlayerManager = PlayerManager;
        }

        public void Init(object sender, ReleaseObjectEventArgs e)
        {
            try
            {
                //_logger.Info("ReleaseObjectController.Init");
                int id = e.Released.Id;
                string name = e.Released.Name;
                Player player = _PlayerManager.Get(id);

                if (player != null) 
                {
                    _logger.Info($"Enemy Released: {name}");
                    _PlayerManager.Remove(id, player);
                }
            } catch (Exception ex) { _logger.Error(ex); }
        }

    }
}
