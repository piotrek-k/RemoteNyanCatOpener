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
        private System.Diagnostics.Process process;

        public HubClient(TextWriter traceWriter)
        {
            _traceWriter = traceWriter;
            //process = new System.Diagnostics.Process();
        }

        public async Task RunAsync(string url)
        {
            var querystringData = new Dictionary<string, string>();
            querystringData.Add("AppVersion", "" + Program.DaneAplikacji.VersionOfApp);
            querystringData.Add("OS_UserName", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            var connection = new HubConnection(url, querystringData);
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
                if (data[0] == Program.DaneAplikacji.Admin || Program.DaneAplikacji.Admin == "admin")
                {
                    Program.serverMessage("Uruchamianie " + data[1] + " przez admina " + data[0]);
                    try { 
                        process = System.Diagnostics.Process.Start(data[1]);
                    }
                    catch (Exception e)
                    {
                        _hubProxy.Invoke("serverMessage", "Proba uruchomienia " + data[1] + " przez " + data[0] + " nie udała się. Błąd: " + e.Message);
                    }
                    if(data[2] == true.ToString())
                    {
                        _hubProxy.Invoke("serverMessage", "Melduje wykonanie zadania :] Launcher zamkniety.");
                        System.Environment.Exit(1);
                    }
                }
                else
                {
                    Program.serverMessage(data[0] + " probowal uruchomic plik " + data[1] + ". Nie ma do tego uprawnien.");
                }
            });

            _hubProxy.On<string>("closeExe", (data) =>
            {
                if (data == Program.DaneAplikacji.Admin || Program.DaneAplikacji.Admin == "admin")
                {
                    Program.serverMessage("Zamykanie pliku przez" + data);
                    try { 
                        process.CloseMainWindow();
                        process.Close();
                    }
                    catch (Exception e)
                    {
                        _hubProxy.Invoke("serverMessage", "Proba zamkniecia ostatnio otwartej aplikacji przez " + data[0] + " nie udała się. Błąd: " + e.Message);
                    }
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
