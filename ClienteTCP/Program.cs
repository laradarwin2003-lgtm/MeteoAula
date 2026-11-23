using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using ClienteTCP.Red;

// Uso: dotnet run --project ClienteTCP -- [IP PUERTO]
// Sin argumentos: configura 127.0.0.1:5050, pero NO conecta ni envia HELLO.
string ip = "127.0.0.1";
int puerto = 5050;

if (args.Length != 0)
{
    bool correctos = args.Length == 2 &&
        IPAddress.TryParse(args[0], out IPAddress? direccion) &&
        direccion.AddressFamily == AddressFamily.InterNetwork &&
        int.TryParse(args[1], NumberStyles.None, CultureInfo.InvariantCulture, out puerto) &&
        puerto >= 1 && puerto <= 65535;
    if (!correctos)
    {
        Console.Error.WriteLine("Uso: ClienteTCP [IPV4 PUERTO] (puerto de 1 a 65535)");
        Environment.ExitCode = 1;
        return;
    }
    ip = args[0];
}

ClienteTcp.Iniciar(ip, puerto);
