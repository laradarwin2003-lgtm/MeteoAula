# LOGBOOK — MeteoAula

## 1\. Información general

|Dato|Información|
|-|-|
|Proyecto|MeteoAula — Observatorio remoto|
|Asignatura|Aplicaciones Distribuidas · Proyecto 01 – 2026B|
|Solución|`ServidorTCP`, `ClienteTCP` y `Pruebas` en C# / .NET 8|
|Protocolo|METEO/1.0 sobre sockets TCP|
|Registro de pruebas previas|20 de noviembre de 2026|

**Objetivo:** dejar constancia del desarrollo, las pruebas y sus resultados. Los requisitos y criterios de aceptación están en el [Backlog](BACKLOG.md); las instrucciones para ejecutar el programa, en el [README](../README.md).



## 2\. Registro del desarrollo

Las cuatro actividades se organizan siguiendo el plan de trabajo **sugerido** por el enunciado.

|Actividad|Trabajo realizado|Resultado|
|-|-|-|
|**1. Modelado y contrato**|Arquitectura modular, comandos, estados, respuestas y reglas METEO/1.0.|Contrato definido|
|**2. Servidor y lectura**|Listener TCP, mensajes delimitados por LF, HELLO, HELP, LIST, QUIT y manejo de errores.|Servidor funcional|
|**3. Consultas y cliente**|CURRENT, HISTORY, SUMMARY; consola con configuración de IP/puerto y validación de respuestas.|Consultas e interpretación probadas.|
|**4. Integración y pruebas**|Casos positivos y negativos, límites, TCP fragmentado/agrupado, reconexión, cierre limpio y LAN.|Comportamientos satisfactorios reportados.|

## 3\. Especificación del protocolo METEO/1.0

### 3.1 Gramática, encuadre y límites

```text
SOLICITUD  := COMANDO ("|" PARAMETRO)\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\* LF
RESPUESTA  := CODIGO ("|" CAMPO)\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\* LF
LF         := byte 0x0A
CODIFICACION := UTF-8 sin BOM
MAX\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_LINEA  := 512 bytes, incluido LF
```

No se admiten `|` incrustados en campos, caracteres de control, CRLF ni BOM. Un TCP `Read` puede entregar solo parte de una línea o varias juntas: el delimitador real es LF. Fechas `yyyy-MM-ddTHH:mm:ss`, decimales con punto y estaciones `EST001`…`EST005`.

El límite de **512 bytes** se aplica a cada mensaje individual, no a toda la respuesta HISTORY. Se eligió porque las tramas tienen pocos campos de texto y números; el tamaño acota la recepción sin afectar los 20 registros de Guayaquil. El número N se valida por la cantidad de lecturas de cada estación (`1 ≤ N ≤ disponible`), no por un máximo global arbitrario.

### 3.2 Comandos, respuestas y estados

|Comando|Respuesta o efecto|
|-|-|
|`HELLO|1.0`|
|`HELP`|`200|
|`LIST`|`210|
|`CURRENT|EST001`|
|`HISTORY|EST001|
|`SUMMARY|EST001`|
|`QUIT`|`200|

La máquina de estados es **CONECTADO → IDENTIFICADO → CONSULTAS → CERRADO**. Antes de HELLO solo se permiten HELLO, HELP y QUIT. LIST y HISTORY terminan cuando se valida `212|END` tras recibir exactamente el número de registros anunciado por `COUNT`.

### 3.3 Códigos y manejo de errores

|Código|Significado|
|-|-|
|200|Operación correcta|
|210, 211, 212|Conteo, registro, fin de bloque|
|400|Comando desconocido|
|401|Parámetro faltante|
|402|Formato o codificación inválidos|
|404|Estación inexistente|
|409|Estado o HELLO repetido|
|413|Mensaje demasiado grande|
|416|Cantidad fuera de rango|
|428|HELLO requerido|
|500|Error interno|
|505|Versión incompatible|

Las respuestas de error usan `CODIGO|ERROR|MOTIVO`. Los errores normales de solicitud no cierran la sesión; una trama corrupta puede terminar únicamente la conexión afectada. El contrato completo para desarrollar otro cliente está en [BACKLOG.md](BACKLOG.md#4-contrato-del-protocolo-meteo10).

### 3.4 Ejemplos de intercambio

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
S → C: 428|ERROR|HELLO\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_REQUERIDO
C → S: HELLO|1.0
S → C: 200|WELCOME|1.0
```

Los ejemplos abreviados representan la secuencia lógica; cada línea transmitida termina con LF. Los registros `...` son solo una abreviación documental, no un mensaje real del protocolo.

## 4\. Plan de pruebas

Se identificaron casos positivos, negativos, de límite, robustez e integración. Un caso se considera **aprobado** cuando el resultado observado coincide con el esperado; si cambia el contrato, debe repetirse la prueba afectada.

|ID|Tipo|Caso|Esperado|
|-|-|-|-|
|CP-01|Positiva|HELLO, HELP y LIST|Bienvenida, ayuda y cinco estaciones.|
|CP-02|Positiva|CURRENT, HISTORY, SUMMARY|Lecturas y estadísticas coherentes con el repositorio.|
|CP-03|Negativa|LIST antes de HELLO|428 HELLO\_REQUERIDO.|
|CP-04|Negativa|Estación inexistente|404 ESTACION\_INEXISTENTE.|
|CP-05|Negativa|Comando desconocido / parámetro faltante / formato inválido|400 / 401 / 402.|
|CP-06|Negativa|HELLO repetido y versión incompatible|409 / 505.|
|CP-07|Límite|HISTORY con N fuera del total disponible|416 y sesión recuperable.|
|CP-08|Positiva/límite|HISTORY EST002 20 y EST003 2|COUNT 20 / 2, datos y END.|
|CP-09|Protocolo|Mensajes enviados fragmentados|Reensamblaje hasta LF.|
|CP-10|Protocolo|Varios mensajes en una misma escritura TCP|Una respuesta por solicitud.|
|CP-11|Validación cliente|COUNT 2, solo un registro y END|Rechazo de respuesta inconsistente.|
|CP-12|Robustez|Desconexión abrupta y nueva conexión|El servidor continúa disponible.|
|CP-13|Robustez|Ctrl+C, reinicio y conexión fallida cuando está apagado|Cierre limpio y reinicio funcional.|
|CP-14|Integración|Cliente y servidor en dos equipos|Consultas exitosas por LAN.|
|CP-15|Límite|Tramas de 512 y 513 bytes, incluido LF|512 aceptada como longitud; 513 → error 413.|
|CP-16|Integración|QUIT y RECONNECT|Despedida y nuevo HELLO obligatorio.|

## 5\. Registro de ejecución de pruebas

### 5.1 Pruebas manuales





|Caso|Resultado observado|Estado histórico|Imagen de referencia en EVIDENCIAS|
|-|-|-|-|
|CP-01|`200|WELCOME|CASO DE PRUEBA 1|
|CP-02|CURRENT EST001: 13.7 °C, 73 %; HISTORY EST003: 2 lecturas; SUMMARY EST001: media 16.31, mín. 12.4, máx. 21.1 °C.|APROBADO|CASO DE PRUEBA 2|
|CP-03|LIST antes de HELLO devolvió 428.|APROBADO|CASO DE PRUEBA 3|
|CP-04|`CURRENT|EST999` devolvió 404.|CASO DE PRUEBA 4|
|CP-05|Comando no reconocido: 400; falta de parámetro: 401; cantidad no entera: 402.|APROBADO|CASO DE PRUEBA 5|
|CP-06|HELLO repetido: 409; `HELLO|2.0`: 505.|CASO DE PRUEBA 6|
|CP-07|`HISTORY|EST003|CASO DE PRUEBA 7|
|CP-08|`HISTORY|EST002|CASO DE PRUEBA 8|
|CP-09|HELLO y CURRENT fragmentados en tres escrituras TCP; las respuestas fueron correctas.|APROBADO|CASO DE PRUEBA 9|
|CP-10|Cuatro solicitudes en una sola escritura TCP fueron procesadas con cuatro respuestas.|APROBADO|CASO DE PRUEBA 10|
|CP-11|El cliente detectó COUNT incompleto y cerró la conexión afectada.|APROBADO|CASO DE PRUEBA 11|
|CP-12|Tras simular desconexión TCP abrupta, se aceptó otra conexión.|APROBADO|CASO DE PRUEBA 12|
|CP-13|Cierre mediante Ctrl+C, reinicio y manejo del cliente con servidor apagado: correctos.|APROBADO|CASO DE PRUEBA 13.1 CIERRE DEL SERVIDOR<br />CASO DE PRUEBA 13.2 REINICIO DEL SERVIDOR|
|CP-14|El equipo confirmó HELLO, LIST, CURRENT, HISTORY, SUMMARY y QUIT entre dos computadoras.|APROBADO|CASO DE PRUEBA 14.1 SERVIDOR COMPUTADORA 1<br />CASO DE PRUEBA 14.2 CLIENTE COMPUTADORA 2|
|CP-15|conexión entre dos computadoras y funcionamiento|APROBADO|CASO DE PRUEBA 15|
|CP-16|QUIT cerró la sesión; RECONNECT exigió HELLO nuevamente.|APROBADO|CASO DE PRUEBA 16|



### 5.2 Pruebas automáticas

Las ejecuciones automáticas se registran por separado en [Pruebas.md](Pruebas.md).
Este Logbook conserva el plan de pruebas manuales, sus resultados y las referencias a evidencias.

## 6\. Evidencias y referencias de imágenes

Las imágenes se guardan en `Documentacion/Evidencias/`. En cada caso se identifica el **nombre exacto** de su imagen asociada.

## 7\. Incidencias y soluciones

|Incidencia|Ajuste aplicado|Verificación|
|-|-|-|
|Desconexión durante pausas de uso.|Timeouts de lectura/escritura ampliados a **300 000 ms** en ambos extremos.|Operación posterior correcta reportada.|
|Segunda computadora sin acceso al servidor.|Listener en `0.0.0.0:5050` y regla de entrada TCP 5050 en el firewall, sin desactivarlo.|Conexión y consultas entre equipos confirmadas.|
|Necesidad de acotar mensajes de aplicación.|Nuevo máximo de 512 bytes por línea en cliente y servidor.|operación correcta|

## 8\. Conclusiones

* La solución satisface la arquitectura de cliente-servidor con sockets TCP y un protocolo propio. Las pruebas históricas respaldan las operaciones, los estados y la robustez de la implementación previa.
* Las pruebas funcionales permitieron verificar que MeteoAula procesa correctamente las consultas meteorológicas, valida los parámetros recibidos y devuelve códigos de respuesta específicos ante solicitudes incorrectas. Además, se comprobó que los errores controlados no interrumpen innecesariamente la sesión, lo que contribuye a la estabilidad y confiabilidad del sistema.
* La comunicación entre dos computadoras de una misma red local permitió comprobar el funcionamiento distribuido de MeteoAula mediante sockets TCP. Asimismo, las pruebas de fragmentación, agrupación de mensajes, desconexión y reconexión demostraron la capacidad del sistema para gestionar diferentes situaciones de comunicación, manteniendo la integridad de las solicitudes y respuestas.

