using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace microSQL.Models
{
    public class TablaVista : Tabla
    {
        public List<string[]> filasSeleccionadas { get; set; }

        public TablaVista()
        {

        }
        public TablaVista(string nombre, List<string> columnas, List<string[]> filas)
        {
            nombreTabla = nombre;

            this.columnas = columnas;
            this.filasSeleccionadas = filas;
        }

    }
}