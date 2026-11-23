using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ServidorTCP.Red;

// Sintaxis: ServidorTCP [PUERTO [IP_ESCUCHA]]
// Por defecto escucha SOLO en localhost para no exponer el servicio sin permiso.
// Para probar entre equipos: ServidorTCP 5050 0.0.0.0
int puerto = 5050;
IPAddress escucha = IPAddress.Loopback;
if (args.Length > 2 ||
    args.Length > 0 && (!int.TryParse(args[0], out puerto) || puerto < 1 || puerto > 65535) ||
    args.Length == 2 && (!IPAddress.TryParse(args[1], out IPAddress? ipParseada) ||
                         ipParseada.AddressFamily != AddressFamily.InterNetwork))
{
    Console.Error.WriteLine("Uso: ServidorTCP [PUERTO [IP_ESCUCHA_IPV4]]");
    Console.Error.WriteLine("Ejemplo LAN: ServidorTCP 5050 0.0.0.0");
    Environment.ExitCode = 1;
    return;
}

if (args.Length == 2)
    escucha = IPAddress.Parse(args[1]);

using var cancelacion = new CancellationTokenSource();
ConsoleCancelEventHandler alCerrar = (_, evento) =>
{
    evento.Cancel = true; // No matar el proceso sin liberar el listener.
    cancelacion.Cancel();
};
Console.CancelKeyPress += alCerrar;

try
{
    Console.WriteLine("Ctrl+C detiene el servidor de forma ordenada.");
    new ServidorTcp().Iniciar(puerto, cancelacion.Token, escucha);
}
catch (Exception ex)
{
    Console.Error.WriteLine("No fue posible iniciar o mantener el servidor: " + ex.Message);
    Environment.ExitCode = 1;
}
finally
{
    Console.CancelKeyPress -= alCerrar;
}
