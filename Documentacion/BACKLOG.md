# BACKLOG — MeteoAula

## 1\. Información general

**Proyecto:** Observatorio remoto MeteoAula · **Versión de trabajo:** 4 (mensajes de 512 bytes) · **Tecnología:** C# / .NET 8 / TCP.

**Objetivo:** definir las funcionalidades y criterios de aceptación del sistema. La sección 4 contiene el contrato de comunicación necesario para implementar un cliente distinto de `ClienteTCP`. Las pruebas manuales y sus evidencias se documentan en el [Logbook](LOGBOOK.md); los resultados de las ejecuciones automáticas se registran en [Pruebas.md](Pruebas.md).

**Alcance:** consultas meteorológicas de solo lectura sobre cinco estaciones con datos en memoria; atención de un cliente a la vez, sin base de datos ni autenticación.

## 2\. Requisitos funcionales

|ID|Requisito|Criterio de aceptación|Implementación|
|-|-|-|-|
|**RF-01.a**|Catálogo de estaciones|Mínimo 5 estaciones con código, nombre y ciudad.|`ServidorTCP/Datos/RepositorioLecturas.cs`|
|**RF-01.b**|Lecturas meteorológicas|Al menos 10 lecturas por estación, con fecha/hora, °C, humedad y estado.|`RepositorioLecturas.cs`, `Modelos/`|
|**RF-01.c**|Consultas del catálogo|LIST, CURRENT y HISTORY devuelven los datos correctos; `1 ≤ N ≤ lecturas disponibles` por estación.|`ServicioConsultas.cs`, `InterpreteProtocolo.cs`|
|**RF-01.d**|Resumen de temperaturas|SUMMARY devuelve total de registros, promedio, mínimo y máximo.|`ServicioConsultas.cs`|
|**RF-02.a**|Contrato METEO/1.0|Solicitudes y respuestas delimitadas, codificación, versión y límite de longitud definidos.|`TramaProtocolo.cs`, `TramaCliente.cs`|
|**RF-02.b**|Operaciones del protocolo|HELLO, LIST, CURRENT, HISTORY, SUMMARY, HELP y QUIT.|`InterpreteProtocolo.cs`|
|**RF-02.c**|Respuestas y errores|Códigos diferenciados para éxito, comando, parámetros, formato, estación y rango.|`InterpreteProtocolo.cs`|
|**RF-02.d**|Estados de sesión|Antes de HELLO solo se permiten HELLO, HELP y QUIT; QUIT cierra la sesión.|`InterpreteProtocolo.cs`|
|**RF-02.e**|Respuesta con varios registros|LIST/HISTORY entregan `COUNT`, N registros y `END`.|`InterpreteProtocolo.cs`, `ClienteProtocolo.cs`|
|**RF-02.f**|Interpretación del cliente|El cliente valida campos, tipos, recuentos y terminación antes de mostrar resultados.|`ClienteProtocolo.cs`|
|**RF-03.a**|Conexión configurable|Configurar IPv4 y puerto, conectar, enviar consultas y mostrar resultados estructurados.|`ClienteTCP/Red/ClienteTcp.cs`|
|**RF-03.b**|Recuperación de errores|Mensajes comprensibles, detección de desconexión y reconexión manual.|`ClienteTcp.cs`|
|**RF-03.c**|Registro de actividad|Registrar conexiones, comandos, respuestas y cierres en consola del servidor.|`ServidorTCP/Red/ServidorTcp.cs`|

Las cinco estaciones incluidas son `EST001` Quito (15 lecturas), `EST002` Guayaquil (20), `EST003` Cuenca (10), `EST004` Loja (15) y `EST005` Manta (20).

## 3\. Requisitos no funcionales y técnicos

|ID|Condición|Criterio de aceptación|
|-|-|-|
|**RNF-01 Robustez**|Una solicitud inválida no termina el servidor; desconexiones y `Ctrl+C` se gestionan de forma controlada.|El servidor continúa funcionando después de solicitudes inválidas y desconexiones inesperadas. Permite nuevas conexiones y finaliza ordenadamente mediante Ctrl+C.|
|**RNF-02 Interoperabilidad**|El contrato de la sección 4 permite programar un cliente TCP independiente.|Un cliente externo puede comunicarse con el servidor utilizando TCP y las reglas documentadas de METEO/1.0, sin depender de las clases de ClienteTCP.|
|**RNF-03 Modularidad**|Socket, interpretación, servicio de consultas y datos tienen responsabilidades separadas.|El código está organizado en capas con responsabilidades independientes. La comunicación, interpretación, consultas y acceso a datos se encuentran separados.|
|**RNF-04 Verificabilidad**|Hay pruebas positivas, negativas, de límites y TCP; resultados y evidencias en el Logbook.|Se dispone de al menos ocho casos de prueba que incluyen situaciones positivas, negativas, límites y comunicación TCP, con resultados y evidencias registrados en el Logbook.|
|**RNF-05 Alcance**|Una conexión atendida a la vez es suficiente; concurrencia no obligatoria.|El servidor atiende correctamente un cliente a la vez y puede aceptar una nueva conexión cuando finaliza la sesión anterior.|
|**RT-01 Tecnologías**|C#, sockets `TcpClient`/`TcpListener`, cliente de consola y datos precargados en memoria.|La solución utiliza C# y .NET 8, comunicación mediante TcpClient/TcpListener, un cliente de consola y datos precargados en memoria, sin requerir base de datos.|
|**RT-02 Restricciones**|Sin Web API, HTTP, REST ni mensajería que sustituya los sockets; funcionamiento local y en LAN.|La comunicación utiliza sockets TCP sin Web API, HTTP, REST ni sistemas de mensajería externos. Se verifica su funcionamiento en localhost y entre dos computadoras de una red local.|

## 4\. Contrato del protocolo METEO/1.0

### 4.1 Encuadre y convenciones

* **Transporte:** TCP. Cada mensaje es una línea de texto **UTF-8 sin BOM** terminada por **LF (`0x0A`)**, no CRLF. Los mensajes pueden llegar fragmentados o agrupados; hay que leer hasta LF.
* **Sintaxis:** `CAMPO1|CAMPO2|...|CAMPON\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\n`. No se admiten separadores `|` dentro de los campos, caracteres de control, retornos de carro ni BOM; no existe mecanismo de escape.
* **Tamaño:** máximo **512 bytes por línea incluyendo LF**, medidos en bytes UTF-8 y no en caracteres. Si la línea supera el límite, responde `413|ERROR|MENSAJE\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_DEMASIADO\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_GRANDE` y la conexión afectada puede cerrarse. No es un límite al conjunto completo de un historial.Se eligio este valor  ya que, una solicitud/respuesta individual solo contiene códigos, texto breve y campos numéricos; 512 bytes ofrecen margen para los registros actuales y establecen un tope controlado a cada línea. El límite debe cumplirse en solicitudes **y** respuestas.
* **Formato de datos:** fecha `yyyy-MM-ddTHH:mm:ss` sin zona horaria; decimales con punto; códigos de estación `EST` + tres dígitos (`EST001`); ciudad `UIO`, `GYE`, `CUE`, `LOJ` o `MEC`.
* **Historial:** `N` es entero entre 1 y el total disponible para la estación, sin máximo global artificial; las últimas N lecturas se envían en orden cronológico ascendente.



### 4.2 Comandos y respuestas

|Solicitud del cliente|Respuesta del servidor|
|-|-|
|`HELLO|1.0`|
|`HELP`|`200|
|`LIST`|`210|
|`CURRENT|EST001`|
|`HISTORY|EST001|
|`SUMMARY|EST001`|
|`QUIT`|`200|

**Regla para respuestas múltiples:** el cliente debe leer exactamente N registros después de `COUNT`, verificar su estructura y exigir `212|END`. Una línea `END` no cierra la conexión TCP; es el fin de ese bloque de respuesta.

### 4.3 Máquina de estados

|Estado|Operaciones válidas|Transición|
|-|-|-|
|`CONECTADO`|HELLO, HELP, QUIT|HELLO válido → IDENTIFICADO; QUIT → CERRADO|
|`IDENTIFICADO`|LIST, CURRENT, HISTORY, SUMMARY, HELP, QUIT|Primera consulta correcta → CONSULTAS; QUIT → CERRADO|
|`CONSULTAS`|LIST, CURRENT, HISTORY, SUMMARY, HELP, QUIT|Permanece en CONSULTAS; QUIT → CERRADO|
|`CERRADO`|Ninguna: socket cerrado|Fin de la sesión|

Antes de HELLO, una consulta responde `428|ERROR|HELLO\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_REQUERIDO`. Repetir HELLO después de identificarse responde `409|ERROR|HELLO\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_YA\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_EJECUTADO`. Un error normal de solicitud no cambia el estado.

### 4.4 Códigos del protocolo

|Código|Significado representativo|
|-|-|
|`200`|Operación correcta, bienvenida, consulta simple, ayuda o despedida.|
|`210` / `211` / `212`|Inicio de bloque / registro / fin de bloque.|
|`400`|`COMANDO\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_DESCONOCIDO`|
|`401`|`PARAMETRO\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_FALTANTE`|
|`402`|Formato inválido, código incorrecto, cantidad no entera, UTF-8 inválido, etc.|
|`404`|`ESTACION\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_INEXISTENTE`|
|`409`|Operación incompatible con el estado.|
|`413`|`MENSAJE\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_DEMASIADO\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_GRANDE`|
|`416`|`CANTIDAD\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_FUERA\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_DE\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_RANGO`|
|`428`|`HELLO\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_REQUERIDO`|
|`500`|`ERROR\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_INTERNO`|
|`505`|`VERSION\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_INCOMPATIBLE`|

Los errores se codifican como `CODIGO|ERROR|MOTIVO`. Los errores de trama pueden cerrar solo el socket afectado; el servidor permanece disponible para nuevas conexiones.

### 4.5 Ejemplo mínimo de interoperabilidad

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

**Diferencia importante:** `CONFIG`, `CONNECT`, `RECONNECT`, `DISCONNECT`, `STATUS` y `EXIT` son operaciones de consola de `ClienteTCP`, **no se envían por METEO/1.0**.

## 5\. Estado final de los requisitos

|Bloque|Implementación en el código|Verificación documentada|
|-|-|-|
|RF-01: catálogo y consultas|Implementado|resultado en evidencias con pruebas manuales|
|RF-02: protocolo y estados|Implementado|resultado en evidencias|
|RF-03: cliente y conexión|Implementado|Pruebas en localhost y entre dos equipos satisfactorias|
|RNF-01 a RNF-05|Implementado|Véase [LOGBOOK.md](LOGBOOK.md), con evidencias disponibles|



