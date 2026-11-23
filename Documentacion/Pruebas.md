# Pruebas automáticas — MeteoAula

Este archivo registra los resultados **reales** producidos por el proyecto `Pruebas`.

**Ejecución:** desde la carpeta de la solución, utilizar `dotnet run --project Pruebas`,
o establecer `Pruebas` como proyecto de inicio en Visual Studio y presionar `Ctrl + F5`.
No es necesario iniciar `ServidorTCP` por separado: las pruebas TCP levantan servidores temporales.

Cada ejecución agrega al final una tabla con los resultados esperados, los observados,
el estado de cada caso y un resumen. Si una comprobación falla, queda registrada
como **FALLIDO** y las pruebas no ejecutadas no se consideran aprobadas.

## Registro de ejecuciones

### Ejecución:

Origen: proyecto `Pruebas` (.NET 8, ejecución automática).

| Caso ejecutado | Resultado esperado | Resultado obtenido | Estado |
|---|---|---|---|
| estado conectado | Condicion verdadera | Condicion=True | APROBADO |
| HELLO obligatorio | Condicion verdadera | Condicion=True | APROBADO |
| CURRENT prohibido antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| HISTORY prohibido antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| SUMMARY prohibido antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| QUIT permitido antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| error no avanza estado | Condicion verdadera | Condicion=True | APROBADO |
| HELP antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| HELP no avanza estado | Condicion verdadera | Condicion=True | APROBADO |
| HELLO correcto | Condicion verdadera | Condicion=True | APROBADO |
| estado identificado | Condicion verdadera | Condicion=True | APROBADO |
| HELLO repetido | Condicion verdadera | Condicion=True | APROBADO |
| LIST COUNT+5 ITEMS+END | Condicion verdadera | Condicion=True | APROBADO |
| LIST usa Parte 1 real | Condicion verdadera | Condicion=True | APROBADO |
| estado consultas | Condicion verdadera | Condicion=True | APROBADO |
| HISTORY EST002 permite 20 (sin limite global 10) | Condicion verdadera | Condicion=True | APROBADO |
| historial ordenado ascendente por Parte 1 | Condicion verdadera | Condicion=True | APROBADO |
| HISTORY EST003 no permite 15 por validacion Parte 1 | Condicion verdadera | Condicion=True | APROBADO |
| N cero fuera de rango | Condicion verdadera | Condicion=True | APROBADO |
| N no entero formato invalido | Condicion verdadera | Condicion=True | APROBADO |
| parametro faltante | Condicion verdadera | Condicion=True | APROBADO |
| codigo mal formado | Condicion verdadera | Condicion=True | APROBADO |
| estacion inexistente | Condicion verdadera | Condicion=True | APROBADO |
| comando desconocido | Condicion verdadera | Condicion=True | APROBADO |
| parametro extra | Condicion verdadera | Condicion=True | APROBADO |
| codigo sin distinguir mayusculas | Condicion verdadera | Condicion=True | APROBADO |
| resumen de Parte 1 sin recalcular en Protocolo | Condicion verdadera | Condicion=True | APROBADO |
| cantidad mayor a disponible | Condicion verdadera | Condicion=True | APROBADO |
| errores no cierran la sesion | Condicion verdadera | Condicion=True | APROBADO |
| QUIT cierra sesion | Condicion verdadera | Condicion=True | APROBADO |
| version incompatible | Condicion verdadera | Condicion=True | APROBADO |
| UTF8 invalido | Condicion verdadera | Condicion=True | APROBADO |
| limite exacto: 512 bytes incluido LF aceptados | Condicion verdadera | Condicion=True | APROBADO |
| limite 512 bytes por linea | Condicion verdadera | Condicion=True | APROBADO |
| cliente interpreta error 428 antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| cliente envia LIST sin HELLO, servidor decide si autoriza | Condicion verdadera | Condicion=True | APROBADO |
| cliente envia HELLO solo cuando se invoca Saludar | Condicion verdadera | Condicion=True | APROBADO |
| cliente detecta END prematuro | Condicion verdadera | Condicion=True | APROBADO |
| cliente interpreta COUNT+ITEM+END | Condicion verdadera | Condicion=True | APROBADO |
| cliente envio HISTORY correcto | Condicion verdadera | Condicion=True | APROBADO |
| cliente valida campos numericos | Condicion verdadera | Condicion=True | APROBADO |
| TCP servidor inicia en puerto libre | Puerto activo > 0 | Puerto=62515 | APROBADO |
| TCP LIST antes de HELLO se rechaza | 428\|ERROR\|HELLO_REQUERIDO | 428\|ERROR\|HELLO_REQUERIDO | APROBADO |
| TCP HELP antes de HELLO funciona | 200\|HELP\|METEO/1.0\|512\|... | 200\|HELP\|METEO/1.0\|512\|HISTORY_SEGUN_ESTACION\|HELLO,LIST,CURRENT,HISTORY,SUMMARY,HELP,QUIT | APROBADO |
| TCP HELLO identifica | 200\|WELCOME\|1.0 | 200\|WELCOME\|1.0 | APROBADO |
| TCP error de estacion inexistente | 404\|ERROR\|ESTACION_INEXISTENTE | 404\|ERROR\|ESTACION_INEXISTENTE | APROBADO |
| TCP HISTORY fuera de rango | 416\|ERROR\|CANTIDAD_FUERA_DE_RANGO | 416\|ERROR\|CANTIDAD_FUERA_DE_RANGO | APROBADO |
| TCP comando desconocido | 400\|ERROR\|COMANDO_DESCONOCIDO | 400\|ERROR\|COMANDO_DESCONOCIDO | APROBADO |
| TCP parametro faltante | 401\|ERROR\|PARAMETRO_FALTANTE | 401\|ERROR\|PARAMETRO_FALTANTE | APROBADO |
| TCP formato invalido | 402\|ERROR\|CANTIDAD_NO_ENTERA | 402\|ERROR\|CANTIDAD_NO_ENTERA | APROBADO |
| TCP HISTORY sin limite global y COUNT+END | 210\|COUNT\|20, 20 ITEM, 212\|END | 210\|COUNT\|20, 20 ITEM, 212\|END | APROBADO |
| TCP QUIT responde BYE | 200\|BYE | 200\|BYE | APROBADO |
| TCP QUIT cierra el socket | EOF=-1 | EOF=-1 | APROBADO |
| TCP UTF-8 invalido: error y sesion aislada | 402\|ERROR\|UTF8_INVALIDO | 402\|ERROR\|UTF8_INVALIDO | APROBADO |
| TCP mensaje de 513 bytes: error y sesion aislada | 413\|ERROR\|MENSAJE_DEMASIADO_GRANDE | 413\|ERROR\|MENSAJE_DEMASIADO_GRANDE | APROBADO |
| TCP reconecta tras desconexion inesperada | 200\|WELCOME\|1.0 | 200\|WELCOME\|1.0 | APROBADO |
| TCP cliente inactivo se desconecta | EOF o error de socket tras timeout | Desconectado | APROBADO |
| TCP cierre ordenado del listener | Servidor finalizado sin error | Completed=True, Faulted=False | APROBADO |
| 3.a servidor disponible para configurar IP/puerto | Puerto de prueba asignado | Puerto: 62522 | APROBADO |
| 3.a configuraciones invalidas se rechazan | Mensajes claros para IP y puerto | Rechazo de configuracion: True | APROBADO |
| 3.a usuario configura IPv4 y puerto | 127.0.0.1:62522 | Configurado | APROBADO |
| 3.a se establece conexion TCP manual | Conectado a IP:puerto | Conexion establecida | APROBADO |
| 3.b error HELLO requerido comprensible | Codigo 428 explicado | Mostrado=True | APROBADO |
| 3.a HELLO manual tras CONNECT | 200 WELCOME | HELLO aceptado | APROBADO |
| 3.a LIST muestra resultados estructurados | EST001 y EST005 con campos | Estaciones renderizadas | APROBADO |
| 3.a CURRENT HISTORY SUMMARY se interpretan | Datos estructurados | Datos visibles | APROBADO |
| 3.b QUIT y DISCONNECT cierran la sesion | Cierre confirmado | Cierres anunciados | APROBADO |
| 3.b RECONNECT abre segunda conexion bajo orden manual | Dos conexiones distintas | Conexiones en la salida=2 | APROBADO |
| 3.b despues de reconectar se necesita otro HELLO | 428 en primera y segunda sesion | 428 vistos=2 | APROBADO |
| 3.b vuelve al menu tras QUIT y DISCONNECT | Menu sigue disponible | STATUS/EXIT funcionales | APROBADO |
| 3.c servidor registra conexiones en consola | Dos CONEXION ABIERTA | Conexiones=2 | APROBADO |
| 3.c servidor registra comandos recibidos | LIST, HISTORY, HELLO | Comandos en log | APROBADO |
| 3.c servidor registra desconexiones | Dos CONEXION CERRADA | Desconexiones=2 | APROBADO |
| 3.c servidor detiene el listener correctamente | Task completada | Completed=True | APROBADO |
| 3.b error de conexion permite nuevo intento | Fallo comprensible, consola viva | Fallo controlado | APROBADO |

**Resumen:** 75 comprobaciones ejecutadas; 75 aprobadas; 0 fallidas.


### Ejecución: 2026-11-23 09:49:46 -05:00

Origen: proyecto `Pruebas` (.NET 8, ejecución automática).

| Caso ejecutado | Resultado esperado | Resultado obtenido | Estado |
|---|---|---|---|
| estado conectado | Condicion verdadera | Condicion=True | APROBADO |
| HELLO obligatorio | Condicion verdadera | Condicion=True | APROBADO |
| CURRENT prohibido antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| HISTORY prohibido antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| SUMMARY prohibido antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| QUIT permitido antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| error no avanza estado | Condicion verdadera | Condicion=True | APROBADO |
| HELP antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| HELP no avanza estado | Condicion verdadera | Condicion=True | APROBADO |
| HELLO correcto | Condicion verdadera | Condicion=True | APROBADO |
| estado identificado | Condicion verdadera | Condicion=True | APROBADO |
| HELLO repetido | Condicion verdadera | Condicion=True | APROBADO |
| LIST COUNT+5 ITEMS+END | Condicion verdadera | Condicion=True | APROBADO |
| LIST usa Parte 1 real | Condicion verdadera | Condicion=True | APROBADO |
| estado consultas | Condicion verdadera | Condicion=True | APROBADO |
| HISTORY EST002 permite 20 (sin limite global 10) | Condicion verdadera | Condicion=True | APROBADO |
| historial ordenado ascendente por Parte 1 | Condicion verdadera | Condicion=True | APROBADO |
| HISTORY EST003 no permite 15 por validacion Parte 1 | Condicion verdadera | Condicion=True | APROBADO |
| N cero fuera de rango | Condicion verdadera | Condicion=True | APROBADO |
| N no entero formato invalido | Condicion verdadera | Condicion=True | APROBADO |
| parametro faltante | Condicion verdadera | Condicion=True | APROBADO |
| codigo mal formado | Condicion verdadera | Condicion=True | APROBADO |
| estacion inexistente | Condicion verdadera | Condicion=True | APROBADO |
| comando desconocido | Condicion verdadera | Condicion=True | APROBADO |
| parametro extra | Condicion verdadera | Condicion=True | APROBADO |
| codigo sin distinguir mayusculas | Condicion verdadera | Condicion=True | APROBADO |
| resumen de Parte 1 sin recalcular en Protocolo | Condicion verdadera | Condicion=True | APROBADO |
| cantidad mayor a disponible | Condicion verdadera | Condicion=True | APROBADO |
| errores no cierran la sesion | Condicion verdadera | Condicion=True | APROBADO |
| QUIT cierra sesion | Condicion verdadera | Condicion=True | APROBADO |
| version incompatible | Condicion verdadera | Condicion=True | APROBADO |
| UTF8 invalido | Condicion verdadera | Condicion=True | APROBADO |
| limite exacto: 512 bytes incluido LF aceptados | Condicion verdadera | Condicion=True | APROBADO |
| limite 512 bytes por linea | Condicion verdadera | Condicion=True | APROBADO |
| cliente interpreta error 428 antes de HELLO | Condicion verdadera | Condicion=True | APROBADO |
| cliente envia LIST sin HELLO, servidor decide si autoriza | Condicion verdadera | Condicion=True | APROBADO |
| cliente envia HELLO solo cuando se invoca Saludar | Condicion verdadera | Condicion=True | APROBADO |
| cliente detecta END prematuro | Condicion verdadera | Condicion=True | APROBADO |
| cliente interpreta COUNT+ITEM+END | Condicion verdadera | Condicion=True | APROBADO |
| cliente envio HISTORY correcto | Condicion verdadera | Condicion=True | APROBADO |
| cliente valida campos numericos | Condicion verdadera | Condicion=True | APROBADO |
| TCP servidor inicia en puerto libre | Puerto activo > 0 | Puerto=62555 | APROBADO |
| TCP LIST antes de HELLO se rechaza | 428\|ERROR\|HELLO_REQUERIDO | 428\|ERROR\|HELLO_REQUERIDO | APROBADO |
| TCP HELP antes de HELLO funciona | 200\|HELP\|METEO/1.0\|512\|... | 200\|HELP\|METEO/1.0\|512\|HISTORY_SEGUN_ESTACION\|HELLO,LIST,CURRENT,HISTORY,SUMMARY,HELP,QUIT | APROBADO |
| TCP HELLO identifica | 200\|WELCOME\|1.0 | 200\|WELCOME\|1.0 | APROBADO |
| TCP error de estacion inexistente | 404\|ERROR\|ESTACION_INEXISTENTE | 404\|ERROR\|ESTACION_INEXISTENTE | APROBADO |
| TCP HISTORY fuera de rango | 416\|ERROR\|CANTIDAD_FUERA_DE_RANGO | 416\|ERROR\|CANTIDAD_FUERA_DE_RANGO | APROBADO |
| TCP comando desconocido | 400\|ERROR\|COMANDO_DESCONOCIDO | 400\|ERROR\|COMANDO_DESCONOCIDO | APROBADO |
| TCP parametro faltante | 401\|ERROR\|PARAMETRO_FALTANTE | 401\|ERROR\|PARAMETRO_FALTANTE | APROBADO |
| TCP formato invalido | 402\|ERROR\|CANTIDAD_NO_ENTERA | 402\|ERROR\|CANTIDAD_NO_ENTERA | APROBADO |
| TCP HISTORY sin limite global y COUNT+END | 210\|COUNT\|20, 20 ITEM, 212\|END | 210\|COUNT\|20, 20 ITEM, 212\|END | APROBADO |
| TCP QUIT responde BYE | 200\|BYE | 200\|BYE | APROBADO |
| TCP QUIT cierra el socket | EOF=-1 | EOF=-1 | APROBADO |
| TCP UTF-8 invalido: error y sesion aislada | 402\|ERROR\|UTF8_INVALIDO | 402\|ERROR\|UTF8_INVALIDO | APROBADO |
| TCP mensaje de 513 bytes: error y sesion aislada | 413\|ERROR\|MENSAJE_DEMASIADO_GRANDE | 413\|ERROR\|MENSAJE_DEMASIADO_GRANDE | APROBADO |
| TCP reconecta tras desconexion inesperada | 200\|WELCOME\|1.0 | 200\|WELCOME\|1.0 | APROBADO |
| TCP cliente inactivo se desconecta | EOF o error de socket tras timeout | Desconectado | APROBADO |
| TCP cierre ordenado del listener | Servidor finalizado sin error | Completed=True, Faulted=False | APROBADO |
| 3.a servidor disponible para configurar IP/puerto | Puerto de prueba asignado | Puerto: 62569 | APROBADO |
| 3.a configuraciones invalidas se rechazan | Mensajes claros para IP y puerto | Rechazo de configuracion: True | APROBADO |
| 3.a usuario configura IPv4 y puerto | 127.0.0.1:62569 | Configurado | APROBADO |
| 3.a se establece conexion TCP manual | Conectado a IP:puerto | Conexion establecida | APROBADO |
| 3.b error HELLO requerido comprensible | Codigo 428 explicado | Mostrado=True | APROBADO |
| 3.a HELLO manual tras CONNECT | 200 WELCOME | HELLO aceptado | APROBADO |
| 3.a LIST muestra resultados estructurados | EST001 y EST005 con campos | Estaciones renderizadas | APROBADO |
| 3.a CURRENT HISTORY SUMMARY se interpretan | Datos estructurados | Datos visibles | APROBADO |
| 3.b QUIT y DISCONNECT cierran la sesion | Cierre confirmado | Cierres anunciados | APROBADO |
| 3.b RECONNECT abre segunda conexion bajo orden manual | Dos conexiones distintas | Conexiones en la salida=2 | APROBADO |
| 3.b despues de reconectar se necesita otro HELLO | 428 en primera y segunda sesion | 428 vistos=2 | APROBADO |
| 3.b vuelve al menu tras QUIT y DISCONNECT | Menu sigue disponible | STATUS/EXIT funcionales | APROBADO |
| 3.c servidor registra conexiones en consola | Dos CONEXION ABIERTA | Conexiones=2 | APROBADO |
| 3.c servidor registra comandos recibidos | LIST, HISTORY, HELLO | Comandos en log | APROBADO |
| 3.c servidor registra desconexiones | Dos CONEXION CERRADA | Desconexiones=2 | APROBADO |
| 3.c servidor detiene el listener correctamente | Task completada | Completed=True | APROBADO |
| 3.b error de conexion permite nuevo intento | Fallo comprensible, consola viva | Fallo controlado | APROBADO |

**Resumen:** 75 comprobaciones ejecutadas; 75 aprobadas; 0 fallidas.

