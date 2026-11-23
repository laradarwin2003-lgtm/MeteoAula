using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ServidorTCP.Protocolo;
using ServidorTCP.Servicios;
using ClienteTCP.Protocolo;

using var registroPruebas = new RegistroPruebas();
int ejecutadas = 0;
void Verificar(bool condicion, string titulo,
    string esperado = "Condicion verdadera", string obtenido = "")
{
    ejecutadas++;
    registroPruebas.Agregar(titulo, condicion, esperado,
        string.IsNullOrEmpty(obtenido) ? "Condicion=" + condicion : obtenido);
    if (!condicion) throw new Exception("FALLO: " + titulo);
    Console.WriteLine("OK: " + titulo);
}

var servicio = new ServicioConsultas(); // PARTE 1 original
var sesion = new InterpreteProtocolo(servicio);
Verificar(sesion.Estado == EstadoSesion.CONECTADO, "estado conectado");
Verificar(sesion.ProcesarLinea("LIST").Lineas.Single().StartsWith("428|ERROR|"), "HELLO obligatorio");
Verificar(sesion.ProcesarLinea("CURRENT|EST001").Lineas.Single() == "428|ERROR|HELLO_REQUERIDO",
    "CURRENT prohibido antes de HELLO");
Verificar(sesion.ProcesarLinea("HISTORY|EST001|2").Lineas.Single() == "428|ERROR|HELLO_REQUERIDO",
    "HISTORY prohibido antes de HELLO");
Verificar(sesion.ProcesarLinea("SUMMARY|EST001").Lineas.Single() == "428|ERROR|HELLO_REQUERIDO",
    "SUMMARY prohibido antes de HELLO");
var sesionSinHello = new InterpreteProtocolo(servicio);
Verificar(sesionSinHello.ProcesarLinea("QUIT").CerrarConexion &&
          sesionSinHello.Estado == EstadoSesion.CERRADO,
    "QUIT permitido antes de HELLO");
Verificar(sesion.Estado == EstadoSesion.CONECTADO, "error no avanza estado");
Verificar(sesion.ProcesarLinea("HELP").Lineas.Single().StartsWith("200|HELP|"), "HELP antes de HELLO");
Verificar(sesion.Estado == EstadoSesion.CONECTADO, "HELP no avanza estado");
Verificar(sesion.ProcesarLinea("HELLO|1.0").Lineas.Single() == "200|WELCOME|1.0", "HELLO correcto");
Verificar(sesion.Estado == EstadoSesion.IDENTIFICADO, "estado identificado");
Verificar(sesion.ProcesarLinea("HELLO|1.0").Lineas.Single().StartsWith("409|ERROR|"), "HELLO repetido");

var listado = sesion.ProcesarLinea("LIST");
Verificar(listado.Lineas[0] == "210|COUNT|5" && listado.Lineas.Count == 7 &&
          listado.Lineas.Last() == "212|END", "LIST COUNT+5 ITEMS+END");
Verificar(listado.Lineas[1] == "211|STATION|EST001|Quito Centro|UIO", "LIST usa Parte 1 real");
Verificar(sesion.Estado == EstadoSesion.CONSULTAS, "estado consultas");

var historia20 = sesion.ProcesarLinea("HISTORY|EST002|20");
Verificar(historia20.Lineas[0] == "210|COUNT|20" && historia20.Lineas.Count == 22 &&
          historia20.Lineas.Last() == "212|END", "HISTORY EST002 permite 20 (sin limite global 10)");
Verificar(historia20.Lineas[1].StartsWith("211|READING|2026-10-01T08:00:00|"),
    "historial ordenado ascendente por Parte 1");
Verificar(sesion.ProcesarLinea("HISTORY|EST003|15").Lineas.Single().StartsWith("416|ERROR|"),
    "HISTORY EST003 no permite 15 por validacion Parte 1");
Verificar(sesion.ProcesarLinea("HISTORY|EST001|0").Lineas.Single().StartsWith("416|ERROR|"),
    "N cero fuera de rango");
Verificar(sesion.ProcesarLinea("HISTORY|EST001|abc").Lineas.Single().StartsWith("402|ERROR|"),
    "N no entero formato invalido");
Verificar(sesion.ProcesarLinea("HISTORY|EST001").Lineas.Single().StartsWith("401|ERROR|"),
    "parametro faltante");
Verificar(sesion.ProcesarLinea("HISTORY|XYZ|2").Lineas.Single().StartsWith("402|ERROR|"),
    "codigo mal formado");
Verificar(sesion.ProcesarLinea("CURRENT|EST999").Lineas.Single().StartsWith("404|ERROR|"),
    "estacion inexistente");
Verificar(sesion.ProcesarLinea("OTRO").Lineas.Single().StartsWith("400|ERROR|"),
    "comando desconocido");
Verificar(sesion.ProcesarLinea("CURRENT|EST001|OTRO").Lineas.Single().StartsWith("402|ERROR|"),
    "parametro extra");
Verificar(sesion.ProcesarLinea("CURRENT|est001").Lineas.Single().StartsWith("200|CURRENT|EST001|"),
    "codigo sin distinguir mayusculas");
Verificar(sesion.ProcesarLinea("SUMMARY|EST001").Lineas.Single().StartsWith("200|SUMMARY|EST001|15|"),
    "resumen de Parte 1 sin recalcular en Protocolo");
Verificar(sesion.ProcesarLinea("HISTORY|EST002|21").Lineas.Single().StartsWith("416|ERROR|"),
    "cantidad mayor a disponible");
Verificar(sesion.ProcesarLinea("HISTORY|EST002|20").Lineas.Count == 22,
    "errores no cierran la sesion");
Verificar(sesion.ProcesarLinea("QUIT").CerrarConexion && sesion.Estado == EstadoSesion.CERRADO,
    "QUIT cierra sesion");

var nuevo = new InterpreteProtocolo(servicio);
Verificar(nuevo.ProcesarLinea("HELLO|2.0").Lineas.Single().StartsWith("505|ERROR|"),
    "version incompatible");
using (var caracteresInvalidos = new MemoryStream(new byte[] { 0xC3, 0x28, 0x0A }))
{
    try { TramaProtocolo.Leer(caracteresInvalidos); throw new Exception("No rechazo UTF8 invalido"); }
    catch (ExcepcionProtocolo e) { Verificar(e.Codigo == 402, "UTF8 invalido"); }
}
using (var enLimite = new MemoryStream(Encoding.UTF8.GetBytes(new string('A', 511) + "\n")))
{
    string? linea = TramaProtocolo.Leer(enLimite);
    Verificar(linea != null && linea.Length == 511,
        "limite exacto: 512 bytes incluido LF aceptados");
}
using (var grande = new MemoryStream(Encoding.UTF8.GetBytes(new string('A', 512) + "\n")))
{
    try { TramaProtocolo.Leer(grande); throw new Exception("No rechazo 513 bytes"); }
    catch (ExcepcionProtocolo e) { Verificar(e.Codigo == 413, "limite 512 bytes por linea"); }
}

// Cliente con un Stream ficticio: registra solicitudes y ofrece respuestas predefinidas.
// Cliente no necesita identificarse automaticamente: debe interpretar el 428 del servidor.
using (var s = Simular("428|ERROR|HELLO_REQUERIDO\n"))
{
    var cliente = new ClienteProtocolo(s);
    try { cliente.Listar(); throw new Exception("No detecto HELLO_REQUERIDO"); }
    catch (ErrorServidorException e) { Verificar(e.Codigo == 428, "cliente interpreta error 428 antes de HELLO"); }
    Verificar(s.Enviados == "LIST\n", "cliente envia LIST sin HELLO, servidor decide si autoriza");
}
using (var s = Simular("200|WELCOME|1.0\n"))
{
    var cliente = new ClienteProtocolo(s);
    cliente.Saludar();
    Verificar(s.Enviados == "HELLO|1.0\n", "cliente envia HELLO solo cuando se invoca Saludar");
}
static ScriptedStream Simular(string respuesta) => new ScriptedStream(Encoding.UTF8.GetBytes(respuesta));
using (var s = Simular("210|COUNT|2\n211|READING|2026-10-01T08:00:00|20.5|65|Soleado\n212|END\n"))
{
    var cliente = new ClienteProtocolo(s);
    try { cliente.Historial("EST001", 2); throw new Exception("No detecto historial incompleto"); }
    catch (InvalidDataException ex) { Verificar(ex.Message.Contains("incompleto"), "cliente detecta END prematuro"); }
}
using (var s = Simular("210|COUNT|2\n211|READING|2026-10-01T08:00:00|20.5|65|Soleado\n211|READING|2026-10-02T08:00:00|21|62|Nublado\n212|END\n"))
{
    var cliente = new ClienteProtocolo(s);
    Verificar(cliente.Historial("EST001", 2).Count == 2, "cliente interpreta COUNT+ITEM+END");
    Verificar(s.Enviados.Contains("HISTORY|EST001|2\n"), "cliente envio HISTORY correcto");
}
using (var s = Simular("210|COUNT|1\n211|READING|2026-10-01T08:00:00|20.5|120|Soleado\n212|END\n"))
{
    var cliente = new ClienteProtocolo(s);
    try { cliente.Historial("EST001", 1); throw new Exception("No detecto humedad invalida"); }
    catch (InvalidDataException ex) { Verificar(ex.Message.Contains("Humedad"), "cliente valida campos numericos"); }
}
PruebasTcp.Ejecutar((ok, titulo, esperado, obtenido) =>
    Verificar(ok, titulo, esperado, obtenido));
PruebasClienteConexion.Ejecutar((ok, titulo, esperado, obtenido) =>
    Verificar(ok, titulo, esperado, obtenido));
Console.WriteLine($"\n{ejecutadas} comprobaciones completadas.");

sealed class ScriptedStream : Stream
{
    private readonly MemoryStream entrantes;
    private readonly MemoryStream salientes = new MemoryStream();
    public string Enviados => Encoding.UTF8.GetString(salientes.ToArray());
    public ScriptedStream(byte[] datos) => entrantes = new MemoryStream(datos);
    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => entrantes.Read(buffer, offset, count);
    public override int ReadByte() => entrantes.ReadByte();
    public override void Write(byte[] buffer, int offset, int count) => salientes.Write(buffer, offset, count);
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
