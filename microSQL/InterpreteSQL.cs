﻿using System;
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
                //To Do...
                //Mensaje de error
            }
            else
            {
                //Buscar el comando 'GO'
                if (!texto.Contains(palabrasReservadas["GO"]))
                {
                    //No se encontro el comando en las instrucciones

                    //To Do...
                    //Mensaje de error
                }
                else
                {
                    /*Se separaran todas las instrucciones con la plabra 'GO', luego se reemplazaran las palabras reservadas 
                      y tipos de datos por caracteres griegos (por simplicidad) para identificar donde comienzan y terminar 
                      las distintas partes de la instruccion */
                      
                    //separar diferentes instrucciones

                    List<string> instrucciones = separarComandos(texto).ToList();


                    //Verificar si la palabra final fue un 'GO'
                    instrucciones[instrucciones.Count - 1] = instrucciones.Last().Replace(" " , "");

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
                                //To Do...
                                //Mensaje de error (Formato Incorrecto)
                            }
                        }
                    }
                    else
                    {
                        //To Do...
                        //Error no se escribio 'GO'
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
            }

            string[] algo = texto.Split(separador);
            Resultado[5] = algo[algo.Length - 1];
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

        private static bool contienePalabrasReservadas(string texto) {
            //Resumen: Verifica si el texto posee otra palabra reservada que no sea el comando principal... debido a que cada instruccion solo deberi tener una de estas palabras...
            bool encontrado = false;
            int contador = 0;
            Dictionary<string, string> palabrasReservadas = obtenerPalabrasReservadas();

            foreach (var item in palabrasReservadas)
            {
                if (item.Value == palabrasReservadas["CREATE TABLE"] || item.Value == palabrasReservadas["INSERT INTO"] || item.Value == palabrasReservadas["DELETE"] || item.Value == palabrasReservadas["DROP TABLE"] || item.Value == palabrasReservadas["UPDATE"])
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
                if (item != "")
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

            string resultado =  "";

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

        //Metodos con instrucciones
        //--------------------------------------------------------------------------------------

        public static void crearTabla(string texto) {

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
                        else if ( sentences[i] != "" && buscarNombreTabla == true)
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
                    //To Do... 
                    //Mensaje de error, sintaxis incorrecta
                }

            }
            else
            {
                //To Do..
                //Error No se encontro una llave primaria
            }
            
        }

        public static void insertarDatos(string texto) {

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

            //Pasos a seguir...
            bool buscarNombreTabla = false;
            
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

                if (sentences.Length != 5) //Verificar formato correcro de instrucciones
                {
                    if (sentences[0] != "α" || sentences[3] != "~") //Buscar errores
                    {
                        //To Do... 
                        //Error... instrucciones incorrrectas
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
                            for (int i = 0; i < arrayColumnas.length; i++)
                            {

                            }
                        }
                        else
                        {
                            //To DO... Mensaje de error
                            //Cantidad de columnas es diferente a cantidad de valores que se desean ingresar
                        }

                    }
                }
                else
                {
                    //To Do... 
                    //Error... instrucciones incorrrectas
                }

                //-----------------------------------
                //Instancia al metodo insertar en tabla.
                
            }
            else
            {
                //To Do..
                //Error  formato incorrecto
            }


        }

        public static void seleccionarDatos(string texto) { } //To Do...
        public static void eliminarFilas(string texto) { } //To Do...
        public static void eliminarTabla(string texto) { } //To Do...
        public static void actualizarDatos(string texto) { } //To Do...

    }
}