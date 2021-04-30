using Commander.Lib.Models;
using Commander.Lib.Services;
using Decal.Adapter;
using System;

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
        private DebuffInformation.Factory _debuffInformationFactory;

        public ServerDispatchControllerImpl(
            PlayerManager playerManager,
            DebuffInformation.Factory debuffInformationFactory,
            Logger logger)
        {
            _logger = logger;
            _playerManager = playerManager;
            _debuffInformationFactory = debuffInformationFactory;

        }

        public void Init(object sender, NetworkMessageEventArgs e)
        {
            try
            {
                if (e.Message.Type == 0xF755) // ApplyVisual, used to detect debuffs
                {
                    _processApplyVisual(e);
                }

                if (e.Message.Type == 0xF7B0) // Game Event
                {
                    _processGameEvent(e);
                }

                if (e.Message.Type == 0x19E) // PlayerKilled
                {
                    _processPlayerKilled(e);
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _processPlayerKilled(NetworkMessageEventArgs e)
        {
            int killed = e.Message.Value<int>("killed");
            int killer = e.Message.Value<int>("killer");
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

        }

        private void _processUpdateHealth(NetworkMessageEventArgs e)
        {
            int id = e.Message.Value<int>("object");
            double hp = e.Message.Value<double>("health") * 100;
            Player player = _playerManager.Get(id);

            if (player == null)
                return;

            if (hp > 40 && player.LowHealth)
            {
                player.LowHealth = false;
                _playerManager.Update(player.Id, player);
            }

            if (hp < 40 && !player.LowHealth)
            {
                player.LowHealth = true;
                _playerManager.Update(player.Id, player);
            }

        }

        private void _processGameEvent(NetworkMessageEventArgs e)
        {
            int gameEvent = e.Message.Value<int>("event");

            if (gameEvent == 0x01C0) // Update Health
                _processUpdateHealth(e);
        }

    }


}
