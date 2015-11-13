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
            Clients.Others.runExe(new string[] { users[Context.ConnectionId], path });
        }

        public void setNickname(string name)
        {
            if (users.Any(x => x.Value == name))
            {
                Clients.Caller.serverResponse("Nazwa uzytkownika jest juz zajeta");
            }
            else
            {
                users[Context.ConnectionId] = name;
                Clients.Caller.serverResponse("Od teraz nazywasz sie " + users[Context.ConnectionId]);
            }
        }

        public void askToBeAdmin()
        {
            Clients.Others.someoneAsksToBeAdmin(users[Context.ConnectionId]);
        }

        public void notifyUserHeBecomeAdmin(string nazwaAdmina)
        {
            var key = users.FirstOrDefault(x => x.Value == nazwaAdmina).Key;
            if(key != null)
            {
                Clients.Client(key).youveBecomeAdmin(users[Context.ConnectionId]);
            }
        }

        public override Task OnConnected()
        {
            users.Add(Context.ConnectionId, "PC");

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            //stopCalled: true - user closed pc client, false - lost connection
            users.Remove(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }
    }
}