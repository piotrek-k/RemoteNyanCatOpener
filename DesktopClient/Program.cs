using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DesktopClient
{
    public class DaneAplikacji
    {
        public string Admin { get; set; }
    }

    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public static DaneAplikacji DaneAplikacji = new DaneAplikacji();
        public static string command = "";
        public static bool consoleHidden = false;

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();

            DaneAplikacji da = new DaneAplikacji();
            var url = "http://remotenyancatopener.azurewebsites.net/";
            var writer = Console.Out;
            var hubClient = new HubClient(writer);
            hubClient.RunAsync(url).Wait();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bezprzewodowy Uruchamiacz NyanCata v1");
            Console.WriteLine("Aby zakonczyc wykonywanie komendy, wpisz 'exit'");
            Console.WriteLine("Dostepne komendy:");
            Console.WriteLine("'chat' - komunikacja z reszta komputerow");
            Console.WriteLine("'startexe' - uruchom plik exe na innych komputerach (tych ktore zaakcpetuja cie jako admina)");
            Console.WriteLine("'beadmin' - zapytaj innych czy mozesz byc adminem");
            Console.WriteLine("'username' - zmien swoja nazwe");
            Console.WriteLine("'hide' - ukryj konsole. F3+Escape - wyswietl ja ponownie");
            Console.WriteLine("'hardcoremode' - wylacz autoryzacje admina, kazdy moze uruchomic ci startexe");
            Console.ResetColor();

            //string command = "";
            while (command != "exit")
            {
                if (Console.ReadKey(true).Key == ConsoleKey.F3 && consoleHidden)
                {
                    while (true)
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        {
                            ShowWindow(handle, SW_SHOW);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                Console.WriteLine("Wpisz komende: ");
                command = Console.ReadLine();
                if (command == "chat")
                {
                    //Console.WriteLine("Wpisz wiadomosc do przeslania");

                    string text;
                    do
                    {
                        Console.Write("chat: ");
                        text = Console.ReadLine();

                        hubClient._hubProxy.Invoke("serverMessage", text);
                    }
                    while (text != "exit");
                }
                else if (command == "startexe")
                {
                    Console.Write("Podaj nazwe pliku: ");
                    string path = Console.ReadLine();
                    hubClient._hubProxy.Invoke("openExeEverywhere", path);
                }
                else if (command == "beadmin")
                {
                    Console.WriteLine("Wyslano zapytanie");
                    hubClient._hubProxy.Invoke("askToBeAdmin");
                }
                else if (command == "username")
                {
                    Console.Write("Podaj nowa nazwe uzytkownika: ");
                    string name = Console.ReadLine();
                    hubClient._hubProxy.Invoke("setNickname", name);
                    Console.WriteLine("Zapytanie wyslane, poczekaj na komunikat z serwera");
                }
                else if (command == "hide")
                {
                    ShowWindow(handle, SW_HIDE);
                    consoleHidden = true;
                }
                else if(command == "hardcoremode")
                {
                    DaneAplikacji.Admin = "admin";
                }
            }
        }
        public static void serverMessage(string text)
        {
            //Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void chatMessage(string text)
        {
            //Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("chat: " + text);
            Console.ResetColor();
        }
    }
}