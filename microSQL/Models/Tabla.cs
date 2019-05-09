using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Text;

using ArbolB_; //DLL del arbol B

namespace microSQL
{
    public class Tabla
    {
       
        public string nombreTabla { get; set; }

        public List<string> tiposDeDatos { get; set; }
        public List<string> columnas { get; set; }

        public string columnaLlave { get; set; } //nombre de llave primaria en 'columnas'
        private int indexLlave { get; set; } 
        
        public ArbolBM arbol { get; set; }

        //Constructor
        public Tabla() {}
        private Tabla(string nombre, List<string> tiposDeDatos, List<string> nombresColumnas, string llaveColumna)
        {
            this.nombreTabla = nombre;
            this.tiposDeDatos = tiposDeDatos;
            this.columnas = nombresColumnas;
            this.columnaLlave = llaveColumna;
        }
        private Tabla(string nombre, List<string> tiposDeDatos, List<string> nombresColumnas, int llaveColumna, ArbolBM filas)
        {
            this.nombreTabla = nombre;
            this.tiposDeDatos = tiposDeDatos;
            this.columnas = nombresColumnas;
            this.indexLlave = llaveColumna;
            this.arbol = filas;
        }
        
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
                int indexLlave = 0;

                nombreTabla = archivo.Name.Replace(".tabla", "");

                leerArchivoConfiguracionTabla(carpetaTabla + "/" + nombreTabla + ".tabla", ref datos, ref columnas, ref llave);
                leerArchivoArbolB(carpetaArbolB + "/" + nombreTabla + ".arbolb", ref filas);

                //Agregar filas al Arbol B+
                indexLlave = columnas.FindIndex(x => x == llave);
                ArbolBM arbol = new ArbolBM(columnas.ToArray());
                Objeto obj = new Objeto(columnas.Count, indexLlave);
                
                if (filas.Count > 0)
                {
                    //Objeto[] objetos = arbol.ArbolALista();
                    agregarFilasAlArbol(obj, filas, indexLlave, ref arbol);

                }

                //Agregar tabla a lista
                //Controllers.HomeController.tablas.Add(new Tabla(nombreTabla, datos, columnas, llave, filas)); //To Do... Borrar
                Controllers.HomeController.tablas.Add(new Tabla(nombreTabla, datos, columnas, indexLlave, arbol)); 
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

                            filas.Add(palabras); 
                            //Anadir arreglo de filas
                            
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
                //Mensaje de error
                microSQL.InterpreteSQL.error("Ya existe el archivo");

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

        private bool tiposDeDatosCorrectos(List<string> tiposDeDatos)
        {
            //Verificar nombre correcto de cada tipo de dato ingresado en crear
            bool resultado = true;

            foreach (var item in tiposDeDatos)
            {
                if (item != "INT" && item != "VARCHAR(100)" && item != "DATETIME")
                {
                    resultado = false;
                }
            }

            return resultado;
        }

        private bool errorEnTipoDeDato(string dato, string valor)
        {
            //Verifica que un valor ingresado sea del tipo de dato correcto

            bool error = false;

            switch (dato)
            {
                case "INT":

                    foreach (char item in valor) //Verificar cada caracter de la cadena
                    {
                        if (!Char.IsNumber(item)) //No es un numero
                        {
                            error = true;
                            break;
                        }
                    }

                    break;
                case "VARCHAR(100)":
                    break;
                case "DATETIME":

                    int cantidadDeDiagonales = 0;

                    foreach (char item in valor) //Verificar cada caracter de la cadena
                    {
                        if (!Char.IsNumber(item)) // No es un numero
                        {
                            if (item != '/') // No es una diagonal
                            {
                                error = true;
                                break;
                            }
                            else
                            {
                                cantidadDeDiagonales++;
                            }
                        }
                    }

                    if (cantidadDeDiagonales != 2) //Verificar que solo existan dos diagonales
                    {
                        error = true;
                        break;
                    }

                    break;
                default:
                    break;
            }

            return error;

        }

        private bool verificarErroresDeTiposDeDatoEnArray(string[] array)
        {
            //Verifica que cada elemento en un array insertado tenga el tipo de dato correcto

            bool error = false;

            for (int i = 0; i < array.Length; i++)
            {
                if (errorEnTipoDeDato(this.tiposDeDatos[i], array[i]) == true)
                {
                    error = true;
                    break;
                }
            }

            return error;
        }

        private bool esNumero(string cadena)
        {
            bool error = false;
            foreach (char item in cadena) //Verificar cada caracter de la cadena
            {
                if (!Char.IsNumber(item)) //No es un numero
                {
                    error = true;
                    break;
                }
            }
            return !error;
        }

        private static void agregarFilasAlArbol(Objeto config, List<string[]> filas, int indexLlave, ref ArbolBM arbol)
        {
            try
            {
                foreach (var array in filas)
                {
                    config.elementos = array;
                    config.id = Convert.ToInt32(array[indexLlave]);
                    arbol.insertar(config);

                    config = new Objeto(array.Length, indexLlave);
                }
            }
            catch (Exception)
            {
                microSQL.InterpreteSQL.error("Error al agregar");
                throw;
            }
        }

        //Metodos derivados de instrucciones SQL
        //---------------------------------------------------------
        public void crearTabla(string nombre, string llave, List<string> col, List<string> datos)
        {
            if (microSQL.Controllers.HomeController.tablas.FindIndex(x => x.nombreTabla == nombre) > -1) //Solo pasa si la tabla no existe
            {
                microSQL.InterpreteSQL.error("La tabla ya existe");
            }
            else //La tabla no existe
            {
                if (tiposDeDatosCorrectos(datos))
                {
                    if (datos[col.FindIndex(x => x == llave)] == "INT")
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
                    else
                    {
                        microSQL.InterpreteSQL.error("La llave debe ser de tipo INT");
                    }
                }
                else
                {
                    microSQL.InterpreteSQL.error("Tipo de dato No valido");
                }
            }
        }

        public void insertarDatos(string[] valores)
        {
            if (verificarErroresDeTiposDeDatoEnArray(valores) == false)
            {
                if (esNumero(valores[indexLlave]))
                {

                    string rutaFolder = System.Web.HttpContext.Current.Server.MapPath("~/microSQL");
                    string rutaFilas = rutaFolder + "/arbolesb/" + this.nombreTabla + ".arbolb";

                    //Anadir filas a archivo

                    escribirEnArchivo(valores.ToList(), rutaFilas);

                    //Mostrar en pantalla la insercion
                    Objeto nuevo = new Objeto(columnas.Count, indexLlave);
                    nuevo.id = Convert.ToInt16(valores[indexLlave]);
                    nuevo.elementos = valores;

                    this.arbol.insertar(nuevo);

                    List<string[]> filas = new List<string[]>();

                    //Mostrar en pantalla resultado
                    Objeto[] objetos = arbol.ArbolALista();
                    foreach (var item in objetos)
                    {
                        filas.Add(item.elementos);
                    }


                    microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista(this.nombreTabla, this.columnas, filas);
                }
                else
                {
                    microSQL.InterpreteSQL.error("La llave debe ser de tipo INT");
                }
            }
            else
            {
                microSQL.InterpreteSQL.error("Error en el tipo de dato");     
            }
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

            //Busqueda en arbol B

            Objeto[] filas = this.arbol.ArbolALista();

            foreach (var objeto in filas)
            {
                string[] arreglo = objeto.elementos;

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

        public void seleccionarDatos(string[] columnas, string buscar, bool like, string ColumnaDeBusqueda)
        {

            //Se selcceccionan ciertas filas, y solo ciertas columnas

            //Si se busca un valor igual like sera falso
            //Si se busca un valor del tipo %m, like sera true

            List<int> indexColumnas = new List<int>();
            List<string[]> filasSeleccionadas = new List<string[]>();
            List<string> fila = new List<string>();
            int indexLlave = this.columnas.FindIndex(x => x == columnaLlave);

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
            
            //Realizar busqueda
            if (like == false) // Se realiza una busqueda con operador =
            {
                Objeto objeto = this.arbol.Buscar(ColumnaDeBusqueda, buscar);
                string[] arreglo = objeto.elementos;


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
            else // Se realiza una busqueda con operador LIKE
            {

                buscar = buscar.Replace("%", "");
                buscar = buscar.Replace("‘", "");

                Objeto[] objetos = arbol.ValoresX(ColumnaDeBusqueda, buscar);

                foreach (var obj in objetos)
                {
                    string[] arreglo = obj.elementos;

                    for (int i = 0; i < arreglo.Length; i++)
                    {
                        if (indexColumnas.Contains(i)) //Si es una de las columnas seleccionadas
                        {
                            fila.Add(arreglo[i]);
                        }
                    }

                    //Verificar si coincide con la busqueda 'LIKE'
                    
                        //Agregar fila actual a las filas seleccionadas
                        filasSeleccionadas.Add(fila.ToArray());
                
                    fila.Clear();
                }
            }

            
            //Mostrar en pantalla resultado
            microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista(this.nombreTabla, columnas.ToList(), filasSeleccionadas);
            
        }

        public void eliminarFila(string buscar, string columna)
        {
            
            try
            {

                Objeto[] objetos = this.arbol.Eliminar(columna, buscar);
                List<string[]> filas = new List<string[]>();
                foreach (var item in objetos)
                {
                    filas.Add(item.elementos);
                }

                string rutaFolder = System.Web.HttpContext.Current.Server.MapPath("~/microSQL");
                string rutaFilas = rutaFolder + "/arbolesb/" + this.nombreTabla + ".arbolb";

                //Anadir filas a archivo

                //Crear archivo en blanco
                FileStream file = System.IO.File.Create(rutaFilas);
                file.Close();

                foreach (var arreglo in filas)
                {
                    escribirEnArchivo(arreglo.ToList(), rutaFilas);
                }

                //Mostrar en pantalla la insercion
                
                microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista(this.nombreTabla, this.columnas, filas);
                

            }
            catch (Exception)
            {
                microSQL.InterpreteSQL.error("Error desconocido");
                throw;
            }

        }

        public void eliminarTodasLasFilas()
        {
            this.arbol = new ArbolBM(null); //Eliminar todo

            //Mostrar en pantalla resultado
            microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista(this.nombreTabla, this.columnas, null);

        }

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

            //Mostrar en pantalla resultado
            microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista("Tabla Borrada", null, null);

        }

        //Extra... 
        public void actualizarDatos(string columna, string id, string nuevoValor, string columnaID)
        {

            nuevoValor = nuevoValor.Replace("'", "");

            int indexLlave = this.columnas.FindIndex(x => x == columnaID);
            int indexColumna = this.columnas.FindIndex(x => x == columna);

            //Verificar que exitan las columnnas
            if (indexColumna > -1 && indexLlave > -1)
            {
                //Realizar busqueda
                    try
                    {
                            //actualizar fila
                            //Encuentra la posicion actual del arreglo en la lista y en ese arreglo busca la posicion del elemento a cambiar.
               
                            if (errorEnTipoDeDato(this.tiposDeDatos[indexColumna], nuevoValor) == false)//verificar tipo de dato de valor a modificar
                            {
                         //this.filas[indexArreglo][indexColumna] = nuevoValor; //Actualizar valor
                         arbol.Actualizar(columnaID, id, nuevoValor, columna);
                            }
                            else
                            {
                                microSQL.InterpreteSQL.error("Error en tipo de dato");
                            }
                            
                    }
                    catch (Exception)
                    {
                        microSQL.InterpreteSQL.error("Error desconocido");
                        throw;
                    }

                
            }
            else
            {
                microSQL.InterpreteSQL.error("No existe la columna");
            }

            //Mostrar en pantalla resultado
            Objeto[] objetos = arbol.ArbolALista();
            List<string[]> filas = new List<string[]>();
            foreach (var item in objetos)
            {
                filas.Add(item.elementos);
            }

            microSQL.Controllers.HomeController.tablaActual = new Models.TablaVista(this.nombreTabla, this.columnas, filas);

        }

        //Extra
        
        public void GuardarJson(Objeto[] A, string[] B)
        {
            /*
              A = ARBOL
              B = COLUMNAS
              */

            string Nombre = this.nombreTabla;

            string root = System.Web.HttpContext.Current.Server.MapPath("~/JSON"); ;
            string path = root + "/" + Nombre + ".json";

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            if (!System.IO.File.Exists(path))
            {
                using (FileStream strm = System.IO.File.Create(path))
                using (StreamWriter sw = new StreamWriter(strm))
                {
                    sw.WriteLine("[");
                    string m = "No info";
                    sw.WriteLine(m);
                    sw.Close();
                }
            }
            if (System.IO.File.Exists(path))
            {
                using (StreamWriter sw = System.IO.File.CreateText(path))
                {
                    sw.WriteLine("[");
                    for (int i = 0; i < A.Length; i++)
                    {
                        string m;
                        m = "{";
                        for (int j = 0; j < B.Length; j++)
                        {

                            m += ("\u0022" + B[j] + "\u0022" + ":" + "\u0022" + A[i].elementos[j] + "\u0022");
                            if (j != B.Length - 1)
                            {
                                m += ",";
                            }
                        }
                        m += "}";
                        if (i != A.Length - 1)
                        {
                            m += ",";
                        }
                        m += "\n";
                        sw.WriteLine(m);
                    }
                    sw.WriteLine("]");
                    sw.Close();
                }
            }

        }
    }
}