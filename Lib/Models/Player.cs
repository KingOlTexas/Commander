using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commander.Lib.Models
{
    public class Player : EventArgs
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool Enemy { get; private set; }
        public bool LowHealth { get; set; }
        public int MonarchId { get; private set; }
        public string MonarchName { get; private set; }
        public List<DebuffInformation> Debuffs;

        public delegate Player Factory(WorldObject player, bool enemy);

        public Player(WorldObject player, bool enemy)
        {
            Id = player.Id;
            Enemy = enemy;
            Name = player.Name;
            MonarchId = player.Values(LongValueKey.Monarch);
            MonarchName = player.Values(StringValueKey.MonarchName);
            LowHealth = false;
            Debuffs = new List<DebuffInformation>();
        }
    }

    public class PlayerIcon
    {
        public readonly int Id;
        public readonly D3DObj Icon;
        public PlayerIcon(int id, D3DObj icon)
        {
            Id = id;
            Icon = icon;
        }

        public delegate PlayerIcon Factory(int id, D3DObj icon);
    }

    public class DebuffInformation
    {
        public int Spell { get; set; }
        public DateTime StartTime;
        public delegate DebuffInformation Factory(int spell, DateTime startTime);

        public DebuffInformation(int spell, DateTime startTime)
        {
            Spell = spell;
            StartTime = startTime;
        }

    }
}
