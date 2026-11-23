using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ClienteTCP.Protocolo;

namespace ClienteTCP.Red
{
    // Consola y transporte TCP sincrono. No implementa la logica del protocolo.
    // Los comandos CONNECT, CONFIG, DISCONNECT, RECONNECT, STATUS y EXIT son
    // operaciones LOCALES: nunca se envian como mensajes METEO/1.0 al servidor.
    public static class ClienteTcp
    {
        public static void Iniciar(string ip = "127.0.0.1", int puerto = 5050) =>
            Iniciar(Console.In, Console.Out, ip, puerto);

        // Entrada/salida inyectables para probar la consola sin modificar el protocolo.
        public static void Iniciar(TextReader entrada, TextWriter salida,
            string ip = "127.0.0.1", int puerto = 5050)
        {
            if (entrada is null) throw new ArgumentNullException(nameof(entrada));
            if (salida is null) throw new ArgumentNullException(nameof(salida));
            if (!DireccionValida(ip, puerto))
                throw new ArgumentException("IP IPv4 o puerto inicial invalidos (1..65535)");

            TcpClient? socket = null;
            NetworkStream? flujo = null;
            ClienteProtocolo? protocolo = null;
            bool identificado = false;
            bool terminar = false;

            // Un cierre local no manda HELLO ni crea otra sesion en segundo plano.
            void Desconectar()
            {
                protocolo = null;
                identificado = false;
                flujo?.Dispose();
                flujo = null;
                socket?.Dispose();
                socket = null;
            }

            void Conectar()
            {
                if (socket != null)
                {
                    salida.WriteLine("Ya estas conectado. Usa DISCONNECT antes de CONNECT o RECONNECT.");
                    return;
                }
                var candidato = new TcpClient(AddressFamily.InterNetwork);
                try
                {
                    // TCP SINCRONO: el usuario decide cuando realizar la conexion.
                    candidato.Connect(IPAddress.Parse(ip), puerto);
                    candidato.ReceiveTimeout = 300000;
                    candidato.SendTimeout = 300000;
                    NetworkStream nuevoFlujo = candidato.GetStream();
                    socket = candidato;
                    flujo = nuevoFlujo;
                    protocolo = new ClienteProtocolo(nuevoFlujo);
                    identificado = false;
                    salida.WriteLine($"Conectado a {ip}:{puerto}. Estado: CONECTADO (sin HELLO).");
                    salida.WriteLine("Antes de HELLO solo se admiten HELLO, HELP y QUIT.");
                }
                catch (SocketException ex)
                {
                    candidato.Dispose();
                    salida.WriteLine($"No se pudo conectar a {ip}:{puerto}: {ex.Message}");
                    salida.WriteLine("Puedes corregir la IP/puerto con CONFIG o intentar RECONNECT.");
                }
                catch (IOException ex)
                {
                    candidato.Dispose();
                    salida.WriteLine("Error al abrir la conexion: " + ex.Message);
                }
            }

            salida.WriteLine("MeteoAula - cliente TCP sincrono, METEO/1.0");
            salida.WriteLine($"Destino configurado: {ip}:{puerto} (IPv4)");
            salida.WriteLine("Escribe CONNECT para conectar; CONFIG <IP> <PUERTO> para cambiar el destino.");
            MostrarAyudaLocal(salida);

            try
            {
                while (!terminar)
                {
                    salida.Write(socket == null ? "meteo[sin conexion]> " : "meteo> ");
                    string? linea = entrada.ReadLine();
                    if (linea == null) break;
                    string[] partes = linea.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length == 0) continue;
                    string comando = partes[0].ToUpperInvariant();

                    // CONFIG y CONNECT validan IP IPv4 y puerto sin modificar la Parte 1.
                    if (comando == "CONFIG" || comando == "CONNECT")
                    {
                        if (socket != null)
                        {
                            salida.WriteLine("Ya hay una conexion activa. Usa DISCONNECT o RECONNECT.");
                            continue;
                        }
                        if (comando == "CONFIG" && partes.Length != 3 ||
                            comando == "CONNECT" && partes.Length != 1 && partes.Length != 3)
                        {
                            salida.WriteLine("Uso: CONFIG <IP> <PUERTO> o CONNECT [IP PUERTO]");
                            continue;
                        }
                        if (partes.Length == 3)
                        {
                            if (!int.TryParse(partes[2], NumberStyles.None,
                                    CultureInfo.InvariantCulture, out int nuevoPuerto) ||
                                !DireccionValida(partes[1], nuevoPuerto))
                            {
                                salida.WriteLine("IP o puerto invalidos. Usa IPv4 y puerto de 1 a 65535.");
                                continue;
                            }
                            ip = partes[1];
                            puerto = nuevoPuerto;
                            salida.WriteLine($"Destino configurado: {ip}:{puerto}");
                        }
                        if (comando == "CONNECT") Conectar();
                        continue;
                    }
                    if (comando == "RECONNECT" && partes.Length == 1)
                    {
                        if (socket != null)
                        {
                            Desconectar();
                            salida.WriteLine("Conexion anterior cerrada.");
                        }
                        Conectar(); // Reconexion SOLO tras orden del usuario; requiere HELLO nuevo.
                        continue;
                    }
                    if (comando == "DISCONNECT" && partes.Length == 1)
                    {
                        if (socket == null) salida.WriteLine("No hay una conexion activa.");
                        else
                        {
                            Desconectar();
                            salida.WriteLine("Conexion local cerrada. Usa CONNECT o RECONNECT.");
                        }
                        continue;
                    }
                    if (comando == "STATUS" && partes.Length == 1)
                    {
                        salida.WriteLine(socket == null
                            ? $"Sin conexion. Destino: {ip}:{puerto}"
                            : $"Conectado a {ip}:{puerto}. Estado local: " +
                              (identificado ? "IDENTIFICADO / CONSULTAS" : "CONECTADO (sin HELLO)"));
                        continue;
                    }
                    if (comando == "EXIT" && partes.Length == 1)
                    {
                        terminar = true;
                        continue;
                    }
                    if (comando == "HELP" && socket == null && partes.Length == 1)
                    {
                        MostrarAyudaLocal(salida);
                        continue;
                    }
                    if (socket == null || protocolo == null)
                    {
                        salida.WriteLine("No estas conectado. Escribe CONNECT o CONNECT <IP> <PUERTO>.");
                        continue;
                    }

                    try
                    {
                        // El servidor controla la maquina de estados. Por ello LIST antes
                        // de HELLO se ENVIA y el servidor devuelve el error 428.
                        if (comando == "HELLO" && partes.Length <= 2)
                        {
                            string version = partes.Length == 2 ? partes[1] : "1.0";
                            protocolo.Saludar(version);
                            identificado = true;
                            salida.WriteLine("Identificacion correcta: 200 WELCOME 1.0");
                        }
                        else if (partes.Length == 1 &&
                                 comando.StartsWith("HELLO|", StringComparison.Ordinal))
                        {
                            string version = partes[0].Substring("HELLO|".Length);
                            protocolo.Saludar(version);
                            identificado = true;
                            salida.WriteLine("Identificacion correcta: 200 WELCOME 1.0");
                        }
                        else if (comando == "LIST" && partes.Length == 1)
                        {
                            foreach (var estacion in protocolo.Listar())
                                salida.WriteLine($"{estacion.Codigo}: {estacion.Nombre} ({estacion.Ciudad})");
                        }
                        else if (comando == "CURRENT" && partes.Length == 2)
                        {
                            var actual = protocolo.Actual(partes[1]);
                            MostrarLectura(salida, actual.Lectura, actual.CodigoEstacion);
                        }
                        else if (comando == "HISTORY" && partes.Length == 3)
                        {
                            if (!int.TryParse(partes[2], NumberStyles.Integer,
                                    CultureInfo.InvariantCulture, out int n))
                            {
                                salida.WriteLine("La cantidad N debe ser un entero.");
                                continue;
                            }
                            foreach (var lectura in protocolo.Historial(partes[1], n))
                                MostrarLectura(salida, lectura, partes[1]);
                        }
                        else if (comando == "SUMMARY" && partes.Length == 2)
                        {
                            var resumen = protocolo.Resumen(partes[1]);
                            salida.WriteLine($"{resumen.CodigoEstacion}: {resumen.TotalLecturas} lecturas, " +
                                $"promedio {resumen.Promedio:0.##}, min {resumen.Minimo:0.##}, " +
                                $"max {resumen.Maximo:0.##} C");
                        }
                        else if (comando == "HELP" && partes.Length == 1)
                        {
                            salida.WriteLine(protocolo.Ayuda());
                            MostrarAyudaLocal(salida);
                        }
                        else if (comando == "QUIT" && partes.Length == 1)
                        {
                            protocolo.Salir(); // Respuesta 200|BYE verificada por ClienteProtocolo.
                            Desconectar();
                            salida.WriteLine("Sesion finalizada (200 BYE). Usa CONNECT para otra sesion.");
                        }
                        else
                        {
                            salida.WriteLine("Comando invalido en consola. Usa HELP para ver la sintaxis.");
                        }
                    }
                    catch (ErrorServidorException ex)
                    {
                        salida.WriteLine($"Error del protocolo {ex.Codigo}: " + ExplicarError(ex.Codigo, ex.Message));
                        if (ex.Codigo == 428 && !identificado)
                            salida.WriteLine("Primero escribe HELLO para identificarte.");
                    }
                    catch (InvalidDataException ex)
                    {
                        salida.WriteLine("Mensaje invalido: " + ex.Message);
                        Desconectar(); // No reutilizar una conexion desincronizada.
                        salida.WriteLine("Conexion cerrada por seguridad. Usa RECONNECT.");
                    }
                    catch (IOException ex)
                    {
                        salida.WriteLine("Servidor desconectado o tiempo de espera agotado: " + ex.Message);
                        Desconectar();
                        salida.WriteLine("Puedes intentar RECONNECT manualmente.");
                    }
                    catch (SocketException ex)
                    {
                        salida.WriteLine("Fallo de conexion con el servidor: " + ex.Message);
                        Desconectar();
                        salida.WriteLine("Puedes intentar RECONNECT manualmente.");
                    }
                    catch (ObjectDisposedException)
                    {
                        Desconectar();
                        salida.WriteLine("La conexion se cerro. Puedes intentar RECONNECT.");
                    }
                }
            }
            finally
            {
                Desconectar();
                salida.WriteLine("Cliente finalizado.");
            }
        }

        private static bool DireccionValida(string ip, int puerto) =>
            puerto >= 1 && puerto <= 65535 &&
            IPAddress.TryParse(ip, out IPAddress? direccion) &&
            direccion.AddressFamily == AddressFamily.InterNetwork;

        private static string ExplicarError(int codigo, string mensaje) => codigo switch
        {
            400 => "Comando desconocido (" + mensaje + ")",
            401 => "Falta un parametro (" + mensaje + ")",
            402 => "Formato de solicitud invalido (" + mensaje + ")",
            404 => "La estacion solicitada no existe (" + mensaje + ")",
            409 => "Operacion no permitida en este estado (" + mensaje + ")",
            413 => "Mensaje de mas de 512 bytes (" + mensaje + ")",
            416 => "Cantidad fuera del rango disponible (" + mensaje + ")",
            428 => "Debes ejecutar HELLO antes de consultar (" + mensaje + ")",
            505 => "Version incompatible; usa 1.0 (" + mensaje + ")",
            500 => "Error interno del servidor (" + mensaje + ")",
            _ => mensaje
        };

        private static void MostrarAyudaLocal(TextWriter salida)
        {
            salida.WriteLine("Comandos locales: CONFIG <IP> <PUERTO>, CONNECT [IP PUERTO], " +
                "RECONNECT, DISCONNECT, STATUS, EXIT.");
            salida.WriteLine("Comandos METEO/1.0 conectado: HELLO, LIST, CURRENT EST001, " +
                "HISTORY EST001 2, SUMMARY EST001, HELP, QUIT.");
            salida.WriteLine("QUIT cierra la sesion remota; EXIT cierra la aplicacion cliente.");
        }

        private static void MostrarLectura(TextWriter salida, LecturaRemota lectura, string codigo) =>
            salida.WriteLine($"{codigo} | {lectura.FechaHora:yyyy-MM-dd HH:mm:ss} | " +
                $"{lectura.TemperaturaCelsius:0.##} C | {lectura.HumedadPorcentaje}% | {lectura.Estado}");
    }
}
