using Commander.Lib.Models;
using Commander.Lib.Services;
using Decal.Adapter.Wrappers;
using System;

namespace Commander.Lib.Controllers
{
    public interface MoveObjectController
    {
        void Init(object sender, MoveObjectEventArgs e);
    }

    public class MoveObjectControllerImpl : ObjectControllerBase, MoveObjectController
    {
        private Logger _logger;

        public MoveObjectControllerImpl(
            Player.Factory playerFactory,
            Logger logger, 
            PlayerManager PlayerManager,
            SettingsManager settingsManager,
            RelogManager relogManager,
            GlobalProvider globals,
            LoginSessionManager loginSessionManager) : base(
                playerFactory,
                logger.Scope("MoveObjectController"), 
                PlayerManager, 
                globals,
                settingsManager, 
                relogManager,
                loginSessionManager)
        {
            _logger = logger;
        }

        public void Init(object sender, MoveObjectEventArgs e)
        {
            try
            {
                ProcessWorldObject(e.Moved);
            } catch (Exception ex) { _logger.Error(ex); }
        }
    }
}
