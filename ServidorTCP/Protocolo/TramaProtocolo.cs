using System;
using System.IO;
using System.Text;

namespace ServidorTCP.Protocolo
{
    public static class EspecificacionMeteo
    {
        public const string Nombre = "METEO/1.0";
        public const string Version = "1.0";
        public const int MaxBytesPorLinea = 512; // Incluye LF, no es limite de HISTORY.
        public static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
    }

    public sealed class ExcepcionProtocolo : Exception
    {
        public int Codigo { get; }
        public ExcepcionProtocolo(int codigo, string mensaje) : base(mensaje)
        {
            Codigo = codigo;
        }
    }

    // Encuadre de mensajes: una trama = texto UTF-8 SIN BOM + LF.
    // Esta clase no crea sockets: la clase Red existente le proporciona el Stream TCP.
    public static class TramaProtocolo
    {
        public static string CrearLinea(params string[] campos)
        {
            if (campos == null || campos.Length == 0)
                throw new ExcepcionProtocolo(402, "FORMATO_INVALIDO");

            foreach (string campo in campos)
            {
                if (campo == null)
                    throw new ExcepcionProtocolo(402, "CAMPO_NULO");
                foreach (char c in campo)
                {
                    if (c == '|' || char.IsControl(c) || c == '\uFEFF')
                        throw new ExcepcionProtocolo(402, "CARACTER_NO_PERMITIDO");
                }
            }

            string linea = string.Join("|", campos);
            ValidarLinea(linea);
            return linea; // LF se agrega solo al transmitir.
        }

        public static void ValidarLinea(string linea)
        {
            if (linea == null)
                throw new ExcepcionProtocolo(402, "FORMATO_INVALIDO");
            foreach (char c in linea)
            {
                if (char.IsControl(c) || c == '\uFEFF')
                    throw new ExcepcionProtocolo(402, "CARACTER_NO_PERMITIDO");
            }

            int bytes;
            try { bytes = EspecificacionMeteo.Utf8.GetByteCount(linea) + 1; }
            catch (EncoderFallbackException)
            {
                throw new ExcepcionProtocolo(402, "UTF8_INVALIDO");
            }
            if (bytes > EspecificacionMeteo.MaxBytesPorLinea)
                throw new ExcepcionProtocolo(413, "MENSAJE_DEMASIADO_GRANDE");
        }

        public static void Escribir(Stream flujo, string linea)
        {
            ValidarLinea(linea);
            byte[] datos = EspecificacionMeteo.Utf8.GetBytes(linea + "\n");
            flujo.Write(datos, 0, datos.Length); // Síncrono.
            flujo.Flush();
        }

        public static string? Leer(Stream flujo)
        {
            using var buffer = new MemoryStream();
            while (true)
            {
                int siguiente = flujo.ReadByte(); // Síncrono.
                if (siguiente < 0)
                {
                    if (buffer.Length == 0) return null;
                    throw new ExcepcionProtocolo(402, "MENSAJE_SIN_LF");
                }
                if (buffer.Length + 1 > EspecificacionMeteo.MaxBytesPorLinea)
                    throw new ExcepcionProtocolo(413, "MENSAJE_DEMASIADO_GRANDE");
                if (siguiente == '\n')
                {
                    string linea;
                    try { linea = EspecificacionMeteo.Utf8.GetString(buffer.ToArray()); }
                    catch (DecoderFallbackException)
                    {
                        throw new ExcepcionProtocolo(402, "UTF8_INVALIDO");
                    }
                    ValidarLinea(linea);
                    return linea;
                }
                buffer.WriteByte((byte)siguiente);
            }
        }
    }
}
