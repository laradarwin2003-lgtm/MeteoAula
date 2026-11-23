# MeteoAula — Observatorio remoto

Proyecto académico de **Aplicaciones Distribuidas (Proyecto 01 – 2026B)**. Implementa consultas meteorológicas de solo lectura sobre un protocolo propio, **METEO/1.0**, mediante sockets TCP.

---

## 1. Descripción

Un servidor ofrece un catálogo de cinco estaciones con 80 lecturas precargadas; el cliente consulta las estaciones, la lectura más reciente, un historial y estadísticas de temperatura.

El sistema atiende **un cliente a la vez**. No tiene base de datos, HTTP ni autenticación.

## 2. Tecnologías y requisitos

- C#, **.NET 8** y `System.Net.Sockets` (`TcpClient` / `TcpListener`).
- Visual Studio con compatibilidad para .NET 8 **o** el SDK .NET 8 para compilar desde una terminal.
- Windows o un sistema compatible con .NET 8; acceso a la red local para la prueba entre equipos.
- Protocolo: UTF-8 sin BOM, campos separados por `|`, una línea por mensaje terminada en LF (`\n`), hasta **512 bytes por línea, incluido LF**.

## 3. Estructura de la solución

```text
MeteoAula/
├── MeteoAula.sln
├── ServidorTCP/
│   ├── Modelos/      # Entidades meteorológicas
│   ├── Datos/        # Repositorio precargado en memoria
│   ├── Servicios/    # Consultas y estadísticas
│   ├── Protocolo/    # Mensajes, validaciones y estados
│   └── Red/          # Listener y conexiones TCP
├── ClienteTCP/
│   ├── Protocolo/    # Interpretación y validación de respuestas
│   └── Red/          # Conexión y consola interactiva
├── Pruebas/          # Comprobaciones automáticas y registro en Documentacion/Pruebas.md
└── Documentacion/
    ├── BACKLOG.md
    ├── LOGBOOK.md
    ├── README.md
    ├── Pruebas.md    # Resultados automáticos registrados por ejecución
    └── Evidencias/   # Capturas disponibles
```

La solución contiene `ServidorTCP` y `ClienteTCP`, además del proyecto `Pruebas` para ejecutar verificaciones automatizadas.

## 4. Instalación y ejecución

Una vez descargada la solución se debe descomprimir y abrir el archivo ejecutable `MeteoAula.sln`, después se debe marcar como proyectos de inicio al `ServidorTCP` y `ClienteTCP` en ese orden.

### 4.1. Localhost

- Se debe correr el programa. El servidor se iniciará primero y mostrará la siguiente salida:

**En `ServidorTCP`:**

```text
--------------------------------------------------------
[FECHA HORA] ESCUCHANDO METEO/1.0 en 0.0.0.0:5050
--------------------------------------------------------
```

`Ctrl+C` detiene el servidor de forma ordenada.

- Después, en el cliente, usar esta configuración y usar el aplicativo:

**En `ClienteTCP`:**

```text
CONFIG 127.0.0.1 5050
CONNECT
HELLO
LIST
CURRENT EST001
HISTORY EST003 2
SUMMARY EST001
QUIT
```

`CONNECT` **no envía `HELLO` automáticamente**. La sesión debe identificarse manualmente antes de consultar. `QUIT` cierra la conexión remota, no la aplicación del cliente.

### 4.2. Dos computadoras en red local

**En la computadora del servidor:** se debe solo ejecutar el servidor.

**En la computadora del cliente:** realizamos la siguiente configuración, cambia la dirección por la **IPv4 real del servidor**:

```text
CONFIG 192.168.100.135 5050
CONNECT
HELLO
LIST
QUIT
```

`192.168.100.135` es la dirección utilizada en una prueba de desarrollo; puede ser distinta en otra red. Ambos equipos deben estar en una red que permita comunicarse.

Si es necesario, agrega en Windows una **regla de entrada TCP para el puerto 5050**, restringida a red privada y a la IP del cliente (opcional).

**No se debe desactivar el firewall ni exponer este servicio a Internet.**

**Visual Studio:** selecciona `ServidorTCP` como proyecto de inicio y ejecútalo; después inicia `ClienteTCP` en otra instancia. El perfil de inicio incluido del servidor contiene `5050 0.0.0.0`, pensado para la prueba de LAN. Para limitarlo a localhost, cambia el perfil a `5050 127.0.0.1` o utiliza el comando local anterior. Detén el servidor ordenadamente con `Ctrl+C`.

#### Habilitación del puerto TCP 5050

**Procedimiento**

1. En la computadora servidor, presionar Windows + R.
2. Escribir `wf.msc` y presionar Enter.
3. Seleccionar Reglas de entrada.
4. Hacer clic en Nueva regla.
5. Seleccionar Puerto y presionar Siguiente.
6. Seleccionar TCP e introducir el puerto local 5050.
7. Seleccionar Permitir la conexión.
8. Marcar el perfil Privado de la red de confianza.
9. Asignar el nombre MeteoAula - TCP 5050.
10. Finalizar y comprobar que la regla esté habilitada.

**Parámetros de la regla**

| Parámetro | Valor |
|---|---|
| Tipo de regla | Entrada |
| Protocolo | TCP |
| Puerto | 5050 |
| Acción | Permitir conexión |
| Perfil | Privado |

**Configuración del servidor**

En Visual Studio se establecieron los siguientes argumentos de inicio:

```text
5050 0.0.0.0
```

El servidor escucha en el puerto 5050 mediante las interfaces IPv4 disponibles.

**Verificación de conectividad**

Desde la segunda computadora se debe ejecutar:

```powershell
Test-NetConnection 192.168.100.135 -Port 5050
```

El resultado esperado deberá ser:

```text
TcpTestSucceeded : True
```

### 4.3. Configuración y reconexión

El cliente admite `CONFIG <IP> <PUERTO>`, `CONNECT [IP PUERTO]`, `RECONNECT`, `DISCONNECT`, `STATUS` y `EXIT`.

Después de cada reconexión es necesario enviar `HELLO` nuevamente.

El timeout de envío/recepción configurado es de **300 000 ms (5 minutos)**.

### 4.4. Pruebas automáticas

Ejecutar desde la carpeta que contiene `MeteoAula.sln`:

```powershell
dotnet run --project Pruebas
```

También se puede establecer `Pruebas` como proyecto de inicio en Visual Studio y ejecutar con `Ctrl + F5`.

El proyecto inicia servidores temporales en puertos libres: **no es necesario iniciar `ServidorTCP` aparte**.

Cada ejecución agrega sus resultados reales a [Pruebas.md](Pruebas.md), sin modificar el [LOGBOOK.md](LOGBOOK.md) ni sus evidencias manuales.

Las comprobaciones de tamaño verifican **512 y 513 bytes**, incluido LF.

## 5. Comandos disponibles

| Comando en ClienteTCP | Uso |
|---|---|
| `HELLO` | Inicia la sesión METEO/1.0 (`HELLO\|`) |
| `LIST` | Lista las cinco estaciones. |
| `CURRENT EST001` | Consulta la lectura más reciente. |
| `HISTORY EST002 20` | Consulta hasta N lecturas disponibles para esa estación. |
| `SUMMARY EST001` | Obtiene promedio, mínimo y máximo de temperatura. |
| `HELP` | Muestra las operaciones disponibles. |
| `QUIT` | Cierra correctamente la sesión TCP. |

`CONFIG`, `CONNECT`, `RECONNECT`, `DISCONNECT`, `STATUS` y `EXIT` **son órdenes locales del cliente** y no forman parte de los siete comandos METEO/1.0.

## 6. Documentación adicional

- [BACKLOG — requisitos, criterios de aceptación y contrato para otros clientes](BACKLOG.md).
- [LOGBOOK — especificación resumida, pruebas, resultados y evidencias](LOGBOOK.md).
- [Pruebas — registro de ejecuciones automáticas](Pruebas.md).
