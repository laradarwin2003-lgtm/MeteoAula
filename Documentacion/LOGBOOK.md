
# LOGBOOK — MeteoAula

**Proyecto:** Observatorio remoto MeteoAula  
**Asignatura:** ITID713 — Aplicaciones Distribuidas  
**Periodo:** 2026B

| Campo | Información |
|---|---|
| Integrantes | — |
| Grupo | — |
| Tecnología | C# / .NET 8 / sockets TCP |
| Protocolo | METEO/1.0 |
| Documentos relacionados | [BACKLOG.md](BACKLOG.md), [Pruebas.md](Pruebas.md), [README.md](../README.md) |

> **Nota de registro:** las siete sesiones siguientes son una reconstrucción retrospectiva propuesta a partir del trabajo desarrollado en MeteoAula. Las fechas, horarios y participantes deben completarse con los datos reales. Las imágenes se incorporarán en `Documentacion/Evidencias/`.

## 1. Objetivos y alcance

- Implementar un sistema distribuido cliente-servidor de consultas meteorológicas mediante sockets TCP.
- Mantener cinco estaciones con datos precargados y permitir consultas y resúmenes de temperatura.
- Definir un protocolo propio con comandos, delimitación, códigos de respuesta, estados y validaciones.
- Comprobar la funcionalidad, robustez y comunicación entre dos computadoras, documentando las evidencias.

**Alcance:** consultas de solo lectura, sin autenticación ni base de datos; atención de un cliente a la vez. Las pruebas automáticas se registran en `Pruebas.md`.

---

## 2. Registro del desarrollo: siete sesiones

### Sesión 1. Planificación del proyecto y definición de la arquitectura

| Campo | Registro |
|---|---|
| Reunión | 1 |
| Fecha | — |
| Horario de trabajo | — |
| Participantes | — |

**Actividades realizadas**

Se analizaron los requisitos del observatorio remoto y se definió su alcance: cinco estaciones meteorológicas en memoria, consultas de solo lectura y comunicación TCP mediante un protocolo propio. Se organizó la solución en `ServidorTCP` y `ClienteTCP` utilizando C# y .NET 8, separando dentro del servidor las capas `Modelos`, `Datos`, `Servicios`, `Protocolo` y `Red`. También se estableció el orden de desarrollo y documentación.

**Resultado:** Arquitectura acordada y alcance delimitado antes de integrar la comunicación.

**Evidencias**

- **Imagen 1.** Explorador de soluciones con ServidorTCP, ClienteTCP y las carpetas de la arquitectura.

### Sesión 2. Catálogo meteorológico, modelos y consultas

| Campo | Registro |
|---|---|
| Reunión | 2 |
| Fecha | — |
| Horario de trabajo | — |
| Participantes | — |

**Actividades realizadas**

Se implementaron los modelos `Ciudad`, `EstadoTiempo`, `Lectura`, `Estacion` y `ResumenTemperatura`, junto con un repositorio en memoria de cinco estaciones: Quito (15 lecturas), Guayaquil (20), Cuenca (10), Loja (15) y Manta (20). En `ServicioConsultas` se desarrollaron los métodos para listar estaciones, obtener la lectura más reciente, consultar las últimas N lecturas y calcular el promedio, mínimo y máximo de temperatura, validando N según los registros disponibles.

**Resultado:** Catálogo y lógica de consultas disponibles para su uso por el intérprete del protocolo.

**Evidencias**

- **Imagen 2.** Clase RepositorioLecturas mostrando las estaciones y algunas lecturas precargadas.
- **Imagen 3.** Clase ServicioConsultas y sus métodos de consulta y resumen.

### Sesión 3. Diseño e implementación del protocolo METEO/1.0

| Campo | Registro |
|---|---|
| Reunión | 3 |
| Fecha | — |
| Horario de trabajo | — |
| Participantes | — |

**Actividades realizadas**

Se diseñó el protocolo METEO/1.0 sobre TCP, estableciendo mensajes UTF-8 sin BOM, separador `|`, terminador LF y límite de 512 bytes por línea. Se implementaron los comandos `HELLO`, `HELP`, `LIST`, `CURRENT`, `HISTORY`, `SUMMARY` y `QUIT`, así como respuestas de múltiples registros mediante `COUNT` y `END`. También se definieron la máquina de estados y los códigos de error para solicitudes inválidas, integrando el intérprete con el servicio de consultas.

**Resultado:** Contrato del protocolo METEO/1.0 y su intérprete integrados con la lógica de consultas.

**Evidencias**

- **Imagen 4.** Código de InterpreteProtocolo mostrando comandos, validaciones o estados.
- **Imagen 5.** Consola con LIST rechazado antes de HELLO (428) y LIST aceptado después.

### Sesión 4. Servidor TCP, delimitación y manejo de conexiones

| Campo | Registro |
|---|---|
| Reunión | 4 |
| Fecha | — |
| Horario de trabajo | — |
| Participantes | — |

**Actividades realizadas**

Se implementó `ServidorTCP` mediante `TcpListener` y `TcpClient`, permitiendo recibir solicitudes delimitadas por LF y registrar conexiones, comandos, respuestas y cierres en consola. Se configuró el puerto 5050 y la escucha mediante `0.0.0.0` para las pruebas LAN. Además, se incorporaron mecanismos para manejar desconexiones inesperadas, tiempos de espera y cierre ordenado, realizando pruebas con mensajes fragmentados, agrupados y finalización mediante `Ctrl+C`.

**Resultado:** Servidor capaz de procesar solicitudes completas y permanecer disponible después de errores controlados.

**Evidencias**

- **Imagen 6.** Consola de ServidorTCP escuchando en 0.0.0.0:5050 y registrando operaciones.
- **Imagen 7.** Prueba de PowerShell con mensajes fragmentados o agrupados.

### Sesión 5. Cliente TCP e interpretación de consultas

| Campo | Registro |
|---|---|
| Reunión | 5 |
| Fecha | — |
| Horario de trabajo | — |
| Participantes | — |

**Actividades realizadas**

Se desarrolló `ClienteTCP` como aplicación de consola con los comandos locales `CONFIG`, `CONNECT`, `RECONNECT`, `DISCONNECT`, `STATUS` y `EXIT`. Se implementó la interpretación de respuestas simples y múltiples, validando campos, tipos numéricos, cantidad de registros y marcador `END`. Posteriormente, se comprobaron los comandos `LIST`, `CURRENT`, `HISTORY` y `SUMMARY`, junto con el manejo de errores y la identificación obligatoria mediante `HELLO`.

**Resultado:** Cliente funcional para consultar estaciones meteorológicas y administrar manualmente la conexión.

**Evidencias**

- **Imagen 8.** Consola ClienteTCP con configuración de IP/puerto, CONNECT y HELLO.
- **Imagen 9.** Consola ClienteTCP con LIST, CURRENT, HISTORY y SUMMARY y sus resultados.

### Sesión 6. Validación funcional, errores, límites y pruebas automáticas

| Campo | Registro |
|---|---|
| Reunión | 6 |
| Fecha | — |
| Horario de trabajo | — |
| Participantes | — |

**Actividades realizadas**

Se organizaron los casos CP-01 a CP-16 para comprobar comandos válidos e inválidos, estados del protocolo, versiones incompatibles, estaciones inexistentes, rangos y robustez TCP. Se realizaron comprobaciones de fragmentación, agrupación de mensajes, respuestas `COUNT` incompletas y desconexiones. También se preparó la validación del límite de 512/513 bytes y se reincorporó el proyecto `Pruebas` para registrar los resultados automáticos en `Documentacion/Pruebas.md`, de forma independiente del Logbook.

**Resultado:** Pruebas funcionales organizadas y sistema de registro automático incorporado, pendiente de verificar en su ejecución definitiva.

**Evidencias**

- **Imagen 10.** Consola mostrando los códigos de error 400, 401, 402, 404, 409, 416 o 505.
- **Imagen 11.** Prueba de COUNT incompleto y END prematuro.
- **Imagen 12.** Prueba del límite de 512 y 513 bytes.
- **Imagen 13.** Proyecto Pruebas en Visual Studio y archivo Pruebas.md después de una ejecución real.

### Sesión 7. Integración en LAN, solución de incidencias y documentación

| Campo | Registro |
|---|---|
| Reunión | 7 |
| Fecha | — |
| Horario de trabajo | — |
| Participantes | — |

**Actividades realizadas**

Se configuró el servidor para escuchar en `0.0.0.0:5050` y se habilitó una regla de entrada TCP en el firewall sin desactivar la protección. Se diagnosticaron problemas de conectividad y se confirmó la comunicación entre el servidor (`192.168.100.135`) y el cliente (`192.168.100.128`), incluyendo consultas meteorológicas y reconexión mediante un nuevo `HELLO`. Finalmente, se organizaron `README.md`, `BACKLOG.md`, `LOGBOOK.md` y `Pruebas.md`, preparando la documentación para su publicación en GitHub.

**Resultado:** Comunicación cliente-servidor entre dos computadoras confirmada y documentación organizada según su propósito.

**Evidencias**

- **Imagen 14.** Regla de entrada del firewall para el puerto TCP 5050.
- **Imagen 15.** Cliente remoto conectado al servidor y realizando consultas.
- **Imagen 16.** Servidor registrando la conexión desde la IP de la segunda computadora.
- **Imagen 17.** Consola con QUIT, RECONNECT, error 428 y nuevo HELLO.

---

## 3. Especificación resumida del protocolo METEO/1.0

### 3.1 Gramática, delimitación y límites

```text
SOLICITUD    := COMANDO ("|" PARAMETRO)* LF
RESPUESTA    := CODIGO ("|" CAMPO)* LF
LF           := byte 0x0A
CODIFICACION := UTF-8 sin BOM
MAX_LINEA    := 512 bytes, incluido LF
```

Cada línea se interpreta una vez recibido LF, incluso si TCP entrega una parte o varias líneas en una lectura. No se admite CRLF, BOM, caracteres de control ni el separador `|` dentro de los campos. El límite se aplica **a cada línea**, no al total de un bloque HISTORY. Los decimales utilizan punto, la fecha `yyyy-MM-ddTHH:mm:ss` y los códigos de estación tienen el formato `EST` seguido de tres dígitos.

### 3.2 Comandos y respuestas

| Solicitud | Respuesta o efecto |
|---|---|
| <code>HELLO&#124;1.0</code> | <code>200&#124;WELCOME&#124;1.0</code> |
| `HELP` | Ayuda 200 con versión, límite y comandos disponibles. |
| `LIST` | <code>210&#124;COUNT&#124;5</code>, cinco registros <code>211&#124;STATION...</code> y <code>212&#124;END</code>. |
| <code>CURRENT&#124;EST001</code> | <code>200&#124;CURRENT...</code> con fecha/hora, temperatura, humedad y estado. |
| <code>HISTORY&#124;EST003&#124;2</code> | <code>210&#124;COUNT&#124;2</code>, dos registros <code>211&#124;READING...</code> y <code>212&#124;END</code>. |
| <code>SUMMARY&#124;EST001</code> | <code>200&#124;SUMMARY...</code> con total, promedio, mínimo y máximo. |
| `QUIT` | <code>200&#124;BYE</code> y cierre de sesión. |

En HISTORY, N puede ser cualquier entero entre 1 y la cantidad de lecturas disponibles de la estación; no se establece un máximo global arbitrario. LIST y HISTORY concluyen después de recibir el número anunciado de registros y `212|END`.

### 3.3 Estados de sesión

`CONECTADO → IDENTIFICADO → CONSULTAS → CERRADO`.

Antes de `HELLO` solamente se permiten `HELLO`, `HELP` y `QUIT`. Una consulta previa a HELLO genera el error 428, mientras que repetir HELLO genera el error 409. Los errores normales no cierran la sesión ni modifican su estado.

### 3.4 Códigos principales y manejo de errores

| Código | Significado |
|---|---|
| 200 | Éxito y respuestas simples |
| 210/211/212 | COUNT / registro / END |
| 400 | Comando desconocido |
| 401 | Parámetro faltante |
| 402 | Formato o codificación inválidos |
| 404 | Estación inexistente |
| 409 | Operación incompatible con el estado |
| 413 | Mensaje demasiado grande |
| 416 | Cantidad fuera de rango |
| 428 | HELLO requerido |
| 500 | Error interno |
| 505 | Versión incompatible |

Las respuestas de error siguen `CODIGO|ERROR|MOTIVO`. Los errores de trama pueden cerrar únicamente la conexión afectada. El servidor permanece disponible para nuevos clientes.

### 3.5 Ejemplos de intercambio

**Caso correcto:**

```text
C → S: HELLO|1.0
S → C: 200|WELCOME|1.0
C → S: HISTORY|EST003|2
S → C: 210|COUNT|2
S → C: 211|READING|2026-10-05T08:00:00|11.6|81|Nublado
S → C: 211|READING|2026-10-05T15:00:00|17.8|63|ParcialmenteNublado
S → C: 212|END
C → S: QUIT
S → C: 200|BYE
```

**Caso de error de estado:**

```text
C → S: LIST
S → C: 428|ERROR|HELLO_REQUERIDO
C → S: HELLO|1.0
S → C: 200|WELCOME|1.0
```

El contrato completo del protocolo está documentado en [BACKLOG.md](BACKLOG.md).

---

## 4. Registro resumido de pruebas manuales y evidencias

La siguiente tabla relaciona los casos de prueba con las sesiones y sus evidencias. Los resultados de las pruebas automáticas se encuentran en `Pruebas.md`.

| Caso | Verificación | Situación según antecedentes | Evidencia |
|---|---|---|---|
| CP-01 | HELLO, HELP y LIST | Indicada en la secuencia de comprobación | Imagen 5 y 9 |
| CP-02 | CURRENT, HISTORY y SUMMARY | Indicada en la secuencia de comprobación | Imagen 9 |
| CP-03 | LIST antes de HELLO: error 428 | Verificada en pruebas previas | Imagen 5 |
| CP-04 | Estación inexistente: error 404 | Verificada en pruebas previas | Imagen 10 |
| CP-05 | Comando desconocido, parámetro faltante y formato inválido: 400/401/402 | Verificada en pruebas previas | Imagen 10 |
| CP-06 | HELLO repetido y versión incompatible: 409/505 | Verificada en pruebas previas | Imagen 10 |
| CP-07 | HISTORY fuera de rango: 416 | Verificada en pruebas previas | Imagen 10 |
| CP-08 | HISTORY EST002 20 y respuesta COUNT + END | Verificada en pruebas previas | Imagen 9 |
| CP-09 | Comandos TCP fragmentados | Verificada en pruebas previas | Imagen 7 |
| CP-10 | Mensajes agrupados en una escritura TCP | Verificada en pruebas previas | Imagen 7 |
| CP-11 | COUNT incompleto y END prematuro | Referida en pruebas previas | Imagen 11 |
| CP-12 | Desconexión abrupta | Referida en pruebas previas | Imagen 6 |
| CP-13 | Ctrl+C, liberación del puerto y reinicio | Referida en pruebas previas | Imagen 6 |
| CP-14 | Comunicación entre dos computadoras | Confirmada en la integración LAN | Imagen 15 y 16 |
| CP-15 | Límite de 512 y 513 bytes | Pendiente de confirmar con resultado de la versión actual | Imagen 12 |
| CP-16 | QUIT, RECONNECT y nuevo HELLO | Por corroborar en la evidencia final | Imagen 17 |

**Pruebas automáticas:** las ejecuciones del proyecto `Pruebas` se registran en [Pruebas.md](Pruebas.md), sin duplicar los resultados en este documento.

---

## 5. Cierre y conclusiones

- La separación de Datos, Servicios, Protocolo y Red permite mantener la lógica meteorológica independiente de la transmisión mediante sockets TCP.
- La máquina de estados, los códigos de error y las respuestas COUNT + END proporcionan un intercambio verificable y evitan interpretar mensajes incompletos como resultados válidos.
- La comunicación entre dos computadoras evidenció la importancia de configurar correctamente las direcciones IP, el puerto de escucha y el firewall sin desactivar la protección del sistema.
- El registro por sesiones complementa el contrato del Backlog, las instrucciones del README y el historial de pruebas automáticas.

**Por completar:** fechas, horarios, participantes, capturas de Imagen 1 a Imagen 17 y comprobación final de los casos CP-15 y CP-16.

