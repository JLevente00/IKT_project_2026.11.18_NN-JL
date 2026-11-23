// ============================================================================
// Konzolos rajzoló
// 1-es tanuló – Felület és cellatörlés.
//
// A Console.SetCursorPosition paramétereinek sorrendje (oszlop, sor).
// A rajzlap koordinátái (x, y) sorrendűek. A bal felső cella helye (0, 0).
// ============================================================================

using System;
using System.IO;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// A programnak interaktív konzolra van szüksége.
if (Console.IsInputRedirected || Console.IsOutputRedirected)
{
    Console.WriteLine("A program interaktív konzolablakban futtatható.");
    return;
}

// A rajzlap és a menü elrendezése miatt minimális ablakméretet kérünk.
const int minimalisSzelesseg = 76;
const int minimalisMagassag = 23;

if (Console.WindowWidth < minimalisSzelesseg ||
    Console.WindowHeight < minimalisMagassag)
{
    Console.WriteLine(
        $"Nagyítsd a konzolablakot legalább {minimalisSzelesseg} oszloposra " +
        $"és {minimalisMagassag} sorosra, majd indítsd újra a programot.");
    return;
}

Console.CursorVisible = false;

try
{
    var rajzolo = new Rajzolo();
    rajzolo.Futtatas();
}
finally
{
    // Akkor is visszaállítjuk a konzol állapotát, ha futás közben hiba történik.
    Console.ResetColor();
    Console.CursorVisible = true;
    Console.Clear();
    Console.WriteLine("A konzolos rajzoló bezárult.");
}

// ============================================================================
// A Rajzolo osztály tartalmazza a program működését.
// ============================================================================
sealed class Rajzolo
{
    // A bal oldali panel szélessége. A rajzlap a konzol fennmaradó részét kapja.
    private const int PanelSzelesseg = 24;
    private const int RajzlapKezdoOszlop = PanelSzelesseg;
    private const int AlsoSegedSorok = 3;

    // A rajzlap minden cellájához eltároljuk a karaktert és a színt.
    private char[,] karakterek;
    private ConsoleColor[,] szinek;

    private int rajzlapSzelesseg;
    private int rajzlapMagassag;

    // A kurzor koordinátái a rajzlapon belül nullától indulnak.
    private int kurzorOszlop;
    private int kurzorSor;

    private char aktualisKarakter = '#';
    private ConsoleColor aktualisSzin = ConsoleColor.Cyan;

    // Ha az érték true, az eszköz le van téve, és mozgás közben rajzol.
    // Induláskor az eszköz fel van emelve.
    private bool eszkozLeteve = false;

    private bool menuNyitva = false;
    private int menuValasztas = 0;
    private string hibaUzenet = "";
    private string uzenet = "M: Menü | Nyilak: Mozgás | Szóköz: Eszköz | F1–F12: Nyomdák";

    private readonly string[] menuPontok =
    {
        "Karakter választása",
        "Szín választása",
        "Cella törlése",
        "Minta betöltése",
        "Kilépés"
    };

    private readonly char[] valaszthatoKarakterek =
    {
        '#', '*', '+', 'X', '@', '%', 'O', '.', ' '
    };

    private readonly ConsoleColor[] valaszthatoSzinek =
    {
        ConsoleColor.White,
        ConsoleColor.Gray,
        ConsoleColor.DarkGray,
        ConsoleColor.Red,
        ConsoleColor.DarkRed,
        ConsoleColor.Green,
        ConsoleColor.DarkGreen,
        ConsoleColor.Blue,
        ConsoleColor.DarkBlue,
        ConsoleColor.Cyan,
        ConsoleColor.DarkCyan,
        ConsoleColor.Yellow,
        ConsoleColor.DarkYellow,
        ConsoleColor.Magenta,
        ConsoleColor.DarkMagenta
    };

    // A nevek sorrendje megegyezik a választható színek sorrendjével.
    private readonly string[] szinNevek =
    {
        "Fehér", "Szürke", "Sötétszürke", "Piros", "Sötétpiros",
        "Zöld", "Sötétzöld", "Kék", "Sötétkék", "Cián",
        "Sötétcián", "Sárga", "Sötétsárga", "Bíbor", "Sötétbíbor"
    };

    public Rajzolo()
    {
        // A rajzlap szélessége a konzolablak méretétől függ.
        MeretekFrissitese();

        karakterek = new char[rajzlapMagassag, rajzlapSzelesseg];
        szinek = new ConsoleColor[rajzlapMagassag, rajzlapSzelesseg];

        RajzTorlese();
        kurzorOszlop = rajzlapSzelesseg / 2;
        kurzorSor = rajzlapMagassag / 2;
    }

    // A rajzlap a fejléc és az alsó információs sáv közötti területet használja.
    private void MeretekFrissitese()
    {
        rajzlapSzelesseg = Console.WindowWidth - RajzlapKezdoOszlop - 1;
        rajzlapMagassag = Console.WindowHeight - 5;

        if (rajzlapSzelesseg < 1 || rajzlapMagassag < 1)
            throw new InvalidOperationException("Túl kicsi a konzolablak.");
    }

    // ------------------------------------------------------------------------
    // Aktuális cella törlése
    // ------------------------------------------------------------------------
    private void AktualisCellaTorlese()
    {
        // A kurzor alatti cellát töröljük.
        CellabaIr(kurzorOszlop, kurzorSor, ' ', ConsoleColor.Gray);
        uzenet = "Az aktuális cella törölve.";
        hibaUzenet = "";
    }

    // ------------------------------------------------------------------------
    // Rajzlap és cellák kezelése
    // ------------------------------------------------------------------------
    private bool ErvenyesKoordinata(int x, int y)
    {
        return x >= 0 && x < rajzlapSzelesseg &&
               y >= 0 && y < rajzlapMagassag;
    }

    private void CellabaIr(int x, int y, char karakter, ConsoleColor szin)
    {
        if (!ErvenyesKoordinata(x, y))
            return;

        karakterek[y, x] = karakter;
        szinek[y, x] = szin;
    }

    private void RajzTorlese()
    {
        for (int y = 0; y < rajzlapMagassag; y++)
        {
            for (int x = 0; x < rajzlapSzelesseg; x++)
            {
                karakterek[y, x] = ' ';
                szinek[y, x] = ConsoleColor.Gray;
            }
        }
    }

    private void Hiba(string szoveg)
    {
        hibaUzenet = szoveg;
        uzenet = szoveg;

        // Windowson hangjelzést, más rendszeren konzolcsengőt használunk.
        try
        {
            if (OperatingSystem.IsWindows())
                Console.Beep(850, 140);
            else
                Console.Write('\a');
        }
        catch
        {
            // Hangjelzés nélkül is megjelenik a hibaüzenet.
        }
    }

    // ------------------------------------------------------------------------
    // Képernyő kirajzolása
    // ------------------------------------------------------------------------
    private void Megjelenites()
    {
        // A konzolablak futás közbeni átméretezését is kezeljük.
        int ujSzelesseg = Console.WindowWidth - RajzlapKezdoOszlop - 1;
        int ujMagassag = Console.WindowHeight - 5;

        if (ujSzelesseg != rajzlapSzelesseg ||
            ujMagassag != rajzlapMagassag)
        {
            // Átméretezéskor megőrizzük a korábbi rajz elférő részét.
            char[,] regiKarakterek = karakterek;
            ConsoleColor[,] regiSzinek = szinek;
            int regiSzelesseg = rajzlapSzelesseg;
            int regiMagassag = rajzlapMagassag;

            rajzlapSzelesseg = Math.Max(1, ujSzelesseg);
            rajzlapMagassag = Math.Max(1, ujMagassag);
            karakterek = new char[rajzlapMagassag, rajzlapSzelesseg];
            szinek = new ConsoleColor[rajzlapMagassag, rajzlapSzelesseg];
            RajzTorlese();

            for (int y = 0; y < Math.Min(regiMagassag, rajzlapMagassag); y++)
            {
                for (int x = 0; x < Math.Min(regiSzelesseg, rajzlapSzelesseg); x++)
                {
                    karakterek[y, x] = regiKarakterek[y, x];
                    szinek[y, x] = regiSzinek[y, x];
                }
            }

            kurzorOszlop = Math.Clamp(kurzorOszlop, 0, rajzlapSzelesseg - 1);
            kurzorSor = Math.Clamp(kurzorSor, 0, rajzlapMagassag - 1);
        }

        Console.ResetColor();
        Console.Clear();

        // 2-es tanuló – A keret a rajzolható cellákon kívül van.
        int balKeret = RajzlapKezdoOszlop - 1;
        int jobbKeret = RajzlapKezdoOszlop + rajzlapSzelesseg;
        string vizszintesVonal = new string('─', rajzlapSzelesseg);
        string felsoVonal = vizszintesVonal;
        const string rajzlapCim = "─ Rajzlap ";
        if (rajzlapSzelesseg > rajzlapCim.Length)
            felsoVonal = rajzlapCim + vizszintesVonal.Substring(rajzlapCim.Length);

        // Fejléc.
        Console.SetCursorPosition(0, 0);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Konzolos rajzoló".PadRight(balKeret));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"┌{felsoVonal}┐");
        Console.ResetColor();

        // Bal oldali panel.
        if (menuNyitva)
            MenuPanelRajzolasa();
        AllapotPanelRajzolasa();

        // A rajzlap minden celláját a tárolt karakterrel és színnel rajzoljuk ki.
        for (int y = 0; y < rajzlapMagassag; y++)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(balKeret, y + 1);
            Console.Write('│');
            Console.SetCursorPosition(jobbKeret, y + 1);
            Console.Write('│');
            Console.ResetColor();

            for (int x = 0; x < rajzlapSzelesseg; x++)
            {
                Console.SetCursorPosition(RajzlapKezdoOszlop + x, y + 1);

                // A kurzor helyét fordított színekkel mutatjuk.
                if (x == kurzorOszlop && y == kurzorSor)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    char kurzorKarakter = karakterek[y, x] == ' '
                        ? ' '
                        : karakterek[y, x];
                    Console.Write(kurzorKarakter);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = szinek[y, x];
                    Console.Write(karakterek[y, x]);
                    Console.ResetColor();
                }
            }
        }

        Console.SetCursorPosition(balKeret, rajzlapMagassag + 1);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"└{vizszintesVonal}┘");
        Console.ResetColor();

        // Alsó információs sorok.
        int alsoSor = Console.WindowHeight - AlsoSegedSorok;
        Console.SetCursorPosition(0, alsoSor + 1);
        string statusz = hibaUzenet.Length > 0
            ? "Hiba: " + hibaUzenet
            : uzenet;
        Console.ForegroundColor = hibaUzenet.Length > 0
            ? ConsoleColor.Red
            : ConsoleColor.Gray;
        Console.Write(statusz.PadRight(Math.Max(1, Console.WindowWidth - 1))
            .Substring(0, Math.Max(1, Console.WindowWidth - 1)));
        Console.ResetColor();

        Console.SetCursorPosition(0, alsoSor + 2);
        Console.Write("Kilépés: M, majd a Kilépés menüpont. Bal felső cella: (0, 0).");

        // A konzol kurzorát a kiemelt rajzlapi cellára állítjuk.
        Console.SetCursorPosition(
            Math.Min(Console.WindowWidth - 1, RajzlapKezdoOszlop + kurzorOszlop),
            Math.Min(Console.WindowHeight - 1, kurzorSor + 1));
    }

    private void AllapotPanelRajzolasa()
    {
        int sor = menuNyitva ? 9 : 4;
        int sorkoz = menuNyitva ? 1 : 2;
        int szinIndex = Array.IndexOf(valaszthatoSzinek, aktualisSzin);
        KiirPanelSort(sor, $"Pozíció: ({kurzorOszlop}, {kurzorSor})");
        KiirPanelSort(sor + sorkoz, $"Karakter: [{LathatoKarakter(aktualisKarakter)}]");
        KiirPanelSort(sor + 2 * sorkoz, $"Szín: {szinNevek[szinIndex]}");
        KiirPanelSort(sor + 3 * sorkoz, eszkozLeteve ? "Eszköz: Letéve" : "Eszköz: Felemelve");

        if (menuNyitva)
            return;

        KiirPanelSort(2, "M: Menü megnyitása");
        KiirPanelSort(13, "Nyilak: mozgás");
        KiirPanelSort(14, "Szóköz: eszköz");
        KiirPanelSort(15, "F1–F3: X");
        KiirPanelSort(16, "F4–F6: Kereszt");
        KiirPanelSort(17, "F7–F9: Négyzetkeret");
        KiirPanelSort(18, "F10–F12: Rombuszkeret");
    }

    private void MenuPanelRajzolasa()
    {
        KiirPanelSort(2, "Menü");
        for (int i = 0; i < menuPontok.Length; i++)
        {
            Console.SetCursorPosition(0, 4 + i);
            string jel = i == menuValasztas ? "> " : "  ";
            Console.ForegroundColor = i == menuValasztas
                ? ConsoleColor.Yellow
                : ConsoleColor.Gray;
            Console.Write((jel + menuPontok[i]).PadRight(PanelSzelesseg - 1));
            Console.ResetColor();
        }

        KiirPanelSort(14, "Fel/Le: választás");
        KiirPanelSort(15, "Enter: megnyitás");
        KiirPanelSort(16, "M vagy Esc: vissza");
    }

    private void KiirPanelSort(int sor, string szoveg)
    {
        if (sor < 0 || sor >= Console.WindowHeight)
            return;

        Console.SetCursorPosition(0, sor);
        Console.ResetColor();
        string rovid = szoveg.Length > PanelSzelesseg - 1
            ? szoveg.Substring(0, PanelSzelesseg - 1)
            : szoveg;
        Console.Write(rovid.PadRight(PanelSzelesseg - 1));
    }

    private static string LathatoKarakter(char karakter)
    {
        return karakter == ' ' ? "szóköz" : karakter.ToString();
    }

    // ----------------------------------------------------------------
    // 2-es tanuló:
    // ----------------------------------------------------------------

    private bool MenuBillentyuKezelese(ConsoleKeyInfo billentyu)
    {
        ConsoleKey gomb = billentyu.Key;
        hibaUzenet = "";

        if (gomb == ConsoleKey.UpArrow || gomb == ConsoleKey.DownArrow)
        {
            int irany = gomb == ConsoleKey.UpArrow ? -1 : 1;
            menuValasztas = (menuValasztas + irany + menuPontok.Length) % menuPontok.Length;
            uzenet = $"Menüpont: {menuPontok[menuValasztas]}.";
        }
        else if (gomb == ConsoleKey.M || gomb == ConsoleKey.Escape)
        {
            menuNyitva = false;
            uzenet = "Menü bezárva. A rajzolás folytatható.";
        }
        else if (gomb != ConsoleKey.Enter)
        {
            Hiba("A menüben a Fel/Le, az Enter, az M és az Esc használható.");
        }
        else if (menuValasztas == 4)
        {
            return false;
        }
        else if (menuValasztas == 2)
        {
            AktualisCellaTorlese();
        }
        else if (menuValasztas == 3)
        {
            Console.Clear();
            Console.CursorVisible = true;
            Console.Write("A .txt fájl útvonala (üres sorral mégse): ");
            string? utvonal = Console.ReadLine();
            Console.CursorVisible = false;

            if (string.IsNullOrWhiteSpace(utvonal))
            {
                uzenet = "A betöltést megszakítottad.";
                return true;
            }

            utvonal = utvonal.Trim().Trim('"');
            if (!utvonal.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                Hiba("Csak .txt fájl tölthető be.");
                return true;
            }

            string minta;
            try
            {
                using FileStream fajl = File.OpenRead(utvonal);
                long legnagyobbFajlmeret = (rajzlapSzelesseg + 2L) * rajzlapMagassag * 4 + 4;
                if (fajl.Length > legnagyobbFajlmeret)
                {
                    Hiba("A fájl túl nagy a rajzlaphoz.");
                    return true;
                }

                using StreamReader olvaso = new StreamReader(fajl);
                minta = olvaso.ReadToEnd().Replace("\r\n", "\n").Replace('\r', '\n');
            }
            catch (Exception kivetel) when (kivetel is IOException || kivetel is UnauthorizedAccessException ||
                                            kivetel is ArgumentException || kivetel is NotSupportedException)
            {
                Hiba("A fájl nem olvasható. Ellenőrizd az útvonalat.");
                return true;
            }

            int sor = 0;
            int oszlop = 0;
            foreach (char jel in minta)
            {
                if (sor >= rajzlapMagassag || (jel != '\n' && oszlop >= rajzlapSzelesseg))
                {
                    Hiba("A minta nem fér el a rajzlapon.");
                    return true;
                }
                if (jel == '\n')
                {
                    sor = sor + 1;
                    oszlop = 0;
                }
                else if (char.IsControl(jel))
                {
                    Hiba("A mintában olyan jel van, amely nem jeleníthető meg.");
                    return true;
                }
                else
                {
                    oszlop = oszlop + 1;
                }
            }


            RajzTorlese();
            sor = 0;
            oszlop = 0;
            foreach (char jel in minta)
            {
                if (jel == '\n')
                {
                    sor = sor + 1;
                    oszlop = 0;
                }
                else
                {
                    CellabaIr(oszlop, sor, jel, aktualisSzin);
                    oszlop = oszlop + 1;
                }
            }

            menuNyitva = false;
            uzenet = "A minta betöltve. Folytathatod a rajzolást.";
        }
        else
        {
            bool karaktertValaszt = menuValasztas == 0;
            int index = karaktertValaszt
                ? Array.IndexOf(valaszthatoKarakterek, aktualisKarakter)
                : Array.IndexOf(valaszthatoSzinek, aktualisSzin);
            int darab = karaktertValaszt ? valaszthatoKarakterek.Length : valaszthatoSzinek.Length;

            while (true)
            {
                uzenet = "Válassz a nyilakkal, majd nyomj Entert.";
                Megjelenites();
                KiirPanelSort(14, "Fel/Le: váltás");
                KiirPanelSort(15, "Enter: mentés");
                KiirPanelSort(16, "M vagy Esc: mégse");

                if (karaktertValaszt)
                {
                    KiirPanelSort(18, $"Jelölt: [{LathatoKarakter(valaszthatoKarakterek[index])}]");
                }
                else if (Console.WindowHeight > 18)
                {
                    KiirPanelSort(18, "Jelölt:");
                    Console.SetCursorPosition(8, 18);
                    Console.ForegroundColor = valaszthatoSzinek[index];
                    Console.Write(szinNevek[index]);
                    Console.ResetColor();
                }

                gomb = Console.ReadKey(true).Key;
                hibaUzenet = "";
                if (gomb == ConsoleKey.M || gomb == ConsoleKey.Escape)
                {
                    uzenet = "A választást megszakítottad.";
                    break;
                }
                if (gomb == ConsoleKey.Enter)
                {
                    if (karaktertValaszt)
                    {
                        aktualisKarakter = valaszthatoKarakterek[index];
                        uzenet = $"A rajzkarakter: [{LathatoKarakter(aktualisKarakter)}].";
                    }
                    else
                    {
                        aktualisSzin = valaszthatoSzinek[index];
                        uzenet = $"A rajzszín: {szinNevek[index]}.";
                    }
                    break;
                }
                if (gomb == ConsoleKey.UpArrow)
                    index = (index - 1 + darab) % darab;
                else if (gomb == ConsoleKey.DownArrow)
                    index = (index + 1) % darab;
                else
                    Hiba("A választóban a Fel/Le, az Enter, az M és az Esc használható.");
            }
        }

        return true;
    }

    public void Futtatas()
    {
        Megjelenites();

        bool fut = true;
        while (fut)
        {
            ConsoleKeyInfo billentyu = Console.ReadKey(true);
            ConsoleKey gomb = billentyu.Key;

            if (menuNyitva)
            {
                fut = MenuBillentyuKezelese(billentyu);
            }
            else if (gomb == ConsoleKey.M || gomb == ConsoleKey.Escape)
            {
                menuNyitva = true;
                menuValasztas = 0;
                hibaUzenet = "";
                uzenet = "Menü megnyitva.";
            }
            else
            {
                int ujKurzorOszlop = kurzorOszlop;
                int ujKurzorSor = kurzorSor;
                if (gomb == ConsoleKey.UpArrow)
                    ujKurzorSor = kurzorSor - 1;
                else if (gomb == ConsoleKey.DownArrow)
                    ujKurzorSor = kurzorSor + 1;
                else if (gomb == ConsoleKey.LeftArrow)
                    ujKurzorOszlop = kurzorOszlop - 1;
                else if (gomb == ConsoleKey.RightArrow)
                    ujKurzorOszlop = kurzorOszlop + 1;

                bool mozognaAKurzor = ujKurzorOszlop != kurzorOszlop || ujKurzorSor != kurzorSor;
                bool eszkozValtas = gomb == ConsoleKey.Spacebar;
                if (mozognaAKurzor || eszkozValtas)
                {
                    bool oszlopErvenyes = ujKurzorOszlop >= 0 && ujKurzorOszlop < rajzlapSzelesseg;
                    bool sorErvenyes = ujKurzorSor >= 0 && ujKurzorSor < rajzlapMagassag;
                    if (!oszlopErvenyes || !sorErvenyes)
                    {
                        Hiba("A rajzlap szélén túl nem lehet lépni.");
                    }
                    else
                    {
                        kurzorOszlop = ujKurzorOszlop;
                        kurzorSor = ujKurzorSor;
                        hibaUzenet = "";
                        uzenet = "Kurzor mozgatva.";

                        if (eszkozValtas)
                        {
                            if (eszkozLeteve)
                            {
                                eszkozLeteve = false;
                                uzenet = "Az eszköz felemelve.";
                            }
                            else
                            {
                                eszkozLeteve = true;
                                uzenet = "Az eszköz letéve.";
                            }
                        }

                        if (eszkozLeteve)
                        {
                            karakterek[kurzorSor, kurzorOszlop] = aktualisKarakter;
                            szinek[kurzorSor, kurzorOszlop] = aktualisSzin;
                        }
                    }
                }
            }

            if (!fut)
                break;

            Megjelenites();


            if (menuNyitva)
                continue;

            int meret = 0;
            if (gomb == ConsoleKey.F1 || gomb == ConsoleKey.F4 ||
                gomb == ConsoleKey.F7 || gomb == ConsoleKey.F10)
                meret = 3;
            else if (gomb == ConsoleKey.F2 || gomb == ConsoleKey.F5 ||
                     gomb == ConsoleKey.F8 || gomb == ConsoleKey.F11)
                meret = 5;
            else if (gomb == ConsoleKey.F3 || gomb == ConsoleKey.F6 ||
                     gomb == ConsoleKey.F9 || gomb == ConsoleKey.F12)
                meret = 7;

            if (meret == 0)
                continue;

            string alakzat = "X";
            if (gomb == ConsoleKey.F4 || gomb == ConsoleKey.F5 || gomb == ConsoleKey.F6)
                alakzat = "kereszt";
            else if (gomb == ConsoleKey.F7 || gomb == ConsoleKey.F8 || gomb == ConsoleKey.F9)
                alakzat = "négyzetkeret";
            else if (gomb == ConsoleKey.F10 || gomb == ConsoleKey.F11 || gomb == ConsoleKey.F12)
                alakzat = "rombuszkeret";

            int sugar = meret / 2;
            if (kurzorOszlop - sugar < 0 || kurzorOszlop + sugar >= rajzlapSzelesseg ||
                kurzorSor - sugar < 0 || kurzorSor + sugar >= rajzlapMagassag)
            {
                Hiba($"A {meret}×{meret} méretű nyomda nem fér el ezen a helyen.");
                Megjelenites();
                continue;
            }

            for (int sorEltolas = -sugar; sorEltolas <= sugar; sorEltolas = sorEltolas + 1)
            {
                for (int oszlopEltolas = -sugar; oszlopEltolas <= sugar; oszlopEltolas = oszlopEltolas + 1)
                {
                    int vizszintesTavolsag = Math.Abs(oszlopEltolas);
                    int fuggolegesTavolsag = Math.Abs(sorEltolas);
                    bool jelolt = false;

                    if (alakzat == "X")
                        jelolt = vizszintesTavolsag == fuggolegesTavolsag;
                    else if (alakzat == "kereszt")
                        jelolt = oszlopEltolas == 0 || sorEltolas == 0;
                    else if (alakzat == "négyzetkeret")
                        jelolt = vizszintesTavolsag == sugar || fuggolegesTavolsag == sugar;
                    else if (alakzat == "rombuszkeret")
                        jelolt = vizszintesTavolsag + fuggolegesTavolsag == sugar;

                    if (!jelolt)
                        continue;

                    int celOszlop = kurzorOszlop + oszlopEltolas;
                    int celSor = kurzorSor + sorEltolas;
                    karakterek[celSor, celOszlop] = aktualisKarakter;
                    szinek[celSor, celOszlop] = aktualisSzin;

                    Console.ForegroundColor = aktualisSzin;
                    if (oszlopEltolas == 0 && sorEltolas == 0)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.SetCursorPosition(RajzlapKezdoOszlop + celOszlop, celSor + 1);
                    Console.Write(aktualisKarakter);
                    Console.ResetColor();
                }
            }

            hibaUzenet = "";
            uzenet = $"Elhelyezve: {meret}×{meret} méretű {alakzat}.";
            Console.SetCursorPosition(0, Console.WindowHeight - AlsoSegedSorok + 1);
            Console.ForegroundColor = ConsoleColor.Gray;
            int statuszSzelesseg = Console.WindowWidth - 1;
            Console.Write(uzenet.PadRight(statuszSzelesseg).Substring(0, statuszSzelesseg));
            Console.ResetColor();
            Console.SetCursorPosition(RajzlapKezdoOszlop + kurzorOszlop, kurzorSor + 1);
        }
    }
}

