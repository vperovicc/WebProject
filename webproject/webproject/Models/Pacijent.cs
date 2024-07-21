using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webproject.Models
{
    public class Pacijent
    {
        private string korIme;
        private string jmbg;
        private string sifra;
        private string ime;
        private string prezime;
        private DateTime datumRodj;
        private string email;
        private List<Termin> termini;

        public Pacijent() { }

        public Pacijent(string korIme, string jmbg, string sifra, string ime, string prezime, DateTime datumRodj, string email, List<Termin> termini)
        {
            this.KorIme = korIme;
            this.Jmbg = jmbg;
            this.Sifra = sifra;
            this.Ime = ime;
            this.Prezime = prezime;
            this.DatumRodj = datumRodj;
            this.Email = email;
            this.Termini = termini;
        }

        public string KorIme { get => korIme; set => korIme = value; }
        public string Jmbg { get => jmbg; set => jmbg = value; }
        public string Sifra { get => sifra; set => sifra = value; }
        public string Ime { get => ime; set => ime = value; }
        public string Prezime { get => prezime; set => prezime = value; }
        public DateTime DatumRodj { get => datumRodj; set => datumRodj = value; }
        public string Email { get => email; set => email = value; }
        public List<Termin> Termini { get => termini; set => termini = value; }
    }
}