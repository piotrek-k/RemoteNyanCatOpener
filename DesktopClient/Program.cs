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
        public int VersionOfApp = 6;
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
            while (true)
            {
                string password = Console.ReadLine();
                if (password == "tmm")
                {
                    break;
                }
                else if (password == "qs")
                {
                    command = "qsi";
                    break;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            if (command != "qsi") { 
                Console.WriteLine("Bezprzewodowy Uruchamiacz NyanCata v" + DaneAplikacji.VersionOfApp);
                Console.WriteLine("Aby zakonczyc wykonywanie komendy, wpisz 'exit'");
                Console.WriteLine("Dostepne komendy:");
                Console.WriteLine("'chat' - komunikacja z reszta komputerow");
                Console.WriteLine("'startexe' - uruchom plik exe na innych komputerach (tych ktore zaakcpetuja cie jako admina)");
                Console.WriteLine("'closeexe' - zamknij poprzednio otwarty plik exe");
                Console.WriteLine("'beadmin' - zapytaj innych czy mozesz byc adminem");
                Console.WriteLine("'username' - zmien swoja nazwe");
                Console.WriteLine("'hide' - ukryj konsole. DownArrow+Escape+Backspace - wyswietl ja ponownie");
                Console.WriteLine("'hardcoremode' - wylacz autoryzacje admina, kazdy moze uruchomic ci startexe");
                Console.WriteLine("'qs' - quick setup - hardcoremode + hide");
            }
            Console.WriteLine("**********************");
            Console.WriteLine("Oczekiwanie na polaczenie z serwerem... (jesli nie pojawia sie czerwony komunikat, wejdz na Centrum Dowodzenia w przegladarce aby uruchomic serwer)");
            Console.WriteLine("**********************");
            Console.ResetColor();

            var handle = GetConsoleWindow();

            DaneAplikacji da = new DaneAplikacji();
#if DEBUG
            var url = "http://localhost:50043/";
            Console.WriteLine("Connecting to local server");
#else
            var url = "http://remotenyancatopener.azurewebsites.net/";
#endif
            var writer = Console.Out;
            var hubClient = new HubClient(writer);
            hubClient.RunAsync(url).Wait();

            //string command = "";
            while (command != "exit")
            {
                if (consoleHidden)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.DownArrow)
                    {
                        while (true)
                        {
                            if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                            {
                                while (true)
                                {
                                    if (Console.ReadKey(true).Key == ConsoleKey.Backspace)
                                    {
                                        ShowWindow(handle, SW_SHOW);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                if (command != "qsi")
                {
                    Console.WriteLine("Wpisz komende: ");
                    command = Console.ReadLine();
                }
                if (command == "chat")
                {
                    string text;
                    do
                    {
                        Console.Write("chat: ");
                        text = Console.ReadLine();

                        hubClient._hubProxy.Invoke("serverMessage", text);
                    }
                    while (text != "exit");
                }
                if (command == "startexe")
                {
                    Console.Write("Podaj nazwe pliku: ");
                    string path = Console.ReadLine();
                    hubClient._hubProxy.Invoke("openExeEverywhere", path);
                }
                if(command == "closeexe")
                {
                    hubClient._hubProxy.Invoke("closeExeEverywhere");
                }
                if (command == "beadmin")
                {
                    Console.WriteLine("Wyslano zapytanie");
                    hubClient._hubProxy.Invoke("askToBeAdmin");
                }
                if (command == "username")
                {
                    Console.Write("Podaj nowa nazwe uzytkownika: ");
                    string name = Console.ReadLine();
                    hubClient._hubProxy.Invoke("setNickname", name);
                    Console.WriteLine("Zapytanie wyslane, poczekaj na komunikat z serwera");
                }
                if (command == "hardcoremode" || command == "qs" || command == "qsi")
                {
                    DaneAplikacji.Admin = "admin";
                }
                if (command == "hide" || command == "qs" || command == "qsi")
                {
                    ShowWindow(handle, SW_HIDE);
                    consoleHidden = true;
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