using Commander.Lib.Models;
using Commander.Lib.Services;
using Commander.Models;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;

namespace Commander.Lib.Controllers
{
    public class ObjectControllerBase
    {
        private LoginSessionManager _loginSessionManager;
        private PlayerManager _playerManager;
        private SettingsManager _settingsManager;
        private GlobalProvider _globals;
        private RelogManager _relogManager;
        private Player.Factory _playerFactory;
        private Logger _logger;

        public ObjectControllerBase(
            Player.Factory playerFactory,
            Logger logger,
            PlayerManager PlayerManager,
            GlobalProvider globals,
            SettingsManager settingsManager,
            RelogManager relogManager,
            LoginSessionManager loginSessionManager)
        {
            _logger = logger;
            _loginSessionManager = loginSessionManager;
            _playerManager = PlayerManager;
            _settingsManager = settingsManager;
            _relogManager = relogManager;
            _playerFactory = playerFactory;
            _globals = globals;
        }

        protected void ProcessWorldObject(WorldObject obj)
        {
            LoginSession session = _loginSessionManager.Session;
            Settings settings = _settingsManager.Settings;

            if (session == null || settings == null)
            {
                _processPreSession(obj);
            }
            else
            {
                _processPostSession(obj);
            }
        }

        private void _processPreSession(WorldObject obj)
        {
            if (WorldObjectService.IsPlayer(obj.Id))
            {
                _playerManager.CachePlayer(obj.Id);
                return;
            }
        }

        private void _processPlayerObject(WorldObject wo)
        {
            Settings settings = _settingsManager.Settings;
            LoginSession session = _loginSessionManager.Session;

            int woMonarch = wo.Values(LongValueKey.Monarch);
            string woName = wo.Name;
            int woId = wo.Id;
            int id = session.Id;
            int monarch = session.Monarch;
            bool enemy = woMonarch != monarch;
            bool self = woId == id;

            if (self)
                return;

            if (_playerManager.Get(woId) != null)
            {
                //WorldObjectService.RequestId(woId);
                return;
            }

            double distance = WorldObjectService.GetDistanceFromPlayer(woId, id);
            int playerDistance = Convert.ToInt32(distance);

            if (playerDistance > 300)
                return;

            if (enemy)
            {
                _processObjectEnemy(wo);
                return;
            }

            _processObjectFriendly(wo);
        }

        private void _processObjectFriendly(WorldObject wo)
        {
            _playerManager.Add(_playerFactory(wo, false));
        }

        private void _processObjectEnemy(WorldObject wo)
        {
            Settings settings = _settingsManager.Settings;

            bool relogging = _globals.Relogging;
            bool relog = settings.Relog;

            _playerManager.Add(_playerFactory(wo, true));

            if (relog && !relogging)
            {
                _logger.WriteToChat($"Logged by: {wo.Name}");
                _globals.Relogging = true;
                _relogManager.Init(wo.Name);
                WorldObjectService.Logout();
            }
        }

        private void _processPostSession(WorldObject wo)
        {
            if (WorldObjectService.GetSelf().Id != _loginSessionManager.Session.Id)
            {
                _loginSessionManager.Clear();
                _processPreSession(wo);
                return;
            }

            if (WorldObjectService.IsPlayer(wo.Id))
            {
                _processPlayerObject(wo);
            } 
        }
    }
}
