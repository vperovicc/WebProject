using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using webproject.Models;

namespace webproject.Controllers
{
    public class AdminController : Controller
    {
        List<Pacijent> pacijenti;
        public ActionResult Index()
        {
            pacijenti = ReadPacijentiFromXml();
            ViewBag.pacijenti = pacijenti;
            return View("~/Views/Admin/Index.cshtml");
        }

        [HttpPost]
        public ActionResult DodajPacijenta(string korIme, string jmbg, string sifra, string ime, string prezime, DateTime datumRodj, string email)
        {
            pacijenti = ReadPacijentiFromXml();

            if (pacijenti.Any(p => p.KorIme == korIme))
            {
                ViewBag.ErrorMessage = "Korisničko ime već postoji.";
                return RedirectToAction("Index");
            }

            if (pacijenti.Any(p => p.Jmbg == jmbg))
            {
                ViewBag.ErrorMessage = "JMBG već postoji.";
                return RedirectToAction("Index");
            }

            if (pacijenti.Any(p=>p.Email==email))
            {
                ViewBag.ErrorMessage = "E-mail vec postoji";
                return RedirectToAction("Index");
            }

            Pacijent noviPacijent = new Pacijent
            {
                KorIme = korIme,
                Jmbg = jmbg,
                Sifra = sifra,
                Ime = ime,
                Prezime = prezime,
                DatumRodj = datumRodj,
                Email = email,
                Termini = new List<Termin>()
            };

            pacijenti.Add(noviPacijent);
            WritePacijentiToXml(pacijenti);

            return RedirectToAction("Index");
        }

        public ActionResult ObrisiPacijenta(string korIme)
        {
            pacijenti = ReadPacijentiFromXml();

            Pacijent pacijentZaBrisanje = pacijenti.FirstOrDefault(p => p.KorIme == korIme);

            if (pacijentZaBrisanje != null)
            {
                pacijenti.Remove(pacijentZaBrisanje);
                WritePacijentiToXml(pacijenti);
            }
            else
            {
                ViewBag.ErrorMessage = "Korisnik sa datim korisničkim imenom ne postoji.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AzurirajPacijenta(string korImeProvera, string sifra, string ime, string prezime, DateTime datumRodj, string email)
        {
            pacijenti = ReadPacijentiFromXml();

            Pacijent pacijentZaAzuriranje = pacijenti.FirstOrDefault(p => p.KorIme == korImeProvera);

            if (pacijentZaAzuriranje != null)
            {                
                if (pacijenti.Any(p => p.Email == email && p.KorIme != korImeProvera))
                {
                    ViewBag.ErrorMessage = "Email adresa je već u upotrebi.";
                    return RedirectToAction("Index");
                }

                pacijentZaAzuriranje.Sifra = sifra;
                pacijentZaAzuriranje.Ime = ime;
                pacijentZaAzuriranje.Prezime = prezime;
                pacijentZaAzuriranje.DatumRodj = datumRodj;
                pacijentZaAzuriranje.Email = email;

                WritePacijentiToXml(pacijenti);
            }
            else
            {
                ViewBag.ErrorMessage = "Korisnik sa datim korisničkim imenom ne postoji.";
            }

            return RedirectToAction("Index");
        }


        private void WritePacijentiToXml(List<Pacijent> pacijenti)
        {
            try
            {
                string filePath = Server.MapPath("~/App_Data/Pacijent.xml");

                XDocument xml = new XDocument(
                    new XElement("Pacijenti",
                        from pacijent in pacijenti
                        select new XElement("Pacijent",
                            new XElement("KorIme", pacijent.KorIme),
                            new XElement("Jmbg", pacijent.Jmbg),
                            new XElement("Sifra", pacijent.Sifra),
                            new XElement("Ime", pacijent.Ime),
                            new XElement("Prezime", pacijent.Prezime),
                            new XElement("DatumRodj", pacijent.DatumRodj.ToString("yyyy-MM-dd")),
                            new XElement("Email", pacijent.Email),
                            new XElement("Termini",
                                from termin in pacijent.Termini
                                select new XElement("Termin",
                                    new XElement("Lekar",
                                        new XElement("KorIme", termin.Lekar.KorIme),
                                        new XElement("Sifra", termin.Lekar.Sifra),
                                        new XElement("Ime", termin.Lekar.Ime),
                                        new XElement("Prezime", termin.Lekar.Prezime),
                                        new XElement("DatumRodj", termin.Lekar.DatumRodj.ToString("yyyy-MM-dd")),
                                        new XElement("Email", termin.Lekar.Email)
                                    ),
                                    new XElement("Status", termin.Status.ToString()),
                                    new XElement("ZakazanTermin", termin.ZakazanTermin.ToString("yyyy-MM-ddTHH:mm:ss")),
                                    new XElement("OpisTerapije", termin.OpisTerapije)
                                )
                            )
                        )
                    )
                );

                xml.Save(filePath);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom pisanja u XML fajl: " + ex.Message;
            }
        }

        private List<Pacijent> ReadPacijentiFromXml()
        {
            List<Pacijent> listaPacijenata = new List<Pacijent>();

            try
            {
                string filePath = Server.MapPath("~/App_Data/Pacijent.xml");

                XDocument xml = XDocument.Load(filePath);

                listaPacijenata = (from pacijent in xml.Descendants("Pacijent")
                                   select new Pacijent
                                   {
                                       KorIme = (string)pacijent.Element("KorIme"),
                                       Jmbg = (string)pacijent.Element("Jmbg"),
                                       Sifra = (string)pacijent.Element("Sifra"),
                                       Ime = (string)pacijent.Element("Ime"),
                                       Prezime = (string)pacijent.Element("Prezime"),
                                       DatumRodj = (DateTime)pacijent.Element("DatumRodj"),
                                       Email = (string)pacijent.Element("Email"),
                                       Termini = (from termin in pacijent.Element("Termini").Elements("Termin")
                                                  select new Termin
                                                  {
                                                      Lekar = new Lekar
                                                      {
                                                          KorIme = (string)termin.Element("Lekar").Element("KorIme"),
                                                          Sifra = (string)termin.Element("Lekar").Element("Sifra"),
                                                          Ime = (string)termin.Element("Lekar").Element("Ime"),
                                                          Prezime = (string)termin.Element("Lekar").Element("Prezime"),
                                                          DatumRodj = (DateTime)termin.Element("Lekar").Element("DatumRodj"),
                                                          Email = (string)termin.Element("Lekar").Element("Email")
                                                      },
                                                      Status = (StatusTermina)Enum.Parse(typeof(StatusTermina), (string)termin.Element("Status")),
                                                      ZakazanTermin = (DateTime)termin.Element("ZakazanTermin"),
                                                      OpisTerapije = (string)termin.Element("OpisTerapije")
                                                  }).ToList()
                                   }).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja XML fajla: " + ex.Message;
            }

            return listaPacijenata;
        }
    }
}