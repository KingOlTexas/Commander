using System.IO;
using Newtonsoft.Json;
using Commander.Models;

namespace Commander.Lib.Services
{
    public interface SettingsManager
    {
        Settings Read();
        Settings Write();
        Settings Settings { get; set; }
        Settings Init(string server, string account, string name);
    }

    public class SettingsManagerImpl : SettingsManager
    {
        public Settings Settings { get; set; }
        private Logger _logger;
        private Settings.Factory _settingsFactory;
        private string _pluginPath;
        private string _characterPath;
        private string _filePath;
        private string _pluginName;
        private string _server;
        private string _name;
        private string _account;

        public SettingsManagerImpl(
            Logger logger, 
            GlobalProvider globals, 
            Settings.Factory settingsFactory)
        {
            _logger = logger.Scope("SettingsManager");
            _settingsFactory = settingsFactory;
            _pluginPath = globals.PluginPath;
            _pluginName = globals.PluginName;
        }

        public Settings Init(string server, string account, string name)
        {
            _server = server;
            _name = name;
            _account = account;
            _characterPath = $@"{_pluginPath}\{_server}\{_account}\{_name}";
            _filePath = $@"{_characterPath}\{_pluginName}.json";

            return Read();
        }

        public Settings Write()
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                Directory.CreateDirectory(_characterPath);
                using (StreamWriter sw = new StreamWriter(_filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, Settings);
                }

                _logger.Info($@"Writing to: {_filePath}");

            } catch(IOException ex) { _logger.Error(ex); }

            return Settings;
        }

        public Settings Read()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string jsonString = File.ReadAllText(_filePath);
                    Settings = JsonConvert.DeserializeObject<Settings>(jsonString);
                    _logger.Info("Settings successfully read");
                }
                else
                {
                    Settings = _settingsFactory();
                    Write();
                }

            } catch (IOException ex)
            {
                _logger.Error(ex);
                Settings = _settingsFactory();
            }

            return Settings;
        }

    }
}
