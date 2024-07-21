using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webproject.Models
{
    public enum StatusTermina
    {
        SLOBODAN,
        ZAKAZAN
    }

    public class Termin
    {
        private Lekar lekar;
        private StatusTermina status;
        private DateTime zakazanTermin;
        private string opisTerapije;

        public Termin() { }
        public Termin(Lekar lekar, StatusTermina status, DateTime zakazanTermin, string opisTerapije)
        {
            this.Lekar = lekar;
            this.Status = status;
            this.ZakazanTermin = zakazanTermin;
            this.OpisTerapije = opisTerapije;
        }

        public Lekar Lekar { get => lekar; set => lekar = value; }
        public StatusTermina Status { get => status; set => status = value; }
        public DateTime ZakazanTermin { get => zakazanTermin; set => zakazanTermin = value; }
        public string OpisTerapije { get => opisTerapije; set => opisTerapije = value; }
    }
}