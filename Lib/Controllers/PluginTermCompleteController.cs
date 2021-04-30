using Commander.Lib.Services;
using Commander.Lib.Views;
using System;

namespace Commander.Lib.Controllers
{
    public interface PluginTermCompleteController
    {
        void Init(object sender, EventArgs e);
    }

    public class PluginTermCompleteControllerImpl : PluginTermCompleteController
    {
        private Logger _logger;
        private PlayerManager _PlayerManager;
        private RelogManager _relogManager;
        private LoginCompleteController  _loginCompleteController;
        private GlobalProvider _globals;
        private MainView _mainView;
        private DebuffManager _debuffManager;

        public PluginTermCompleteControllerImpl(
            Logger logger, 
            GlobalProvider globals,
            MainView mainView,
            LoginCompleteController loginCompleteController,
            DebuffManager debuffManager,
            RelogManager relogManager,
            PlayerManager PlayerManager)
        {
            _logger = logger.Scope("PluginTermCompleteController");
            _PlayerManager = PlayerManager;
            _globals = globals;
            _relogManager = relogManager;
            _mainView = mainView;
            _loginCompleteController = loginCompleteController;
            _debuffManager = debuffManager;
        }

        public void Init(object sender, EventArgs e)
        {
            try
            {
                _logger.Info("PluginTermCompleteController.Init");
                _mainView.Dispose();
                _loginCompleteController.Clear();
                _debuffManager.Stop();

                if (_globals.Relogging)
                {
                    _relogManager.Start();
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }
    }
}
