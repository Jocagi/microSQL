using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace microSQL.Models
{
    public class PalabrasReservadas
    {
        
        public string SELECT { get; set; }
        public string FROM { get; set; }
        public string DELETE { get; set; }
        public string WHERE { get; set; }
        public string CREATE_TABLE { get; set; }
        public string DROP_TABLE { get; set; }
        public string INSERT_INTO { get; set; }
        public string VALUES { get; set; }
        public string GO { get; set; }
        public string UPDATE { get; set; }
        
    }
}