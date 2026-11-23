using System;

namespace ServidorTCP.Modelos
{
    // Representa una lectura meteorologica de solo lectura
    public class Lectura
    {
        public DateTime FechaHora { get; }
        public double TemperaturaCelsius { get; }
        public int HumedadPorcentaje { get; }
        public EstadoTiempo Estado { get; }

        // Inicializa los datos inmutables de la lectura
        public Lectura(
            DateTime fechaHora,
            double temperaturaCelsius,
            int humedadPorcentaje,
            EstadoTiempo estado)
        {
            FechaHora = fechaHora;
            TemperaturaCelsius = temperaturaCelsius;
            HumedadPorcentaje = humedadPorcentaje;
            Estado = estado;
        }
    }
}
