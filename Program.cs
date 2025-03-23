using System;
using System.Collections.Generic;
using System.Linq;

namespace KontenerowiecApp
{
    public class OverfillException : Exception
    {
        public OverfillException(string msg) : base(msg) {}
    }

    public class HazardousOperationException : Exception
    {
        public HazardousOperationException(string msg) : base(msg) {}
    }

    public abstract class Kontener
    {
        private static int GlobalNumber = 1;

        public string NumerSeryjny { get; private set; }
        public double MasaLadunku { get; protected set; }
        public double MaksLadownosc { get; private set; }

        protected Kontener(double maksLad, string typ)
        {
            NumerSeryjny = $"KON-{typ}-{GlobalNumber++}";
            MaksLadownosc = maksLad;
            MasaLadunku = 0;
        }

        public virtual void Zaladuj(double masa)
        {
            if (MasaLadunku + masa > MaksLadownosc)
                throw new OverfillException($"przekroczono możliwy do załadowania ładunek kontenera {NumerSeryjny}");
            MasaLadunku += masa;
        }

        public virtual void Rozladuj()
        {
            MasaLadunku = 0;
        }

        public virtual string Info()
        {
            return $"{NumerSeryjny} (ładunek {MasaLadunku}/{MaksLadownosc} kg)";
        }
    }

    public class Kontenerowiec
    {
        private List<Kontener> kontenery = new List<Kontener>();

        public string Nazwa { get; private set; }
        public double Predkosc { get; private set; }
        public int MaksKontenerow { get; private set; }
        public double MaksWaga { get; private set; } 

        public Kontenerowiec(string nazwa, double predkosc, int maksKont, double maksWagaTon)
        {
            Nazwa = nazwa;
            Predkosc = predkosc;
            MaksKontenerow = maksKont;
            MaksWaga = maksWagaTon * 1000.0;
        }

        public void DodajKontener(Kontener k)
        {
            if (kontenery.Count >= MaksKontenerow)
                throw new InvalidOperationException($"Statek {Nazwa} ma już maksymalną liczbę kontenerów!");
            
            double aktualnaWaga = ObliczWageCalkowita() + k.MasaLadunku;
            if (aktualnaWaga > MaksWaga)
                throw new InvalidOperationException($"Statek {Nazwa} przekroczyłby maksymalną wagę {MaksWaga} kg!");

            kontenery.Add(k);
        }

        public void UsunKontener(string nrSeryjny)
        {
            var kont = kontenery.FirstOrDefault(x => x.NumerSeryjny == nrSeryjny);
            if (kont == null)
                throw new ArgumentException($"Kontener {nrSeryjny} nie znajduje się na statku {Nazwa}.");
            kontenery.Remove(kont);
        }

        public void RozladujKontener(string nrSeryjny)
        {
            var kont = kontenery.FirstOrDefault(x => x.NumerSeryjny == nrSeryjny);
            if (kont == null)
                throw new ArgumentException($"Kontener {nrSeryjny} nie znajduje się na statku {Nazwa}.");
            kont.Rozladuj();
        }

        public double ObliczWageCalkowita()
        {
            return kontenery.Sum(x => x.MasaLadunku);
        }

        public string Info()
        {
            return $"{Nazwa} (speed={Predkosc}, maxContainerNum={MaksKontenerow}, maxWeight={MaksWaga})";
        }

        public string WypiszKontenery()
        {
            if (kontenery.Count == 0) return "Brak";
            return string.Join(", ", kontenery.Select(k => k.Info()));
        }

        public static void PrzeniesKontener(string nrSeryjny, Kontenerowiec zrodlo, Kontenerowiec cel)
        {
            var k = zrodlo.kontenery.FirstOrDefault(x => x.NumerSeryjny == nrSeryjny);
            if (k == null)
                throw new ArgumentException($"Kontener {nrSeryjny} nie znajduje się na statku {zrodlo.Nazwa}.");
            cel.DodajKontener(k);
            zrodlo.kontenery.Remove(k);
        }
    }

    class Program
    {
        static List<Kontenerowiec> listaKontenerowcow = new List<Kontenerowiec>();
        static List<Kontener> listaKontenerow = new List<Kontener>();

        static void Main(string[] args)
        {
            while(true)
            {
                Console.Clear();

                Console.WriteLine("Lista kontenerowców:");
                if (listaKontenerowcow.Count == 0)
                    Console.WriteLine("Brak");
                else
                {
                    for(int i=0; i<listaKontenerowcow.Count; i++)
                    {
                        var st = listaKontenerowcow[i];
                        Console.WriteLine($"{st.Info()}");
                    }
                }

                Console.WriteLine("\nLista kontenerów:");
                if (listaKontenerow.Count == 0)
                    Console.WriteLine("Brak");
                else
                {
                    var wolne = listaKontenerow.Where(k => !CzyNaJakimsStatku(k.NumerSeryjny)).ToList();
                    if (wolne.Count == 0)
                        Console.WriteLine("Brak");
                    else
                    {
                        foreach(var k in wolne)
                        {
                            Console.WriteLine(k.Info());
                        }
                    }
                }
                
                Console.WriteLine("\nMożliwe akcje:");
                Console.WriteLine("1. Dodaj kontenerowiec");
                Console.WriteLine("2. Usuń kontenerowiec");
                Console.WriteLine("3. Dodaj kontener");
                Console.WriteLine("4. Usuń kontener (jeśli wolny)");
                Console.WriteLine("5. Załaduj ładunek do kontenera");
                Console.WriteLine("6. Rozładuj kontener");
                Console.WriteLine("7. Umieść kontener na kontenerowcu");
                Console.WriteLine("8. Usuń kontener z kontenerowca");
                Console.WriteLine("9. Przenieś kontener między kontenerowcami");
                Console.WriteLine("0. Zakończ");

                Console.Write("\nWybierz opcję: ");
                string opcja = Console.ReadLine();

                try
                {
                    switch(opcja)
                    {
                        case "1": DodajKontenerowiec(); break;
                        case "2": UsunKontenerowiec(); break;
                        case "3": DodajKontener(); break;
                        case "4": UsunKontener(); break;
                        case "5": ZaladujKontener(); break;
                        case "6": RozladujKontener(); break;
                        case "7": UmiescKontenerNaKontenerowcu(); break;
                        case "8": UsunKontenerZKontenerowca(); break;
                        case "9": PrzeniesKontenerMiedzyKontenerowcami(); break;
                        case "0": return; // wyjście
                        default: Console.WriteLine("Nieznana opcja!"); break;
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Błąd: {e.Message}");
                }

                Console.WriteLine("\nNaciśnij Enter, aby kontynuować...");
                Console.ReadLine();
            }
        }
        
        static bool CzyNaJakimsStatku(string nrSeryjny)
        {
            return listaKontenerowcow.Any(st =>
                st.Info().Contains(nrSeryjny));
        }

        static void DodajKontenerowiec()
        {
            Console.Write("Podaj nazwę kontenerowca: ");
            string nazwa = Console.ReadLine();
            Console.Write("Prędkość (węzły): ");
            double spd = double.Parse(Console.ReadLine());
            Console.Write("Maks. liczba kontenerów: ");
            int maxCnt = int.Parse(Console.ReadLine());
            Console.Write("Maks. waga (tony): ");
            double maxWt = double.Parse(Console.ReadLine());

            var nowy = new Kontenerowiec(nazwa, spd, maxCnt, maxWt);
            listaKontenerowcow.Add(nowy);
            Console.WriteLine("Dodano nowy kontenerowiec.");
        }

        static void UsunKontenerowiec()
        {
            if (listaKontenerowcow.Count == 0)
            {
                Console.WriteLine("Brak kontenerowców do usunięcia.");
                return;
            }
            for(int i=0; i<listaKontenerowcow.Count; i++)
            {
                Console.WriteLine($"{i+1}. {listaKontenerowcow[i].Info()}");
            }
            Console.Write("Wybierz numer do usunięcia: ");
            int idx = int.Parse(Console.ReadLine()) - 1;
            if (idx < 0 || idx >= listaKontenerowcow.Count)
            {
                Console.WriteLine("Błędny wybór.");
                return;
            }
            listaKontenerowcow.RemoveAt(idx);
            Console.WriteLine("Usunięto kontenerowiec.");
        }

        static void DodajKontener()
        {
            Console.Write("Podaj maks. ładowność kontenera (kg): ");
            double m = double.Parse(Console.ReadLine());
            Console.WriteLine("Na potrzeby przykładu tworzymy tylko kontener bazowy (lub jeden z typów).");
            var k = new KontenerBazowy(m);

            listaKontenerow.Add(k);
            Console.WriteLine($"Dodano kontener: {k.NumerSeryjny}");
        }

        static void UsunKontener()
        {
            var wolne = listaKontenerow.Where(k => !CzyNaJakimsStatku(k.NumerSeryjny)).ToList();
            if (wolne.Count == 0)
            {
                Console.WriteLine("Brak wolnych kontenerów.");
                return;
            }
            for(int i=0; i<wolne.Count; i++)
            {
                Console.WriteLine($"{i+1}. {wolne[i].Info()}");
            }
            Console.Write("Wybierz kontener do usunięcia: ");
            int id = int.Parse(Console.ReadLine()) - 1;
            if (id < 0 || id >= wolne.Count)
            {
                Console.WriteLine("Błędny wybór.");
                return;
            }
            listaKontenerow.Remove(wolne[id]);
            Console.WriteLine("Kontener usunięty.");
        }

        static void ZaladujKontener()
        {
            if (listaKontenerow.Count == 0)
            {
                Console.WriteLine("Brak kontenerów.");
                return;
            }
            for(int i=0; i<listaKontenerow.Count; i++)
            {
                Console.WriteLine($"{i+1}. {listaKontenerow[i].Info()}");
            }
            Console.Write("Wybierz kontener: ");
            int idx = int.Parse(Console.ReadLine()) - 1;
            if (idx < 0 || idx >= listaKontenerow.Count)
            {
                Console.WriteLine("Błędny wybór.");
                return;
            }
            Console.Write("Podaj masę do załadowania: ");
            double masa = double.Parse(Console.ReadLine());
            listaKontenerow[idx].Zaladuj(masa);
            Console.WriteLine("Załadowano.");
        }

        static void RozladujKontener()
        {
            if (listaKontenerow.Count == 0)
            {
                Console.WriteLine("Brak kontenerów.");
                return;
            }
            for(int i=0; i<listaKontenerow.Count; i++)
            {
                Console.WriteLine($"{i+1}. {listaKontenerow[i].Info()}");
            }
            Console.Write("Wybierz kontener do rozładowania: ");
            int id = int.Parse(Console.ReadLine()) - 1;
            if (id < 0 || id >= listaKontenerow.Count)
            {
                Console.WriteLine("Błędny wybór.");
                return;
            }
            listaKontenerow[id].Rozladuj();
            Console.WriteLine("Kontener rozładowany.");
        }

        static void UmiescKontenerNaKontenerowcu()
        {
            if (listaKontenerowcow.Count == 0)
            {
                Console.WriteLine("Brak kontenerowców.");
                return;
            }
            for(int i=0; i<listaKontenerowcow.Count; i++)
            {
                Console.WriteLine($"{i+1}. {listaKontenerowcow[i].Info()}");
            }
            Console.Write("Wybierz kontenerowiec: ");
            int sidx = int.Parse(Console.ReadLine()) - 1;
            if (sidx < 0 || sidx >= listaKontenerowcow.Count)
            {
                Console.WriteLine("Błędny wybór.");
                return;
            }

            var wolne = listaKontenerow.Where(k => !CzyNaJakimsStatku(k.NumerSeryjny)).ToList();
            if (wolne.Count == 0)
            {
                Console.WriteLine("Brak wolnych kontenerów.");
                return;
            }
            for(int i=0; i<wolne.Count; i++)
            {
                Console.WriteLine($"{i+1}. {wolne[i].Info()}");
            }
            Console.Write("Wybierz kontener: ");
            int cidx = int.Parse(Console.ReadLine()) - 1;
            if (cidx < 0 || cidx >= wolne.Count)
            {
                Console.WriteLine("Błędny wybór.");
                return;
            }
            listaKontenerowcow[sidx].DodajKontener(wolne[cidx]);
            Console.WriteLine("Kontener dodany do kontenerowca.");
        }

        static void UsunKontenerZKontenerowca()
        {
            if (listaKontenerowcow.Count == 0)
            {
                Console.WriteLine("Brak kontenerowców.");
                return;
            }
            for(int i=0; i<listaKontenerowcow.Count; i++)
            {
                Console.WriteLine($"{i+1}. {listaKontenerowcow[i].Info()}");
            }
            Console.Write("Wybierz kontenerowiec: ");
            int sidx = int.Parse(Console.ReadLine()) - 1;
            if (sidx < 0 || sidx >= listaKontenerowcow.Count)
            {
                Console.WriteLine("Błędny wybór.");
                return;
            }

            Console.Write("Podaj numer seryjny kontenera do usunięcia: ");
            string nr = Console.ReadLine();

            listaKontenerowcow[sidx].UsunKontener(nr);
            Console.WriteLine("Kontener usunięty z kontenerowca.");
        }

        static void PrzeniesKontenerMiedzyKontenerowcami()
        {
            if (listaKontenerowcow.Count < 2)
            {
                Console.WriteLine("Potrzeba co najmniej dwóch kontenerowców.");
                return;
            }
            Console.WriteLine("Kontenerowce:");
            for(int i=0; i<listaKontenerowcow.Count; i++)
            {
                Console.WriteLine($"{i+1}. {listaKontenerowcow[i].Info()}");
            }
            Console.Write("Wybierz kontenerowiec źródłowy: ");
            int z = int.Parse(Console.ReadLine()) - 1;
            Console.Write("Wybierz kontenerowiec docelowy: ");
            int d = int.Parse(Console.ReadLine()) - 1;
            if (z < 0 || z >= listaKontenerowcow.Count || d < 0 || d >= listaKontenerowcow.Count || z == d)
            {
                Console.WriteLine("Błędne dane.");
                return;
            }

            Console.Write("Podaj numer seryjny kontenera: ");
            string nr = Console.ReadLine();

            Kontenerowiec.PrzeniesKontener(nr, listaKontenerowcow[z], listaKontenerowcow[d]);
            Console.WriteLine("Przeniesiono kontener.");
        }
    }

    public class KontenerBazowy : Kontener
    {
        public KontenerBazowy(double maks) : base(maks, "B") { }
    }
}
