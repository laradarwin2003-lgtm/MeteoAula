using System;
using System.IO;
using System.Text;

namespace ClienteTCP.Protocolo
{
    // Mismo encuadre METEO/1.0, sin dependencia del proyecto ServidorTCP.
    internal static class TramaCliente
    {
        private const int MaxBytes = 512; // Por linea, incluido LF.
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);

        public static void Enviar(Stream stream, params string[] campos)
        {
            if (campos.Length == 0) throw new InvalidDataException("Mensaje vacio");
            foreach (string campo in campos)
            {
                if (campo == null) throw new InvalidDataException("Campo nulo");
                foreach (char c in campo)
                    if (c == '|' || char.IsControl(c) || c == '\uFEFF')
                        throw new InvalidDataException("Campo con separador o control");
            }
            byte[] bytes = Utf8.GetBytes(string.Join("|", campos) + "\n");
            if (bytes.Length > MaxBytes)
                throw new InvalidDataException("El mensaje excede 512 bytes incluido LF");
            stream.Write(bytes, 0, bytes.Length); // TCP sincronico
            stream.Flush();
        }

        public static string[] Leer(Stream stream)
        {
            using var buffer = new MemoryStream();
            while (true)
            {
                int dato = stream.ReadByte();
                if (dato < 0) throw new EndOfStreamException("Conexion cerrada antes de completar la respuesta");
                if (buffer.Length + 1 > MaxBytes)
                    throw new InvalidDataException("Respuesta supera 512 bytes por linea");
                if (dato == '\n')
                {
                    string texto;
                    try { texto = Utf8.GetString(buffer.ToArray()); }
                    catch (DecoderFallbackException ex)
                    {
                        throw new InvalidDataException("UTF-8 invalido", ex);
                    }
                    foreach (char c in texto)
                        if (char.IsControl(c) || c == '\uFEFF')
                            throw new InvalidDataException("Caracter de control o BOM no permitido");
                    return texto.Split('|');
                }
                buffer.WriteByte((byte)dato);
            }
        }
    }
}
