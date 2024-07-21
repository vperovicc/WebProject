using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webproject.Models
{
    

    public class ZdravstveniKarton
    {
        private List<Termin> termini;
        private Pacijent pacijent;

        public ZdravstveniKarton() { }

        public ZdravstveniKarton(List<Termin> termini, Pacijent pacijent)
        {
            this.Termini = termini;
            this.Pacijent = pacijent;
        }

        public List<Termin> Termini { get => termini; set => termini = value; }
        public Pacijent Pacijent { get => pacijent; set => pacijent = value; }
    }
}