using Commander.Lib.Models;
using Commander.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Reflection;
using System.Timers;

namespace Commander.Lib.Services
{
    public interface PlayerManager
    {
        void Add(Player player);
        void Remove(int id, Player player);
        void Update(int id, Player player);
        void CachePlayer(int id);
        List<int> GetCache();
        Player Get(int id);
        Player GetByName(string name);
        void Clear();
        void ClearCache();
        Dictionary<int, Player> PlayersInstance();
        event EventHandler<Player> PlayerAdded;
        event EventHandler<Player> PlayerRemoved;
        event EventHandler<Player> PlayerUpdated;
    }

    public class PlayerManagerImpl : PlayerManager
    {
        private Logger _logger;
        private LoginSessionManager _loginSessionManager;
        private SettingsManager _settingsManager;
        private List<int> _preSessionPlayerCache = new List<int>();
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private Timer _ghostObjectTimer;
        private bool isPlayingSound = false;

        public event EventHandler<Player> PlayerAdded;
        public event EventHandler<Player> PlayerRemoved;
        public event EventHandler<Player> PlayerUpdated;

        public PlayerManagerImpl(
            Logger logger,
            SettingsManager settingsManager,
            LoginSessionManager loginSessionManager)
        {
            _logger = logger.Scope("PlayerManager");
            _loginSessionManager = loginSessionManager;
            _settingsManager = settingsManager;
            _ghostObjectTimer = new Timer();
            _ghostObjectTimer.Interval = 1000 * 60 * 3;
            _ghostObjectTimer.AutoReset = true;
            _ghostObjectTimer.Elapsed += _ghostObjectTimer_Elapsed;
            _ghostObjectTimer.Start();
        }

        public void CachePlayer(int id)
        {
            if (!_preSessionPlayerCache.Contains(id))
                _preSessionPlayerCache.Add(id);
        }

        public List<int> GetCache()
        {
            return _preSessionPlayerCache;
        }

        private void _processGhostObjects()
        {
            int currentId = _loginSessionManager.Session.Id;

            foreach (KeyValuePair<int, Player> player in _players)
            {
                int playerId = player.Value.Id;
                if (
                    !WorldObjectService.IsValidObject(playerId) ||
                    WorldObjectService.GetDistanceFromPlayer(currentId, playerId) > 1000)
                {
                    _logger.Info($"Player: {player.Value.Name} is not a valid object");
                    Remove(playerId, Get(playerId));
                }
            }
        }

        private void _ghostObjectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.Info("Checking for GhostPlayerObjects");

            if (_players.Count > 0 || _loginSessionManager.Session != null)
                _processGhostObjects();
        }

        public Dictionary<int, Player> PlayersInstance()
        {
            return _players;
        }

        protected virtual void OnPlayerUpdated(Player player)
        {
            PlayerUpdated?.Invoke(this, player);
        }

        protected virtual void OnPlayerRemoved(Player player)
        {
            PlayerRemoved?.Invoke(this, player);
        }

        protected virtual void OnPlayerAdded(Player player)
        {
            PlayerAdded?.Invoke(this, player);
        }

        public void Update(int id, Player player)
        {
            Player currentPlayer = Get(id);

            if (currentPlayer == null)
            {
                Add(player);
                return;
            }

            _players[id] = player;
            OnPlayerUpdated(player);
        }

        public void Remove(int id, Player player)
        {
            _players.Remove(id);
            OnPlayerRemoved(player);
            _logger.WriteToChat($"Player Removed: {player.Name}");
        }

        public void Clear()
        {
            _logger.Info("Clear()");
            _players.Clear();
            ClearCache();
        }

        public void Add(Player player)
        {
            LoginSession session = _loginSessionManager.Session;
            Settings settings = _settingsManager.Settings;
            string soundPath;

            if (Get(player.Id) != null)
                return;

            _players.Add(player.Id, player);
            OnPlayerAdded(player);

            if (session == null)
                return;

            if (player.Enemy)
            {
                _logger.WriteToChat($"Enemy Added: {player.Name}");
                soundPath = "Commander.Assets.Audio.enemy.wav";
                if (settings.EnemySounds)
                {
                    PlaySoundFromResource(soundPath);
                }
            }
            else
            {
                _logger.WriteToChat($"Friendly Added: {player.Name}");
                soundPath = "Commander.Assets.Audio.friendly.wav";
                if (settings.FriendlySounds)
                {
                    PlaySoundFromResource(soundPath);
                }
            }

        }

        public Player Get(int id)
        {
            if (_players.TryGetValue(id, out Player player))
            {
                return player;
            }
            else
            {
                return null;
            }
        }

        public Player GetByName(string name)
        {
            foreach (KeyValuePair<int, Player> entry in _players)
            {
                if (entry.Value.Name == name)
                {
                    return entry.Value;
                }
            }

            return null;
        }

        /* https://stackoverflow.com/a/18946392 */
        private Stream GetResourceStream(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
            return resourceStream;
        }

        private void PlaySoundFromResource(string resourceName)
        {
            if (isPlayingSound)
                return;

            isPlayingSound = true;

            Stream resourceStream = GetResourceStream(resourceName);
            if (resourceStream != null)
            {
                using (Stream input = resourceStream)
                {
                    new SoundPlayer(input).Play();
                }
            }

            isPlayingSound = false;
        }

        public void ClearCache()
        {
            _preSessionPlayerCache.Clear();
        }
    }
}
