using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace Commander.Lib.Services
{
    public static class WorldObjectService
    {
        public static double GetDistanceFromPlayer(int src, int dest)
        {
            return CoreManager.Current.WorldFilter.Distance(src, dest) * 240;
        }

        public static bool IsValidObject(int id)
        {
            return CoreManager.Current.Actions.IsValidObject(id);
        }

        public static bool IsSpellKnown(int id)
        {
            return CoreManager.Current.CharacterFilter.IsSpellKnown(id);
        }

        public static bool IsPlayer(int id)
        {
            return CoreManager.Current.WorldFilter[id].ObjectClass == ObjectClass.Player;
        }

        public static WorldObject GetWorldObject(int id)
        {
            return CoreManager.Current.WorldFilter[id];
        }

        public static void Logout()
        {
            CoreManager.Current.Actions.Logout();
        }

        public static int GetVitae()
        {
            return CoreManager.Current.CharacterFilter.Vitae;
        }

        public static CharacterFilter GetSelf()
        {
            return CoreManager.Current.CharacterFilter;
        }

        public static void RequestId(int id)
        {
            CoreManager.Current.Actions.RequestId(id);
        }

        public static void SelectItem(int id)
        {
            CoreManager.Current.Actions.SelectItem(id);
        }
        public static void CastSpell(int spell, int playerId)
        {
            CoreManager.Current.Actions.CastSpell(spell, playerId);
        }

        public static void CastHeal(int playerId)
        {
            if (WorldObjectService.IsSpellKnown(4310))
            {
                CastSpell(4310, playerId);
            } else
            {
                CastSpell(2072, playerId);
            }
        }
    }
}
