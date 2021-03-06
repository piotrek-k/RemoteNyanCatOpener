﻿using Microsoft.AspNet.SignalR.Client;
using Server.HubModels;
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
            if (Program.DaneAplikacji.CurrentUserName != "incognito")
            {
                querystringData.Add("OS_UserName", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                Program.DaneAplikacji.CurrentUserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
            var connection = new HubConnection(url, querystringData);
            //connection.TraceWriter = _traceWriter;

            _hubProxy = connection.CreateHubProxy("MyHub1");

            _hubProxy.On<string[]>("clientMessage", (data) =>
            {
                //_traceWriter.WriteLine(data[0] + ": " + data[1]);
                Program.chatMessage(data[0] + ": " + data[1]);
            });

            _hubProxy.On<RunExe>("runExe", (data) =>
            {
                //RunExe data = JsonConvert.DeserializeObject<RunExe>(json);
                process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                if (data.HideOpenedFile)
                {
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }
                //startInfo.Arguments = data.Arguments;

                if (data.UserName == Program.DaneAplikacji.Admin || Program.DaneAplikacji.Admin == "admin")
                {
                    Program.serverMessage("Uruchamianie " + data.Path + " przez admina " + data.UserName + " z nastepujacymi argumentami: '" + data.Arguments + "'. " + (data.HideOpenedFile ? "Plik bedzie ukryty." : ""));
                    try
                    {
                        startInfo.FileName = data.Path;
                        startInfo.Arguments = data.Arguments;
                        process = System.Diagnostics.Process.Start(startInfo);
                    }
                    catch (Exception e)
                    {
                        _hubProxy.Invoke("serverMessage", "Proba uruchomienia " + data.Path + " przez " + data.UserName + " nie udała się. Błąd: " + e.Message);
                    }
                    if (data.ThenCloseLauncher)
                    {
                        _hubProxy.Invoke("serverMessage", "Melduje wykonanie zadania :] Launcher zostanie zamkniety za 0.5 sec.");
                        System.Threading.Thread.Sleep(1000);
                        System.Environment.Exit(1);
                    }
                }
                else
                {
                    Program.serverMessage(data.UserName + " probowal uruchomic plik " + data.Path + ". Nie ma do tego uprawnien.");
                    _hubProxy.Invoke("serverMessage", data.UserName + " probowal uruchomic plik " + data.Path + ". Nie ma do tego uprawnien.");
                }
            });

            _hubProxy.On<string>("closeExe", (data) =>
            {
                if (data == Program.DaneAplikacji.Admin || Program.DaneAplikacji.Admin == "admin")
                {
                    Program.serverMessage("Zamykanie pliku przez" + data);
                    try
                    {
                        process.CloseMainWindow();
                        process.Close();
                    }
                    catch (Exception e)
                    {
                        _hubProxy.Invoke("serverMessage", "Proba zamkniecia ostatnio otwartej aplikacji przez " + data[0] + " nie udała się. Błąd: " + e.Message);
                    }
                }
            });

            _hubProxy.On<CreateFile>("createFile", (data) =>
            {
                //CreateFile data = JsonConvert.DeserializeObject<CreateFile>(json);
                //string path = @"E:\AppServ\Example.txt";
                try
                {
                    if (data.UserName == Program.DaneAplikacji.Admin || Program.DaneAplikacji.Admin == "admin")
                    {
                        if (!File.Exists(data.Path))
                        {
                            File.Create(data.Path).Close();
                            TextWriter tw = new StreamWriter(data.Path);
                            tw.WriteLine(data.Content);
                            tw.Close();
                        }
                        else if (File.Exists(data.Path))
                        {
                            TextWriter tw = new StreamWriter(data.Path, false);
                            tw.WriteLine(data.Content);
                            tw.Close();
                        }
                        _hubProxy.Invoke("serverMessage", data.UserName + " utworzył nowy plik o sciezce: '" + data.Path + "' i zawartosci: '" + data.Content + "' u uzytkownika: " + Program.DaneAplikacji.CurrentUserName);
                    }
                    else
                    {
                        _hubProxy.Invoke("serverMessage", data.UserName + " nie ma uprawnien do utworzenia nowego pliku u uzytkownika " + Program.DaneAplikacji.CurrentUserName);
                    }
                }
                catch (Exception e)
                {
                    _hubProxy.Invoke("serverMessage", "Nie mozna utworzyc pliku. Blad: " + e.Message);
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
            //await _hubProxy.Invoke("serverMessage", "***New Computer connected");
        }
    }
}
