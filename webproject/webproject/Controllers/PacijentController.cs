using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using webproject.Models;

namespace webproject.Controllers
{
    public class PacijentController : Controller
    {
        // GET: Pacijent
        public ActionResult Index()
        {
            List<Termin> slobodniTermini = GetAllSlobodniTermini();
            ViewBag.termini = slobodniTermini;

            string pacijentKor = (string)Session["LoggedInPacijentUsername"];
            List<Termin> prosliTermini = GetProsliTerminiForPacijent(pacijentKor);
            ViewBag.prosliTermini = prosliTermini;
            return View();
        }

        private List<Termin> GetProsliTerminiForPacijent(string pacijentKorIme)
        {
            List<Termin> prosliTermini = new List<Termin>();
            try
            {
                string filePath = Server.MapPath("~/App_Data/Pacijent.xml");
                XDocument xml = XDocument.Load(filePath);

                XElement pacijent = xml.Root.Elements("Pacijent")
                                           .FirstOrDefault(p => (string)p.Element("KorIme") == pacijentKorIme);

                if (pacijent != null)
                {
                    var termini = pacijent.Element("Termini")?.Elements("Termin")
                                          .Where(t => DateTime.Parse((string)t.Element("ZakazanTermin")) < DateTime.Now)
                                          .Select(t => new Termin
                                          {
                                              Lekar = new Lekar
                                              {
                                                  KorIme = (string)t.Element("Lekar")?.Element("KorIme"),
                                                  Ime = (string)t.Element("Lekar")?.Element("Ime"),
                                                  Prezime = (string)t.Element("Lekar")?.Element("Prezime")
                                              },
                                              ZakazanTermin = (DateTime)t.Element("ZakazanTermin"),
                                              OpisTerapije = (string)t.Element("OpisTerapije")
                                          }).ToList();

                    if (termini != null)
                    {
                        prosliTermini.AddRange(termini);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja XML fajla: " + ex.Message;
            }

            return prosliTermini;
        }

        private List<Termin> GetAllSlobodniTermini()
        {
            List<Termin> slobodniTermini = new List<Termin>();

            try
            {
                string filePath = Server.MapPath("~/App_Data/Lekar.xml");
                XDocument xml = XDocument.Load(filePath);

                var lekari = xml.Descendants("Lekar");

                foreach (var lekar in lekari)
                {
                    var terminiElements = lekar.Element("Termini")?.Elements("Termin");

                    if (terminiElements != null)
                    {
                        var termini = (from termin in terminiElements
                                       where (string)termin.Element("Status") == "SLOBODAN"
                                       select new Termin
                                       {
                                           Lekar = new Lekar
                                           {
                                               KorIme = (string)lekar.Element("KorIme"),
                                               Sifra = (string)lekar.Element("Sifra"),
                                               Ime = (string)lekar.Element("Ime"),
                                               Prezime = (string)lekar.Element("Prezime"),
                                               DatumRodj = (DateTime)lekar.Element("DatumRodj"),
                                               Email = (string)lekar.Element("Email")
                                           },
                                           Status = (StatusTermina)Enum.Parse(typeof(StatusTermina), (string)termin.Element("Status")),
                                           ZakazanTermin = (DateTime)termin.Element("ZakazanTermin"),
                                           OpisTerapije = (string)termin.Element("OpisTerapije")
                                       }).ToList();

                        slobodniTermini.AddRange(termini);
                    }
                }
                System.Diagnostics.Debug.WriteLine("Broj pronađenih slobodnih termina: " + slobodniTermini.Count);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Greška prilikom čitanja XML fajla: " + ex.Message;
            }

            return slobodniTermini;
        }

        private List<Lekar> ReadLekariFromXml()
        {
            string filePath = Server.MapPath("~/App_Data/Lekar.xml");
            XDocument xml = XDocument.Load(filePath);

            List<Lekar> lekari = xml.Root.Elements("Lekar")
                .Select(x => new Lekar
                {
                    KorIme = (string)x.Element("KorIme"),
                    Sifra = (string)x.Element("Sifra"),
                    Ime = (string)x.Element("Ime"),
                    Prezime = (string)x.Element("Prezime"),
                    DatumRodj = (DateTime?)x.Element("DatumRodj") ?? DateTime.MinValue,
                    Email = (string)x.Element("Email"),
                    Termini = x.Element("Termini")?.Elements("Termin")
                        .Select(t => new Termin
                        {
                            Lekar = t.Element("Lekar") != null ? new Lekar
                            {
                                KorIme = (string)t.Element("Lekar")?.Element("KorIme"),
                                Sifra = (string)t.Element("Lekar")?.Element("Sifra"),
                                Ime = (string)t.Element("Lekar")?.Element("Ime"),
                                Prezime = (string)t.Element("Lekar")?.Element("Prezime"),
                                DatumRodj = (DateTime?)t.Element("Lekar")?.Element("DatumRodj") ?? DateTime.MinValue,
                                Email = (string)t.Element("Lekar")?.Element("Email")
                            } : null,
                            Status = t.Element("Status") != null ? (StatusTermina)Enum.Parse(typeof(StatusTermina), (string)t.Element("Status")) : StatusTermina.SLOBODAN,
                            ZakazanTermin = (DateTime?)t.Element("ZakazanTermin") ?? DateTime.MinValue,
                            OpisTerapije = (string)t.Element("OpisTerapije")
                        }).ToList()
                }).ToList();

            return lekari;
        }


        [HttpPost]
        public ActionResult ZakaziTermin(string korImeLekara, DateTime zakazanTermin)
        {
            List<Lekar> lekari = ReadLekariFromXml();
            Lekar lekar = lekari.FirstOrDefault(l => l.KorIme == korImeLekara);
            if (lekar != null)
            {
                Termin termin = lekar.Termini.FirstOrDefault(t => t.ZakazanTermin == zakazanTermin && t.Status == StatusTermina.SLOBODAN);
                if (termin != null)
                {
                    termin.Status = StatusTermina.ZAKAZAN;
                    UpdateLekariXml(lekari);
                    AddTerminToPacijentXml(termin);

                    ViewBag.Message = "Termin uspešno zakazan!";
                }
                else
                {
                    ViewBag.Message = "Termin nije pronađen ili je već zakazan.";
                }
            }
            else
            {
                ViewBag.Message = "Lekar nije pronađen.";
            }

            return RedirectToAction("Index");
        }

        private void AddTerminToPacijentXml(Termin termin)
        {
            string filePath = Server.MapPath("~/App_Data/Pacijent.xml");
            XDocument xml = XDocument.Load(filePath);

            string pacijentKorIme = (string)Session["LoggedInPacijentUsername"];

            XElement pacijent = xml.Root.Elements("Pacijent")
                                       .FirstOrDefault(p => (string)p.Element("KorIme") == pacijentKorIme);
            if (pacijent != null)
            {
                pacijent.Element("Termini")?.Add(
                    new XElement("Termin",
                        new XElement("Lekar",
                            new XElement("KorIme", termin.Lekar.KorIme),
                            new XElement("Sifra", termin.Lekar.Sifra),
                            new XElement("Ime", termin.Lekar.Ime),
                            new XElement("Prezime", termin.Lekar.Prezime),
                            new XElement("DatumRodj", termin.Lekar.DatumRodj),
                            new XElement("Email", termin.Lekar.Email)
                        ),
                        new XElement("Status", termin.Status.ToString()),
                        new XElement("ZakazanTermin", termin.ZakazanTermin),
                        new XElement("OpisTerapije", termin.OpisTerapije)
                    )
                );

                xml.Save(filePath);
            }
            else
            {
                ViewBag.ErrorMessage = "Pacijent nije pronađen.";
            }
        }

        private void UpdateLekariXml(List<Lekar> lekari)
        {
            string filePath = Server.MapPath("~/App_Data/Lekar.xml");
            XDocument xml = new XDocument(
                new XElement("Lekari",
                    lekari.Select(l => new XElement("Lekar",
                        new XElement("KorIme", l.KorIme),
                        new XElement("Sifra", l.Sifra),
                        new XElement("Ime", l.Ime),
                        new XElement("Prezime", l.Prezime),
                        new XElement("DatumRodj", l.DatumRodj),
                        new XElement("Email", l.Email),
                        new XElement("Termini",
                            l.Termini.Select(t => new XElement("Termin",
                                t.Lekar != null ? new XElement("Lekar",
                                    new XElement("KorIme", t.Lekar.KorIme),
                                    new XElement("Sifra", t.Lekar.Sifra),
                                    new XElement("Ime", t.Lekar.Ime),
                                    new XElement("Prezime", t.Lekar.Prezime),
                                    new XElement("DatumRodj", t.Lekar.DatumRodj),
                                    new XElement("Email", t.Lekar.Email)
                                ) : null,
                                new XElement("Status", t.Status.ToString()),
                                new XElement("ZakazanTermin", t.ZakazanTermin),
                                new XElement("OpisTerapije", t.OpisTerapije)
                            ))
                        )
                    ))
                )
            );
            xml.Save(filePath);
        }


        

    }
}