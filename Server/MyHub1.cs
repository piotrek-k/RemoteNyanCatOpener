using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Server
{
    public class MyHub1 : Hub
    {
        static Dictionary<string, string> users = new Dictionary<string, string>();

        public void serverMessage(string message)
        {
            Clients.Others.clientMessage(new string[] { users[Context.ConnectionId], message });
        }

        public void openExeEverywhere(string path)
        {
            if (!(path.Contains(@"\") || path.Contains(@"/"))) //zabezpieczenie przed poruszaniem sie po drzewie plikow
            {
                Clients.Others.runExe(new string[] { users[Context.ConnectionId], path });
            }
            else
            {
                Clients.Caller.serverResponse(@"Znaki \ i / sa niedozwolone");
            }
        }

        public void closeExeEverywhere()
        {
            Clients.Others.closeExe(users[Context.ConnectionId]);
        }


        public void setNickname(string name)
        {
            if (name == "admin" && users.Any(x => x.Value == name))
            {
                Clients.Caller.serverResponse("Nazwa uzytkownika jest juz zajeta");
            }
            else
            {
                users[Context.ConnectionId] = name;
                Clients.Caller.serverResponse("Od teraz nazywasz sie " + users[Context.ConnectionId]);
                if(name == "chuj")
                {
                    Clients.Caller.serverResponse("Milej zabawy... chuju :]");
                }
            }
        }

        public void askToBeAdmin()
        {
            Clients.Others.someoneAsksToBeAdmin(users[Context.ConnectionId]);
        }

        public void notifyUserHeBecomeAdmin(string nazwaAdmina)
        {
            var key = users.FirstOrDefault(x => x.Value == nazwaAdmina).Key;
            if (key != null)
            {
                Clients.Client(key).youveBecomeAdmin(users[Context.ConnectionId]);
            }
        }

        public override Task OnConnected()
        {
            Random rnd1 = new Random();
            string newUserName;
            do
            {
                newUserName = "User" + rnd1.Next(0, 1000);
            }
            while (users.Any(x => x.Value == newUserName) && users.Count < 999);

            if (users.Count >= 999)
            {
                newUserName = "UserX";
            }

            users.Add(Context.ConnectionId, newUserName);
            Clients.Client(Context.ConnectionId).serverResponse(users.Count + " uzytkownikow online. Uzywajac tego programu uroczyscie przysiegasz ze knujesz cos niedobrego :]");

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            //stopCalled: true - user closed pc client, false - lost connection
            string name = users[Context.ConnectionId];
            users.Remove(Context.ConnectionId);
            Clients.All.serverResponse("Uzytkownik " + name + " rozlaczony. Aktualnie " + users.Count + " komputerow online.");

            return base.OnDisconnected(stopCalled);
        }
    }
}