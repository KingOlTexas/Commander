namespace Commander.Models
{
    public class Settings
    {
        public bool Debug = false;
        public bool LogOnDeath = true;
        public bool LogOnVitae = true;
        public bool Relog = false;
        public bool EnemySounds = true;
        public bool FriendlySounds = true;
        public int RelogDuration = 5;
        public int VitaeLimit = 10;

        public delegate Settings Factory();
    }
}
