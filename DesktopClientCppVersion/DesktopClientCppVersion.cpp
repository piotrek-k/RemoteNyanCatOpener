#include "stdafx.h"
#include <stdlib.h>
#include <iostream>
#include <sstream>
#include "signalrclient\hub_connection.h"
#include "cpprest\json.h"
#include "cpprest\http_client.h"
#include <ppl.h>
#include <windows.h>
#include <iostream>
#include <fstream>

using namespace web;
using namespace web::http;
using namespace web::http::client;

signalr::hub_proxy GlobalProxy;
struct stop_now_t { };
//Process^ myProcess = gcnew Process;

int StringToWString(std::wstring &ws, const std::string &s)
{
	std::wstring wsTmp(s.begin(), s.end());

	ws = wsTmp;

	return 0;
}

void send_message(signalr::hub_proxy proxy, const utility::string_t& message)
{
	web::json::value args{};
	args[0] = web::json::value::string(message);
	//args[1] = web::json::value(message);

	// if you get an internal compiler error uncomment the lambda below or install VS Update 4
	proxy.invoke<void>(U("serverMessage"), args/*, [](const web::json::value&){}*/)
		.then([](pplx::task<void> invoke_task)  // fire and forget but we need to observe exceptions
	{
		try
		{
			invoke_task.get();
		}
		catch (const std::exception &e)
		{
			ucout << U("Error while sending data: ") << e.what();
		}
	});
}

void connections_from_server(signalr::hub_proxy proxy)
{
	proxy.on(U("clientMessage"), [](json::value obj)
	{
		//wykrzacza sie przy polskich literach
		/*try {
			std::wcout << "Message Deployed \n";
			std::wstring name = obj.at(0).as_array()[0].as_string();
			std::wstring message = obj.at(0).as_array()[1].as_string();
			std::wcout << "chat: " << name << ": " << message << "\n";
		}
		catch (const std::exception& ex) {
			std::wcout << "\n";
		}*/
	});

	proxy.on(U("runExe"), [](web::json::value obj)
	{
		//Process^ myProcess = gcnew Process;
		std::wcout << "RunExe Deployed";
		try {
			utility::string_t userName = obj[0][U("UserName")].as_string();
			std::wstring path = obj[0][U("Path")].as_string();
			std::wstring args = obj[0][U("Arguments")].as_string();
			bool close = obj[0][U("ThenCloseLauncher")].as_bool();
			bool hide = obj[0][U("HideOpenedFile")].as_bool();
			std::wcout << "Data: User: " << userName << " Path:" << path << " Close: " << close << " Hide: " << hide << "\n";
			LPCWSTR lpcPath = path.c_str();
			LPCWSTR lpcArgs = args.c_str();
			//CreateProcess(path.c_str(), NULL, NULL, NULL, FALSE, 0, NULL, NULL, NULL, NULL);
			//ShellExecuteW(GetDesktopWindow(), "open", path, NULL, NULL, SW_SHOW);
			ShellExecuteW(GetDesktopWindow(), NULL, lpcPath, lpcArgs, NULL, !hide);

			std::wstring closeString = close ? L"true" : L"false";
			std::wstring hideString = hide ? L"true" : L"false";
			send_message(GlobalProxy, L"Uruchomiono " + path + L" z argumentami " + args + L" Close: " + closeString + L" Hide: " + hideString + L" UserName: " + userName);

			if (close) {
				send_message(GlobalProxy, L"Zamykanie");
				std::wcout << "Czekam na zamkniecie";
				Sleep(1000);
				std::wcout << "Zamykam";
				//throw stop_now_t();
				std::exit(0);
			}
			
		}
		catch (const std::exception& ex) {
			//std::wcout  << "\n";
			//std::string str;
			//std::wstring str2(ex.what());
			//StringToWString(str2, str);
			////std::string str(StringToWString());
			//send_message(GlobalProxy, L"Error: " + str);
		}
	});

	/*proxy.on(U("closeExe"), [](web::json::value obj)
	{
		std::wcout << "Trying to close\n";
		std::wstring userName = obj.as_string();
		std::wcout << "Closing... UserName: " << userName << "\n";
	});*/

	proxy.on(U("createFile"), [](web::json::value obj)
	{
		try {
			utility::string_t userName = obj[0][U("UserName")].as_string();
			utility::string_t path = obj[0][U("Path")].as_string();
			utility::string_t content = obj[0][U("Content")].as_string();
			std::wcout << "Data: User: " << userName << " Path:" << path << " Content: " << content.c_str() << "\n";
			
			std::ofstream myfile;
			myfile.open(path);
			myfile << content.c_str();
			myfile.close();
			
			send_message(GlobalProxy, L"Plik utworzony");
		}
		catch (const std::exception& ex) {
			std::wcout << "\n";
			/*send_message(GlobalProxy, L"Error: " + ex.what);*/
		}
	});

	proxy.on(U("serverResponse"), [](web::json::value obj)
	{
		utility::string_t message = obj.as_string();
		std::wcout << "Message from server: " << message << "\n";
	});
}

void chat() //const utility::string_t& name
{
	const utility::string_t& name = L"Pietrek";
#if _DEBUG
	signalr::hub_connection connection{ U("http://localhost:50043/") };
	std::cout << "Connectng to local adress";
#else
	signalr::hub_connection connection{ U("http://remotenyancatopener.azurewebsites.net/") };
#endif
	auto proxy = connection.create_hub_proxy(U("MyHub1"));
	GlobalProxy = proxy;

	connections_from_server(proxy);

	connection.start()
		.then([proxy, name]()
	{
		std::cout << "Nawiazano polaczenie...\n";
		ucout << U("Enter your message:");
		for (;;)
		{
			utility::string_t message;
			std::getline(ucin, message);

			if (message == U(":q"))
			{
				break;
			}

			send_message(proxy, message);
		}
	})
		.then([&connection]() // fine to capture by reference - we are blocking so it is guaranteed to be valid
	{
		return connection.stop();
	})
		.then([](pplx::task<void> stop_task)
	{
		try
		{
			stop_task.get();
			ucout << U("connection stopped successfully") << std::endl;
		}
		catch (const std::exception &e)
		{
			ucout << U("exception when starting or stopping connection: ") << e.what() << std::endl;
		}
	}).get();
}

int main()
{
	//ucout << U("Enter your name: ");
	//utility::string_t name;
	//std::getline(ucin, name);

	try {
		ShowWindow(GetConsoleWindow(), SW_HIDE);
		chat();
	}
	catch (stop_now_t& stop) {
		return 0;
	}
	system("pause");

	return 0;
}
