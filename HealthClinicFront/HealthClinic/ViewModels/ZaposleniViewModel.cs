﻿using HealthClinic.Dialogs;
using HealthClinic.Utilities;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using HealthClinic.Views.Dialogs.Brisanje;
using System.Windows.Input;
using Syncfusion.Pdf.Tables;
using System.Data;
using HealthClinic.Views.Dialogs.ProduzeneInformacije;
using Model.BusinessHours;
using HealthClinic.Model.Users;

namespace HealthClinic.ViewModels
{
    public class ZaposleniViewModel : ObservableObject
    {   
        public ZaposleniViewModel()
        {
            PieChart();                // init piecharta

            ucitavanjePodatakaUTabelu();
            DodajZaposlenogCommand = new RelayCommand(DodajZaposlenog);
            
            IzmeniZaposlenogCommand = new RelayCommand(IzmeniZaposlenog);
            GenerisiIzvestajZaposlenogCommand = new RelayCommand(GenerisiIzvestaj);
            RadniKalendarCommand = new RelayCommand(PodesavanjeRadnihKalendaraZaposlenih);
            IzbrisiZaposlenogCommand = new RelayCommand(IzbrisiZaposlenog);

            PocetniDatum = DateTime.Now.Date;
            KrajnjiDatum = DateTime.Now.Date;

            // potvrdjujem izmenjene podatke
            PotvrdaIzmenePodatakaCommand = new RelayCommand(PotvrdaIzmenePodataka);

            // potvrdjujem dodavanje podataka
            PotvrdaDodavanjaPodatakaCommand = new RelayCommand(PotvrdaDodavanjaPodataka);

            // potvrdjujem brisanje podataka
            PotvrdaBrisanjaPodatakaCommand = new RelayCommand(PotvrdaBrisanjaPodataka);

            odredjivanjeMogucihTipovaZaposlenih();

            // prikazivanje radnog kalendara selekovanog zaposlenog
            PrikazRadnogKalendaraCommand = new RelayCommand(PrikaziRadniKalendarZaposlenog);


            // potvrdujem odredjivanje radnog vremena zaposlenih
            PotvrdaOdredjivanjaRadnogVremenaCommand = new RelayCommand(OdrediRadnoVremeZaposlenih);

            PrikaziSlobodneLekareCommand = new RelayCommand(PrikaziSlobodneLekare);

        }

        #region Radno vreme zaposlenih

        private DateTime _pocetniDatum;

        public DateTime PocetniDatum
        {
            get { return _pocetniDatum; }
            set { _pocetniDatum = value; OnPropertyChanged("PocetniDatum"); }
        }

        private DateTime _krajnjiDatum;

        public DateTime KrajnjiDatum
        {
            get { return _krajnjiDatum; }
            set { _krajnjiDatum = value; OnPropertyChanged("KrajnjiDatum"); }
        }


        private DateTime _pocetniSat;

        public DateTime PocetniSat
        {
            get { return _pocetniSat; }
            set { _pocetniSat = value; OnPropertyChanged("PocetniSat"); }
        }

        private DateTime _krajnjiSat;

        public DateTime KrajnjiSat
        {
            get { return _krajnjiSat; }
            set { _krajnjiSat = value; }
        }


        #endregion

        #region Selektovani zaposlen

        private Employee _selektovaniZaposleni;


        // Bajndujem na SelectedItem u tabeli i dalje radim sa njim sta hocu
        // mogu ga dalje prikazivati
        // a moze se i proslediti u dijalog
        // tako sto se .DatContext tog dijalog postavi na this
        public Employee SelektovaniZaposleni
        {
            get { return _selektovaniZaposleni; }
            set { _selektovaniZaposleni = value; OnPropertyChanged("SelektovaniZaposleni"); }
        }

        #endregion

        #region Zaposleni za izmenu

        private Employee _zaposleniZaIzmenu;

        public Employee ZaposleniZaIzmenu
        {
            get { return _zaposleniZaIzmenu; }
            set { _zaposleniZaIzmenu = value; OnPropertyChanged("ZaposleniZaIzmenu"); }
        }


        #endregion

        #region Zaposleni za dodavanje

        private Employee _zaposleniZaDodavanje;

        public Employee ZaposleniZaDodavanje
        {
            get { return _zaposleniZaDodavanje; }
            set { _zaposleniZaDodavanje = value; OnPropertyChanged("ZaposleniZaDodavanje"); }
        }


        #endregion

        #region Trenutno aktivni prozor

        /// <summary>
        /// Promenljiva u kojoj cuvam trenutno otvoreni prozor
        /// kako bih kasnije u komandama u zavisnosti od komande
        /// recimo zatvorio trenutni prozor
        /// </summary>
        private Window _trenutniProzor;

        public Window TrenutniProzor
        {
            get { return _trenutniProzor; }
            set { _trenutniProzor = value; }
        }

        #endregion

        #region Komande

        public RelayCommand PotvrdaBrisanjaPodatakaCommand { get; private set; }
        public RelayCommand PotvrdaDodavanjaPodatakaCommand { get; private set; }
        public RelayCommand PotvrdaIzmenePodatakaCommand { get; private set; }
        public RelayCommand RadniKalendarCommand { get; private set; }
        public RelayCommand GenerisiIzvestajZaposlenogCommand { get; private set; }
        public RelayCommand DodajZaposlenogCommand { get; private set; }
        public RelayCommand IzmeniZaposlenogCommand { get; private set; }
        public RelayCommand IzbrisiZaposlenogCommand { get; private set; }
        public RelayCommand PrikazRadnogKalendaraCommand { get; private set; }
        public RelayCommand PotvrdaOdredjivanjaRadnogVremenaCommand { get; private set; }
        public RelayCommand PrikaziSlobodneLekareCommand { get; private set; }


        #endregion

        #region Funkcije koje komande koriste

        public void PotvrdaBrisanjaPodataka(object obj)
        {
            // sprecavam kada niko nije selektovan da ne dodje do erroa
            if (SelektovaniZaposleni is null)
                return;


            foreach (Employee trenutniZaposleni in Zaposleni)
            {
                if(trenutniZaposleni.Username == SelektovaniZaposleni.Username)
                {
                    MessageBox.Show("Uspesno ste izbrisali radnika sa korisnickim imenom " + trenutniZaposleni.Username);
                    podesiBrojOdredjenihZaposlenih(trenutniZaposleni, -1);
                    Zaposleni.Remove(trenutniZaposleni);

                    break;
                }
            }

            this.TrenutniProzor.Close();
        }

        public void PotvrdaDodavanjaPodataka(object ojb)
        {
            if (!validanZaposleni(ZaposleniZaDodavanje))
                return;

            // dodajem zaposlenog ukoliko je odgovor bio potvrdan
            ZaposleniZaDodavanje.BusinessHours = new BusinessHoursModel();
            ZaposleniZaDodavanje.BusinessHours.FromDate = DateTime.Now;
            ZaposleniZaDodavanje.BusinessHours.ToDate = DateTime.Now;

            Zaposleni.Add(ZaposleniZaDodavanje);
            podesiBrojOdredjenihZaposlenih(ZaposleniZaDodavanje, 1);
            this.TrenutniProzor.Close();
        }

        public void PotvrdaIzmenePodataka(object obj)
        {
            // regulisem da prvo povecam kolicinu novo izmenjenog broja odredjenog tipa zaposlenih
            podesiBrojOdredjenihZaposlenih(ZaposleniZaIzmenu, 1);

            //podesiBrojOdredjenihLekova(SelektovaniLek, -1);
            podesiBrojOdredjenihZaposlenih(SelektovaniZaposleni, -1);

            if (!validanZaposleni(ZaposleniZaIzmenu))
                return;

            // selektovani objekat prima vrednosti od menjanog objekta
            SelektovaniZaposleni.Name = ZaposleniZaIzmenu.Name;
            SelektovaniZaposleni.Surname = ZaposleniZaIzmenu.Surname;
            SelektovaniZaposleni.JobPosition = ZaposleniZaIzmenu.JobPosition;
            SelektovaniZaposleni.Password = ZaposleniZaIzmenu.Password;
            SelektovaniZaposleni.Username = ZaposleniZaIzmenu.Username;

            this.TrenutniProzor.Close();
        }

        public void PodesavanjeRadnihKalendaraZaposlenih(object obj)
        {
            IzabraniLekari = new ObservableCollection<Employee>();
            IzabraniLekar = new Employee();
            IzabraniLekarZaUklanjanje = new Employee();
            SlobodniLekari = new ObservableCollection<Employee>();

            TrenutniProzor = new RadniKalendarDijalog();
            TrenutniProzor.DataContext = this;             // kako bi povezao i ViewModel Zaposlenih za ovaj dijalog
            TrenutniProzor.ShowDialog();
        }

        public void GenerisiIzvestaj(object obj)
        {
            using (PdfDocument doc = new PdfDocument())
            {
                //Create a new PDF document.
                //PdfDocument doc = new PdfDocument();

                //Add a page.
                PdfPage page = doc.Pages.Add();

                // Create a PdfLightTable.
                PdfLightTable pdfLightTable = new PdfLightTable();

                // Initialize DataTable to assign as DateSource to the light table.
                DataTable table = new DataTable();

                //Include columns to the DataTable.
                table.Columns.Add("Ime");

                table.Columns.Add("Prezime");

                table.Columns.Add("Struka");

                //Include rows to the DataTable.
                foreach (Employee zaposlen in Zaposleni)
                {
                    table.Rows.Add(new string[] { zaposlen.Name, zaposlen.Surname, zaposlen.JobPosition });
                }


                //Assign data source.
                pdfLightTable.DataSource = table;

                //Draw PdfLightTable.
                pdfLightTable.Draw(page, new PointF(0, 0));

                //Save the document
                doc.Save("C:\\Users\\Vaxi\\Desktop\\6-semestar\\HCI\\projekat\\Vaksi\\HealthClinic\\IzvestajZaposlenih.pdf");

                //Close the document

                doc.Close(true);
            }

            MessageBox.Show("Uspesno kreiran izvestaj zaposlenih, mozete ga pogledati u tekucem direktorijumu");
        }

        public void DodajZaposlenog(object obj)
        {
            // kreiram novog zaposlenog da u slucaju potvrde mogu da ga dodam u listu zaposlenih
            ZaposleniZaDodavanje = new Employee();

            // prikaz dijaloga za dodavanje
            TrenutniProzor = new DodajZaposlenogDijalog();
            TrenutniProzor.DataContext = this;
            TrenutniProzor.ShowDialog();
        }

        public void IzmeniZaposlenog(object obj)
        {
            
            if (SelektovaniZaposleni is null)
                return;

            // Zaposleni za izmenu/stimanje preuzima podatke od selektovanog zaposlenog
            ZaposleniZaIzmenu = new Employee() {
                Name = SelektovaniZaposleni.Name,
                Surname = SelektovaniZaposleni.Surname,
                Password = SelektovaniZaposleni.Password,
                JobPosition = SelektovaniZaposleni.JobPosition,
                Username = SelektovaniZaposleni.Username
            };


            // podesavanje prikaza dijaloga izmene
            TrenutniProzor = new IzmeniZaposlenogDijalog();
            TrenutniProzor.DataContext = this;             // kako bi prebacio podatke iz ovog prozora u dijalog
            TrenutniProzor.ShowDialog();                   // podesavam da i dijalog moze upravljati istim podacima
        }

        public void IzbrisiZaposlenog(object obj)
        {
            if (SelektovaniZaposleni is null)
                return;

            // TODO: Mozda dodati nekad da pise tacno kog zaposlenog brisemo ali u nekim buducim verzijama
            TrenutniProzor = new ObrisiZaposlenogDijalog();
            TrenutniProzor.DataContext = this;
            TrenutniProzor.ShowDialog();
        }

        public void PrikaziRadniKalendarZaposlenog(object obj)
        {
            // prikazivanje bez vremena, samo datum ovako dobijam
            OdDatuma = SelektovaniZaposleni.BusinessHours.FromDate.ToShortDateString();
            DoDatuma = SelektovaniZaposleni.BusinessHours.ToDate.ToShortDateString();

            OdVremena = SelektovaniZaposleni.BusinessHours.FromHour.ToShortTimeString();
            DoVremena = SelektovaniZaposleni.BusinessHours.ToHour.ToShortTimeString();

            TrenutniProzor = new RadnoVremeZaposlenogDijalog();
            TrenutniProzor.DataContext = this;
            TrenutniProzor.ShowDialog();
        }
        
        public void OdrediRadnoVremeZaposlenih(object ojb)
        {
            // dodajem sve izabrane lekare
            foreach (Employee lekar in IzabraniLekari)
            {
                lekar.BusinessHours.FromDate = PocetniDatum;
                lekar.BusinessHours.ToDate = KrajnjiDatum;
                lekar.BusinessHours.FromHour = PocetniSat;
                lekar.BusinessHours.ToHour = KrajnjiSat;

            }
            this.TrenutniProzor.Close();
        }
        
        public void PrikaziSlobodneLekare(object obj)
        {
            SlobodniLekari = dobaviSlobodneLekare();
        }

        #endregion

        #region Slobodni lekari u tom opsegu
        private ObservableCollection<Employee> _slobodniLekari;

        public ObservableCollection<Employee> SlobodniLekari
        {
            get { return _slobodniLekari; }
            set { _slobodniLekari = value; OnPropertyChanged("SlobodniLekari"); }
        }


        #endregion

        #region Izabrani lekari kojima je potrebno promeniti radno vreme

        private ObservableCollection<Employee> _izabraniLekari;

        public ObservableCollection<Employee> IzabraniLekari
        {
            get { return _izabraniLekari; }
            set { _izabraniLekari = value; OnPropertyChanged("IzabraniLekari"); }
        }

        // izabrani lekar u odredjenom trenutku
        private Employee _izabraniLekar;

        public Employee IzabraniLekar
        {
            get { return _izabraniLekar; }
            set
            {
                _izabraniLekar = value;
                // da ne bih prazne dodavao
                if(!(IzabraniLekar is null) && !(IzabraniLekari.Contains(IzabraniLekar)))
                    IzabraniLekari.Add(IzabraniLekar);
                

                OnPropertyChanged("IzabraniLekar");
            }
        }

        // izabrani lekar za uklanjanje

        private Employee _izabraniLekarZaUklanjanje;

        public Employee IzabraniLekarZaUklanjanje
        {
            get { return _izabraniLekarZaUklanjanje; }
            set
            {
                if (_izabraniLekarZaUklanjanje != value)
                {
                    _izabraniLekarZaUklanjanje = value;

                    foreach (Employee lekar in IzabraniLekari)
                    {
                        if (IzabraniLekarZaUklanjanje is null)
                            break;

                        if (lekar.Username == IzabraniLekarZaUklanjanje.Username)
                        {
                            IzabraniLekari.Remove(lekar);
                            break;
                        }
                    }
                    OnPropertyChanged("IzabraniLekarZaUklanjanje");
                }
            }
        }


        #endregion

        #region Ucitavanje podataka zaposlenih

        private ObservableCollection<Employee> _zaposleni;

        public ObservableCollection<Employee> Zaposleni
        {
            get { return _zaposleni; }
            set { _zaposleni = value; OnPropertyChanged("Zaposleni"); }
        }

        private void ucitavanjePodatakaUTabelu()
        {
            //Tabela - popunjavanje
            Zaposleni = new ObservableCollection<Employee>();
            BusinessHoursModel bs = new BusinessHoursModel() { FromDate = new DateTime(2020,1,1), ToDate = new DateTime(2020, 2,1), FromHour= new DateTime(2020,1,1,9,30,30), ToHour= new DateTime(2020, 2, 1, 17, 30, 30) };
            Zaposleni.Add(new Employee()
            {
                Username = "zikaa",
                Name = "Zika",
                Surname = "Vojvodic",
                JobPosition = "Otorinolaringolog",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "dzoni",
                Name = "Nikola",
                Surname = "Zigic",
                JobPosition = "Oftamolog",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "markoni",
                Name = "Marko",
                Surname = "Bogdanovic",
                JobPosition = "Kardio hirurg",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "bobi",
                Name = "Boban",
                Surname = "Jokic",
                JobPosition = "Pedijatar",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "niki",
                Name = "Nikola",
                Surname = "Marjanovic",
                JobPosition = "Lekar opste prakse",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "zare",
                Name = "Zika",
                Surname = "Vojvodic",
                JobPosition = "Otorinolaringolog",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "nidroni",
                Name = "Nikola",
                Surname = "Zigic",
                JobPosition = "Sekretar",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "maron",
                Name = "Marko",
                Surname = "Bogdanovic",
                JobPosition = "Kardio hirurg",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "bobi2",
                Name = "Boban",
                Surname = "Jokic",
                JobPosition = "Pedijatar",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });
            Zaposleni.Add(new Employee()
            {
                Username = "dzoni2",
                Name = "Nikola",
                Surname = "Marjanovic",
                JobPosition = "Lekar opste prakse",
                Password = "*****",
                BusinessHours = new BusinessHoursModel()
                {
                    FromDate = bs.FromDate,
                    ToDate = bs.ToDate,
                    FromHour = bs.FromHour,
                    ToHour = bs.ToHour

                }
            });

            foreach (Employee zaposlen in Zaposleni)
            {
                podesiBrojOdredjenihZaposlenih(zaposlen, 1);
            }
        }


        #endregion

        #region Podesavanje broja odredjenog tipa zaposlenih

        /// <summary>
        /// Podesavam trenutni broj odredjenog tipa zaposlenih
        /// </summary>
        /// <param name="zaposlen"></param>
        /// <param name="koeficijentPravca"> Prosledjuje se broj koji govori koliko povecavam/smanjujem broj odredjenih zaposlenih </param>
        private void podesiBrojOdredjenihZaposlenih(Employee zaposlen, int koeficijentPravca)
        {
            if (zaposlen.JobPosition == "Lekar opste prakse")
            {
                if (this.UkupnoLekaraOpstePrakse is null)
                    BrojacLekaraOpstePrakse = 1;
                else
                    BrojacLekaraOpstePrakse += koeficijentPravca;
                this.UkupnoLekaraOpstePrakse = new ChartValues<int>() { BrojacLekaraOpstePrakse };

            }
            else if (zaposlen.JobPosition == "Stomatolog" || zaposlen.JobPosition == "Kardio hirurg" || zaposlen.JobPosition == "Pedijatar" || zaposlen.JobPosition == "Otorinolaringolog" || zaposlen.JobPosition == "Oftamolog")
            {
                if (this.UkupnoLekaraSpecijalista is null)
                    BrojacLekaraSpecijalista = 1;
                else
                    BrojacLekaraSpecijalista += koeficijentPravca;
                this.UkupnoLekaraSpecijalista = new ChartValues<int>() { BrojacLekaraSpecijalista };
            }
            else if (zaposlen.JobPosition == "Sekretar")
            {
                if (this.UkupnoOstalihZaposlenih is null)
                    BrojacOstalihZaposlenih = 1;
                else
                    BrojacOstalihZaposlenih += koeficijentPravca;
                this.UkupnoOstalihZaposlenih = new ChartValues<int>() { BrojacOstalihZaposlenih };
            }
        }

        #endregion

        #region Grafikon


        private ChartValues<int> _ukupnoLekaraOpstePrakse;
        private ChartValues<int> _ukupnoLekaraSpecijalista;
        private ChartValues<int> _ukupnoOstalihZaposlenih;


        public ChartValues<int> UkupnoLekaraOpstePrakse
        {
            get { return _ukupnoLekaraOpstePrakse; }
            set { _ukupnoLekaraOpstePrakse = value; OnPropertyChanged("UkupnoLekaraOpstePrakse"); }
        }

        public ChartValues<int> UkupnoLekaraSpecijalista
        {
            get { return _ukupnoLekaraSpecijalista; }
            set { _ukupnoLekaraSpecijalista = value; OnPropertyChanged("UkupnoLekaraSpecijalista"); }
        }

        public ChartValues<int> UkupnoOstalihZaposlenih
        {
            get { return _ukupnoOstalihZaposlenih; }
            set { _ukupnoOstalihZaposlenih = value; OnPropertyChanged("UkupnoOstalihZaposlenih"); }
        }

        public Func<ChartPoint, string> PointLabel { get; set; }

        public void PieChart()
        {
            PointLabel = chartPoint => string.Format("{0}({1:P})", chartPoint.Y, chartPoint.Participation);
       

        }
        #endregion

        #region Moguci tipovi zaposlenih

        private ObservableCollection<string> _moguciTipoviZaposlenih;

        public ObservableCollection<string> MoguciTipoviZaposlenih
        {
            get { return _moguciTipoviZaposlenih; }
            set { _moguciTipoviZaposlenih = value; OnPropertyChanged("MoguciTipoviZaposlenih"); }
        }

        private void odredjivanjeMogucihTipovaZaposlenih()
        {
            MoguciTipoviZaposlenih = new ObservableCollection<string>();
            MoguciTipoviZaposlenih.Add("Sekretar");
            MoguciTipoviZaposlenih.Add("Lekar opste prakse");
            MoguciTipoviZaposlenih.Add("Stomatolog");
            MoguciTipoviZaposlenih.Add("Kardio hirurg");
            MoguciTipoviZaposlenih.Add("Otorinolaringolog");
            MoguciTipoviZaposlenih.Add("Oftamolog");
            MoguciTipoviZaposlenih.Add("Pedijatar");
        }

        #endregion

        #region Brojaci odredjene kategorije zaposlenog
        // Brojim lekare opste i specijaliste kao i ostale zaposlene

        /// <summary>
        /// Potreban mi je i brojac koji ce se upisivati u cart,
        /// ne moze direktno i samo brojac da bude ali ni cart.
        /// </summary>
        private int _brojacLekaraOpstePrakse;
        private int _brojacLekaraSpecijalista;
        private int _brojacOstalihZaposlenih;


        public int BrojacLekaraOpstePrakse
        {
            get { return _brojacLekaraOpstePrakse; }
            set { _brojacLekaraOpstePrakse = value; OnPropertyChanged("BrojacLekaraOpstePrakse"); }
        }

        public int BrojacLekaraSpecijalista
        {
            get { return _brojacLekaraSpecijalista; }
            set { _brojacLekaraSpecijalista = value; OnPropertyChanged("BrojacLekaraSpecijalista"); }
        }

        public int BrojacOstalihZaposlenih
        {
            get { return _brojacOstalihZaposlenih; }
            set { _brojacOstalihZaposlenih = value; OnPropertyChanged("BrojacOstalihZaposlenih"); }
        }

        #endregion

        #region Validacija zaposlenog

        private bool validanZaposleni(Employee zaposlen)
        {
            if(zaposlen.Name is null)
            {
                MessageBox.Show("Niste uneli ime zaposlenog");
                return false;
            }

            if (zaposlen.Surname is null)
            {
                MessageBox.Show("Niste uneli prezime zaposlenog");
                return false;
            }

            if (zaposlen.JobPosition is null)
            {
                MessageBox.Show("Niste uneli zanimanje/struku zaposlenog");
                return false;
            }

            if (zaposlen.Username is null)
            {
                MessageBox.Show("Niste uneli korisnicko ime zaposlenog");
                return false;
            }

            // TODO: Kada saznas kako dobiti koje trenutno validacione probleme ima app, ovde to mogu hendlovati

            return true;
        }

        #endregion

        #region Singlton pattern
        private static ZaposleniViewModel instance = null;
        private static readonly object padlock = new object();


        public static ZaposleniViewModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ZaposleniViewModel();
                    }
                    return instance;
                }
            }
        }
        #endregion

        #region Dobavljanje slobodnih lekara
        private ObservableCollection<Employee> dobaviSlobodneLekare()
        {
            ObservableCollection<Employee> slobodniLekari = new ObservableCollection<Employee>();

            foreach (Employee lekar in Zaposleni)
            {
                if (lekar.BusinessHours.ToDate < PocetniDatum)
                    slobodniLekari.Add(lekar);
            }

            // foreach zaposlenog
            // if zadovoljava business hours
            // dodam ga u povratnu vrednost
            return slobodniLekari;

        }

        #endregion

        #region Podaci za prikaz radnog vremena lekara
        private string _odVremena;
        private string _doVremena;
        private string _odDatuma;
        private string _doDatuma;

        
        public string OdDatuma
        {
            get { return _odDatuma; }
            set { _odDatuma = value; OnPropertyChanged("OdDatuma"); }
        }

        public string DoDatuma
        {
            get { return _doDatuma; }
            set { _doDatuma = value; OnPropertyChanged("DoDatuma"); }
        }
        public string OdVremena
        {
            get { return _odVremena; }
            set { _odVremena = value; OnPropertyChanged("OdVremena"); }
        }

        public string DoVremena
        {
            get { return _doVremena; }
            set { _doVremena = value; OnPropertyChanged("DoVremena"); }
        }


        #endregion
    }
}