using System.Collections.Generic;

namespace ServidorTCP.Modelos
{
    // Representa una estacion meteorologica de solo lectura con sus lecturas asociadas
    public class Estacion
    {
        public string Codigo { get; }
        public string Nombre { get; }
        public Ciudad Ciudad { get; }
        public IReadOnlyList<Lectura> Lecturas { get; }

        // Inicializa la estacion con su codigo fijo y sus lecturas de solo lectura
        public Estacion(
            string codigo,
            string nombre,
            Ciudad ciudad,
            List<Lectura> lecturas)
        {
            Codigo = codigo;
            Nombre = nombre;
            Ciudad = ciudad;
            Lecturas = lecturas.AsReadOnly();
        }
    }
}
