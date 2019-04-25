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
        public List<string[]> filas = new List<string[]>(); //To Do... Cambiar lista por arbol B


        //Constructor
        public Tabla() { }
        private Tabla(string nombre, List<string> tiposDeDatos, List<string> nombresColumnas, string llaveColumna)
        {
            this.nombreTabla = nombre;
            this.tiposDeDatos = tiposDeDatos;
            this.columnas = nombresColumnas;
            this.columnaLlave = llaveColumna;
        }
        private Tabla(string nombre, List<string> tiposDeDatos, List<string> nombresColumnas, string llaveColumna, List<string[]> filas)
        {
            this.nombreTabla = nombre;
            this.tiposDeDatos = tiposDeDatos;
            this.columnas = nombresColumnas;
            this.columnaLlave = llaveColumna;
            this.filas = filas;
        }
        //To Do... Hacer sobrecarga a constructor que incluya al arbol B

        //Archivos de tablas
        public static void leerAchivoTablas()
        {
            //Eliminar tablas en controlador
            Controllers.HomeController.tablas.Clear();

            //Resumen: Se leen todos los archivos de tablas y crea la lista en el controlador

            string carpetaTabla = System.Web.HttpContext.Current.Server.MapPath("~/microSQL/tablas");
            string carpetaArbolB = System.Web.HttpContext.Current.Server.MapPath("~/microSQL/arbolesb");

            //Se enlistan todos los archivos en la carpeta de tablas, se leen y definen las propiedades de un objeto tabla
            DirectoryInfo info = new DirectoryInfo(carpetaTabla);
            FileInfo[] listaArchivosEnCarpeta = info.GetFiles();

            foreach (var archivo in listaArchivosEnCarpeta)
            {
                //Variables
                string nombreTabla;
                List<string> datos = new List<string>();
                List<string> columnas = new List<string>();
                List<string[]> filas = new List<string[]>();
                string llave = "";

                nombreTabla = archivo.Name.Replace(".tabla", "");

                leerArchivoConfiguracionTabla(carpetaTabla + "/" + nombreTabla + ".tabla", ref datos, ref columnas, ref llave);
                leerArchivoArbolB(carpetaArbolB + "/" + nombreTabla + ".arbolb", ref filas);

                //Agregar tabla a lista
                Controllers.HomeController.tablas.Add(new Tabla(nombreTabla, datos, columnas, llave, filas));
            }
        }

        private static void leerArchivoConfiguracionTabla(string path, ref List<string> tiposDeDato, ref List<string> columnas, ref string llave)
        {
            if (!String.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    using (var reader = new StreamReader(path))
                    {
                        int i = 0; //la linea del archivo que se esta visitando

                        while (!reader.EndOfStream) //Recorrer archivo hasta el final
                        {
                            var line = reader.ReadLine(); //linea actual

                            //Formato Archivo:
                            // linea[0]  tipos de dato (separados por coma)
                            // linea[1]  nombres de columna (separados por coma)
                            // linea[2]  nombre de columna llave

                            string[] palabras = line.Split(','); //dividir datos seprados por coma
                            switch (i)
                            {
                                case 0:
                                    tiposDeDato = palabras.ToList();
                                    break;
                                case 1:
                                    columnas = palabras.ToList();
                                    break;
                                case 2:
                                    llave = line;
                                    break;
                                default:
                                    break;
                            }

                            i++; //Sumar 1 a la linea
                        }
                    }
                }
            }
        }
        private static void leerArchivoArbolB(string path, ref List<string[]> filas)
        {
            //To Do... Modificar para Arbol B

            if (!String.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    using (var reader = new StreamReader(path))
                    {
                        
                        while (!reader.EndOfStream) //Recorrer archivo hasta el final
                        {
                            var line = reader.ReadLine(); //linea actual
                            
                            string[] palabras = line.Split(','); //dividir datos seprados por coma

                            filas.Add(palabras); //Anadir arreglo de filas
                        }
                    }
                }
                else
                {
                    Controllers.HomeController.mensaje = "No se encontro el archivo";
                }
            }

        }

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
                Controllers.HomeController.mensaje = "El archivo no existe";
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
        public void crearTabla(string nombre, string llave, List<string> col, List<string> datos)
        {

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

            //Mostrar en pantalla la nueva tabla
            microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista(this.nombreTabla, columnas.ToList(), null);
        }

        public void insertarDatos(string[] valores)
        {
            string rutaFolder = System.Web.HttpContext.Current.Server.MapPath("~/microSQL");
            string rutaFilas = rutaFolder + "/arbolesb/" + this.nombreTabla + ".arbolb";

            //Anadir filas a archivo

            escribirEnArchivo(valores.ToList(), rutaFilas);

            //Mostrar en pantalla la incersion

            this.filas.Add(valores);
            microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista(this.nombreTabla, this.columnas, this.filas);
        }

        public void seleccionarDatos(string[] columnas)
        {

            //Se selcceccionan todas las filas, pero solo ciertas columnas

            List<int> indexColumnas = new List<int>();
            List<string[]> filasSeleccionadas = new List<string[]>();
            List<string> fila = new List<string>();

            //Recorrer todas las filas y eliminar las posiciones de columnas

            foreach (var item in columnas)
            {
                if (this.columnas.Contains(item)) //verificar si se encuentra en la lista
                {
                    //Agregar a los indices de columnas
                    indexColumnas.Add(this.columnas.FindIndex(x => x == item)); //buscar indice
                }
            }

            //Ordenar indices
            indexColumnas.Sort();

            //To Do...  Busqueda en arbol B

            foreach (var arreglo in filas)
            {
                for (int i = 0; i < arreglo.Length; i++)
                {
                    if (indexColumnas.Contains(i)) //Si es una de las columnas seleccionadas
                    {
                        fila.Add(arreglo[i]);
                    }
                }

                //Agregar fila actual a las filas seleccionadas
                filasSeleccionadas.Add(fila.ToArray());

                fila.Clear();
            }

            //Mostrar en pantalla resultado
            microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista(this.nombreTabla, columnas.ToList(), filasSeleccionadas);
        }

        public void seleccionarDatos(string[] columnas, string buscar, bool like)
        {

            //Se selcceccionan ciertas filas, y solo ciertas columnas

            /* //Operador =  //Operador 'Like' */

            //Si se busca un valor igual like sera falso
            //Si se busca un valor del tipo %m, like sera true

            //To Do...


        }

        public void eliminarFilas(string buscar) { } //To Do...

        public void eliminarTabla()
        {
            //Elimina los archivos que hacen referencia a esta tabla

            string path1 = System.Web.HttpContext.Current.Server.MapPath("~/microSQL/tablas") + "/" + nombreTabla + ".tabla";
            string path2 = System.Web.HttpContext.Current.Server.MapPath("~/microSQL/arbolesb") + "/" + nombreTabla + ".arbolb";

            if (System.IO.File.Exists(path1) && System.IO.File.Exists(path2))
            {
                //Eliminar archivos
                System.IO.File.Delete(path1);
                System.IO.File.Delete(path2);
            }
        }

        //Extra... 
        public void actualizarDatos(string columna, string valorAntiguo, string nuevoValor) { } //To Do...

        //Extra
        public void exportarJSON() { } //To Do...

    }
}