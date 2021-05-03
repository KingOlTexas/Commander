using Commander.Lib.Models;
using Commander.Lib.Services;
using Commander.Models;
using Decal.Adapter;
using System;
using System.Collections.Generic;

namespace Commander.Lib.Controllers
{
    public interface ServerDispatchController
    {
        void Init(object sender, NetworkMessageEventArgs e);
    }

    public class ServerDispatchControllerImpl : ServerDispatchController
    {
        private PlayerManager _playerManager;
        private Logger _logger;
        private SettingsManager _settingsManager;
        private DeathManager _deathManager;
        private DebuffInformation.Factory _debuffInformationFactory;
        private List<int> _messages = new List<int>();

        public ServerDispatchControllerImpl(
            PlayerManager playerManager,
            SettingsManager settingsManager,
            DeathManager deathManager,
            DebuffInformation.Factory debuffInformationFactory,
            Logger logger)
        {
            _logger = logger.Scope("ServerDispatchController");
            _playerManager = playerManager;
            _settingsManager = settingsManager;
            _deathManager = deathManager;
            _debuffInformationFactory = debuffInformationFactory;

        }

        public void Init(object sender, NetworkMessageEventArgs e)
        {
            try {
                int messageType = e.Message.Type;

                if (messageType == 63408 && e.Message.Value<int>("event") == 201)
                {
                    //_logger.Info($"MessageType: {messageType}");
                }

                if (e.Message.Type == 0xF755) // ApplyVisual, used to detect debuffs
                {
                    _processApplyVisual(e);
                }

                if (e.Message.Type == 0xF7B0) // Game Event
                {
                    _processGameEvent(e);
                }

                if (e.Message.Type == 414) // PlayerKilled
                {
                    _processPlayerKilled(e);
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _processPlayerKilled(NetworkMessageEventArgs e)
        {
            _logger.Info("_processPlayerKilled()");
            int killed = e.Message.Value<int>("killed");
            int killer = e.Message.Value<int>("killer");
            string deathMessage = e.Message.Value<string>("text");

            if (WorldObjectService.IsValidObject(killer) && WorldObjectService.IsPlayer(killer))
                _deathManager.ProcessPkDeath(killer, killed, deathMessage);
        }

        private void _processApplyVisual(NetworkMessageEventArgs e)
        {
            int id = e.Message.Value<int>("object");
            int effect = e.Message.Value<int>("effect");

            if (
                !WorldObjectService.IsValidObject(id) ||
                !WorldObjectService.IsPlayer(id) ||
                !Enum.IsDefined(typeof(Debuff), effect))
            {
                return;
            }

            Player player = _playerManager.Get(id);

            if (player != null)
            {
                _processApplyVisualOnPlayer(player, effect);
            }
        }

        private void _processApplyVisualOnPlayer(Player player, int effect)
        {
            int index = player.Debuffs.FindIndex(obj => obj.Spell == effect);
            if (index != -1)
            {
                player.Debuffs[index].StartTime = DateTime.Now;
            }
            else
            {
                player.Debuffs.Add(_debuffInformationFactory(effect, DateTime.Now));
            }

            _playerManager.Update(player.Id, player);
        }

        private void _processGameEvent(NetworkMessageEventArgs e)
        {
            int gameEvent = e.Message.Value<int>("event");

            if (gameEvent == 201) // IdentifyObject
                return;
                //_processIndentifyObject(e);
                
        }

        private void _processIndentifyObject(NetworkMessageEventArgs e)
        {
            int id = e.Message.Value<int>("object");

            if (!WorldObjectService.IsValidObject(id))
                return;

            if ((e.Message.Value<int>("flags") & 256) <= 0)
                return;

            Player player = _playerManager.Get(id);

            if (player != null)
            {
                _processPlayerIdentified(e, player);
            }
        }

        private void _processPlayerIdentified(NetworkMessageEventArgs e,  Player player)
        {
            int health = e.Message.Value<int>("health");
            int healthMax = e.Message.Value<int>("healthMax");

            if (health <= 0 || healthMax <= 0)
                return;

            decimal pct = ((decimal)health / (decimal)healthMax) * (decimal)100;

            if (pct < 50)
                player.LowHealth = true;
            else
                player.LowHealth = false;

            _playerManager.Update(player.Id, player);
        }
    }


}
