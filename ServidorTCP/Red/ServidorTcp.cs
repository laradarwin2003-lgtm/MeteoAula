using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServidorTCP.Protocolo;
using ServidorTCP.Servicios;

namespace ServidorTCP.Red
{
    // Adaptador TCP de ejemplo: una sesion a la vez; el protocolo y las consultas
    // permanecen fuera de Red. Si ya tienen Red propio, integrar las mismas reglas.
    public class ServidorTcp
    {
        private readonly ServicioConsultas servicio = new ServicioConsultas();
        private readonly int tiempoEsperaMilisegundos;
        private readonly object sincronizacion = new object();
        private readonly Action<string> registrar;
        private TcpListener? listener;

        // Puerto 0 sirve SOLO en pruebas para obtener uno libre automaticamente.
        private int puertoActual;
        public int PuertoActual => Volatile.Read(ref puertoActual);

        public ServidorTcp(int tiempoEsperaMilisegundos = 300000, Action<string>? registrar = null)
        {
            if (tiempoEsperaMilisegundos <= 0)
                throw new ArgumentOutOfRangeException(nameof(tiempoEsperaMilisegundos));
            this.tiempoEsperaMilisegundos = tiempoEsperaMilisegundos;
            this.registrar = registrar ?? Console.WriteLine;
        }

        public void Iniciar(int puerto = 5050, CancellationToken cancelacion = default,
            IPAddress? direccionEscucha = null)
        {
            if (puerto < 0 || puerto > 65535)
                throw new ArgumentOutOfRangeException(nameof(puerto));

            // Por defecto solo localhost. El usuario puede habilitar LAN expresamente
            // usando, por ejemplo, 0.0.0.0 como direccion de escucha.
            IPAddress direccion = direccionEscucha ?? IPAddress.Loopback;
            if (direccion.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("La IP de escucha debe ser IPv4", nameof(direccionEscucha));
            var nuevoListener = new TcpListener(direccion, puerto);
            lock (sincronizacion)
            {
                if (listener != null)
                    throw new InvalidOperationException("El servidor ya esta iniciado");
                listener = nuevoListener;
            }

            try
            {
                nuevoListener.Start();
                Volatile.Write(ref puertoActual, ((IPEndPoint)nuevoListener.LocalEndpoint).Port);
                Registrar($"ESCUCHANDO METEO/1.0 en {direccion}:{PuertoActual}");
                // Al cancelar, Stop() desbloquea AcceptTcpClient de manera limpia.
                using CancellationTokenRegistration registro = cancelacion.Register(() => nuevoListener.Stop());
                while (!cancelacion.IsCancellationRequested)
                {
                    TcpClient cliente;
                    try
                    {
                        cliente = nuevoListener.AcceptTcpClient();
                    }
                    catch (SocketException) when (cancelacion.IsCancellationRequested) { break; }
                    catch (ObjectDisposedException) when (cancelacion.IsCancellationRequested) { break; }
                    catch (SocketException ex)
                    {
                        Console.Error.WriteLine("Error al aceptar un cliente: " + ex.Message);
                        continue;
                    }

                    // El error de un cliente no termina la escucha del servidor.
                    using (cliente)
                    {
                        string remoto = cliente.Client.RemoteEndPoint?.ToString() ?? "desconocido";
                        // Ctrl+C tambien interrumpe una lectura TCP bloqueada.
                        using CancellationTokenRegistration cierreCliente = cancelacion.Register(
                            () => cliente.Dispose());
                        try
                        {
                            cliente.ReceiveTimeout = tiempoEsperaMilisegundos;
                            cliente.SendTimeout = tiempoEsperaMilisegundos;
                            Registrar($"CONEXION ABIERTA: {remoto}");
                            Atender(cliente);
                        }
                        catch (IOException ex)
                        {
                            Console.Error.WriteLine("Error de entrada/salida del cliente: " + ex.Message);
                        }
                        catch (SocketException ex)
                        {
                            Console.Error.WriteLine("Error de socket del cliente: " + ex.Message);
                        }
                        catch (Exception ex)
                        {
                            // Se aisla la sesion afectada y se sigue aceptando clientes.
                            Console.Error.WriteLine("Error inesperado de sesion: " + ex.Message);
                        }
                        finally { Registrar("CONEXION CERRADA: " + remoto); }
                    }
                }
            }
            finally
            {
                nuevoListener.Stop();
                Volatile.Write(ref puertoActual, 0);
                lock (sincronizacion)
                {
                    if (ReferenceEquals(listener, nuevoListener)) listener = null;
                }
                Registrar("Servidor detenido limpiamente");
            }
        }

        private void Atender(TcpClient cliente)
        {
            var interprete = new InterpreteProtocolo(servicio); // Estado por cliente.
            using NetworkStream flujo = cliente.GetStream();
            while (interprete.Estado != EstadoSesion.CERRADO)
            {
                try
                {
                    string? linea = TramaProtocolo.Leer(flujo);
                    if (linea == null) return; // EOF limpio.
                    // Log obligatorio de la consigna 3.c: cada solicitud recibida.
                    // Un registro por linea completa, sin persistencia obligatoria.
                    Registrar("COMANDO RECIBIDO: " + linea);
                    ResultadoProtocolo respuesta = interprete.ProcesarLinea(linea);
                    foreach (string salida in respuesta.Lineas)
                        TramaProtocolo.Escribir(flujo, salida);
                    if (respuesta.Lineas.Count > 0)
                        Registrar("RESPUESTA ENVIADA: " + respuesta.Lineas[0].Split('|')[0]);
                    if (respuesta.CerrarConexion) return; // QUIT.
                }
                catch (ExcepcionProtocolo ex)
                {
                    Registrar($"ERROR DE TRAMA RECIBIDA: {ex.Codigo} {ex.Message}");
                    // En un error de encuadre, el resto del flujo podria quedar
                    // desincronizado. Se intenta informar y se cierra SOLO el cliente.
                    IntentarError(flujo, ex.Codigo, ex.Message);
                    return;
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine("Desconexion o timeout: " + ex.Message);
                    return;
                }
                catch (SocketException ex)
                {
                    Console.Error.WriteLine("Desconexion de socket: " + ex.Message);
                    return;
                }
            }
        }

        private void Registrar(string mensaje) =>
            registrar($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensaje}");

        private static void IntentarError(Stream flujo, int codigo, string mensaje)
        {
            try
            {
                TramaProtocolo.Escribir(flujo, TramaProtocolo.CrearLinea(
                    codigo.ToString(CultureInfo.InvariantCulture), "ERROR", mensaje));
            }
            catch (IOException) { /* El cliente ya pudo desconectarse. */ }
            catch (SocketException) { /* El socket ya pudo cerrarse. */ }
            catch (ObjectDisposedException) { /* No queda un flujo valido. */ }
        }
    }
}
