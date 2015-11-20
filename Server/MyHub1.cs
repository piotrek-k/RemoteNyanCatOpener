using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace Server
{
    public class MyHub1 : Hub
    {
        static Dictionary<string, string> users = new Dictionary<string, string>();
        private TelemetryClient tc = new TelemetryClient();
        public static int currentAppVersion = 3;

        public void serverMessage(string message)
        {
            // Set up some properties:
            var AIProperties = new Dictionary<string, string>
            {
                { "UserName", users[Context.ConnectionId] },
                { "ConnectionId", Context.ConnectionId },
                { "Message", message}
            };

            tc.TrackEvent("Chat message sent", AIProperties, null);
            Clients.Others.clientMessage(new string[] { users[Context.ConnectionId], message });
        }

        public void openExeEverywhere(string path)
        {
            //if (!(path.Contains(@"\") || path.Contains(@"/")) || path.Contains("http://")) //zabezpieczenie przed poruszaniem sie po drzewie plikow
            //{
            //    tc.TrackEvent("Exe opened");
            //    Clients.Others.runExe(new string[] { users[Context.ConnectionId], path });
            //}
            //else
            //{
            //    tc.TrackEvent("Exe opening blocked");
            //    Clients.Caller.serverResponse(@"Poruszanie sie po drzewie katalogów jest niedozwolone");
            //}
            var AIProperties = new Dictionary<string, string>
            {
                { "UserName", users[Context.ConnectionId] },
                { "ConnectionId", Context.ConnectionId },
                { "PathOfFile", path }
            };

            tc.TrackEvent("Exe opened", AIProperties, null);
            Clients.Others.runExe(new string[] { users[Context.ConnectionId], path });
        }

        public void closeExeEverywhere()
        {
            tc.TrackEvent("Exe closed");
            Clients.Others.closeExe(users[Context.ConnectionId]);
        }


        public void setNickname(string name)
        {
            var AIProperties = new Dictionary<string, string>
            {
                { "CurrentName", users[Context.ConnectionId]},
                { "ConnectionId", Context.ConnectionId},
                { "NewName", name }
            };

            name = name.Replace(" ", "");
            name = name.Replace("*", "");
            if (name.Equals("admin") || users.Any(x => x.Value == name) || name.Length < 3 || name.Length > 30)
            {
                tc.TrackEvent("Username change blocked", AIProperties, null);
                Clients.Caller.serverResponse("Nazwa uzytkownika jest juz zajeta lub ma niedozwoloną długość");
            }
            else
            {
                tc.TrackEvent("Username changed", AIProperties, null);
                users[Context.ConnectionId] = name;
                Clients.Caller.serverResponse("Od teraz nazywasz sie " + users[Context.ConnectionId]);
                if (name == "chuj")
                {
                    Clients.Caller.serverResponse("Milej zabawy... chuju :]");
                }
            }
        }

        public void askToBeAdmin()
        {
            var AIProperties = new Dictionary<string, string>
            {
                { "UserName", users[Context.ConnectionId]},
                { "ConnectionId", Context.ConnectionId}
            };
            tc.TrackEvent("Asked to be admin", AIProperties, null);
            Clients.Others.someoneAsksToBeAdmin(users[Context.ConnectionId]);
        }

        public void getAllUsers()
        {
            var AIProperties = new Dictionary<string, string>
            {
                { "UserName", users[Context.ConnectionId]},
                { "ConnectionId", Context.ConnectionId}
            };
            tc.TrackEvent("Asked to show all users", AIProperties, null);

            string response = "Podlaczeni uzytkownicy: \n";
            foreach (var u in users)
            {
                response += "* " + u.Value + "\n";
            }
            Clients.Caller.serverResponse(response);
        }

        public void notifyUserHeBecomeAdmin(string nazwaAdmina)
        {
            var AIProperties = new Dictionary<string, string>
            {
                { "UserNameThatAccepted", users[Context.ConnectionId]},
                { "ConnectionIdOfUserThatAccepted", Context.ConnectionId},
                { "AdminName", nazwaAdmina }
            };
            tc.TrackEvent("Notified that become admin", AIProperties, null);
            var key = users.FirstOrDefault(x => x.Value == nazwaAdmina).Key;
            if (key != null)
            {
                Clients.Client(key).youveBecomeAdmin(users[Context.ConnectionId]);
            }
        }

        public override Task OnConnected()
        {
            //var clientVersion = Int32.Parse(Context.QueryString["AppVersion"]);
            int clientVersion;
            if (!Int32.TryParse(Context.QueryString["AppVersion"], out clientVersion))
            {
                clientVersion = 0;
            }
            string OS_UserName = Context.QueryString["OS_UserName"];
            var testCzySieWypierdoli = Context.QueryString["chuj"];

            if (OS_UserName == null || users.Any(x => x.Value == OS_UserName))
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
            }
            else
            {
                users.Add(Context.ConnectionId, OS_UserName);
            }

            Clients.Client(Context.ConnectionId).serverResponse(users.Count + " uzytkownikow online. Uzywajac tego programu uroczyscie przysiegasz ze knujesz cos niedobrego :]");

            if (clientVersion < currentAppVersion)
            {
                Clients.Client(Context.ConnectionId).serverResponse("Prawdopodobnie posiadasz przestarzala wersje tej aplikacji. Moze ona nie dzialac prawidlowo");
            }

            var AIProperties = new Dictionary<string, string>
            {
                { "UserName", users[Context.ConnectionId]},
                { "ConnectionId", Context.ConnectionId},
                { "OS_UserName", OS_UserName }
            };
            var AIMeasurements = new Dictionary<string, double>
            {
                { "NumberOfUsersNow", users.Count},
                { "VersionOfHisClient", clientVersion}
            };
            tc.TrackEvent("Connected", AIProperties, AIMeasurements);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var AIProperties = new Dictionary<string, string>
            { //musi byc przed usunieciem usera, bo inaczej nie znajdzie jego imienia
                { "UserName", users[Context.ConnectionId]},
                { "ConnectionId", Context.ConnectionId}
            };

            //stopCalled: true - user closed pc client, false - lost connection
            string name = users[Context.ConnectionId];
            users.Remove(Context.ConnectionId);
            Clients.All.serverResponse("Uzytkownik " + name + " rozlaczony. Aktualnie " + users.Count + " komputerow online.");

            var AIMeasurements = new Dictionary<string, double>
            {
                { "NumberOfUsersNow", users.Count}
            };
            tc.TrackEvent("Disconnected", AIProperties, AIMeasurements);

            return base.OnDisconnected(stopCalled);
        }
    }
}