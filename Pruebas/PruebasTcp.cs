using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServidorTCP.Protocolo;
using ServidorTCP.Red;

// Pruebas de extremo a extremo sobre sockets TCP REALES (loopback, puerto libre).
// No se modifica ni simula el servidor; se inicia ServidorTcp real y se cancela
// de forma controlada al terminar.
internal static class PruebasTcp
{
    public static void Ejecutar(Action<bool, string, string, string> verificar)
    {
        using var cancelar = new CancellationTokenSource();
        var servidor = new ServidorTcp(1200);
        Task tarea = Task.Run(() => servidor.Iniciar(0, cancelar.Token));
        try
        {
            bool disponible = SpinWait.SpinUntil(
                () => servidor.PuertoActual != 0 || tarea.IsCompleted, 6000);
            verificar(disponible && servidor.PuertoActual > 0,
                "TCP servidor inicia en puerto libre", "Puerto activo > 0",
                "Puerto=" + servidor.PuertoActual);
            int puerto = servidor.PuertoActual;

            using (var tcp = Conectar(puerto))
            using (NetworkStream flujo = tcp.GetStream())
            {
                TramaProtocolo.Escribir(flujo, "LIST");
                string a = Recibir(flujo);
                verificar(a == "428|ERROR|HELLO_REQUERIDO",
                    "TCP LIST antes de HELLO se rechaza", "428|ERROR|HELLO_REQUERIDO", a);

                TramaProtocolo.Escribir(flujo, "HELP");
                a = Recibir(flujo);
                verificar(a.StartsWith("200|HELP|METEO/1.0|512|", StringComparison.Ordinal),
                    "TCP HELP antes de HELLO funciona", "200|HELP|METEO/1.0|512|...", a);

                TramaProtocolo.Escribir(flujo, "HELLO|1.0");
                a = Recibir(flujo);
                verificar(a == "200|WELCOME|1.0", "TCP HELLO identifica", "200|WELCOME|1.0", a);

                TramaProtocolo.Escribir(flujo, "CURRENT|EST999");
                a = Recibir(flujo);
                verificar(a == "404|ERROR|ESTACION_INEXISTENTE",
                    "TCP error de estacion inexistente", "404|ERROR|ESTACION_INEXISTENTE", a);

                TramaProtocolo.Escribir(flujo, "HISTORY|EST003|15");
                a = Recibir(flujo);
                verificar(a == "416|ERROR|CANTIDAD_FUERA_DE_RANGO",
                    "TCP HISTORY fuera de rango", "416|ERROR|CANTIDAD_FUERA_DE_RANGO", a);

                TramaProtocolo.Escribir(flujo, "OTRO");
                a = Recibir(flujo);
                verificar(a == "400|ERROR|COMANDO_DESCONOCIDO",
                    "TCP comando desconocido", "400|ERROR|COMANDO_DESCONOCIDO", a);

                TramaProtocolo.Escribir(flujo, "HISTORY|EST002");
                a = Recibir(flujo);
                verificar(a == "401|ERROR|PARAMETRO_FALTANTE",
                    "TCP parametro faltante", "401|ERROR|PARAMETRO_FALTANTE", a);

                TramaProtocolo.Escribir(flujo, "HISTORY|EST002|abc");
                a = Recibir(flujo);
                verificar(a == "402|ERROR|CANTIDAD_NO_ENTERA",
                    "TCP formato invalido", "402|ERROR|CANTIDAD_NO_ENTERA", a);

                TramaProtocolo.Escribir(flujo, "HISTORY|EST002|20");
                string cabecera = Recibir(flujo);
                int items = 0;
                for (int i = 0; i < 20; i++)
                {
                    string registro = Recibir(flujo);
                    if (registro.StartsWith("211|READING|", StringComparison.Ordinal)) items++;
                }
                string fin = Recibir(flujo);
                verificar(cabecera == "210|COUNT|20" && items == 20 && fin == "212|END",
                    "TCP HISTORY sin limite global y COUNT+END",
                    "210|COUNT|20, 20 ITEM, 212|END",
                    cabecera + ", " + items + " ITEM, " + fin);

                TramaProtocolo.Escribir(flujo, "QUIT");
                a = Recibir(flujo);
                verificar(a == "200|BYE", "TCP QUIT responde BYE", "200|BYE", a);
                int cierre = flujo.ReadByte();
                verificar(cierre == -1, "TCP QUIT cierra el socket", "EOF=-1", "EOF=" + cierre);
            }

            using (var tcp = Conectar(puerto))
            using (NetworkStream flujo = tcp.GetStream())
            {
                byte[] bytes = { 0xC3, 0x28, 0x0A }; // secuencia UTF-8 invalida
                flujo.Write(bytes, 0, bytes.Length);
                string a = Recibir(flujo);
                verificar(a == "402|ERROR|UTF8_INVALIDO",
                    "TCP UTF-8 invalido: error y sesion aislada", "402|ERROR|UTF8_INVALIDO", a);
            }

            using (var tcp = Conectar(puerto))
            using (NetworkStream flujo = tcp.GetStream())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(new string('A', 512) + "\n");
                flujo.Write(bytes, 0, bytes.Length);
                string a = Recibir(flujo);
                verificar(a == "413|ERROR|MENSAJE_DEMASIADO_GRANDE",
                    "TCP mensaje de 513 bytes: error y sesion aislada",
                    "413|ERROR|MENSAJE_DEMASIADO_GRANDE", a);
            }

            // Cliente se va sin terminar la linea. El servidor no debe morir.
            using (var tcp = Conectar(puerto))
            {
                tcp.LingerState = new LingerOption(true, 0); // RST en desconexion brusca.
                NetworkStream flujo = tcp.GetStream();
                byte[] parcial = Encoding.UTF8.GetBytes("HISTORY|EST001|");
                flujo.Write(parcial, 0, parcial.Length);
            }

            using (var tcp = Conectar(puerto))
            using (NetworkStream flujo = tcp.GetStream())
            {
                TramaProtocolo.Escribir(flujo, "HELLO|1.0");
                string a = Recibir(flujo);
                verificar(a == "200|WELCOME|1.0",
                    "TCP reconecta tras desconexion inesperada", "200|WELCOME|1.0", a);
            }

            using (var tcp = Conectar(puerto))
            using (NetworkStream flujo = tcp.GetStream())
            {
                Thread.Sleep(1600); // Superior al timeout de 1200 ms.
                bool desconectado;
                try { desconectado = flujo.ReadByte() == -1; }
                catch (IOException) { desconectado = true; } // RST remoto tambien significa desconexion.
                verificar(desconectado, "TCP cliente inactivo se desconecta",
                    "EOF o error de socket tras timeout", desconectado ? "Desconectado" : "Sigue conectado");
            }
        }
        finally
        {
            cancelar.Cancel();
            // Si una prueba falla, se intenta liberar el puerto en todos los casos.
            try { tarea.Wait(5000); }
            catch (AggregateException) { /* La prueba previa ya reportara el fallo. */ }
            verificar(tarea.IsCompleted && !tarea.IsFaulted,
                "TCP cierre ordenado del listener", "Servidor finalizado sin error",
                "Completed=" + tarea.IsCompleted + ", Faulted=" + tarea.IsFaulted);
        }
    }

    private static TcpClient Conectar(int puerto)
    {
        var tcp = new TcpClient();
        tcp.ReceiveTimeout = 4000;
        tcp.SendTimeout = 4000;
        tcp.Connect(IPAddress.Loopback, puerto);
        return tcp;
    }

    private static string Recibir(Stream flujo) =>
        TramaProtocolo.Leer(flujo) ?? throw new EndOfStreamException("Servidor cerro antes de responder");
}
