using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using webproject.Models;

namespace webproject.Controllers
{
    public class LekarController : Controller
    {
        List<Termin> termini;
        
        public ActionResult Index()
        {
            string loggedInLekarUsername = (string)Session["LoggedInLekarUsername"];
            termini = GetTerminiForLoggedInLekar(loggedInLekarUsername);

            ViewBag.termini = termini;
            ViewBag.prepisaneTerapije = ReadPacijentiForLekarFromXml(loggedInLekarUsername);
            ViewBag.prosliTermini = GetPacijentiKojiSuBiliKodLogovanogDoktora(loggedInLekarUsername);

            return View();
        }


        [HttpPost]
        public ActionResult KreirajTermin(FormCollection formCollection)
        {
            try
            {
                string loggedInLekarUsername = (string)Session["LoggedInLekarUsername"];
                string filePath = Server.MapPath("~/App_Data/Lekar.xml");
                XDocument xml = XDocument.Load(filePath);

                var lekari = xml.Descendants("Lekar");
                var selectedLekar = lekari.FirstOrDefault(lekar => (string)lekar.Element("KorIme") == loggedInLekarUsername);

                if (selectedLekar != null)
                {
                    Termin noviTermin = new Termin
                    {
                        Lekar = new Lekar
                        {
                            KorIme = (string)selectedLekar.Element("KorIme"),
                            Sifra = (string)selectedLekar.Element("Sifra"),
                            Ime = (string)selectedLekar.Element("Ime"),
                            Prezime = (string)selectedLekar.Element("Prezime"),
                            DatumRodj = (DateTime)selectedLekar.Element("DatumRodj"),
                            Email = (string)selectedLekar.Element("Email")
                        },
                        Status = StatusTermina.SLOBODAN,
                        ZakazanTermin = DateTime.Parse(formCollection["datTermina"]), 
                        OpisTerapije = ""
                    };

                    XElement noviTerminXml = new XElement("Termin",
                        new XElement("Lekar",
                            new XElement("KorIme", noviTermin.Lekar.KorIme),
                            new XElement("Sifra", noviTermin.Lekar.Sifra),
                            new XElement("Ime", noviTermin.Lekar.Ime),
                            new XElement("Prezime", noviTermin.Lekar.Prezime),
                            new XElement("DatumRodj", noviTermin.Lekar.DatumRodj),
                            new XElement("Email", noviTermin.Lekar.Email)
                        ),
                        new XElement("Status", noviTermin.Status),
                        new XElement("ZakazanTermin", noviTermin.ZakazanTermin),
                        new XElement("OpisTerapije", noviTermin.OpisTerapije)
                    );

                    selectedLekar.Element("Termini").Add(noviTerminXml);
                    xml.Save(filePath);

                    termini = GetTerminiForLoggedInLekar(loggedInLekarUsername);
                    ViewBag.termini = termini;
                    ViewBag.prepisaneTerapije = ReadPacijentiForLekarFromXml(loggedInLekarUsername);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Lekar sa korisničkim imenom " + loggedInLekarUsername + " nije pronađen.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja ili čuvanja XML fajla: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        private List<Termin> GetTerminiForLoggedInLekar(string username)
        {
            List<Termin> termini = new List<Termin>();

            try
            {
                string filePath = Server.MapPath("~/App_Data/Lekar.xml");
                XDocument xml = XDocument.Load(filePath);

                System.Diagnostics.Debug.WriteLine("Traženo korisničko ime: " + username);

                var lekari = xml.Descendants("Lekar");
                var selectedLekar = lekari.FirstOrDefault(lekar => (string)lekar.Element("KorIme") == username);

                if (selectedLekar != null)
                {
                    var terminiElements = selectedLekar.Element("Termini")?.Elements("Termin");

                    if (terminiElements != null)
                    {
                        termini = (from termin in terminiElements
                                   select new Termin
                                   {
                                       Lekar = new Lekar
                                       {
                                           KorIme = (string)selectedLekar.Element("KorIme"),
                                           Sifra = (string)selectedLekar.Element("Sifra"),
                                           Ime = (string)selectedLekar.Element("Ime"),
                                           Prezime = (string)selectedLekar.Element("Prezime"),
                                           DatumRodj = (DateTime)selectedLekar.Element("DatumRodj"),
                                           Email = (string)selectedLekar.Element("Email")
                                       },
                                       Status = (StatusTermina)Enum.Parse(typeof(StatusTermina), (string)termin.Element("Status")),
                                       ZakazanTermin = (DateTime)termin.Element("ZakazanTermin"),
                                       OpisTerapije = (string)termin.Element("OpisTerapije")
                                   }).ToList();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Nema termina za izabranog lekara.");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Lekar sa korisničkim imenom " + username + " nije pronađen.");
                }
                System.Diagnostics.Debug.WriteLine("Broj pronađenih termina: " + termini.Count);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja XML fajla: " + ex.Message;
            }

            return termini;
        }

        private List<Pacijent> ReadPacijentiForLekarFromXml(string lekarKorIme)
        {
            List<Pacijent> listaPacijenata = new List<Pacijent>();

            try
            {
                string filePath = Server.MapPath("~/App_Data/Pacijent.xml");

                XDocument xml = XDocument.Load(filePath);

                DateTime danasnjiDatum = DateTime.Now;

                listaPacijenata = xml.Descendants("Pacijent")
                                     .Where(p => p.Element("Termini").Elements("Termin")
                                                  .Any(t => (string)t.Element("Lekar").Element("KorIme") == lekarKorIme &&
                                                            DateTime.Parse((string)t.Element("ZakazanTermin")) < danasnjiDatum))
                                     .Select(p => new Pacijent
                                     {
                                         KorIme = (string)p.Element("KorIme"),
                                         Jmbg = (string)p.Element("Jmbg"),
                                         Sifra = (string)p.Element("Sifra"),
                                         Ime = (string)p.Element("Ime"),
                                         Prezime = (string)p.Element("Prezime"),
                                         DatumRodj = (DateTime)p.Element("DatumRodj"),
                                         Email = (string)p.Element("Email"),
                                         Termini = p.Element("Termini").Elements("Termin")
                                                      .Where(t => (string)t.Element("Lekar").Element("KorIme") == lekarKorIme &&
                                                                  DateTime.Parse((string)t.Element("ZakazanTermin")) < danasnjiDatum)
                                                      .Select(t => new Termin
                                                      {
                                                          Lekar = new Lekar
                                                          {
                                                              KorIme = (string)t.Element("Lekar").Element("KorIme"),
                                                              Sifra = (string)t.Element("Lekar").Element("Sifra"),
                                                              Ime = (string)t.Element("Lekar").Element("Ime"),
                                                              Prezime = (string)t.Element("Lekar").Element("Prezime"),
                                                              DatumRodj = (DateTime)t.Element("Lekar").Element("DatumRodj"),
                                                              Email = (string)t.Element("Lekar").Element("Email")
                                                          },
                                                          Status = (StatusTermina)Enum.Parse(typeof(StatusTermina), (string)t.Element("Status")),
                                                          ZakazanTermin = (DateTime)t.Element("ZakazanTermin"),
                                                          OpisTerapije = (string)t.Element("OpisTerapije")
                                                      }).ToList()
                                     }).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja XML fajla: " + ex.Message;
            }

            return listaPacijenata;
        }


        private List<Pacijent> GetPacijentiKojiSuBiliKodLogovanogDoktora(string loggedInLekarUsername)
        {
            List<Pacijent> pacijenti = new List<Pacijent>();

            try
            {
                string filePath = Server.MapPath("~/App_Data/Pacijent.xml");
                XDocument xml = XDocument.Load(filePath);

                var pacijentiElements = xml.Descendants("Pacijent");

                foreach (var pacijentElement in pacijentiElements)
                {
                    var terminiElements = pacijentElement.Element("Termini")?.Elements("Termin");

                    if (terminiElements != null)
                    {
                        foreach (var terminElement in terminiElements)
                        {
                            string lekarKorIme = (string)terminElement.Element("Lekar").Element("KorIme");
                            DateTime zakazanTermin = (DateTime)terminElement.Element("ZakazanTermin");
                            string opisTerapije = (string)terminElement.Element("OpisTerapije");

                            if (lekarKorIme == loggedInLekarUsername && DateTime.Today > zakazanTermin && opisTerapije == "")
                            {
                                Pacijent pacijent = new Pacijent
                                {
                                    KorIme = (string)pacijentElement.Element("KorIme"),
                                    Jmbg = (string)pacijentElement.Element("Jmbg"),
                                    Sifra = (string)pacijentElement.Element("Sifra"),
                                    Ime = (string)pacijentElement.Element("Ime"),
                                    Prezime = (string)pacijentElement.Element("Prezime"),
                                    DatumRodj = (DateTime)pacijentElement.Element("DatumRodj"),
                                    Email = (string)pacijentElement.Element("Email"),
                                    Termini = new List<Termin>()
                                };

                                pacijenti.Add(pacijent);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja XML fajla: " + ex.Message;
            }

            return pacijenti;
        }

        [HttpPost]
        public ActionResult PrepisTerapiju(FormCollection formCollection)
        {
            try
            {
                string pacijentKorIme = formCollection["pacijent"];
                string opisTerapije = formCollection["opisTerapije"];
                string loggedInLekarUsername = (string)Session["LoggedInLekarUsername"];
                string pacijentFilePath = Server.MapPath("~/App_Data/Pacijent.xml");
                string lekarFilePath = Server.MapPath("~/App_Data/Lekar.xml");

                XDocument pacijentXml = XDocument.Load(pacijentFilePath);

                var pacijenti = pacijentXml.Descendants("Pacijent");
                var selectedPacijent = pacijenti.FirstOrDefault(pacijent => (string)pacijent.Element("KorIme") == pacijentKorIme);

                DateTime? terminDate = null;
                string zakazanTerminValue = null;

                if (selectedPacijent != null)
                {
                    var terminiElements = selectedPacijent.Element("Termini")?.Elements("Termin");

                    if (terminiElements != null)
                    {
                        var terminElement = terminiElements.FirstOrDefault(termin =>
                            (string)termin.Element("Lekar").Element("KorIme") == loggedInLekarUsername &&
                            DateTime.Today > (DateTime)termin.Element("ZakazanTermin") &&
                            (string)termin.Element("OpisTerapije") == "");

                        if (terminElement != null)
                        {
                            terminElement.Element("OpisTerapije").Value = opisTerapije;
                            pacijentXml.Save(pacijentFilePath);

                            terminDate = (DateTime)terminElement.Element("ZakazanTermin");
                            zakazanTerminValue = terminElement.Element("ZakazanTermin").Value;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Nije pronađen odgovarajući termin za prepisivanje terapije kod pacijenta.";
                            return RedirectToAction("Index");
                        }
                    }
                }

                if (terminDate.HasValue)
                {
                    XDocument lekarXml = XDocument.Load(lekarFilePath);

                    var lekari = lekarXml.Descendants("Lekar");
                    var loggedInLekar = lekari.FirstOrDefault(lekar => (string)lekar.Element("KorIme") == loggedInLekarUsername);

                    if (loggedInLekar != null)
                    {
                        var terminiLekarElements = loggedInLekar.Element("Termini")?.Elements("Termin");

                        if (terminiLekarElements != null)
                        {
                            var terminLekarElement = terminiLekarElements.FirstOrDefault(termin =>
                                (string)termin.Element("ZakazanTermin") == zakazanTerminValue &&
                                (string)termin.Element("Lekar").Element("KorIme") == loggedInLekarUsername);

                            if (terminLekarElement != null)
                            {
                                terminLekarElement.Element("OpisTerapije").Value = opisTerapije;
                                lekarXml.Save(lekarFilePath);
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Nije pronađen odgovarajući termin za prepisivanje terapije kod lekara.";
                            }
                        }
                    }
                }

                ViewBag.termini = GetTerminiForLoggedInLekar(loggedInLekarUsername);
                ViewBag.prepisaneTerapije = ReadPacijentiForLekarFromXml(loggedInLekarUsername);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja ili čuvanja XML fajla: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


    }
}
