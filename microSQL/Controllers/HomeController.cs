using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using microSQL.Models;

using ArbolB_;

namespace microSQL.Controllers
{
    public class HomeController : Controller
    {
        public static Dictionary<string, string> palabrasReservadas = new Dictionary<string, string>();
        public static List<Tabla> tablas = new List<Tabla>();
        public static TablaVista tablaActual = new TablaVista("Nueva tabla",new List<string> { "Columna 1", "Columna 2" , "Columna 3" , "Columna 4" }, null );
        public static string mensaje = "";
        public static string textoInstrucciones = ""; 

        public ActionResult Index()
        {
            //To Do.. Mostrar de color distinto cada palabra reservada en la interfaz grafica.....

            palabrasReservadas = Configuracion.leerArchivoConfiguracion(); //configurar diccionario
            Tabla.leerAchivoTablas(); //leer tablas

            return View(tablaActual);
        }
        [HttpPost]
        public ActionResult Index(string texto)
        {
            textoInstrucciones = "";
            mensaje = "";
            InterpreteSQL.leerInstrucciones(texto);

            if (!String.IsNullOrEmpty(mensaje)) //Mantener texto escrito en pantalla, en caso de un error.
            {
                textoInstrucciones = texto;
            }

            return RedirectToAction("Index");
        }

        public ActionResult ConfiguracionDeIdioma()
        {
            //To Do... Metodo para personalizar diccionario
            //To Do... Metodo para regresar el diccionario a sus valores por defecto
            return View();
        }

        public ActionResult exportarJSON()
        {

            if (tablaActual.nombreTabla != null && tablaActual.nombreTabla != "" && tablaActual.nombreTabla != "TablaNueva")
            {
                Tabla exportar = tablas.Find(x => x.nombreTabla == tablaActual.nombreTabla);

                ArbolBM arbol = exportar.arbol;

                string[] columnas = exportar.columnas.ToArray();
                Objeto[] objetos = arbol.ArbolALista();

                exportar.GuardarJson(objetos, columnas);
            }


            return RedirectToAction("Index");
        }
    }
}