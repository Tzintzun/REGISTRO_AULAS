# Aplicacion para el registro en aulas de la ENCB IPN.

Aplicacion de escritorio que permite a los alumnos y personal de la ENCB IPN, registrar el uso de equipos de computo en las aulas de la Unidad de informatica.
## Características principales
- Bloqueo de equipo de computo, para evitar el su uso sin registro.
- Registro de entrada y salida en un equipo de computo.
- Desbloqueo de equipo de computo por administradores.
- Formulario de salida que permite registrar el uso del equipo de computo.
- Registro de impresiones a blanco y negro o color.
- Calculo de presio porcentual para impresiones a color.
- Sancion de usuarios por no registrar salida.
- Comprobacion de alumnos sin conexion.
- Implementacion de logs.

## Estado del proyecto

El proyecto cuenta con la funcionalidad basica. En caso de que no exista conexion con la API, el sistema puede comprobar en una base de datos local cifrada si el usuario exite. En proximas actualizaciones se busca implementar un acualizacion de la base de datos y corregir errores provocados por las fallas de conexion a internet o la API.

## Requisitos del sistema.
- Sistema operativo Windows 10 o superior.
- .Net runtime desktop 8.0.12 o superior (Incluido con el instalador)
- Ghostscript
### Requisitos de desarrollo.
- Visual Studio 2022
- [Microsoft Visual Studio Installer Projects](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects)

## Instalación
El instalador se compila mediante Microsoft Visual Studio Installer Projects, en este se incluye la instlacion de **.Net runtime desktop** y el instalador de **Ghostscript**

## Capturas
### Pantalla principal

![Pantall Principal](https://i.imgur.com/nkKpVKG.png)

En la pantalla principal los usuarios pueden ingresar sus datos para registrarse. En caso de existir algun error, se mostrara un mensaje. 

#### Posibles errores.

|Mensaje|Causa|Solución|
|-------|-----|--------|
|El usuario no se encuentra registrado en la base de datos. | El usuario no está registrado en el sistema SiENCB. | Verificar el registro del usuario en la BD.|
|La IP del equipo no se encuentra registrada. | La IP configurada en la computadora no está asignada a ningún equipo en la BD. | Verificar la IP asignada en el equipo y su registro en BD.|
|El Usuario cuenta con una sesión abierta. | El usuario no registró salida en un equipo anteriormente. | Cerrar el registro desde el sistema SiENCB o en BD. |
|Error al conectarse al servidor, inténtalo más tarde. | Ocurrió algún error en el servidor. | Revisar el archivo de logs para comprobar el error. |

Si se requiere, el personal puede desbloquear el equipo dando clic en el botón Cerrar e ingresando la contraseña del administrador.

![Desbloquear imagenes](https://i.imgur.com/g1yjqki.png)

También, si la aplicación no puede conectarse al servidor de la API, la aplicación verifica en una base de datos local cifrada si el usuario existe; en caso de existir, el equipo se desbloquea.

![Usuario local](https://i.imgur.com/UuQ0TLs.png)


### Vista de bienvenida
Cuando el usuario ingresa sus datos en el  pantalla principal, de ser correctos, se muestra la fotografia del usuario, su nombre y carrera.

![Vista del usuario](https://i.imgur.com/4HKERiy.png)

### Menú de la aplicación.
Si un usuario abre sesión en un equipo, se crea un icono en la bandeja del sistema. Este icono tiene un menú para registrar salida o impresiones.

![Bandeja del sistema](https://i.imgur.com/5y7Kzc1.png)

### Registro de impresiones.
Cuando un usuario quiere registrar el cobro de impresiones, se muestra el sisguiente formulario.

![Registro de impresiones](https://i.imgur.com/KU2oVyt.png)

En esta vista se muestra el saldo del usuario, los tipos de impresiones y el costo por impresiones. Si se requiere realizar impresiones a color, el usuario puede subir un archivo en formato PDF; la aplicación calculará el porcentaje de color por ojo y el coste de esa hoja automáticamente.

### Registro de salida. 
En el formulario de registro de salida, el usuario tiene que seleccionar por lo menos un servicio y un programa utilizado.

![Regisro de salida](https://i.imgur.com/dUoDDkp.png)

Si se requiere, se puede sancionar al usuario marcando el checkbox de la esquina superior derecha. Pedirá la contraseña de administrador para hacer válida la sanción.

![Sansión](https://i.imgur.com/gajVJ2u.png)

