using System;
using System.Collections.Generic;
using ServidorTCP.Datos;
using ServidorTCP.Modelos;

namespace ServidorTCP.Servicios
{
    // Ejecuta las consultas sobre las estaciones y lecturas del repositorio
    public class ServicioConsultas
    {
        // Constantes simbolicas para el minimo de historial y el redondeo del promedio
        public const int MIN_HISTORIAL = 1;
        private const int DECIMALES_PROMEDIO = 2;

        private readonly RepositorioLecturas repositorio;

        // Inicializa el servicio con el repositorio local de lecturas
        public ServicioConsultas()
        {
            repositorio = new RepositorioLecturas();
        }

        // Devuelve la lista completa de estaciones disponibles
        public IReadOnlyList<Estacion> ListarEstaciones()
        {
            return repositorio.ObtenerTodas();
        }

        // Devuelve la lectura con la fecha y hora mas reciente de una estacion
        public Lectura ObtenerLecturaMasReciente(string codigo)
        {
            Estacion estacion = ObtenerEstacion(codigo);
            IReadOnlyList<Lectura> lecturas = estacion.Lecturas;

            Lectura reciente = lecturas[0];

            for (int i = 1; i < lecturas.Count; i++)
            {
                if (lecturas[i].FechaHora > reciente.FechaHora)
                {
                    reciente = lecturas[i];
                }
            }

            return reciente;
        }

        // Devuelve N lecturas mas recientes ordenadas por fecha; el maximo es el total de la estacion
        public IReadOnlyList<Lectura> ObtenerHistorial(string codigo, int cantidad)
        {
            Estacion estacion = ObtenerEstacion(codigo);
            int maximo = estacion.Lecturas.Count;

            if (cantidad < MIN_HISTORIAL || cantidad > maximo)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidad),
                    "La cantidad debe estar entre " + MIN_HISTORIAL
                    + " y " + maximo + " para la estacion " + estacion.Codigo + ".");
            }

            // Ordena una copia por fecha para no modificar los datos originales
            List<Lectura> ordenadas = new List<Lectura>(estacion.Lecturas);
            ordenadas.Sort((primera, segunda) => primera.FechaHora.CompareTo(segunda.FechaHora));

            int inicio = ordenadas.Count - cantidad;
            List<Lectura> historial = ordenadas.GetRange(inicio, cantidad);

            return historial.AsReadOnly();
        }

        // Calcula el promedio, minimo y maximo de temperatura de una estacion
        public ResumenTemperatura ObtenerResumenTemperatura(string codigo)
        {
            Estacion estacion = ObtenerEstacion(codigo);
            IReadOnlyList<Lectura> lecturas = estacion.Lecturas;

            double suma = 0.0;
            double minimo = lecturas[0].TemperaturaCelsius;
            double maximo = lecturas[0].TemperaturaCelsius;

            for (int i = 0; i < lecturas.Count; i++)
            {
                double temperatura = lecturas[i].TemperaturaCelsius;
                suma += temperatura;

                if (temperatura < minimo)
                {
                    minimo = temperatura;
                }

                if (temperatura > maximo)
                {
                    maximo = temperatura;
                }
            }

            double promedio = Math.Round(suma / lecturas.Count, DECIMALES_PROMEDIO);

            return new ResumenTemperatura(
                estacion.Codigo,
                promedio,
                minimo,
                maximo,
                lecturas.Count);
        }

        // Busca una estacion por codigo y lanza excepcion si no existe
        private Estacion ObtenerEstacion(string codigo)
        {
            Estacion estacion = repositorio.BuscarPorCodigo(codigo);

            if (estacion == null)
            {
                throw new KeyNotFoundException(
                    "No existe una estacion con el codigo: " + codigo);
            }

            return estacion;
        }
    }
}
