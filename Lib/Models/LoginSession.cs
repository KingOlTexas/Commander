namespace Commander.Models
{
    public class LoginSession
    {
        public int Id;
        public int Vitae;
        public int Health;
        public int LoginStatus;
        public int ServerPopulation;
        public int Monarch;
        public string MonarchName;
        public string Server;
        public string Name;
        public string AccountName;

        public delegate LoginSession Factory(
            int id,
            int vitae,
            int health,
            int loginStatus,
            int serverPopulation,
            int monarch,
            string monarchName,
            string server,
            string name,
            string accountName);

        public LoginSession(
            int id,
            int vitae,
            int health,
            int loginStatus,
            int serverPopulation,
            int monarch,
            string monarchName,
            string server,
            string name,
            string accountName)
        {
            Id = id;
            Vitae = vitae;
            Health = health;
            LoginStatus = loginStatus;
            ServerPopulation = serverPopulation;
            Monarch = monarch;
            MonarchName = monarchName;
            Server = server;
            Name = name;
            AccountName = accountName;
        }
    }
}
