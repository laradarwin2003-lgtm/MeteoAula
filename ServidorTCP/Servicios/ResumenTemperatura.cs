namespace ServidorTCP.Servicios
{
    // Resultado de solo lectura con el resumen de temperatura de una estacion
    public class ResumenTemperatura
    {
        public string CodigoEstacion { get; }
        public double Promedio { get; }
        public double Minimo { get; }
        public double Maximo { get; }
        public int TotalLecturas { get; }

        // Inicializa los valores calculados del resumen de temperatura
        public ResumenTemperatura(
            string codigoEstacion,
            double promedio,
            double minimo,
            double maximo,
            int totalLecturas)
        {
            CodigoEstacion = codigoEstacion;
            Promedio = promedio;
            Minimo = minimo;
            Maximo = maximo;
            TotalLecturas = totalLecturas;
        }
    }
}
