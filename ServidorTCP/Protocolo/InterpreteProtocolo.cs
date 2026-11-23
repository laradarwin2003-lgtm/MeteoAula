using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ServidorTCP.Modelos;
using ServidorTCP.Servicios;

namespace ServidorTCP.Protocolo
{
    public enum EstadoSesion
    {
        CONECTADO,
        IDENTIFICADO,
        CONSULTAS,
        CERRADO
    }

    public class ResultadoProtocolo
    {
        public IReadOnlyList<string> Lineas { get; }
        public bool CerrarConexion { get; }

        public ResultadoProtocolo(IReadOnlyList<string> lineas, bool cerrarConexion = false)
        {
            Lineas = lineas;
            CerrarConexion = cerrarConexion;
        }
    }

    // Instanciar una vez POR CLIENTE; NO colocar una sola instancia global para varios clientes.
    // Interpreta texto y delega todas las consultas en ServicioConsultas de la Parte 1.
    public class InterpreteProtocolo
    {
        private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;
        private static readonly Regex PatronCodigo = new Regex(
            @"^EST[0-9]{3}$", RegexOptions.CultureInvariant);
        private readonly ServicioConsultas servicio;

        public EstadoSesion Estado { get; private set; } = EstadoSesion.CONECTADO;

        public InterpreteProtocolo(ServicioConsultas servicioConsultas)
        {
            servicio = servicioConsultas ?? throw new ArgumentNullException(nameof(servicioConsultas));
        }

        public ResultadoProtocolo ProcesarLinea(string solicitud)
        {
            if (Estado == EstadoSesion.CERRADO)
                return Unica("409", "ERROR", "SESION_CERRADA");

            try
            {
                TramaProtocolo.ValidarLinea(solicitud);
                string[] campos = solicitud.Split('|');
                string comando = campos[0];

                if (!new[] { "HELLO", "LIST", "CURRENT", "HISTORY", "SUMMARY", "HELP", "QUIT" }
                    .Contains(comando, StringComparer.Ordinal))
                    throw new ExcepcionProtocolo(400, "COMANDO_DESCONOCIDO");

                if (Estado == EstadoSesion.CONECTADO &&
                    comando != "HELLO" && comando != "HELP" && comando != "QUIT")
                    throw new ExcepcionProtocolo(428, "HELLO_REQUERIDO");

                switch (comando)
                {
                    case "HELLO":
                        VerificarParametros(campos, 2);
                        if (Estado != EstadoSesion.CONECTADO)
                            throw new ExcepcionProtocolo(409, "HELLO_YA_EJECUTADO");
                        if (campos[1] != EspecificacionMeteo.Version)
                            throw new ExcepcionProtocolo(505, "VERSION_INCOMPATIBLE");
                        Estado = EstadoSesion.IDENTIFICADO;
                        return Unica("200", "WELCOME", EspecificacionMeteo.Version);

                    case "HELP":
                        VerificarParametros(campos, 1);
                        return Unica("200", "HELP", EspecificacionMeteo.Nombre, EspecificacionMeteo.MaxBytesPorLinea.ToString(Cultura),
                            "HISTORY_SEGUN_ESTACION",
                            "HELLO,LIST,CURRENT,HISTORY,SUMMARY,HELP,QUIT");

                    case "QUIT":
                        VerificarParametros(campos, 1);
                        Estado = EstadoSesion.CERRADO;
                        return UnicaCerrar("200", "BYE");

                    case "LIST":
                    {
                        VerificarParametros(campos, 1);
                        IReadOnlyList<Estacion> estaciones = servicio.ListarEstaciones();
                        var registros = estaciones.Select(e => TramaProtocolo.CrearLinea(
                            "211", "STATION", e.Codigo, e.Nombre, e.Ciudad.ToString())).ToList();
                        ResultadoProtocolo resultado = Multiple(registros);
                        Estado = EstadoSesion.CONSULTAS;
                        return resultado;
                    }

                    case "CURRENT":
                    {
                        VerificarParametros(campos, 2);
                        string codigo = ObtenerCodigo(campos[1]);
                        Lectura lectura = servicio.ObtenerLecturaMasReciente(codigo);
                        ResultadoProtocolo resultado = Unica("200", "CURRENT", codigo,
                            Fecha(lectura.FechaHora), Numero(lectura.TemperaturaCelsius),
                            lectura.HumedadPorcentaje.ToString(Cultura), lectura.Estado.ToString());
                        Estado = EstadoSesion.CONSULTAS;
                        return resultado;
                    }

                    case "HISTORY":
                    {
                        VerificarParametros(campos, 3);
                        string codigo = ObtenerCodigo(campos[1]);
                        int cantidad = ObtenerEntero(campos[2]);
                        // No existe un limite maximo en el protocolo; ServicioConsultas
                        // valida 1 <= cantidad <= lecturas disponibles de ESTA estacion.
                        IReadOnlyList<Lectura> lecturas = servicio.ObtenerHistorial(codigo, cantidad);
                        var registros = lecturas.Select(l => TramaProtocolo.CrearLinea(
                            "211", "READING", Fecha(l.FechaHora), Numero(l.TemperaturaCelsius),
                            l.HumedadPorcentaje.ToString(Cultura), l.Estado.ToString())).ToList();
                        ResultadoProtocolo resultado = Multiple(registros);
                        Estado = EstadoSesion.CONSULTAS;
                        return resultado;
                    }

                    case "SUMMARY":
                    {
                        VerificarParametros(campos, 2);
                        string codigo = ObtenerCodigo(campos[1]);
                        ResumenTemperatura r = servicio.ObtenerResumenTemperatura(codigo);
                        ResultadoProtocolo resultado = Unica("200", "SUMMARY", r.CodigoEstacion,
                            r.TotalLecturas.ToString(Cultura), Numero(r.Promedio),
                            Numero(r.Minimo), Numero(r.Maximo));
                        Estado = EstadoSesion.CONSULTAS;
                        return resultado;
                    }

                    default:
                        throw new ExcepcionProtocolo(400, "COMANDO_DESCONOCIDO");
                }
            }
            catch (ExcepcionProtocolo ex)
            {
                return Unica(ex.Codigo.ToString(Cultura), "ERROR", ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return Unica("404", "ERROR", "ESTACION_INEXISTENTE");
            }
            catch (ArgumentOutOfRangeException)
            {
                return Unica("416", "ERROR", "CANTIDAD_FUERA_DE_RANGO");
            }
            catch (Exception)
            {
                return Unica("500", "ERROR", "ERROR_INTERNO");
            }
        }

        private static string ObtenerCodigo(string valor)
        {
            string codigo = valor.Trim().ToUpperInvariant();
            if (!PatronCodigo.IsMatch(codigo))
                throw new ExcepcionProtocolo(402, "CODIGO_ESTACION_INVALIDO");
            return codigo;
        }

        private static int ObtenerEntero(string valor)
        {
            if (!Regex.IsMatch(valor, @"^[+-]?[0-9]+$", RegexOptions.CultureInvariant))
                throw new ExcepcionProtocolo(402, "CANTIDAD_NO_ENTERA");
            if (!int.TryParse(valor, NumberStyles.AllowLeadingSign, Cultura, out int numero))
                throw new ExcepcionProtocolo(416, "CANTIDAD_FUERA_DE_RANGO");
            return numero;
        }

        private static void VerificarParametros(string[] campos, int cantidadEsperada)
        {
            if (campos.Length < cantidadEsperada)
                throw new ExcepcionProtocolo(401, "PARAMETRO_FALTANTE");
            if (campos.Length > cantidadEsperada)
                throw new ExcepcionProtocolo(402, "CANTIDAD_DE_CAMPOS_INVALIDA");
            for (int i = 1; i < campos.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(campos[i]))
                    throw new ExcepcionProtocolo(401, "PARAMETRO_FALTANTE");
            }
        }

        private static string Fecha(DateTime valor) =>
            valor.ToString("yyyy-MM-ddTHH:mm:ss", Cultura);
        private static string Numero(double valor) => valor.ToString("0.##", Cultura);

        private static ResultadoProtocolo Unica(params string[] campos) =>
            new ResultadoProtocolo(new[] { TramaProtocolo.CrearLinea(campos) });

        private static ResultadoProtocolo UnicaCerrar(params string[] campos) =>
            new ResultadoProtocolo(new[] { TramaProtocolo.CrearLinea(campos) }, true);

        private static ResultadoProtocolo Multiple(List<string> registros)
        {
            // Prepara y valida TODAS las lineas antes de transmitir la primera.
            var lineas = new List<string> { TramaProtocolo.CrearLinea("210", "COUNT", registros.Count.ToString(Cultura)) };
            lineas.AddRange(registros);
            lineas.Add(TramaProtocolo.CrearLinea("212", "END"));
            return new ResultadoProtocolo(lineas);
        }
    }
}
