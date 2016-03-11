# EMTNow
Proyecto personal en Windows Phone 8.1 que hace uso de los servicios web públicos de la EMT. La versión desarrollada hasta ahora es capaz de consultar los tiempos de espera de los autobuses de la Comunidad de Madrid, gestionar las paradas favoritas del usuario y localizar las paradas cercanas.

Para ejecutar la solución de Visual Studio en local, hay que registrarse previamente en la web de OpenData de la EMT:
http://opendata.emtmadrid.es/Formulario.aspx

Una vez realizado ese paso, tendremos acceso a la API de la EMT mediante un usuario y contraseña que hay que actualizar en el código fuente localizado en Comun\Constantes.cs dentro del proyecto:

public const string IdClient = "TODO";
public const string PassKey = "TODO";
