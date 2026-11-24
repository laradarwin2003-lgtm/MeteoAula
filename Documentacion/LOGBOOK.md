
# LOGBOOK — MeteoAula

**Proyecto:** Observatorio remoto MeteoAula  
**Asignatura:** ITID713 — Aplicaciones Distribuidas  
**Periodo:** 2026B

| Campo | Información |
|---|---|
| Integrantes | Darwin Lara, Anahi Rojas|
| Grupo | GR1B_06 |
| Tecnología | C# / .NET 8 / sockets TCP |
| Protocolo | METEO/1.0 |
| Documentos relacionados | [BACKLOG.md](BACKLOG.md), [Pruebas.md](Pruebas.md), [README.md](../README.md) |


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
| Fecha | 12/11/2026 |
| Horario de trabajo | 11:00 - 14:00 |
| Participantes | Todos los integrantes|

**Actividades realizadas**

Se analizaron los requisitos del observatorio remoto y se definió su alcance: cinco estaciones meteorológicas en memoria, consultas de solo lectura y comunicación TCP mediante un protocolo propio. Se organizó la solución en `ServidorTCP` y `ClienteTCP` utilizando C# y .NET 8, separando dentro del servidor las capas `Modelos`, `Datos`, `Servicios`, `Protocolo` y `Red`. También se estableció el orden de desarrollo y documentación.

**Resultado:** Arquitectura acordada y alcance delimitado antes de integrar la comunicación.

**Evidencias**
(Revisar en evidencias)
- **Imagen 1.** Explorador de soluciones con ServidorTCP, ClienteTCP y las carpetas de la arquitectura .

### Sesión 2. Catálogo meteorológico, modelos y consultas

| Campo | Registro |
|---|---|
| Reunión | 2 |
| Fecha | 14/11/2026 |
| Horario de trabajo | 16:00 - 18:00 |
| Participantes | Todos los participantes |

**Actividades realizadas**

Se implementaron los modelos `Ciudad`, `EstadoTiempo`, `Lectura`, `Estacion` y `ResumenTemperatura`, junto con un repositorio en memoria de cinco estaciones: Quito (15 lecturas), Guayaquil (20), Cuenca (10), Loja (15) y Manta (20). En `ServicioConsultas` se desarrollaron los métodos para listar estaciones, obtener la lectura más reciente, consultar las últimas N lecturas y calcular el promedio, mínimo y máximo de temperatura, validando N según los registros disponibles.

**Resultado:** Catálogo y lógica de consultas disponibles para su uso por el intérprete del protocolo.

**Evidencias**
(Revisar en evidencias)
- **Imagen 2.** Clase RepositorioLecturas mostrando las estaciones y algunas lecturas precargadas.
- **Imagen 3.** Clase ServicioConsultas y sus métodos de consulta y resumen.

### Sesión 3. Diseño e implementación del protocolo METEO/1.0

| Campo | Registro |
|---|---|
| Reunión | 3 |
| Fecha | 15/11/2026 |
| Horario de trabajo | 16:00 - 18:00 |
| Participantes | Todos los participantes |

**Actividades realizadas**

Se diseñó el protocolo METEO/1.0 sobre TCP, estableciendo mensajes UTF-8 sin BOM, separador `|`, terminador LF y límite de 512 bytes por línea. Se implementaron los comandos `HELLO`, `HELP`, `LIST`, `CURRENT`, `HISTORY`, `SUMMARY` y `QUIT`, así como respuestas de múltiples registros mediante `COUNT` y `END`. También se definieron la máquina de estados y los códigos de error para solicitudes inválidas, integrando el intérprete con el servicio de consultas.

**Resultado:** Contrato del protocolo METEO/1.0 y su intérprete integrados con la lógica de consultas.

**Evidencias**
(Revisar en evidencias)
- **Imagen 4.** Código de InterpreteProtocolo mostrando comandos, validaciones o estados.
- **Imagen 5.** Consola con LIST rechazado antes de HELLO (428) y LIST aceptado después.

### Sesión 4. Servidor TCP, delimitación y manejo de conexiones

| Campo | Registro |
|---|---|
| Reunión | 4 |
| Fecha | 19/11/2026 |
| Horario de trabajo | 11:00 - !3:00 |
| Participantes | todos los participantes |

**Actividades realizadas**

Se implementó `ServidorTCP` mediante `TcpListener` y `TcpClient`, permitiendo recibir solicitudes delimitadas por LF y registrar conexiones, comandos, respuestas y cierres en consola. Se configuró el puerto 5050 y la escucha mediante `0.0.0.0` para las pruebas LAN. Además, se incorporaron mecanismos para manejar desconexiones inesperadas, tiempos de espera y cierre ordenado, realizando pruebas con mensajes fragmentados, agrupados y finalización mediante `Ctrl+C`.

**Resultado:** Servidor capaz de procesar solicitudes completas y permanecer disponible después de errores controlados.

**Evidencias**
(Revisar en evidencias)
- **Imagen 6.** Consola de ServidorTCP escuchando en 0.0.0.0:5050 y registrando operaciones.
- **Imagen 7.** Prueba de PowerShell con mensajes fragmentados o agrupados.

### Sesión 5. Cliente TCP e interpretación de consultas

| Campo | Registro |
|---|---|
| Reunión | 5 |
| Fecha | 19/11/2026 |
| Horario de trabajo | 14:00 - 16:00 |
| Participantes | Todos los participantes |

**Actividades realizadas**

Se desarrolló `ClienteTCP` como aplicación de consola con los comandos locales `CONFIG`, `CONNECT`, `RECONNECT`, `DISCONNECT`, `STATUS` y `EXIT`. Se implementó la interpretación de respuestas simples y múltiples, validando campos, tipos numéricos, cantidad de registros y marcador `END`. Posteriormente, se comprobaron los comandos `LIST`, `CURRENT`, `HISTORY` y `SUMMARY`, junto con el manejo de errores y la identificación obligatoria mediante `HELLO`.

**Resultado:** Cliente funcional para consultar estaciones meteorológicas y administrar manualmente la conexión.

**Evidencias**
(Revisar en evidencias)
- **Imagen 8.** Consola ClienteTCP con configuración de IP/puerto, CONNECT y HELLO. Consola ClienteTCP con LIST, CURRENT, HISTORY y SUMMARY y sus resultados.

### Sesión 6. Validación funcional, errores, límites y pruebas automáticas

| Campo | Registro |
|---|---|
| Reunión | 6 |
| Fecha | 21/11/2025 |
| Horario de trabajo | 14:00 - 16:00 |
| Participantes | Todos los participantes |

**Actividades realizadas**

Se organizaron los casos CP-01 a CP-16 para comprobar comandos válidos e inválidos, estados del protocolo, versiones incompatibles, estaciones inexistentes, rangos y robustez TCP. Se realizaron comprobaciones de fragmentación, agrupación de mensajes, respuestas `COUNT` incompletas y desconexiones. También se preparó la validación del límite de 512/513 bytes y se reincorporó el proyecto `Pruebas` para registrar los resultados automáticos en `Documentacion/Pruebas.md`, de forma independiente del Logbook.

**Resultado:** Pruebas funcionales organizadas y sistema de registro automático incorporado, pendiente de verificar en su ejecución definitiva.

**Evidencias**
(Revisar en evidencias)
- **Imagen 9.** Consola mostrando los códigos de error 400, 401, 402, 404, 409, 416 o 505.
- **Imagen 10.** Prueba de COUNT incompleto y END prematuro.
- **Imagen 11.** Proyecto Pruebas en Visual Studio y archivo Pruebas.md después de una ejecución real.
(Revisar en evidencias)
### Sesión 7. Integración en LAN, solución de incidencias y documentación

| Campo | Registro |
|---|---|
| Reunión | 7 |
| Fecha | 21/11/2026 |
| Horario de trabajo | 17:00 - 20:00 |
| Participantes | — |

**Actividades realizadas**

Se configuró el servidor para escuchar en `0.0.0.0:5050` y se habilitó una regla de entrada TCP en el firewall sin desactivar la protección. Se diagnosticaron problemas de conectividad y se confirmó la comunicación entre el servidor (`192.168.100.135`) y el cliente (`192.168.100.128`), incluyendo consultas meteorológicas y reconexión mediante un nuevo `HELLO`. Finalmente, se organizaron `README.md`, `BACKLOG.md`, `LOGBOOK.md` y `Pruebas.md`, preparando la documentación para su publicación en GitHub.

**Resultado:** Comunicación cliente-servidor entre dos computadoras confirmada y documentación organizada según su propósito.

**Evidencias**

- **Imagen 12.** Regla de entrada del firewall para el puerto TCP 5050.
- **Imagen 13.** Cliente remoto conectado al servidor y realizando consultas.
- **Imagen 14.** Servidor registrando la conexión desde la IP de la segunda computadora y consulta


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
| `HELP` | 200 |
| `LIST` | <code>210&#124;COUNT&#124;5</code>, cinco registros <code>211&#124;STATION...</code> y <code>212&#124;END</code>. |
| <code>CURRENT&#124;EST001</code> | Consulta de la lectura actual de la estacion |
| <code>HISTORY&#124;EST003&#124;2</code> | <code>210&#124;COUNT&#124;2</code>, dos registros <code>211&#124;READING...</code> y <code>212&#124;END</code>. |
| <code>SUMMARY&#124;EST001</code> | Historial solicitado mediante conteo, reguistros y fin de bloque|
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

---

### 3.5 Ejemplos de intercambio

**Caso exitoso:**

```text
C → S: HELLO|1.0
S → C: 200|WELCOME|1.0
C → S: LIST
S → C: 210|COUNT|5
S → C: 211|STATION|EST001|Quito Centro|UIO
S → C: ... (4 registros STATION adicionales)
S → C: 212|END
C → S: QUIT
S → C: 200|BYE
```

**Caso de error:**

```text
C → S: LIST
S → C: 428|ERROR|HELLO_REQUERIDO
C → S: HELLO|1.0
S → C: 200|WELCOME|1.0
```

Los ejemplos abreviados representan la secuencia lógica; cada línea transmitida termina con LF. Los registros `...` son solo una abreviación documental, no un mensaje real del protocolo.

---

## 4. Plan de pruebas

Se identificaron casos positivos, negativos, de límite, robustez e integración. Un caso se considera **aprobado** cuando el resultado observado coincide con el esperado; si cambia el contrato, debe repetirse la prueba afectada.

| ID | Tipo | Caso | Esperado |
|---|---|---|---|
| CP-01 | Positiva | HELLO, HELP y LIST | Bienvenida, ayuda y cinco estaciones. |
| CP-02 | Positiva | CURRENT, HISTORY, SUMMARY | Lecturas y estadísticas coherentes con el repositorio. |
| CP-03 | Negativa | LIST antes de HELLO | 428 HELLO_REQUERIDO. |
| CP-04 | Negativa | Estación inexistente | 404 ESTACION_INEXISTENTE. |
| CP-05 | Negativa | Comando desconocido / parámetro faltante / formato inválido | 400 / 401 / 402. |
| CP-06 | Negativa | HELLO repetido y versión incompatible | 409 / 505. |
| CP-07 | Límite | HISTORY con N fuera del total disponible | 416 y sesión recuperable. |
| CP-08 | Positiva/límite | HISTORY EST002 20 y EST003 2 | COUNT 20 / 2, datos y END. |
| CP-09 | Protocolo | Mensajes enviados fragmentados | Reensamblaje hasta LF. |
| CP-10 | Protocolo | Varios mensajes en una misma escritura TCP | Una respuesta por solicitud. |
| CP-11 | Validación cliente | COUNT 2, solo un registro y END | Rechazo de respuesta inconsistente. |
| CP-12 | Robustez | Desconexión abrupta y nueva conexión | El servidor continúa disponible. |
| CP-13 | Robustez | Ctrl+C, reinicio y conexión fallida cuando está apagado | Cierre limpio y reinicio funcional. |
| CP-14 | Integración | Cliente y servidor en dos equipos | Consultas exitosas por LAN. |
| CP-15 | Límite | Tramas de 512 y 513 bytes, incluido LF | 512 aceptada como longitud; 513 → error 413. |
| CP-16 | Integración | QUIT y RECONNECT | Despedida y nuevo HELLO obligatorio. |

---

## 5. Registro de ejecución de pruebas

### 5.1 Pruebas manuales

| Caso | Resultado observado | Estado histórico | Imagen de referencia en EVIDENCIAS |
|---|---|---|---|
| CP-01 | HELLO devolvió `200|WELCOME|1.0`. | APROBADO | CASO DE PRUEBA 1 |
| CP-02 | CURRENT EST001: 13.7 °C, 73 %; HISTORY EST003: 2 lecturas; SUMMARY EST001: media 16.31, mín. 12.4, máx. 21.1 °C. | APROBADO | CASO DE PRUEBA 2 |
| CP-03 | LIST antes de HELLO devolvió 428. | APROBADO | CASO DE PRUEBA 3 |
| CP-04 | `CURRENT|EST999` devolvió 404. | APROBADO | CASO DE PRUEBA 4 |
| CP-05 | Comando no reconocido: 400; falta de parámetro: 401; cantidad no entera: 402. | APROBADO | CASO DE PRUEBA 5 |
| CP-06 | HELLO repetido: 409; `HELLO|2.0`: 505. | APROBADO | CASO DE PRUEBA 6 |
| CP-07 | `HISTORY|EST003|15` devolvió 416 por cantidad fuera de rango. | APROBADO | CASO DE PRUEBA 7 |
| CP-08 | `HISTORY|EST002|20` devolvió COUNT 20, registros y END. | APROBADO | CASO DE PRUEBA 8 |
| CP-09 | HELLO y CURRENT fragmentados en tres escrituras TCP; las respuestas fueron correctas. | APROBADO | CASO DE PRUEBA 9 |
| CP-10 | Cuatro solicitudes en una sola escritura TCP fueron procesadas con cuatro respuestas. | APROBADO | CASO DE PRUEBA 10 |
| CP-11 | El cliente detectó COUNT incompleto y cerró la conexión afectada. | APROBADO | CASO DE PRUEBA 11 |
| CP-12 | Tras simular desconexión TCP abrupta, se aceptó otra conexión. | APROBADO | CASO DE PRUEBA 12 |
| CP-13 | Cierre mediante Ctrl+C, reinicio y manejo del cliente con servidor apagado: correctos. | APROBADO | CASO DE PRUEBA 13.1 CIERRE DEL SERVIDOR; CASO DE PRUEBA 13.2 REINICIO DEL SERVIDOR |
| CP-14 | El equipo confirmó HELLO, LIST, CURRENT, HISTORY, SUMMARY y QUIT entre dos computadoras. | APROBADO | CASO DE PRUEBA 14.1 SERVIDOR COMPUTADORA 1; CASO DE PRUEBA 14.2 CLIENTE COMPUTADORA 2 |
| CP-15 | Conexión entre dos computadoras y funcionamiento. | APROBADO | CASO DE PRUEBA 15 |
| CP-16 | QUIT cerró la sesión; RECONNECT exigió HELLO nuevamente. | APROBADO | CASO DE PRUEBA 16 |

### 5.2 Pruebas automáticas

Las ejecuciones automáticas se registran por separado en [Pruebas.md](Pruebas.md).

Este Logbook conserva el plan de pruebas manuales, sus resultados y las referencias a evidencias.

---

## 6. Evidencias y referencias de imágenes

Las imágenes se guardan en `Documentacion/Evidencias/`. En cada caso se identifica el **nombre exacto** de su imagen asociada.

---

## 7. Incidencias y soluciones

| Incidencia | Ajuste aplicado | Verificación |
|---|---|---|
| Desconexión durante pausas de uso. | Timeouts de lectura/escritura ampliados a **300 000 ms** en ambos extremos. | Operación posterior correcta reportada. |
| Segunda computadora sin acceso al servidor. | Listener en `0.0.0.0:5050` y regla de entrada TCP 5050 en el firewall, sin desactivarlo. | Conexión y consultas entre equipos confirmadas. |
| Necesidad de acotar mensajes de aplicación. | Nuevo máximo de 512 bytes por línea en cliente y servidor. | Operación correcta. |

---

## 8. Conclusiones

- La solución satisface la arquitectura de cliente-servidor con sockets TCP y un protocolo propio. Las pruebas históricas respaldan las operaciones, los estados y la robustez de la implementación previa.
- Las pruebas funcionales permitieron verificar que MeteoAula procesa correctamente las consultas meteorológicas, valida los parámetros recibidos y devuelve códigos de respuesta específicos ante solicitudes incorrectas. Además, se comprobó que los errores controlados no interrumpen innecesariamente la sesión, lo que contribuye a la estabilidad y confiabilidad del sistema.
- La comunicación entre dos computadoras de una misma red local permitió comprobar el funcionamiento distribuido de MeteoAula mediante sockets TCP. Asimismo, las pruebas de fragmentación, agrupación de mensajes, desconexión y reconexión demostraron la capacidad del sistema para gestionar diferentes situaciones de comunicación, manteniendo la integridad de las solicitudes y respuestas.
