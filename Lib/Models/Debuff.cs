using Decal.Adapter.Wrappers;
using System;

namespace Commander.Lib.Models
{
    public enum Debuff : int
    {
        COORDINATION = 9,
        QUICKNESS = 13,
        FIREVULN = 44,
        PIERCEVULN = 46,
        BLADEVULN = 48,
        ACIDVULN = 50,
        FROSTVULN = 52,
        LIGHTNINGVULN = 54,
        BLUDGEVULN = 56,
        IMPERIL = 56
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

        public DebuffIcon? MapDebuffToIcon(int spell)
        {
            if (spell == 44)
                return DebuffIcon.FIRE;
            if (spell == 46)
                return DebuffIcon.PIERCE;
            if (spell == 48)
                return DebuffIcon.BLADE;
            if (spell == 50)
                return DebuffIcon.ACID;
            if (spell == 52)
                return DebuffIcon.COLD;
            if (spell == 54)
                return DebuffIcon.LIGHTNING;
            if (spell == 56)
                return DebuffIcon.IMPERIL;

            return null;
        }
    }

    public class DebuffObj
    {
        public int Id;
        public int Spell;
        public int Icon;
        public D3DObj D3DObject;

        public delegate DebuffObj Factory(int id, int spell, int icon, D3DObj d3dObj);

        public DebuffObj(int id, int spell, int icon, D3DObj d3dObj)
        {
            Id = id;
            Spell = spell;
            Icon = icon;
            D3DObject = d3dObj;
        }
    }

    public enum DebuffIcon
    {
        IMPERIL = 100686629,
        ACID = 100686625,
        BLADE = 100686683,
        BLUDGEON = 100686637,
        COLD = 100686654,
        FIRE = 100686649,
        LIGHTNING = 100686667,
        PIERCE = 100686678,
        VULNERABILITY = 100686675,
        COORDINATION = 100686641,
        QUICKNESS = 100686680,
        MELEE_PREP = 100688919,
        NULL = 100692970,
    }
}

