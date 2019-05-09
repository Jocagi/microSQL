using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Text;

namespace microSQL
{
    //leer archivo csv con configuracion

    public class Configuracion
    {
        //ubicacion del archivo csv de configuracion
        public static string path = System.Web.HttpContext.Current.Server.MapPath("~/microSQL/microSQL.ini"); 
        //valores por defecto del archivo
        private static List<String> Default = new List<string> { "SELECT", "FROM", "DELETE", "WHERE", "CREATE TABLE", "DROP TABLE", "INSERT INTO", "VALUES", "GO", "UPDATE" };

        //----------------------------------------------------
        //Metodo de lectura

        public static Dictionary<string, string> leerArchivoConfiguracion()
        {

            if (String.IsNullOrEmpty(path))
            {
                //Mensaje de error
                //TO DO...
            }

            if (!System.IO.File.Exists(path)) //No existe el archivo
            {
                crearArchivoConfiguracionDefault();
            }

            using (var reader = new StreamReader(path))
            {
                Dictionary<string, string> palabrasReservadas = new Dictionary<string, string>();
                int count = 0; //la linea del archivo que se esta visitando

                /*
                Lista de palabras reservadas:

                Linea:      Palabra:
                1          SELECT
                2          FROM
                3          DELETE
                4          WHERE
                5          CREATE TABLE
                6          DROP TABLE
                7          INSERT INTO
                8          VALUES 
                9          GO
                10         UPDATE
                
                 */


                while (!reader.EndOfStream) //Recorrer archivo hasta el final
                {
                    var line = reader.ReadLine(); //linea actual

                    string[] palabras = line.Split(','); //dividir datos seprados por coma

                    //anadir palabra en otro idioma al listado de palabras reservadas

                    /* Formato:
                       [Default] 0, [Personalizado] 1
                       SELECT, SELECCIONAR
                     */

                    palabrasReservadas[palabras[0]] = palabras[1];

                    count++; //Sumar 1 a la linea
                }

                //Verificar que el archivo tenga el formato correcto
                if (count != 10) //El archivo no tiene la cantidad de lineas que deberia tener... 
                {
                    //To Do... Mensaje de error...

                    // Usar configuracion por defecto.
                    palabrasReservadas.Clear();

                    foreach (var palabra in Default)
                    {
                        palabrasReservadas.Add(palabra, palabra);
                    }
                }

                return palabrasReservadas;
            }
        }

        //-----------------------------------------------------
        // Metodos de creacion

        private static void construirArchivo(List<string> Nuevo)
        {
            //Resumen: Este metodo solo toma como entrada una lista con palabras en otro idioma, creara un archivo csv con la primera columna de valores por defecto y la segunda con palabras en otro idioma

            //Borrar archivo antiguo

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            //Crear archivo

            //Crear archivo en blanco
            FileStream file = System.IO.File.Create(path);
            file.Close();

            //constructor de strings
            var csv = new StringBuilder();

            for (int i = 0; i < Default.Count; i++)
            {
                var palabra = Default[i];
                var palabraOtroIdioma = Nuevo[i];

                //Crear nuevas lineas en archivo
                var newLine = string.Format("{0},{1}", palabra, palabraOtroIdioma);
                csv.AppendLine(newLine);
            }

            //Escribir en archivo
            System.IO.File.WriteAllText(path, csv.ToString());

            leerArchivoConfiguracion(); //Actualizar Diccionario
        }

        public static void crearArchivoConfiguracionPersonalizado(string SELECT, string FROM, string DELETE, string WHERE, string CREATE_TABLE, string DROP_TABLE, string INSERT_INTO, string VALUES, string GO, string UPDATE)
        {
             //lista con valores por en otro idioma
            List<String> Personalizado = new List<string> { SELECT, FROM, DELETE, WHERE, CREATE_TABLE, DROP_TABLE, INSERT_INTO, VALUES, GO, UPDATE };

            //verificar errores
            for (int i = 0; i < Default.Count; i++)
            {
                if (String.IsNullOrEmpty(Personalizado[i]))
                {
                    Personalizado[i] = Default[i];
                }
            }

            construirArchivo(Personalizado);
        }

        public static void crearArchivoConfiguracionDefault()
        {
            construirArchivo(Default);
        }
        
    }
}