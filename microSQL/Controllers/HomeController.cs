using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace microSQL.Controllers
{
    public class HomeController : Controller
    {
        public static Dictionary<string, string> palabrasReservadas = new Dictionary<string, string>();
        public static List<Tabla> tablas = new List<Tabla>();
        public static string mensaje = "";

        public ActionResult Index()
        {
            //To Do.. Mostrar de color distinto cada palabra reservada en la interfaz grafica.....

            palabrasReservadas = Configuracion.leerArchivoConfiguracion(); //configurar diccionario
            Tabla.leerAchivoTablas(); //leer tablas
            return View();
        }
        [HttpPost]
        public ActionResult Index(string texto)
        {
            mensaje = "";
            InterpreteSQL.leerInstrucciones(texto);
            return RedirectToAction("Index");
        }

        public ActionResult ConfiguracionDeIdioma() 
        {
            //To Do... Metodo para personalizar diccionario
            //To Do... Metodo para regresar el diccionario a sus valores por defecto
            return View();
        }
        
    }
}