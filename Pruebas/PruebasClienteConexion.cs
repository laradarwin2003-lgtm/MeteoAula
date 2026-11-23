using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ClienteTCP.Red;
using ServidorTCP.Red;

// Pruebas del requisito funcional 3 usando TCP de localhost real y consola simulada.
// Los logs se capturan en memoria (el servidor sigue escribiendo en consola por defecto).
internal static class PruebasClienteConexion
{
    public static void Ejecutar(Action<bool, string, string, string> verificar)
    {
        var registros = new ConcurrentQueue<string>();
        var servidor = new ServidorTcp(5000, linea => registros.Enqueue(linea));
        using var cancelacion = new CancellationTokenSource();
        Task tarea = Task.Run(() => servidor.Iniciar(0, cancelacion.Token));
        try
        {
            bool arrancado = SpinWait.SpinUntil(
                () => servidor.PuertoActual > 0 || tarea.IsCompleted, 5000);
            verificar(arrancado && servidor.PuertoActual > 0,
                "3.a servidor disponible para configurar IP/puerto",
                "Puerto de prueba asignado", "Puerto: " + servidor.PuertoActual);
            if (!arrancado || servidor.PuertoActual == 0) return;

            int puerto = servidor.PuertoActual;
            string comandos =
                "CONFIG 300.1.1.1 5050\n" +             // IP invalida
                "CONFIG 127.0.0.1 70000\n" +            // Puerto invalido
                $"CONFIG 127.0.0.1 {puerto}\n" +     // Configuracion valida
                "STATUS\nCONNECT\nLIST\nHELLO\nLIST\n" +
                "CURRENT EST001\nHISTORY EST003 2\nSUMMARY EST001\nQUIT\n" +
                "RECONNECT\nLIST\nHELLO\nLIST\nDISCONNECT\nSTATUS\nEXIT\n";
            using var entradas = new StringReader(comandos);
            using var salidas = new StringWriter();
            ClienteTcp.Iniciar(entradas, salidas);
            string texto = salidas.ToString();

            verificar(texto.Contains("IP o puerto invalidos", StringComparison.Ordinal),
                "3.a configuraciones invalidas se rechazan", "Mensajes claros para IP y puerto",
                "Rechazo de configuracion: " + texto.Contains("IP o puerto invalidos"));
            verificar(texto.Contains($"Destino configurado: 127.0.0.1:{puerto}", StringComparison.Ordinal),
                "3.a usuario configura IPv4 y puerto", $"127.0.0.1:{puerto}", "Configurado");
            verificar(texto.Contains($"Conectado a 127.0.0.1:{puerto}", StringComparison.Ordinal),
                "3.a se establece conexion TCP manual", "Conectado a IP:puerto", "Conexion establecida");
            verificar(texto.Contains("Error del protocolo 428", StringComparison.Ordinal),
                "3.b error HELLO requerido comprensible", "Codigo 428 explicado",
                "Mostrado=" + texto.Contains("Error del protocolo 428"));
            verificar(texto.Contains("Identificacion correcta: 200 WELCOME 1.0", StringComparison.Ordinal),
                "3.a HELLO manual tras CONNECT", "200 WELCOME", "HELLO aceptado");
            verificar(texto.Contains("EST001: Quito Centro (UIO)", StringComparison.Ordinal) &&
                      texto.Contains("EST005: Manta Costa (MEC)", StringComparison.Ordinal),
                "3.a LIST muestra resultados estructurados", "EST001 y EST005 con campos", "Estaciones renderizadas");
            verificar(texto.Contains("ParcialmenteNublado", StringComparison.Ordinal) &&
                      texto.Contains("15 lecturas", StringComparison.Ordinal),
                "3.a CURRENT HISTORY SUMMARY se interpretan", "Datos estructurados", "Datos visibles");
            verificar(texto.Contains("Sesion finalizada (200 BYE)", StringComparison.Ordinal) &&
                      texto.Contains("Conexion local cerrada", StringComparison.Ordinal),
                "3.b QUIT y DISCONNECT cierran la sesion", "Cierre confirmado", "Cierres anunciados");
            verificar(texto.Split("Conectado a 127.0.0.1:").Length >= 3,
                "3.b RECONNECT abre segunda conexion bajo orden manual",
                "Dos conexiones distintas", "Conexiones en la salida=" +
                (texto.Split("Conectado a 127.0.0.1:").Length - 1));
            verificar(texto.Split("Error del protocolo 428").Length == 3,
                "3.b despues de reconectar se necesita otro HELLO",
                "428 en primera y segunda sesion", "428 vistos=" +
                (texto.Split("Error del protocolo 428").Length - 1));
            verificar(texto.Contains("Sin conexion. Destino:", StringComparison.Ordinal) &&
                      texto.Contains("Cliente finalizado", StringComparison.Ordinal),
                "3.b vuelve al menu tras QUIT y DISCONNECT", "Menu sigue disponible", "STATUS/EXIT funcionales");

            // Esperar a que la tarea del servidor haya registrado los cierres
            // de ambas conexiones; el socket y la consola corren en hilos distintos.
            SpinWait.SpinUntil(() =>
                registros.Count(x => x.Contains("CONEXION CERRADA:", StringComparison.Ordinal)) >= 2,
                3000);
            string[] log = registros.ToArray();
            verificar(log.Count(x => x.Contains("CONEXION ABIERTA:", StringComparison.Ordinal)) >= 2,
                "3.c servidor registra conexiones en consola", "Dos CONEXION ABIERTA", "Conexiones=" +
                log.Count(x => x.Contains("CONEXION ABIERTA:")));
            verificar(log.Any(x => x.Contains("COMANDO RECIBIDO: LIST", StringComparison.Ordinal)) &&
                      log.Any(x => x.Contains("COMANDO RECIBIDO: HISTORY|EST003|2", StringComparison.Ordinal)) &&
                      log.Any(x => x.Contains("COMANDO RECIBIDO: HELLO|1.0", StringComparison.Ordinal)),
                "3.c servidor registra comandos recibidos", "LIST, HISTORY, HELLO", "Comandos en log");
            verificar(log.Count(x => x.Contains("CONEXION CERRADA:", StringComparison.Ordinal)) >= 2,
                "3.c servidor registra desconexiones", "Dos CONEXION CERRADA", "Desconexiones=" +
                log.Count(x => x.Contains("CONEXION CERRADA:")));
        }
        finally
        {
            cancelacion.Cancel();
            try { tarea.Wait(5000); }
            catch (AggregateException) { /* Se reporta por verificacion. */ }
            verificar(tarea.IsCompleted && !tarea.IsFaulted,
                "3.c servidor detiene el listener correctamente", "Task completada", "Completed=" + tarea.IsCompleted);
        }

        // Un destino sin servidor debe mostrar el fallo y ofrecer volver al menu.
        // Se obtiene un puerto libre temporalmente (sujeto a la habitual carrera de puertos).
        int sinServidor;
        var temporal = new TcpListener(IPAddress.Loopback, 0);
        try
        {
            temporal.Start();
            sinServidor = ((IPEndPoint)temporal.LocalEndpoint).Port;
        }
        finally
        {
            temporal.Stop();
        }
        using (var entradas = new StringReader($"CONNECT 127.0.0.1 {sinServidor}\nSTATUS\nEXIT\n"))
        using (var salidas = new StringWriter())
        {
            ClienteTcp.Iniciar(entradas, salidas);
            string texto = salidas.ToString();
            verificar(texto.Contains("No se pudo conectar", StringComparison.Ordinal) &&
                      texto.Contains("Sin conexion", StringComparison.Ordinal) &&
                      texto.Contains("Cliente finalizado", StringComparison.Ordinal),
                "3.b error de conexion permite nuevo intento", "Fallo comprensible, consola viva", "Fallo controlado");
        }
    }
}
