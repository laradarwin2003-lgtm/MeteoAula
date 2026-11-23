using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Guarda exclusivamente las comprobaciones realmente ejecutadas.
// No modifica las evidencias ni las pruebas manuales del LOGBOOK.md.
internal sealed class RegistroPruebas : IDisposable
{
    private readonly List<string> filas = new List<string>();
    private readonly DateTimeOffset fecha = DateTimeOffset.Now;
    private int aprobadas;
    private int fallidas;
    private bool finalizado;

    public void Agregar(string caso, bool aprobado, string esperado, string obtenido)
    {
        filas.Add($"| {Escapar(caso)} | {Escapar(esperado)} | {Escapar(obtenido)} | " +
            (aprobado ? "APROBADO" : "FALLIDO") + " |");
        if (aprobado) aprobadas++;
        else fallidas++;
    }

    public void Dispose()
    {
        if (finalizado) return;
        finalizado = true;

        try
        {
            string archivo = BuscarArchivoPruebas();
            string contenido = CrearBloque();

            // AppendAllText crea el archivo si no existe y conserva ejecuciones anteriores.
            // No depende de marcadores dentro del LOGBOOK.md.
            File.AppendAllText(archivo, contenido, new UTF8Encoding(false));
            Console.WriteLine("Pruebas registradas en: " + archivo);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("No se pudieron guardar las pruebas en Pruebas.md: " + ex.Message);
            Environment.ExitCode = 1;
        }
    }

    private string CrearBloque()
    {
        var b = new StringBuilder();
        b.AppendLine();
        b.AppendLine($"### Ejecución: {fecha:yyyy-MM-dd HH:mm:ss zzz}");
        b.AppendLine();
        b.AppendLine("Origen: proyecto `Pruebas` (.NET 8, ejecución automática).");
        b.AppendLine();
        b.AppendLine("| Caso ejecutado | Resultado esperado | Resultado obtenido | Estado |");
        b.AppendLine("|---|---|---|---|");
        foreach (string fila in filas) b.AppendLine(fila);
        b.AppendLine();
        b.AppendLine($"**Resumen:** {filas.Count} comprobaciones ejecutadas; " +
            $"{aprobadas} aprobadas; {fallidas} fallidas.");
        b.AppendLine();
        return b.ToString();
    }

    private static string BuscarArchivoPruebas()
    {
        // AppContext.BaseDirectory es estable al ejecutar con Visual Studio o dotnet run.
        // Subimos hasta la solución, nunca escribimos en bin/Debug/net8.0.
        for (DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
             dir != null; dir = dir.Parent)
        {
            bool esSolucion = File.Exists(Path.Combine(dir.FullName, "MeteoAula.sln")) ||
                              File.Exists(Path.Combine(dir.FullName, "MeteoAula.slnx"));
            string documentacion = Path.Combine(dir.FullName, "Documentacion");
            if (esSolucion && Directory.Exists(documentacion) &&
                Directory.Exists(Path.Combine(dir.FullName, "Pruebas")))
            {
                return Path.Combine(documentacion, "Pruebas.md");
            }
        }

        throw new DirectoryNotFoundException(
            "No se encontró la carpeta Documentacion de la solución MeteoAula.");
    }

    private static string Escapar(string texto) =>
        (texto ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}
