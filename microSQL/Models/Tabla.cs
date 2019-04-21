using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Text;

namespace microSQL
{
    public class Tabla
    {
        public string nombreTabla { get; set; }
        
        public List<string> tiposDeDatos { get; set; }
        public List<string> columnas { get; set; }

        public string columnaLlave { get; set; } //nombre de llave primaria en 'columnas'
        public List<List<string>> filas { get; set; } //To Do... Cambiar lista por arbol B


        //Archivos de tablas
        public void leerAchivoTablas() { } //To Do...

        private void crearArchivo(string path)
        {
            //Verificar existencia del archivo

            if (System.IO.File.Exists(path))
            {
                //To Do...
                //Mensaje de error
            }
            else
            {
                //Crear archivo en blanco
                FileStream file = System.IO.File.Create(path);
                file.Close();
            }
        }
        
        private void escribirEnArchivo(List<string> texto, string path)
        {
            if (System.IO.File.Exists(path))
            {
                //constructor de strings


                string linea;

                //Agregar saltos de linea
                if (archivoEstaVacio(path))
                {
                    linea = "";
                }
                else
                {
                    linea = "\r\n";
                }

                for (int i = 0; i < texto.Count; i++)
                {
                    if (linea != "" && linea != "\r\n")
                    {
                        //Combinar palabras en lista separandolas por coma
                        linea = linea + "," + texto[i];
                    }
                    else if (linea == "\r\n")
                    {
                        linea += texto[i];
                    }
                    else
                    {
                        linea = texto[i];
                    }
                }

                //Escribir en archivo
                System.IO.File.AppendAllText(path, linea);
            }
            else
            {
                //To Do... 
                //Error archivo no existe
            }
        }

        private bool archivoEstaVacio(string path)
        {
            if (String.IsNullOrEmpty(path)) //formato de cadena incorrecto
            {
                throw new InvalidOperationException("Formato de cadena invalido");
            }
            else if (!System.IO.File.Exists(path)) //No existe el archivo
            {
                throw new InvalidOperationException("El archivo  no existe");
            }
            else
            {
                using (var reader = new StreamReader(path))
                {
                    bool contenidoEncontrado = false;

                    while (!reader.EndOfStream) //Recorrer archivo hasta el final
                    {
                        var linea = reader.ReadLine(); //linea actual

                        if (!String.IsNullOrEmpty(linea)) //Verifica si la linea posee texto
                        {
                            contenidoEncontrado = true;
                        }
                    }

                    reader.Close();

                    //Si no hay contenido encontrado, el archivo esta vacio..
                    //De lo contrario, el archivo contiene texto
                    return !contenidoEncontrado;
                }
            }
        }

        //Metodos derivados de instrucciones SQL
        //---------------------------------------------------------
        public void crearTabla(string nombre, string llave, List<string> col, List<string> datos) {

            //Este metodo actúa practimente como un construcctor, pero debe hacerse la instancia a esta clase primero
            //Se define las propiedades de esta tabla y se crean sus archivos

            string rutaFolder = System.Web.HttpContext.Current.Server.MapPath("~/microSQL");

            string rutaColumnas = rutaFolder + "/tablas/" + nombre + ".tabla";
            string rutaFilas = rutaFolder + "/arbolesb/" + nombre + ".arbolb";

            //definir propiedades
            this.nombreTabla = nombre;
            this.columnas = col;
            this.columnaLlave = llave;
            this.tiposDeDatos = datos;

            //crear archivos

            crearArchivo(rutaFilas);
            crearArchivo(rutaColumnas);

            //Definir nombre de columna y tipo de dato en el archivo .tabla
            escribirEnArchivo(this.tiposDeDatos, rutaColumnas);
            escribirEnArchivo(this.columnas, rutaColumnas);
            escribirEnArchivo(new List<string> { this.columnaLlave }, rutaColumnas);
        } 

        public void insertarDatos(string tabla, string columna, string valor) { } //To Do...

        public void seleccionarDatos(string tabla, string columna) { } //To Do...
        public void seleccionarDatos(string tabla, string columna, string valor) { /* //Operador =  //Operador 'Like' */} //To Do...
        public void eliminarFilas(string tabla, string valor) { } //To Do...
        public void eliminarTabla(string tabla) { } //To Do...
        //Extra... 
        public void actualizarDatos(string tabla, string columna, string valorAntiguo, string nuevoValor) { } //To Do...

        //Extra
        public void exportarJSON() { } //To Do...

    }
}