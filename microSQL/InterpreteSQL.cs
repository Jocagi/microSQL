using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace microSQL
{
    public class InterpreteSQL
    {
        //Intrucciones SQL...
        public static void leerInstrucciones(string texto)
        {
            //Leer palabras reservadas
            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();

            if (string.IsNullOrEmpty(texto)) //No se han escrito instrucciones
            {
                //Mensaje de error
                error("No se han escrito instrucciones");
            }
            else
            {
                //Buscar el comando 'GO'
                if (!texto.Contains(palabrasReservadas["GO"]))
                {
                    //No se encontro el comando en las instrucciones
                    //Mensaje de error
                    error("No se encontró el comando 'GO' al final de las instrucciones");
                }
                else
                {
                    /*Se separaran todas las instrucciones con la plabra 'GO', luego se reemplazaran las palabras reservadas 
                      y tipos de datos por caracteres griegos (por simplicidad) para identificar donde comienzan y terminar 
                      las distintas partes de la instruccion */

                    //separar diferentes instrucciones

                    List<string> instrucciones = separarComandos(texto).ToList();


                    //Verificar si la palabra final fue un 'GO'
                    instrucciones[instrucciones.Count - 1] = instrucciones.Last().Replace(" ", "");

                    if (instrucciones.Last() == "")
                    {
                        //Eliminar instruccion vacia
                        instrucciones.RemoveAt(instrucciones.Count - 1);
                        //Recorrer instrucciones
                        foreach (var item in instrucciones)
                        {

                            //Verificar formato
                            if (!contienePalabrasReservadas(item))
                            {
                                //Buscar instrucciones

                                if (item.Contains(palabrasReservadas["CREATE TABLE"])) //crear tabla
                                {
                                    crearTabla(item);
                                }
                                else if (item.Contains(palabrasReservadas["SELECT"])) //seleccionar 
                                {
                                    seleccionarDatos(item);
                                }
                                else if (item.Contains(palabrasReservadas["INSERT INTO"])) //insertar valores
                                {
                                    insertarDatos(item);
                                }
                                else if (item.Contains(palabrasReservadas["DELETE"])) //borrar filas
                                {
                                    eliminarFilas(item);
                                }
                                else if (item.Contains(palabrasReservadas["DROP TABLE"])) //borrar tabla
                                {
                                    eliminarTabla(item);
                                }
                                else if (item.Contains(palabrasReservadas["UPDATE"])) //actualizar tabla
                                {
                                    actualizarDatos(item);
                                }
                            }
                            else
                            {
                                //Mensaje de error (Formato Incorrecto)
                                error("Formato incorrecto");
                            }
                        }
                    }
                    else
                    {
                        //Error no se escribio 'GO'
                        error("No se encontró el comando 'GO' al final de las insrucciones");
                    }

                }
            }
        }

        private static Dictionary<string, string> obtenerPalabrasReservadas()
        {
            //Copia el diccionario del controlador previamente obtenido leyendo los archivos de configuracion
            return microSQL.Controllers.HomeController.palabrasReservadas;
        }

        private static string[] separarStringConAlgoEncerrado(string texto, char separador, char ignorar)
        {
            //Separar cadenas teniendo en cuenta las apostrofes ' ', parentesis ( ) u otros caracteres que encierran strings....
            // '\u0027' apostrofe
            List<string> Resultado = new List<string>();
            int count = 0;
            string palabra = "";
            bool caracterEspecial = false;

            //Buscar en la cadena donde aparecen los apostrofes y tomar el valor de string dentro de ellos

            for (int i = 0; i < texto.Length; i++)
            {
                if (texto.Substring(count, 1) != separador.ToString()) //comparar cada letra con el separador
                {
                    if (texto.Substring(count, 1) == ignorar.ToString()) // apostrofe
                    {
                        caracterEspecial = !caracterEspecial; //cambiar el estado de un ' encontrado
                    }
                    else if (texto.Substring(count, 1) == ' '.ToString()) //espacio
                    {
                        //hacer nada
                    }
                    else
                    {
                        palabra += texto.Substring(count, 1);
                    }
                    count++;
                }
                else if (texto.Substring(count, 1) == separador.ToString() && caracterEspecial == true)
                {
                    palabra += texto.Substring(count, 1);
                    count++;
                }
                else
                {
                    Resultado.Add(palabra); //Agregar al vector
                    palabra = ""; //Reiniciar palabra
                    count++;
                }
                if (i == texto.Length - 1) //Ultimo valor del array
                {
                    Resultado.Add(palabra); //Agregar al vector
                }
            }

            return Resultado.ToArray();
        }

        private static string[] separarComandos(string texto)
        {
            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();

            //Resumen: Separar por cada comando con palabra 'GO' al final

            //Convertir saltos de linea en espacios
            texto = texto.Replace('\u000D', ' ');
            texto = texto.Replace('\u000A', ' ');

            //To DO...
            //Verificar palabra 'GO' al final


            //Reemplazar el valor de la palabra GO con un simbolo ~ para separarlo facilmente
            texto = texto.Replace(palabrasReservadas["GO"], "~");

            return texto.Split('~');

        }

        private static bool contienePalabrasReservadas(string texto)
        {
            //Resumen: Verifica si el texto posee otra palabra reservada que no sea el comando principal... debido a que cada instruccion solo deberi tener una de estas palabras...
            bool encontrado = false;
            int contador = 0;
            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();

            foreach (var item in palabrasReservadas)
            {
                if (item.Value == palabrasReservadas["CREATE TABLE"] || item.Value == palabrasReservadas["INSERT INTO"] || item.Value == palabrasReservadas["SELECT"] || item.Value == palabrasReservadas["DELETE"] || item.Value == palabrasReservadas["DROP TABLE"] || item.Value == palabrasReservadas["UPDATE"])
                {
                    //Verificar si el texto posee una palaba prohibida
                    if (texto.Contains(item.Value))
                    {
                        //se encontro una palabra prohibida
                        contador++;
                    }
                }
            }

            //Solo deberia haber un comando en la instruccion 
            if (contador != 1)
            {
                encontrado = true;
            }

            return encontrado;
        }

        private static string sustituirPalabrasReservadasPorCaracteres(string texto)
        {
            //Simplifica palabras reservadas con un simple caracter

            /*
                                Lista de palabras reservadas:

                                α β θ π Ω μ

                                α          CREATE TABLE
                                α          INSERT INTO
                                α          SELECT
                                α          DELETE
                                α          DROP TABLE
                                α          UPDATE

                                Δ          FROM
                                |          WHERE

                                ~          VALUES
                                %          LIKE
                                           GO

                                ρ	       PRIMARY KEY
                                π          INT
                                β          VARCHAR(100)
                                Ω          DATETIME

                            */

            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();

            texto = texto.Replace(palabrasReservadas["CREATE TABLE"], "α");
            texto = texto.Replace(palabrasReservadas["INSERT INTO"], "α");
            texto = texto.Replace(palabrasReservadas["SELECT"], "α");
            texto = texto.Replace(palabrasReservadas["DELETE"], "α");
            texto = texto.Replace(palabrasReservadas["DROP TABLE"], "α");
            texto = texto.Replace(palabrasReservadas["UPDATE"], "α");
            texto = texto.Replace(palabrasReservadas["FROM"], "Δ");
            texto = texto.Replace(palabrasReservadas["WHERE"], "|");
            texto = texto.Replace(palabrasReservadas["VALUES"], "~");
            texto = texto.Replace("LIKE", "%");

            texto = texto.Replace("PRIMARY KEY", "ρ");
            texto = texto.Replace("INT", "π");
            texto = texto.Replace("VARCHAR(100)", "β");
            texto = texto.Replace("DATETIME", "Ω");

            texto = texto.Replace('(', '#');
            texto = texto.Replace(')', '#');

            return texto;
        }

        private static string[] eliminarPosicionesVacias(string[] array)
        {
            //Devuelve un array que no inclue posiciones con "" como valor

            List<string> nuevoArray = new List<string>();

            foreach (var item in array)
            {
                string temp = item.Replace(" ", ""); //Corregir error en el que se colaban posiciones sólo con espacios

                if (item != "" && temp != "")
                {
                    nuevoArray.Add(item);
                }
            }

            return nuevoArray.ToArray();
        }

        private static string eliminarEspacios(string texto)
        {
            //Se recibe un string con espacios extras al inicio o al final y se eliminan

            string[] nuevoTexto = texto.Split(' ');
            nuevoTexto = eliminarPosicionesVacias(nuevoTexto);

            string resultado = "";

            foreach (var item in nuevoTexto)
            {
                resultado += item;
            }

            return resultado;
        }

        private static string[] corregirArreglo(string[] array)
        {
            //Se recibe un arreglo previamente convalores separados por coma y se formatea manerade eliminar espacios innecesarios
            //Corregir saltos de linea
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = eliminarEspacios(array[i]);
            }
            array = eliminarPosicionesVacias(array);

            return array;
        }

        private static Dictionary<string, string> diccionarioCaracteres()
        {
            //Simplifica palabras reservadas con un simple caracter

            /*
                                Lista de palabras reservadas:

                                α β θ π Ω μ

                                α          CREATE TABLE
                                α          INSERT INTO
                                α          SELECT
                                α          DELETE
                                α          DROP TABLE
                                α          UPDATE

                                Δ          FROM
                                |          WHERE

                                ~          VALUES
                                %          LIKE
                                           GO

                                ρ	       PRIMARY KEY
                                π          INT
                                β          VARCHAR(100)
                                Ω          DATETIME

                            */
            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();
            Dictionary<string, string> diccionario = new Dictionary<string, string>();
            diccionario.Add(palabrasReservadas["CREATE TABLE"], "α");
            diccionario.Add(palabrasReservadas["INSERT INTO"], "α");
            diccionario.Add(palabrasReservadas["SELECT"], "α");
            diccionario.Add(palabrasReservadas["DELETE"], "α");
            diccionario.Add(palabrasReservadas["DROP TABLE"], "α");
            diccionario.Add(palabrasReservadas["UPDATE"], "α");
            diccionario.Add(palabrasReservadas["FROM"], "Δ");
            diccionario.Add(palabrasReservadas["WHERE"], "|");
            diccionario.Add(palabrasReservadas["VALUES"], "~");
            diccionario.Add("LIKE", "%");
            diccionario.Add("PRIMARY KEY", "ρ");
            diccionario.Add("INT", "π");
            diccionario.Add("VARCHAR(100)", "β");
            diccionario.Add("DATETIME", "Ω");

            return diccionario;
        } //Ya no sirve

        private static Queue<char> encolarCaracteres(string texto)
        {
            List<char> caracteres = texto.ToList<char>();
            Queue<char> cola = new Queue<char>();

            foreach (var item in caracteres)
            {
                cola.Enqueue(item);
            }

            return cola;
        } //Ya no sirve

        private static void error()
        {
            //Escribir mensaje de error en pantalla

            error("");
        }

        public static void error(string mensaje)
        {
            //Escribir mensaje de error en pantalla

            if (String.IsNullOrEmpty(mensaje))
            {
                Controllers.HomeController.mensaje = "Error";
            }
            else if (mensaje.Contains("error") || mensaje.Contains("Error") || mensaje.Contains("ERROR"))
            {
                Controllers.HomeController.mensaje = mensaje;
            }
            else
            {
                Controllers.HomeController.mensaje = "Error: " + mensaje;
            }
        }

        private static string modificarCadenaParaBusquedas(string texto)
        { 
            //El proposito de este metodo es resolver un problema al recibir una cadena con el metodo 'SELECT'
            //y separar adecuadamente el texto


            // Se agregan los caracteres # # al inicio y al final de la parte en la que se enccuentra la descripcion de las columnas
            string resultado = "";

            if (!String.IsNullOrEmpty(texto))
            {
                foreach (char letra in texto)
                {
                    switch (letra)
                    {
                        case 'α':
                            resultado += (letra + " # ");
                            break;
                        case 'Δ':
                            resultado += (" # " + letra);
                            break;
                        default:
                            resultado += letra;
                            break;
                    }
                }

            }

            return resultado;

        }

        //Metodos con instrucciones
        //--------------------------------------------------------------------------------------

        public static void crearTabla(string texto)
        {

            /*
            Ejemplo instrucciones:
            CREATE TABLE
            <nombre de la tabla>
            (
            <nombre> <tipo de dato> <primary key>,
            <nombre> <tipo de dato>,
            <nombre> <tipo de dato>
            ...
            )
             */

            //Pasos a seguir...
            bool buscarNombreTabla = false;
            bool buscarLlavePrimaria = false;

            bool error = false;

            //Variables
            string nombreTabla = "";
            string llave = "";
            List<string> tiposDeDato = new List<string>();
            List<string> columnas = new List<string>();

            //Comienza Procedimiento........................

            //Buscar si el texto posee una llave primaria
            if (texto.Contains("PRIMARY KEY"))
            {
                //Reemplazar palabras reservadas por caracteres simples
                texto = sustituirPalabrasReservadasPorCaracteres(texto);

                //Se separa todas las instrucciones (Manteniendo lo que estaba encerrado en parentesis intancto)
                string[] sentences = separarStringConAlgoEncerrado(texto, ' ', '#');

                //Recorrer cada uno de los fragmentos en array

                for (int i = 0; i < sentences.Length; i++)
                {
                    if (error == true)
                    {
                        break;
                    }

                    if (sentences[i] != "")//ignorar valores vacios en array
                    {
                        //Paso 1 Buscar el 'CREATE TABLE'
                        if (sentences[i] == "α" && buscarNombreTabla == false)
                        {
                            //Paso 2 Crear nombre de la tabla
                            for (i = i + 1; i < sentences.Length; i++)
                            {
                                //Buscar nombre de la tabla
                                if (sentences[i] != "")
                                {
                                    //Se encontro el nombre de la tabla
                                    buscarNombreTabla = true;
                                    nombreTabla = sentences[i];
                                    break;
                                }
                                //Se llego al final del arreglo y no se encontro
                                if (i == sentences.Length - 1)
                                {
                                    error = true;
                                    break;
                                }
                            }
                        }
                        //Paso 3 Buscar Listado de Columnas
                        else if (sentences[i] != "" && buscarNombreTabla == true)
                        {
                            string[] listado = sentences[i].Split(',');

                            //recorrer posibles valores de columnas

                            for (int j = 0; j < listado.Length; j++)
                            {
                                string nombre = "";
                                string tipoDeDato = "";


                                //Paso 3.1 Buscar nombre de columna
                                //Paso 3.2 Definir tipo de dato de columna
                                //Paso 3.3 Definir si es la llave

                                //separar nombre de columna de su tipo de dato
                                string[] columna = listado[j].Split(' ');

                                foreach (var item in columna)
                                {
                                    if (item != "") //ignorar posiciones vacias
                                    {
                                        if (nombre == "")//aun no se ha asignado un nombre
                                        {
                                            nombre = item;
                                        }
                                        else if (tipoDeDato == "")//aun no se ha asignado un tipo de dato
                                        {
                                            //Buscar valor de tipo de dato
                                            switch (item)
                                            {
                                                case "π":
                                                    tipoDeDato = "INT";
                                                    break;
                                                case "β":
                                                    tipoDeDato = "VARCHAR(100)";
                                                    break;
                                                case "Ω":
                                                    tipoDeDato = "DATETIME";
                                                    break;
                                                default:
                                                    error = true;
                                                    break;
                                            }
                                            if (error == true)
                                            {
                                                break;
                                            }
                                        }
                                        else if (item == "ρ" && buscarLlavePrimaria == false)//se asigna como llave primaria
                                        {
                                            buscarLlavePrimaria = true;
                                            llave = nombre;
                                        }
                                        else
                                        {
                                            error = true;
                                            break;
                                        }
                                    }
                                }


                                //agregar a las listas
                                columnas.Add(nombre);
                                tiposDeDato.Add(tipoDeDato);

                                nombre = "";
                                tipoDeDato = "";
                            }

                            break;
                        }
                        else
                        {
                            error = true;
                            break;
                        }

                    }
                }


                //--------------------------------------------------------------------------------------
                //Instancia al metodo crear en tabla.

                if (!error) //Si no hay error...
                {
                    Tabla nuevaTabla = new Tabla();
                    nuevaTabla.crearTabla(nombreTabla, llave, columnas, tiposDeDato);

                    microSQL.Controllers.HomeController.tablas.Add(nuevaTabla);
                }
                else
                {
                    //Mensaje de error, sintaxis incorrecta
                    microSQL.InterpreteSQL.error("Syntax Error");
                }
            }
            else
            {
                //Error No se encontro una llave primaria
                microSQL.InterpreteSQL.error("No se encontró una llave primaria. Use el comando 'PRIMARY KEY' para definirla");
            }

        }

        public static void eliminarTabla(string texto)
        {
            /*
            Ejemplo instrucciones:
            DROP TABLE
            <nombre de la tabla>   
             */

            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();

            //Variables
            string nombreTabla = "";

            //Comienza Procedimiento........................

            //Buscar si el texto posee los comandos correctos
            if (texto.Contains(palabrasReservadas["DROP TABLE"]))
            {
                //Reemplazar palabras reservadas por caracteres simples
                texto = sustituirPalabrasReservadasPorCaracteres(texto);

                //Se separa todas las instrucciones
                string[] sentences = texto.Split(' ');

                sentences = eliminarPosicionesVacias(sentences);

                //El resutado deberia ser un array con 2 posiciones....
                // [0]'DROP TABLE' [1]nombre

                //Recorrer cada uno de los fragmentos en array

                if (sentences.Length == 2) //Verificar formato correcro de instrucciones
                {
                    if (sentences[0] == "α")
                    {
                        //Definir nombre de tabla
                        nombreTabla = sentences[1];

                        //Verificar que tabla exista y buscarla
                        int index = Controllers.HomeController.tablas.FindIndex(x => x.nombreTabla == nombreTabla);
                        if (index > -1)
                        {
                            //Eliminar tabla
                            //-----------------------------------------------------------------------
                            Controllers.HomeController.tablas[index].eliminarTabla();
                        }
                    }
                    else
                    {
                        error();
                    }
                }
                else
                {
                    //Error... instrucciones incorrrectas
                    microSQL.InterpreteSQL.error("Syntax Error");
                }
            }
            else
            {
                //Error
                error();
            }
        }

        public static void insertarDatos(string texto)
        {

            /*
            Ejemplo instrucciones:
            INSERT INTO
            <nombre de la tabla>
            (
            <columna>,
            <columna>,
            <columna>
            ...
            )
            VALUES
            (
            <valor>,
            <valor>,
            <valor>
            ...
            )
             */

            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();
            bool error = false;

            //Variables
            string nombreTabla = "";

            List<string> columnas = new List<string>();
            List<string> valores = new List<string>();

            //Comienza Procedimiento........................

            //Buscar si el texto posee los comandos correctos
            if (texto.Contains(palabrasReservadas["INSERT INTO"]) && texto.Contains(palabrasReservadas["VALUES"]))
            {
                //Reemplazar palabras reservadas por caracteres simples
                texto = sustituirPalabrasReservadasPorCaracteres(texto);

                //Se separa todas las instrucciones (Manteniendo lo que estaba encerrado en parentesis intancto)
                string[] sentences = separarStringConAlgoEncerrado(texto, ' ', '#');

                sentences = eliminarPosicionesVacias(sentences);

                //El resutado deberia ser un array con 5 posiciones....
                // [0]'Insert into' [1]nombre [2]columnas [3]'Values' [4]valores

                //Recorrer cada uno de los fragmentos en array

                if (sentences.Length == 5) //Verificar formato correcro de instrucciones
                {
                    if (sentences[0] != "α" || sentences[3] != "~") //Buscar errores
                    {
                        //Error... instrucciones incorrrectas
                        microSQL.InterpreteSQL.error("Syntax Error");
                    }
                    else
                    {
                        //Definir nombre tabla
                        nombreTabla = sentences[1];
                        //Separar nommbre de columnas por coma
                        string[] arrayColumnas = sentences[2].Split(',');
                        //Corregir saltos de linea
                        arrayColumnas = corregirArreglo(arrayColumnas);

                        //Separar valores de columnas por coma, tomando en cuenta el apostrofe (')
                        string[] arrayValores = separarStringConAlgoEncerrado(sentences[4], ',', '\u0027');
                        //Corregir saltos de linea
                        arrayValores = corregirArreglo(arrayValores);

                        //Verificar que exista la misma cantidad de columnas que de valores
                        if (arrayColumnas.Length == arrayValores.Length)
                        {
                            //Verificar que exista la tabla

                            int posicionTabla = Controllers.HomeController.tablas.FindIndex(x => x.nombreTabla == nombreTabla);

                            if (posicionTabla > -1)
                            {
                                //obtener las columnas de la tabla
                                List<string> columnasEnTabla = Controllers.HomeController.tablas[posicionTabla].columnas;

                                //Comparar columnas en tabla con las columnas ingresadas 

                                //Arreglo con los valores ya en la posicion correcta
                                string[] nuevoArrayValores = new string[columnasEnTabla.Count];

                                for (int i = 0; i < arrayColumnas.Length; i++)
                                {
                                    //Verificar que la tabla contenga las columnas
                                    if (columnasEnTabla.Contains(arrayColumnas[i]))
                                    {
                                        //posicion de la columna en la que se desea ingresar el valor
                                        int pos = columnasEnTabla.FindIndex(x => x == arrayColumnas[i]);

                                        //ingresar valor en la posicion correcta de tabla
                                        nuevoArrayValores[pos] = arrayValores[i];
                                    }
                                    else
                                    {
                                        //Mensaje de error
                                        //La tabla no contiene una de las columnas descritas 

                                        microSQL.InterpreteSQL.error("La tabla no contiene una de las columnas descritas");

                                        error = true;
                                        break;
                                    }
                                }
                                //Verificar si no hubo un error en el proceso
                                if (error != true)
                                {
                                    //---------------------------------------------------------------------------------------
                                    //Instancia al metodo insertar en tabla.

                                    //Ingresar array en arbol 
                                    Controllers.HomeController.tablas[posicionTabla].insertarDatos(nuevoArrayValores);
                                }
                            }
                            else
                            {
                                //Error No existe la tabla
                                microSQL.InterpreteSQL.error("No existe la tabla");
                            }
                        }
                        else
                        {
                            //Mensaje de error
                            //Cantidad de columnas es diferente a cantidad de valores que se desean ingresar
                            microSQL.InterpreteSQL.error("Syntax Error");
                        }
                    }
                }
                else
                {
                    //Error... instrucciones incorrrectas
                    microSQL.InterpreteSQL.error("Syntax Error");
                }
            }
            else
            {
                //Error  formato incorrecto
                microSQL.InterpreteSQL.error("Syntax Error");
            }
        }

        public static void seleccionarDatos(string texto)
        {
            /*Ejemplo Instrucciones:
             1.
                SELECT
                <NOMBRE DE LA COLUMNA>,
                ...
                FROM
                <NOMBRE DE LA TABLA>
             2.
                SELECT
                *
                FROM
                <NOMBRE DE LA TABLA>
             3.
                WHERE
                ID = <VALOR>
             4.
                WHERE
                ID LIKE '<VALOR>%'
             */


            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();
            bool error = false;

            //Variables
            string nombreTabla = "";
            List<string> columnas = new List<string>();
            string buscar = "";

            //Comienza Procedimiento........................

            //Buscar si el texto posee los comandos correctos
            if (texto.Contains(palabrasReservadas["SELECT"]) && texto.Contains(palabrasReservadas["FROM"]))
            {
                //Reemplazar palabras reservadas por caracteres simples
                texto = sustituirPalabrasReservadasPorCaracteres(texto);

                //Se separa todas las instrucciones 

                texto = modificarCadenaParaBusquedas(texto);

                string[] sentences = separarStringConAlgoEncerrado(texto, ' ', '#');

                sentences = eliminarPosicionesVacias(sentences);

                //El resutado deberia ser un array con 5 posiciones....
                // [0]'SELECT' [1] columnas / * [2]'FROM' [3]tabla [4]'WHERE' [5]<columna> [6] = / 'LIKE' [7] <valor>


                //Verificar formato correcto de instrucciones y definir tipo de busqueeda

                //No se busca una fila con un valor especifico
                if (sentences.Length == 4)
                {
                    //         Δ FROM
                    //         | WHERE


                    if (sentences[0] != "α" || sentences[2] != "Δ") //Buscar errores
                    {
                        //Error... instrucciones incorrrectas
                        microSQL.InterpreteSQL.error("Syntax Error");
                    }
                    else
                    {
                        //Definir nombre tabla
                        nombreTabla = sentences[3];

                        //Verificar que exista la tabla

                        int posicionTabla = Controllers.HomeController.tablas.FindIndex(x => x.nombreTabla == nombreTabla);

                        if (posicionTabla > -1)
                        {

                            //obtener las columnas de la tabla
                            List<string> columnasEnTabla = Controllers.HomeController.tablas[posicionTabla].columnas;

                            //Separar nommbre de columnas por coma
                            string[] arrayColumnas = sentences[1].Split(',');
                            //Corregir saltos de linea
                            arrayColumnas = corregirArreglo(arrayColumnas);

                            if (arrayColumnas[0] == "*") //Comprobar si se seleccionan todas las columnas con *
                            {
                                if (arrayColumnas.Length > 1)
                                {
                                    error = true;
                                }
                                else
                                {
                                    arrayColumnas = columnasEnTabla.ToArray();
                                }
                            }
                            else
                            {
                                for (int i = 0; i < arrayColumnas.Length; i++)
                                {
                                    //Verificar que la tabla contenga las columnas
                                    if (!columnasEnTabla.Contains(arrayColumnas[i]))
                                    {
                                        //Mensaje de error
                                        //La tabla no contiene una de las columnas descritas 

                                        microSQL.InterpreteSQL.error("La tabla no contiene una de las columnas descritas");

                                        error = true;
                                        break;
                                    }

                                }
                            }

                            //Verificar si no hubo un error en el proceso
                            if (error != true)
                            {
                                //---------------------------------------------------------------------------------------
                                //Instancia al metodo seleccionar en tabla.

                                Controllers.HomeController.tablas[posicionTabla].seleccionarDatos(arrayColumnas);
                            }
                        }
                        else
                        {
                            //Error No existe la tabla
                            microSQL.InterpreteSQL.error("No existe la tabla");
                        }
                    }
                }

                //Se busca una fila usando el comando 'LIKE' o con =
                else if (sentences.Length == 8)
                {
                    //         Δ FROM
                    //         | WHERE
                    //         % LIKE

                    if (sentences[0] != "α" || sentences[2] != "Δ") //Buscar errores
                    {
                        //Error... instrucciones incorrrectas
                        microSQL.InterpreteSQL.error("Syntax Error");
                    }
                    else
                    {
                        //Definir nombre tabla
                        nombreTabla = sentences[3];

                        //Verificar que exista la tabla

                        int posicionTabla = Controllers.HomeController.tablas.FindIndex(x => x.nombreTabla == nombreTabla);

                        if (posicionTabla > -1)
                        {

                            //obtener las columnas de la tabla
                            List<string> columnasEnTabla = Controllers.HomeController.tablas[posicionTabla].columnas;
                            string[] arrayColumnas;

                            if (sentences[1].Contains("*")) //Comprobar si se seleccionan todas las columnas con *
                            {
                                arrayColumnas = columnasEnTabla.ToArray(); //Seleccionar todas las columnas
                            }
                            else
                            {
                                //Separar nommbre de columnas por coma
                                arrayColumnas = sentences[1].Split(',');
                                //Corregir saltos de linea
                                arrayColumnas = corregirArreglo(arrayColumnas);


                                for (int i = 0; i < arrayColumnas.Length; i++)
                                {
                                    //Verificar que la tabla contenga las columnas
                                    if (!columnasEnTabla.Contains(arrayColumnas[i]))
                                    {
                                        //Mensaje de error
                                        //La tabla no contiene una de las columnas descritas 

                                        microSQL.InterpreteSQL.error("La tabla no contiene una de las columnas descritas");

                                        error = true;
                                        break;
                                    }
                                }

                                //Verificar llave de la tabla
                                if (sentences[5] != Controllers.HomeController.tablas[posicionTabla].columnaLlave)
                                {
                                    microSQL.InterpreteSQL.error("Llave de busqueda incorrecta");
                                    error = true;
                                }
                            }

                            //Verificar si no hubo un error en el proceso
                            if (error != true)
                            {
                                //---------------------------------------------------------------------------------------
                                //Instancia al metodo seleccionar en tabla.

                                if (sentences[6] == "=")
                                {
                                    Controllers.HomeController.tablas[posicionTabla].seleccionarDatos(arrayColumnas, sentences[7], false);
                                }
                                else if (sentences[6] == "LIKE")
                                {
                                    Controllers.HomeController.tablas[posicionTabla].seleccionarDatos(arrayColumnas, sentences[7], true);
                                }
                                else
                                {
                                    //Error
                                    microSQL.InterpreteSQL.error("Syntax Error");
                                }    
                            }
                        }
                        else
                        {
                            //Error No existe la tabla
                            microSQL.InterpreteSQL.error("No existe la tabla");
                        }
                    }
                }

                else
                {
                    //Error... instrucciones incorrrectas
                    microSQL.InterpreteSQL.error("Syntax Error");
                }
            }
            else
            {
                //Error  formato incorrecto
                microSQL.InterpreteSQL.error("Syntax Error");
            }


        }

        public static void eliminarFilas(string texto)
        {
            /*Ejemplo Instrucciones:
             1.
                DELETE FROM
                <NOMBRE DE LA TABLA>
             2.
                DELETE FROM
                <NOMBRE DE LA TABLA>
                WHERE
                ID = <VALOR A BUSCAR>
             */


            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();
            bool error = false;

            //Variables
            string nombreTabla = "";
            string buscar = "";

            //Comienza Procedimiento........................

            //Buscar si el texto posee los comandos correctos
            if (texto.Contains(palabrasReservadas["DELETE"]) && texto.Contains(palabrasReservadas["FROM"]))
            {
                //Reemplazar palabras reservadas por caracteres simples
                texto = sustituirPalabrasReservadasPorCaracteres(texto);

                //Se separa todas las instrucciones 

                texto = modificarCadenaParaBusquedas(texto);

                string[] sentences = separarStringConAlgoEncerrado(texto, ' ', '#');

                sentences = eliminarPosicionesVacias(sentences);

                //El resutado deberia ser un array con 7 posiciones....
                // [0]'DELETE' [1] 'FROM' [2] tabla [3]'WHERE' [4]<columna> [5] = / 'LIKE' [6] <valor>


                //Verificar formato correcto de instrucciones y definir tipo de busqueeda

                //Se busca eliminar todas las filas
                if (sentences.Length == 3)
                {
                    //         Δ FROM
                    
                    if (sentences[0] != "α" || sentences[1] != "Δ") //Buscar errores
                    {
                        //Error... instrucciones incorrrectas
                        microSQL.InterpreteSQL.error("Syntax Error");
                    }
                    else
                    {
                        //Definir nombre tabla
                        nombreTabla = sentences[2];

                        //Verificar que exista la tabla

                        int posicionTabla = Controllers.HomeController.tablas.FindIndex(x => x.nombreTabla == nombreTabla);

                        if (posicionTabla > -1)
                        {
                            
                            //Verificar si no hubo un error en el proceso
                            if (error != true)
                            {
                                //---------------------------------------------------------------------------------------
                                //Instancia al metodo eliminar en tabla.

                                Controllers.HomeController.tablas[posicionTabla].eliminarTodasLasFilas();
                            }
                        }
                        else
                        {
                            //Error No existe la tabla
                            microSQL.InterpreteSQL.error("No existe la tabla");
                        }
                    }
                }

                //Se busca una fila usando el comando 'LIKE' o con =
                else if (sentences.Length == 7)
                {
                    //         Δ FROM
                    //         | WHERE
                    //         % LIKE

                    if (sentences[0] != "α" || sentences[1] != "Δ") //Buscar errores
                    {
                        //Error... instrucciones incorrrectas
                        microSQL.InterpreteSQL.error("Syntax Error");
                    }
                    else
                    {
                        //Definir nombre tabla
                        nombreTabla = sentences[2];

                        //Verificar que exista la tabla

                        int posicionTabla = Controllers.HomeController.tablas.FindIndex(x => x.nombreTabla == nombreTabla);

                        if (posicionTabla > -1)
                        {
                            //Verificar llave de la tabla
                            if (sentences[4] != Controllers.HomeController.tablas[posicionTabla].columnaLlave)
                            {
                                microSQL.InterpreteSQL.error("Llave de busqueda incorrecta");
                                error = true;
                            }


                            //Verificar si no hubo un error en el proceso
                            if (error != true)
                            {
                                //---------------------------------------------------------------------------------------
                                //Instancia al metodo seleccionar en tabla.

                                if (sentences[5] == "=")
                                {
                                    Controllers.HomeController.tablas[posicionTabla].eliminarFila(sentences[6], false);
                                }
                                else if (sentences[5] == "LIKE")
                                {
                                    Controllers.HomeController.tablas[posicionTabla].eliminarFila(sentences[6], true);
                                }
                                else
                                {
                                    //Error
                                    microSQL.InterpreteSQL.error("Syntax Error");
                                }
                            }
                        }
                        else
                        {
                            //Error No existe la tabla
                            microSQL.InterpreteSQL.error("No existe la tabla");
                        }
                    }
                }

                else
                {
                    //Error... instrucciones incorrrectas
                    microSQL.InterpreteSQL.error("Syntax Error");
                }
            }
            else
            {
                //Error  formato incorrecto
                microSQL.InterpreteSQL.error("Syntax Error");
            }
            
        }

        //Extra...
        public static void actualizarDatos(string texto) { } //To Do...

    }
}