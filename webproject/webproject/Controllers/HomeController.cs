using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using webproject.Models;

namespace webproject.Controllers
{
    public class HomeController : Controller
    {
        List<Administrator> administratori;
        List<Lekar> lekari;
        List<Pacijent> pacijenti;

        public ActionResult Index()
        { 

            return View("~/Views/Home/Index.cshtml");
        }

        [HttpPost]
        public ActionResult Login(string korIme, string lozinka)
        {
            administratori = ReadAdministratoriFromXml();
            pacijenti = ReadPacijentiFromXml();
            lekari = ReadLekariFromXml();
            var admin = administratori.FirstOrDefault(a => a.KorIme == korIme && a.Sifra == lozinka);
            var pacijent = pacijenti.FirstOrDefault(p => p.KorIme == korIme && p.Sifra == lozinka);
            var lekar = lekari.FirstOrDefault(p => p.KorIme == korIme && p.Sifra == lozinka);

            if (admin != null)
            {
                Session["LoggedInUsername"] = admin.KorIme;
                Session["UserType"] = "Admin";
                return RedirectToAction("Index","Admin");
            }
            else if(pacijent != null)
            {
                Session["LoggedInPacijentUsername"] = pacijent.KorIme;
                Session["UserType"] = "Pacijent";
                return RedirectToAction("Index","Pacijent");
            }
            else if (lekar != null)
            {
                Session["LoggedInLekarUsername"] = lekar.KorIme;
                Session["UserType"] = "Lekar";
                return RedirectToAction("Index","Lekar");
            }
            else
            {
                return RedirectToAction("Index");
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


        private List<Administrator> ReadAdministratoriFromXml()
        {
            List<Administrator> listaAdministratora = new List<Administrator>();

            try
            {
                string filePath = Server.MapPath("~/App_Data/Administrator.xml");

                XDocument xml = XDocument.Load(filePath);

                listaAdministratora = (from admin in xml.Descendants("Administrator")
                                       select new Administrator
                                       {
                                           KorIme = (string)admin.Element("KorIme"),
                                           Sifra = (string)admin.Element("Sifra"),
                                           Ime = (string)admin.Element("Ime"),
                                           Prezime = (string)admin.Element("Prezime"),
                                           DatumRodj = (DateTime)admin.Element("DatumRodj")
                                       }).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja XML fajla za administratore: " + ex.Message;
            }

            return listaAdministratora;
        }

        public List<Lekar> ReadLekariFromXml()
        {
            List<Lekar> listaLekara = new List<Lekar>();

            try
            {
                string filePath = Server.MapPath("~/App_Data/Lekar.xml");

                XDocument xml = XDocument.Load(filePath);

                if (xml.Root != null)
                {
                    listaLekara = (from lekar in xml.Root.Elements("Lekar")
                                   select new Lekar
                                   {
                                       KorIme = (string)lekar.Element("KorIme") ?? string.Empty,
                                       Sifra = (string)lekar.Element("Sifra") ?? string.Empty,
                                       Ime = (string)lekar.Element("Ime") ?? string.Empty,
                                       Prezime = (string)lekar.Element("Prezime") ?? string.Empty,
                                       DatumRodj = (DateTime?)lekar.Element("DatumRodj") ?? DateTime.MinValue,
                                       Email = (string)lekar.Element("Email") ?? string.Empty,
                                       Termini = lekar.Element("Termini") != null ? (from termin in lekar.Element("Termini").Elements("Termin")
                                                                                     select new Termin
                                                                                     {
                                                                                         Lekar = new Lekar
                                                                                         {
                                                                                             KorIme = (string)termin.Element("Lekar")?.Element("KorIme") ?? string.Empty,
                                                                                             Sifra = (string)termin.Element("Lekar")?.Element("Sifra") ?? string.Empty,
                                                                                             Ime = (string)termin.Element("Lekar")?.Element("Ime") ?? string.Empty,
                                                                                             Prezime = (string)termin.Element("Lekar")?.Element("Prezime") ?? string.Empty,
                                                                                             DatumRodj = (DateTime?)termin.Element("Lekar")?.Element("DatumRodj") ?? DateTime.MinValue,
                                                                                             Email = (string)termin.Element("Lekar")?.Element("Email") ?? string.Empty
                                                                                         },
                                                                                         Status = Enum.TryParse((string)termin.Element("Status"), out StatusTermina status) ? status : StatusTermina.SLOBODAN,
                                                                                         ZakazanTermin = (DateTime?)termin.Element("ZakazanTermin") ?? DateTime.MinValue,
                                                                                         OpisTerapije = (string)termin.Element("OpisTerapije") ?? string.Empty
                                                                                     }).ToList() : new List<Termin>()
                                   }).ToList();
                }
                else
                {
                    ViewBag.ErrorMessage = "Koren XML dokumenta nije pronađen.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja XML fajla za lekare: " + ex.Message;
            }

            return listaLekara;
        }





    }
}