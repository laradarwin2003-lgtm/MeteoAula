using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClienteTCP.Protocolo
{
    public sealed record EstacionRemota(string Codigo, string Nombre, string Ciudad);
    public sealed record LecturaRemota(DateTime FechaHora, double TemperaturaCelsius,
        int HumedadPorcentaje, string Estado);
    public sealed record LecturaActualRemota(string CodigoEstacion, LecturaRemota Lectura);
    public sealed record ResumenRemoto(string CodigoEstacion, int TotalLecturas,
        double Promedio, double Minimo, double Maximo);

    public sealed class ErrorServidorException : Exception
    {
        public int Codigo { get; }
        public ErrorServidorException(int codigo, string mensaje) : base(mensaje) => Codigo = codigo;
    }

    // Cliente del PROTOCOLO. No crea sockets ni imprime mensajes sin interpretar.
    public sealed class ClienteProtocolo
    {
        private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;
        private readonly Stream flujo;

        public ClienteProtocolo(Stream flujoTcp) =>
            flujo = flujoTcp ?? throw new ArgumentNullException(nameof(flujoTcp));

        public void Saludar(string version = "1.0")
        {
            TramaCliente.Enviar(flujo, "HELLO", version);
            string[] r = LeerRespuesta();
            Exigir(r, 3, "200", "WELCOME");
            if (r[2] != "1.0") throw new InvalidDataException("Version de bienvenida incompatible");
        }

        public IReadOnlyList<EstacionRemota> Listar()
        {
            TramaCliente.Enviar(flujo, "LIST");
            return LeerMultiple("LIST", "STATION", ParsearEstacion);
        }

        public LecturaActualRemota Actual(string codigo)
        {
            TramaCliente.Enviar(flujo, "CURRENT", codigo);
            string[] r = LeerRespuesta();
            Exigir(r, 7, "200", "CURRENT");
            ValidarCodigo(r[2]);
            if (!string.Equals(r[2], codigo.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Codigo de estacion no coincide con la consulta");
            return new LecturaActualRemota(r[2], ParsearLectura(
                new[] { "211", "READING", r[3], r[4], r[5], r[6] }));
        }

        public IReadOnlyList<LecturaRemota> Historial(string codigo, int n)
        {
            TramaCliente.Enviar(flujo, "HISTORY", codigo, n.ToString(Cultura));
            List<LecturaRemota> resultado = LeerMultiple("HISTORY", "READING", ParsearLectura, n);
            for (int i = 1; i < resultado.Count; i++)
            {
                if (resultado[i].FechaHora < resultado[i - 1].FechaHora)
                    throw new InvalidDataException("El historial no esta ordenado de antiguo a reciente");
            }
            return resultado;
        }

        public ResumenRemoto Resumen(string codigo)
        {
            TramaCliente.Enviar(flujo, "SUMMARY", codigo);
            string[] r = LeerRespuesta();
            Exigir(r, 7, "200", "SUMMARY");
            ValidarCodigo(r[2]);
            if (!string.Equals(r[2], codigo.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Codigo de estacion no coincide con la consulta");
            int total = Entero(r[3], "TotalLecturas");
            double promedio = Numero(r[4], "Promedio");
            double minimo = Numero(r[5], "Minimo");
            double maximo = Numero(r[6], "Maximo");
            if (total < 1 || minimo > maximo || promedio < minimo || promedio > maximo)
                throw new InvalidDataException("Resumen de temperatura inconsistente");
            return new ResumenRemoto(r[2], total, promedio, minimo, maximo);
        }

        public string Ayuda()
        {
            TramaCliente.Enviar(flujo, "HELP");
            string[] r = LeerRespuesta();
            Exigir(r, 6, "200", "HELP");
            if (r[2] != "METEO/1.0" || r[3] != "512" ||
                r[4] != "HISTORY_SEGUN_ESTACION")
                throw new InvalidDataException("Parametros HELP invalidos");
            string[] esperados = { "HELLO", "LIST", "CURRENT", "HISTORY", "SUMMARY", "HELP", "QUIT" };
            if (!r[5].Split(',').SequenceEqual(esperados))
                throw new InvalidDataException("Lista HELP no coincide con el protocolo");
            return $"{r[2]} | Maximo 512 bytes por linea | HISTORY depende de cada estacion | {r[5]}";
        }

        public void Salir()
        {
            TramaCliente.Enviar(flujo, "QUIT");
            string[] r = LeerRespuesta();
            Exigir(r, 2, "200", "BYE");
        }

        private string[] LeerRespuesta()
        {
            string[] r = TramaCliente.Leer(flujo);
            if (r.Length > 0 && int.TryParse(r[0], NumberStyles.None, Cultura, out int codigo)
                && codigo >= 400 && codigo <= 599)
            {
                if (r.Length != 3 || r[1] != "ERROR" || string.IsNullOrWhiteSpace(r[2]))
                    throw new InvalidDataException("Formato de error no valido");
                throw new ErrorServidorException(codigo, r[2]);
            }
            return r;
        }

        private List<T> LeerMultiple<T>(string comando, string etiqueta,
            Func<string[], T> parsear, int? cantidadSolicitada = null)
        {
            string[] cabecera = LeerRespuesta();
            Exigir(cabecera, 3, "210", "COUNT");
            int cantidad = Entero(cabecera[2], "COUNT");
            if (cantidad < 0) throw new InvalidDataException("COUNT negativo");
            if (cantidadSolicitada.HasValue && cantidad != cantidadSolicitada.Value)
                throw new InvalidDataException(
                    $"{comando}: COUNT no coincide con la cantidad solicitada");

            var registros = new List<T>();
            for (int i = 0; i < cantidad; i++)
            {
                string[] item;
                try { item = LeerRespuesta(); }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidDataException($"{comando} incompleto: {i} de {cantidad} registros", ex);
                }
                if (item.Length == 2 && item[0] == "212" && item[1] == "END")
                    throw new InvalidDataException($"{comando} incompleto: {i} de {cantidad} registros");
                if (item.Length < 2 || item[0] != "211" || item[1] != etiqueta)
                    throw new InvalidDataException($"{comando}: ITEM invalido en posicion {i + 1}");
                registros.Add(parsear(item));
            }

            string[] fin;
            try { fin = LeerRespuesta(); }
            catch (EndOfStreamException ex)
            {
                throw new InvalidDataException($"{comando} incompleto: falta END", ex);
            }
            Exigir(fin, 2, "212", "END");
            // Solo se devuelven resultados despues de validar cantidad y terminacion.
            return registros;
        }

        private static EstacionRemota ParsearEstacion(string[] r)
        {
            Exigir(r, 5, "211", "STATION");
            ValidarCodigo(r[2]);
            if (string.IsNullOrWhiteSpace(r[3]))
                throw new InvalidDataException("Nombre de estacion vacio");
            if (!new[] { "UIO", "GYE", "CUE", "LOJ", "MEC" }.Contains(r[4]))
                throw new InvalidDataException("Ciudad no reconocida");
            return new EstacionRemota(r[2], r[3], r[4]);
        }

        private static LecturaRemota ParsearLectura(string[] r)
        {
            Exigir(r, 6, "211", "READING");
            if (!DateTime.TryParseExact(r[2], "yyyy-MM-ddTHH:mm:ss", Cultura,
                DateTimeStyles.None, out DateTime fecha))
                throw new InvalidDataException("Fecha de lectura invalida");
            double temperatura = Numero(r[3], "TemperaturaCelsius");
            int humedad = Entero(r[4], "HumedadPorcentaje");
            if (humedad < 0 || humedad > 100)
                throw new InvalidDataException("Humedad fuera de 0 a 100");
            if (!new[] { "Soleado", "Nublado", "ParcialmenteNublado", "Lluvioso", "Ventoso" }.Contains(r[5]))
                throw new InvalidDataException("EstadoTiempo desconocido");
            return new LecturaRemota(fecha, temperatura, humedad, r[5]);
        }

        private static void Exigir(string[] campos, int total, string codigo, string tipo)
        {
            if (campos.Length != total || campos[0] != codigo || campos[1] != tipo)
                throw new InvalidDataException($"Respuesta invalida: se esperaba {codigo}|{tipo} con {total} campos");
        }

        private static void ValidarCodigo(string codigo)
        {
            if (!Regex.IsMatch(codigo, @"^EST[0-9]{3}$", RegexOptions.CultureInvariant))
                throw new InvalidDataException("Codigo de estacion invalido en respuesta");
        }

        private static int Entero(string valor, string campo)
        {
            if (!int.TryParse(valor, NumberStyles.Integer, Cultura, out int resultado))
                throw new InvalidDataException($"Campo {campo} no es un entero valido");
            return resultado;
        }

        private static double Numero(string valor, string campo)
        {
            if (!double.TryParse(valor, NumberStyles.Float, Cultura, out double resultado) ||
                double.IsNaN(resultado) || double.IsInfinity(resultado))
                throw new InvalidDataException($"Campo {campo} no es un numero valido");
            return resultado;
        }
    }
}
