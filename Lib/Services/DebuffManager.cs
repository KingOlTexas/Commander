using Commander.Lib.Models;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Commander.Lib.Services
{
    public interface DebuffManager
    {
        void Start();
        void Stop();
        void Add(Player player);
    }

    public class DebuffManagerImpl : DebuffManager
    {
        private Logger _logger;
        private PlayerManager _playerManager;
        private List<Player> _debuffedPlayers;
        private Timer _debuffTimer;
        private TimeSpan _remaining;

        public DebuffManagerImpl(
            PlayerManager playerManager,
            Logger logger)
        {
            _logger = logger.Scope("DebuffManager");
            _playerManager = playerManager;
            _debuffedPlayers = new List<Player>();
            _debuffTimer = new Timer();
            _debuffTimer.Interval = 15000;
            _debuffTimer.AutoReset = true;
            _debuffTimer.Elapsed += _debuffTimer_Elapsed;
            _playerManager.PlayerUpdated += _playerManager_PlayerUpdated;
        }

        private void Update(Player player)
        {
            _logger.Info($"Update(Player {player.Name})");
            int index = _debuffedPlayers.FindIndex(_player => _player.Id == player.Id);
            _debuffedPlayers[index] = player;
        }

        private void _playerManager_PlayerUpdated(object sender, Player player)
        {
            try
            {
                if (player.Debuffs.Count == 0)
                    return;

                if (_debuffedPlayers.Contains(player))
                    Update(player);

                Start();

            } catch (Exception ex) { _logger.Error(ex); }
        }

        public void Start()
        {
            _logger.Info("Start()");
            _debuffTimer.Start();
        }

        public void Add(Player player)
        {
            _logger.Info($"Add(Player {player})");
            _debuffedPlayers.Add(player);
        }

        public void Stop()
        {
            _logger.Info("Stop()");
            _debuffTimer.Stop();
        }

        private void _processTimeLimitReached(Player player, DebuffInformation info)
        {
            player.Debuffs.Remove(info);

            if (player.Debuffs.Count == 0)
            {
                _debuffedPlayers.Remove(player);
            }

            _playerManager.Update(player.Id, player);
        }

        private void _processDebuffedPlayer(Player player)
        {
            _logger.Info($"Player in _debuffedPlayers: {player.Name}");
            foreach (DebuffInformation info in player.Debuffs)
            {
                _remaining = (TimeSpan.FromMinutes(Convert.ToDouble(5)) - (DateTime.Now - info.StartTime));
                _logger.Info(
                    $"DebuffInformation: Spell: [{info.Spell}] -- _remaining: {_remaining.Minutes}:{_remaining.Seconds}");

                if (_remaining.Minutes <= 0 && _remaining.Seconds <= 0)
                {
                    _processTimeLimitReached(player, info);
                }
            }
        }

        private void _debuffTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.Info("=====_deuffTimer_Elapsed(Event)=====");
            _logger.Info($"_debuffedPlayers.count: {_debuffedPlayers.Count.ToString()}");

            if (_debuffedPlayers.Count == 0)
            {
                Stop();
            }

            foreach (Player player in _debuffedPlayers)
            {
                _processDebuffedPlayer(player);
            }
        }
    }
}
