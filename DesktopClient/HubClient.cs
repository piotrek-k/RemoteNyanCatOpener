using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient
{
    class HubClient
    {
        private TextWriter _traceWriter;
        public IHubProxy _hubProxy;

        public HubClient(TextWriter traceWriter)
        {
            _traceWriter = traceWriter;
        }

        public async Task RunAsync(string url)
        {
            var connection = new HubConnection(url);
            //connection.TraceWriter = _traceWriter;

            _hubProxy = connection.CreateHubProxy("MyHub1");

            _hubProxy.On<string[]>("clientMessage", (data) =>
            {
                //_traceWriter.WriteLine(data[0] + ": " + data[1]);
                Program.chatMessage(data[0] + ": " + data[1]);
            });

            _hubProxy.On<string[]>("runExe", (data) =>
            {
                //_traceWriter.WriteLine("Uruchamianie " + data[1] + " przez admina " + data[0]);
                Program.serverMessage("Uruchamianie " + data[1] + " przez admina " + data[0]);
                if (data[0] == Program.DaneAplikacji.Admin || Program.DaneAplikacji.Admin == "admin")
                {
                    System.Diagnostics.Process.Start(data[1]);
                }
            });

            _hubProxy.On<string>("serverResponse", (data) =>
            {
                //_traceWriter.WriteLine("***Odpowiedz z serwera: " + data);
                Program.serverMessage("***Odpowiedz z serwera: " + data);
            });

            _hubProxy.On<string>("someoneAsksToBeAdmin", (data) =>
            {
                Program.serverMessage("*************************");
                Program.serverMessage(data + " chce byc adminem. Admin moze uruchamiac skrypt na twoim komputerze. Wpisz 'tak' jesli tego chcesz, inna odpowiedz odrzuci pytanie");
                //Program.command = "";
                string wantsToBeAdmin = Console.ReadLine();
                if (wantsToBeAdmin == "tak")
                {
                    Program.DaneAplikacji.Admin = data;
                    Program.serverMessage(data + " zostal adminem");
                    _hubProxy.Invoke("notifyUserHeBecomeAdmin", Program.DaneAplikacji.Admin);
                }
                else
                {
                    Program.serverMessage("Odrzucono");
                }
                Program.serverMessage("*************************");
            });

            _hubProxy.On<string>("youveBecomeAdmin", (data) =>
            {
                _traceWriter.WriteLine(data + " zaakceptowal cie jako admina");
            });

            await connection.Start();
            await _hubProxy.Invoke("serverMessage", "***New Computer connected");
        }
    }
}
