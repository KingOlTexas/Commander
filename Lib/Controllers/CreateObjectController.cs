using Commander.Lib.Models;
using Commander.Lib.Services;
using Decal.Adapter.Wrappers;
using System;

namespace Commander.Lib.Controllers
{
    public interface CreateObjectController
    {
        void Init(object sender, CreateObjectEventArgs e);
    }

    public class CreateObjectControllerImpl : ObjectControllerBase, CreateObjectController
    {
        private Logger _logger;
        private LoginSessionManager _loginSessionManager;

        public CreateObjectControllerImpl(
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
            _loginSessionManager = loginSessionManager;
        }

        public void Init(object sender, CreateObjectEventArgs e)
        {
            try
            {
                ProcessWorldObject(e.New);
            } catch (Exception ex) { _logger.Error(ex); }
        }

    }
}
