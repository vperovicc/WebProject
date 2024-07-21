using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webproject.Models
{
    public class Administrator
    {
        private string korIme;
        private string sifra;
        private string ime;
        private string prezime;
        private DateTime datumRodj;

        public string KorIme { get => korIme; set => korIme = value; }
        public string Sifra { get => sifra; set => sifra = value; }
        public string Ime { get => ime; set => ime = value; }
        public string Prezime { get => prezime; set => prezime = value; }
        public DateTime DatumRodj { get => datumRodj; set => datumRodj = value; }

        public Administrator() { }
        public Administrator(string korIme, string sifra, string ime, string prezime, DateTime datumRodj)
        {
            this.KorIme = korIme;
            this.Sifra = sifra;
            this.Ime = ime;
            this.Prezime = prezime;
            this.DatumRodj = datumRodj;
        }
    }
}