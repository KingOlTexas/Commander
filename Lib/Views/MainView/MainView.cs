using Commander.Lib.Models;
using Commander.Lib.Services;
using Commander.Models;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Drawing;
using VirindiViewService.Controls;

namespace Commander.Lib.Views
{
    public class MainView : BaseView
    {
        private GlobalProvider _globals;
        private Logger _logger;
        private Debugger _debugger;
        private SettingsManager _settingsManager;
        private PlayerManager _playerManager;
        private DebuffObj.Factory _debuffObjFactory;
        private Settings _settings;
        private HudCheckBox _debug;
        private HudCheckBox _logOnDeath;
        private HudCheckBox _logOnVitae;
        private HudCheckBox _enemySounds;
        private HudCheckBox _friendlySounds;
        private HudCheckBox _friendlyIcon;
        private HudCheckBox _enemyIcon;
        private HudCheckBox _relog;
        private HudTextBox _vitaeLimit;
        private HudTextBox _relogDuration;
        private HudList _enemyListView;
        private HudList _friendlyListView;
        private List<PlayerIcon> _playerIcons;
        private List<DebuffObj> _debuffObjects;
        private PlayerIcon.Factory _playerIconFactory;

        const int FriendlyIcon = 100675625;	
        const int EnemyIcon = 100690759;

        public MainView(
            Logger logger,
            GlobalProvider globals,
            PlayerManager playerManager,
            SettingsManager settingsManager,
            DebuffObj.Factory debuffObjFactory,
            PlayerIcon.Factory playerIconfactory,
            Debugger debugger) : base(logger.Scope("MainView"), globals)
        {
            _logger = logger;
            _debugger = debugger;
            _playerManager = playerManager;
            _settingsManager = settingsManager;
            _debuffObjFactory = debuffObjFactory;
            _playerIconFactory = playerIconfactory;
            _globals = globals;
        }

        public void Init()
        {
            try
            {
                _logger.Info("Init()");

                CreateFromXMLResource("Commander.Lib.Views.MainView.mainView.xml");
                _debug = (HudCheckBox)view["DebugCheckBox"];
                _logOnDeath = (HudCheckBox)view["LogOnDeath"];
                _logOnVitae = (HudCheckBox)view["LogOnVitae"];
                _vitaeLimit = (HudTextBox)view["VitaeLimit"];
                _relog = (HudCheckBox)view["Relog"];
                _relogDuration = (HudTextBox)view["RelogDuration"];
                _enemyListView = (HudList)view["EnemyList"];
                _friendlyListView = (HudList)view["FriendlyList"];
                _enemySounds = (HudCheckBox)view["EnemySounds"];
                _friendlySounds = (HudCheckBox)view["FriendlySounds"];
                _friendlyIcon = (HudCheckBox)view["FriendlyIcon"];
                _enemyIcon = (HudCheckBox)view["EnemyIcon"];

                _settings = _settingsManager.Settings;

                _debug.Checked = _settings.Debug;
                _logOnDeath.Checked = _settings.LogOnDeath;
                _logOnVitae.Checked = _settings.LogOnVitae;
                _vitaeLimit.Text = _settings.VitaeLimit.ToString();
                _relog.Checked = _settings.Relog;
                _relogDuration.Text = _settings.RelogDuration.ToString();
                _enemySounds.Checked = _settings.EnemySounds;
                _friendlySounds.Checked = _settings.FriendlySounds;
                _friendlyIcon.Checked = _settings.FriendlyIcon;
                _enemyIcon.Checked = _settings.EnemyIcon;

                foreach (KeyValuePair<int, Player> entry in _playerManager.PlayersInstance())
                {
                    _processPlayerAdd(entry.Value);
                }

                _debuffObjects = new List<DebuffObj>();
                _playerIcons = new List<PlayerIcon>();
                RegisterEvents();
            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void RegisterEvents()
        {
            _logger.Info("RegisterEvents()");
            _debug.Change += DebugChange;
            _logOnDeath.Change += LogOnDeathChange;
            _logOnVitae.Change += LogOnVitaeChange;
            _vitaeLimit.Change += VitaeLimitChange;
            _relog.Change += RelogChange;
            _relogDuration.Change += RelogDurationChange;
            _playerManager.PlayerAdded += _playerManager_PlayerAdded;
            _playerManager.PlayerRemoved += _playerManager_PlayerRemoved;
            _playerManager.PlayerUpdated += _playerManager_PlayerUpdated;
            _enemyListView.Click += _enemyListView_Click;
            _friendlyListView.Click += _friendlyListView_Click;
            _enemySounds.Change += _enemySounds_Change;
            _friendlySounds.Change += _friendlySounds_Change;
            _enemyIcon.Change += _enemyIcon_Change;
            _friendlyIcon.Change += _friendlyIcon_Change;
        }

        private void _friendlyIcon_Change(object sender, EventArgs e)
        {
            try
            {
                _logger.WriteToChat($"FriendlyIconChange[EVENT]: {_friendlyIcon.Checked}");
                _settingsManager.Settings.FriendlyIcon = _friendlyIcon.Checked;
                _settingsManager.Write();
                
                foreach(PlayerIcon icon in _playerIcons)
                {
                    Player player = _playerManager.Get(icon.Id);
                    
                    if (player != null && !player.Enemy)
                    {
                        icon.Icon.Visible = _settings.FriendlyIcon;
                    }
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _enemyIcon_Change(object sender, EventArgs e)
        {
            try
            {
                _logger.WriteToChat($"EnemyIconChange[EVENT]: {_enemyIcon.Checked}");
                _settingsManager.Settings.EnemyIcon = _enemyIcon.Checked;
                _settingsManager.Write();

                foreach(PlayerIcon icon in _playerIcons)
                {
                    Player player = _playerManager.Get(icon.Id);
                    
                    if (player != null && player.Enemy)
                    {
                        icon.Icon.Visible = _settings.EnemyIcon;
                    }
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _friendlySounds_Change(object sender, EventArgs e)
        {
            try
            {
                _logger.WriteToChat($"FriendlySoundsChange[EVENT]: {_friendlySounds.Checked}");
                _settingsManager.Settings.FriendlySounds = _friendlySounds.Checked;
                _settingsManager.Write();

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _enemySounds_Change(object sender, EventArgs e)
        {
            try
            {
                _logger.WriteToChat($"EnemeySoundsChange[EVENT]: {_enemySounds.Checked}");
                _settingsManager.Settings.EnemySounds = _enemySounds.Checked;
                _settingsManager.Write();

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void UnRegisterEvents()
        {
            _logger.Info("UnRegisterEvents()");
            _debug.Change -= DebugChange;
            _logOnDeath.Change -= LogOnDeathChange;
            _logOnVitae.Change -= LogOnVitaeChange;
            _vitaeLimit.Change -= VitaeLimitChange;
            _relog.Change -= RelogChange;
            _relogDuration.Change -= RelogDurationChange;
            _playerManager.PlayerAdded -= _playerManager_PlayerAdded;
            _playerManager.PlayerRemoved -= _playerManager_PlayerRemoved;
            _playerManager.PlayerUpdated -= _playerManager_PlayerUpdated;
            _enemyListView.Click -= _enemyListView_Click;
            _friendlyListView.Click -= _friendlyListView_Click;
            _enemySounds.Change -=_enemySounds_Change;
            _friendlySounds.Change -= _enemySounds_Change;
            _friendlyIcon.Change -= _friendlyIcon_Change;
            _enemyIcon.Change -= _enemyIcon_Change;
        }

        private void _playerManager_PlayerUpdated(object sender, Player player)
        {
            try
            {
                PlayerIcon playerIcon = _playerIcons.Find(icon => icon.Id == player.Id);
                float fade = player.LowHealth ? 0.2f : 0;
                playerIcon.Icon.PFade = fade;

                Predicate<DebuffObj> debuffed = obj => obj.Id == player.Id;
                foreach (var obj in _debuffObjects.FindAll(debuffed))
                {
                    obj.D3DObject.Visible = false;
                    obj.D3DObject.Dispose();
                    _debuffObjects.Remove(obj);
                }

                int index = 0;
                foreach(DebuffInformation info in player.Debuffs)
                {
                    int spell = info.Spell;
                    if (info.MapDebuffToIcon(spell) != null && WorldObjectService.IsValidObject(player.Id))
                    {
                        int icon = (int)info.MapDebuffToIcon(spell);
                        D3DObj obj = CoreManager.Current.D3DService.MarkObjectWithIcon(player.Id, icon);
                        float dz = _globals.Host.Actions.Underlying.ObjectHeight(player.Id) + ((float)0 * 0.5f);
                        obj.Scale(0.5f);
                        obj.Anchor(player.Id, 0.2f, 0.0f, 0.0f, dz);
                        obj.PBounce = 0.0f;
                        obj.Autoscale = false;
                        obj.HBounce = 0.0f;
                        obj.OrientToCamera(true);
                        obj.POrbit = 2f;
                        obj.ROrbit = 0.5f;
                        obj.AnimationPhaseOffset =  index * ((2f / 8f));
                        obj.Visible = true;
                        DebuffObj debuffObj = _debuffObjFactory(
                            player.Id,
                            info.Spell,
                            icon,
                            obj);
                        
                        _debuffObjects.Add(debuffObj);
                        ++index;
                    }
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _enemyListView_Click(object sender, int row, int col)
        {
            try
            {
                _processListView_Clicked(_enemyListView, row, col);

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _friendlyListView_Click(object sender, int row, int col)
        {
            try
            {
                _processListView_Clicked(_friendlyListView, row, col);

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _processListView_Clicked(HudList listView, int row, int col)
        {
            try
            {
                HudList.HudListRowAccessor acc = listView[row];
                string name = ((HudStaticText)acc[1]).Text;
                Player player = _playerManager.GetByName(name);

                if (player == null)
                    return;

                _globals.Core.Actions.SelectItem(player.Id);

                if (player.Enemy)
                    return;

                if (col == 2)
                {
                    WorldObjectService.CastHeal(player.Id);
                }

                if (col == 3)
                {
                    
                   WorldObjectService.CastSpell(2082, player.Id);
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _playerManager_PlayerRemoved(object sender, Player player)
        {
            try
            {
                _processPlayerRemove(player);

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _processPlayerRemove(Player player)
        {
            PlayerIcon playerIcon = _playerIcons.Find(icon => icon.Id == player.Id);

            if (playerIcon != null) 
                playerIcon.Icon.Dispose();

            _playerIcons.Remove(playerIcon);
            HudList playersView = player.Enemy ? _enemyListView : _friendlyListView;
            for (int i = 0; i < playersView.RowCount; i++)
            {
                HudStaticText name = (HudStaticText)playersView[i][1];
                if (name.Text == player.Name)
                {
                    playersView.RemoveRow(i);
                }
            }
        }

        private void _playerManager_PlayerAdded(object sender, Player player)
        {
            try
            {
                _processPlayerAdd(player);

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void _processPlayerAdd(Player player)
        {
            bool enemy = player.Enemy;
            HudList playersView = enemy ? _enemyListView : _friendlyListView;
            int icon = enemy ? EnemyIcon : FriendlyIcon;
            D3DObj playerIcon = CoreManager.Current.D3DService.MarkObjectWithShape(player.Id, D3DShape.Sphere, Color.Red.ToArgb());
            playerIcon.Scale(enemy ? 0.3f : 0.3f);
            playerIcon.Anchor(player.Id, 0.2f, 0.0f, 0.0f, 2.5f);
            playerIcon.Color = enemy ? Color.FromArgb(200, Color.Red).ToArgb() : Color.FromArgb(220, Color.LightBlue).ToArgb();
            playerIcon.OrientToCamera(true);
            _playerIcons.Add(_playerIconFactory(player.Id, playerIcon));

            HudList.HudListRowAccessor row = playersView.AddRow();
            ((HudPictureBox)row[0]).Image = icon;
            ((HudStaticText)row[1]).Text = player.Name;
            ((HudStaticText)row[1]).TextColor = enemy ? Color.Red : Color.LightBlue;

            if (!enemy)
            {
                ((HudStaticText)row[1]).TextColor = Color.LightGreen;
                ((HudPictureBox) row[2]).Image = 100670841;
                ((HudPictureBox) row[3]).Image = 100670842;
            }
        }

        private void RelogDurationChange(object sender, EventArgs e)
        {
            try
            {
                if (Int32.TryParse(_relogDuration.Text, out int relogDuration) && relogDuration >= 1 && relogDuration <= 360)
                {
                    _logger.WriteToChat($"MainView.RelogDurationChange[EVENT]: {_relogDuration.Text}");
                    _settingsManager.Settings.RelogDuration = relogDuration;
                    _settingsManager.Write();
                }

            } catch (Exception ex) { _logger.Error(ex); }

        }

        private void RelogChange(object sender, EventArgs e)
        {
            try
            {
                _logger.WriteToChat($"MainView.RelogChange[EVENT]: {_relog.Checked}");
                _settingsManager.Settings.Relog = _relog.Checked;
                _settingsManager.Write();

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void VitaeLimitChange(object sender, EventArgs e)
        {
            try
            {
                if (Int32.TryParse(_vitaeLimit.Text, out int vitaeLimit) && vitaeLimit >= 5 && vitaeLimit <= 40)
                {
                    _logger.WriteToChat($"MainView.VitaeLimitChange[EVENT]: {_vitaeLimit.Text}");
                    _settingsManager.Settings.VitaeLimit = vitaeLimit;
                    _settingsManager.Write();
                }

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void LogOnVitaeChange(object sender, EventArgs e)
        {
            try
            {
                _logger.WriteToChat($"MainView.LogOnVitaeChange[EVENT]: {_logOnVitae.Checked}");
                _settingsManager.Settings.LogOnVitae = _logOnVitae.Checked;
                _settingsManager.Write();

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void LogOnDeathChange(object sender, EventArgs e)
        {
            try
            {
                _logger.WriteToChat($"MainView.LogOnDeathChange[EVENT]: {_logOnDeath.Checked}");
                _settingsManager.Settings.LogOnDeath = _logOnDeath.Checked;
                _settingsManager.Write();

            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void DebugChange(object sender, EventArgs e)
        {
            try
            {
                _logger.WriteToChat($"MainView.DebugChange[EVENT]: {_debug.Checked}");

                _debugger.Toggle();
                _settingsManager.Settings.Debug = _debug.Checked;
                _settingsManager.Write();

            } catch (Exception ex) { _logger.Error(ex); }
        }

        new public void Dispose()
        {
            base.Dispose();
            UnRegisterEvents();
            _clearIcons();
            _clearDebuffObjects();
        }

        private void _clearDebuffObjects()
        {
            foreach(DebuffObj debuffObj in _debuffObjects)
            {
                debuffObj.D3DObject.Dispose();
            }

            _debuffObjects.Clear();
        }

        private void _clearIcons()
        {
            foreach(PlayerIcon playerIcon in _playerIcons)
            {
                playerIcon.Icon.Dispose();
            }

            _playerIcons.Clear();
        }
    }
}
