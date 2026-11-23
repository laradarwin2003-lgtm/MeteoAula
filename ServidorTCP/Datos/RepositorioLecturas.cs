using System;
using System.Collections.Generic;
using ServidorTCP.Modelos;

namespace ServidorTCP.Datos
{
    // Repositorio en memoria con las estaciones y lecturas precargadas en modo solo lectura
    public class RepositorioLecturas
    {
        private readonly IReadOnlyList<Estacion> estaciones;

        // Inicializa las 5 estaciones locales con 10, 15 y 20 lecturas repartidas en varios dias
        public RepositorioLecturas()
        {
            List<Estacion> listaInicial = new List<Estacion>
            {
                // Estacion 1: EST001 - Quito (15 lecturas)
                new Estacion(
                    "EST001",
                    "Quito Centro",
                    Ciudad.UIO,
                    new List<Lectura>
                    {
                        new Lectura(new DateTime(2026, 10, 1, 8, 0, 0), 13.5, 75, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 1, 15, 0, 0), 19.2, 58, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 2, 8, 0, 0), 12.8, 78, EstadoTiempo.Lluvioso),
                        new Lectura(new DateTime(2026, 10, 2, 15, 0, 0), 18.4, 62, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 3, 8, 0, 0), 14.0, 70, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 3, 15, 0, 0), 21.1, 49, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 4, 8, 0, 0), 13.9, 71, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 4, 15, 0, 0), 20.3, 53, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 5, 8, 0, 0), 12.4, 80, EstadoTiempo.Lluvioso),
                        new Lectura(new DateTime(2026, 10, 5, 15, 0, 0), 17.6, 66, EstadoTiempo.Lluvioso),
                        new Lectura(new DateTime(2026, 10, 6, 8, 0, 0), 13.1, 74, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 6, 15, 0, 0), 19.8, 55, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 7, 8, 0, 0), 14.3, 68, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 7, 15, 0, 0), 20.6, 51, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 8, 8, 0, 0), 13.7, 73, EstadoTiempo.ParcialmenteNublado)
                    }),

                // Estacion 2: EST002 - Guayaquil (20 lecturas)
                new Estacion(
                    "EST002",
                    "Guayaquil Malecon",
                    Ciudad.GYE,
                    new List<Lectura>
                    {
                        new Lectura(new DateTime(2026, 10, 1, 8, 0, 0), 25.4, 82, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 1, 15, 0, 0), 31.2, 65, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 2, 8, 0, 0), 25.8, 80, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 2, 15, 0, 0), 30.5, 68, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 3, 8, 0, 0), 26.1, 79, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 3, 15, 0, 0), 32.0, 62, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 4, 8, 0, 0), 25.0, 84, EstadoTiempo.Lluvioso),
                        new Lectura(new DateTime(2026, 10, 4, 15, 0, 0), 29.4, 72, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 5, 8, 0, 0), 25.6, 81, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 5, 15, 0, 0), 31.0, 66, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 6, 8, 0, 0), 26.3, 78, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 6, 15, 0, 0), 32.4, 60, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 7, 8, 0, 0), 25.9, 80, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 7, 15, 0, 0), 30.8, 67, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 8, 8, 0, 0), 25.2, 83, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 8, 15, 0, 0), 29.9, 70, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 9, 8, 0, 0), 26.0, 79, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 9, 15, 0, 0), 31.6, 64, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 10, 8, 0, 0), 25.7, 81, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 10, 15, 0, 0), 31.1, 66, EstadoTiempo.Soleado)
                    }),

                // Estacion 3: EST003 - Cuenca (10 lecturas)
                new Estacion(
                    "EST003",
                    "Cuenca Historico",
                    Ciudad.CUE,
                    new List<Lectura>
                    {
                        new Lectura(new DateTime(2026, 10, 1, 8, 0, 0), 11.8, 80, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 1, 15, 0, 0), 18.2, 60, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 2, 8, 0, 0), 11.2, 83, EstadoTiempo.Lluvioso),
                        new Lectura(new DateTime(2026, 10, 2, 15, 0, 0), 16.9, 67, EstadoTiempo.Lluvioso),
                        new Lectura(new DateTime(2026, 10, 3, 8, 0, 0), 12.5, 76, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 3, 15, 0, 0), 19.4, 55, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 4, 8, 0, 0), 12.1, 78, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 4, 15, 0, 0), 18.7, 59, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 5, 8, 0, 0), 11.6, 81, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 5, 15, 0, 0), 17.8, 63, EstadoTiempo.ParcialmenteNublado)
                    }),

                // Estacion 4: EST004 - Loja (15 lecturas)
                new Estacion(
                    "EST004",
                    "Loja Valle",
                    Ciudad.LOJ,
                    new List<Lectura>
                    {
                        new Lectura(new DateTime(2026, 10, 1, 8, 0, 0), 14.2, 76, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 1, 15, 0, 0), 20.5, 58, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 2, 8, 0, 0), 13.8, 79, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 2, 15, 0, 0), 19.6, 62, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 3, 8, 0, 0), 14.6, 74, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 3, 15, 0, 0), 21.3, 54, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 4, 8, 0, 0), 13.5, 81, EstadoTiempo.Lluvioso),
                        new Lectura(new DateTime(2026, 10, 4, 15, 0, 0), 18.9, 65, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 5, 8, 0, 0), 14.1, 77, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 5, 15, 0, 0), 20.2, 59, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 6, 8, 0, 0), 14.8, 73, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 6, 15, 0, 0), 21.0, 56, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 7, 8, 0, 0), 13.9, 78, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 7, 15, 0, 0), 19.8, 61, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 8, 8, 0, 0), 14.4, 75, EstadoTiempo.Ventoso)
                    }),

                // Estacion 5: EST005 - Manta (20 lecturas)
                new Estacion(
                    "EST005",
                    "Manta Costa",
                    Ciudad.MEC,
                    new List<Lectura>
                    {
                        new Lectura(new DateTime(2026, 10, 1, 8, 0, 0), 24.0, 80, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 1, 15, 0, 0), 28.6, 68, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 2, 8, 0, 0), 24.3, 79, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 2, 15, 0, 0), 29.1, 66, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 3, 8, 0, 0), 23.8, 82, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 3, 15, 0, 0), 27.9, 71, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 4, 8, 0, 0), 24.5, 78, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 4, 15, 0, 0), 29.4, 65, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 5, 8, 0, 0), 24.1, 80, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 5, 15, 0, 0), 28.8, 67, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 6, 8, 0, 0), 23.9, 81, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 6, 15, 0, 0), 28.2, 70, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 7, 8, 0, 0), 24.6, 77, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 7, 15, 0, 0), 29.5, 64, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 8, 8, 0, 0), 24.2, 79, EstadoTiempo.Ventoso),
                        new Lectura(new DateTime(2026, 10, 8, 15, 0, 0), 28.7, 68, EstadoTiempo.ParcialmenteNublado),
                        new Lectura(new DateTime(2026, 10, 9, 8, 0, 0), 23.7, 83, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 9, 15, 0, 0), 27.6, 72, EstadoTiempo.Nublado),
                        new Lectura(new DateTime(2026, 10, 10, 8, 0, 0), 24.4, 78, EstadoTiempo.Soleado),
                        new Lectura(new DateTime(2026, 10, 10, 15, 0, 0), 29.0, 66, EstadoTiempo.Ventoso)
                    })
            };

            estaciones = listaInicial.AsReadOnly();
        }

        // Devuelve todas las estaciones registradas en modo solo lectura
        public IReadOnlyList<Estacion> ObtenerTodas()
        {
            return estaciones;
        }

        // Busca una estacion por su codigo sin distinguir mayusculas; retorna null si no existe
        public Estacion BuscarPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return null;
            }

            string codigoLimpio = codigo.Trim();

            foreach (Estacion estacion in estaciones)
            {
                if (estacion.Codigo.Equals(codigoLimpio, StringComparison.OrdinalIgnoreCase))
                {
                    return estacion;
                }
            }

            return null;
        }
    }
}
