using System;
using System.Collections.Generic;
using System.Linq;
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
        public static DaneAplikacji DaneAplikacji = new DaneAplikacji();
        public static string command = "";

        static void Main(string[] args)
        {
            DaneAplikacji da = new DaneAplikacji();
            var url = "http://localhost:50043/";
            var writer = Console.Out;
            var hubClient = new HubClient(writer);
            hubClient.RunAsync(url).Wait();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bezprzewodowy Uruchamiacz NyanCata v1");
            Console.WriteLine("Aby zakonczyc wykonywanie komendy, wpisz 'exit'");
            Console.WriteLine("Dostepne komendy:");
            Console.WriteLine("'chat' - komunikacja z reszta komputerow");
            Console.WriteLine("'startexe' - uruchom plik exe na innych komputerach (musisz byc adminem)");
            Console.WriteLine("'beadmin' - zapytaj innych czy mozesz byc adminem");
            Console.WriteLine("'username' - zmien swoja nazwe");
            Console.ResetColor();

            //string command = "";
            while (command != "exit")
            {
                Console.WriteLine("Wpisz komende: ");
                command = Console.ReadLine();
                if (command == "chat")
                {
                    Console.WriteLine("Wpisz wiadomosc do przeslania");

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