// ============================================================================
// Konzolos rajzoló
// 1-es tanuló – Felület és cellatörlés.
//
// A Console.SetCursorPosition paramétereinek sorrendje (oszlop, sor).
// A rajzlap koordinátái (x, y) sorrendűek. A bal felső cella helye (0, 0).
// ============================================================================

using System;
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
    private char[,] _karakterek;
    private ConsoleColor[,] _szinek;

    private int _rajzlapSzelesseg;
    private int _rajzlapMagassag;

    // A kurzor koordinátái a rajzlapon belül nullától indulnak.
    private int _x;
    private int _y;

    private char _aktualisKarakter = '#';
    private ConsoleColor _aktualisSzin = ConsoleColor.Cyan;

    // Ha az érték true, az eszköz le van téve, és mozgás közben rajzol.
    // Induláskor az eszköz fel van emelve.
    private bool _eszkozLeteve = false;

    private bool _menuNyitva = false;
    private int _menuValasztas = 0;
    private string _hibaUzenet = "";
    private string _uzenet = "M: Menü | Nyilak: Mozgás | Szóköz: Eszköz | F1–F12: Nyomdák";

    private readonly string[] _menuPontok =
    {
        "Karakter választása",
        "Szín választása",
        "Cella törlése",
        "Kilépés"
    };

    private readonly char[] _valaszthatoKarakterek =
    {
        '#', '*', '+', 'X', '@', '%', 'O', '.', ' '
    };

    private readonly ConsoleColor[] _valaszthatoSzinek =
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
    private readonly string[] _szinNevek =
    {
        "Fehér", "Szürke", "Sötétszürke", "Piros", "Sötétpiros",
        "Zöld", "Sötétzöld", "Kék", "Sötétkék", "Cián",
        "Sötétcián", "Sárga", "Sötétsárga", "Bíbor", "Sötétbíbor"
    };

    public Rajzolo()
    {
        // A rajzlap szélessége a konzolablak méretétől függ.
        MeretekFrissitese();

        _karakterek = new char[_rajzlapMagassag, _rajzlapSzelesseg];
        _szinek = new ConsoleColor[_rajzlapMagassag, _rajzlapSzelesseg];

        RajzTorlese();
        _x = _rajzlapSzelesseg / 2;
        _y = _rajzlapMagassag / 2;
    }

    // A rajzlap a fejléc és az alsó információs sáv közötti területet használja.
    private void MeretekFrissitese()
    {
        _rajzlapSzelesseg = Console.WindowWidth - RajzlapKezdoOszlop - 1;
        _rajzlapMagassag = Console.WindowHeight - 5;

        if (_rajzlapSzelesseg < 1 || _rajzlapMagassag < 1)
            throw new InvalidOperationException("Túl kicsi a konzolablak.");
    }

    // ------------------------------------------------------------------------
    // Aktuális cella törlése
    // ------------------------------------------------------------------------
    private void AktualisCellaTorlese()
    {
        // A kurzor alatti cellát töröljük.
        CellabaIr(_x, _y, ' ', ConsoleColor.Gray);
        _uzenet = "Az aktuális cella törölve.";
        _hibaUzenet = "";
    }

    // ------------------------------------------------------------------------
    // Rajzlap és cellák kezelése
    // ------------------------------------------------------------------------
    private bool ErvenyesKoordinata(int x, int y)
    {
        return x >= 0 && x < _rajzlapSzelesseg &&
               y >= 0 && y < _rajzlapMagassag;
    }

    private void CellabaIr(int x, int y, char karakter, ConsoleColor szin)
    {
        if (!ErvenyesKoordinata(x, y))
            return;

        _karakterek[y, x] = karakter;
        _szinek[y, x] = szin;
    }

    private void RajzTorlese()
    {
        for (int y = 0; y < _rajzlapMagassag; y++)
        {
            for (int x = 0; x < _rajzlapSzelesseg; x++)
            {
                _karakterek[y, x] = ' ';
                _szinek[y, x] = ConsoleColor.Gray;
            }
        }
    }

    private void Hiba(string szoveg)
    {
        _hibaUzenet = szoveg;
        _uzenet = szoveg;

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

        if (ujSzelesseg != _rajzlapSzelesseg ||
            ujMagassag != _rajzlapMagassag)
        {
            // Átméretezéskor megőrizzük a korábbi rajz elférő részét.
            char[,] regiKarakterek = _karakterek;
            ConsoleColor[,] regiSzinek = _szinek;
            int regiSzelesseg = _rajzlapSzelesseg;
            int regiMagassag = _rajzlapMagassag;

            _rajzlapSzelesseg = Math.Max(1, ujSzelesseg);
            _rajzlapMagassag = Math.Max(1, ujMagassag);
            _karakterek = new char[_rajzlapMagassag, _rajzlapSzelesseg];
            _szinek = new ConsoleColor[_rajzlapMagassag, _rajzlapSzelesseg];
            RajzTorlese();

            for (int y = 0; y < Math.Min(regiMagassag, _rajzlapMagassag); y++)
            {
                for (int x = 0; x < Math.Min(regiSzelesseg, _rajzlapSzelesseg); x++)
                {
                    _karakterek[y, x] = regiKarakterek[y, x];
                    _szinek[y, x] = regiSzinek[y, x];
                }
            }

            _x = Math.Clamp(_x, 0, _rajzlapSzelesseg - 1);
            _y = Math.Clamp(_y, 0, _rajzlapMagassag - 1);
        }

        Console.ResetColor();
        Console.Clear();

        // Fejléc.
        Console.SetCursorPosition(0, 0);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Konzolos rajzoló".PadRight(PanelSzelesseg));
        Console.ResetColor();
        Console.WriteLine("Rajzlap");

        // Bal oldali panel.
        if (_menuNyitva)
            MenuPanelRajzolasa();
        AllapotPanelRajzolasa();

        // A rajzlap minden celláját a tárolt karakterrel és színnel rajzoljuk ki.
        for (int y = 0; y < _rajzlapMagassag; y++)
        {
            for (int x = 0; x < _rajzlapSzelesseg; x++)
            {
                Console.SetCursorPosition(RajzlapKezdoOszlop + x, y + 1);

                // A kurzor helyét fordított színekkel mutatjuk.
                if (x == _x && y == _y)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    char kurzorKarakter = _karakterek[y, x] == ' '
                        ? ' '
                        : _karakterek[y, x];
                    Console.Write(kurzorKarakter);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = _szinek[y, x];
                    Console.Write(_karakterek[y, x]);
                    Console.ResetColor();
                }
            }
        }

        // Alsó információs sorok.
        int alsoSor = Console.WindowHeight - 3;
        Console.SetCursorPosition(0, alsoSor);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('─', Math.Max(1, Console.WindowWidth - 1)));
        Console.ResetColor();

        Console.SetCursorPosition(0, alsoSor + 1);
        string statusz = _hibaUzenet.Length > 0
            ? "Hiba: " + _hibaUzenet
            : _uzenet;
        Console.ForegroundColor = _hibaUzenet.Length > 0
            ? ConsoleColor.Red
            : ConsoleColor.Gray;
        Console.Write(statusz.PadRight(Math.Max(1, Console.WindowWidth - 1))
            .Substring(0, Math.Max(1, Console.WindowWidth - 1)));
        Console.ResetColor();

        Console.SetCursorPosition(0, alsoSor + 2);
        Console.Write("Kilépés: M, majd a Kilépés menüpont. Bal felső cella: (0, 0).");

        // A konzol kurzorát a kiemelt rajzlapi cellára állítjuk.
        Console.SetCursorPosition(
            Math.Min(Console.WindowWidth - 1, RajzlapKezdoOszlop + _x),
            Math.Min(Console.WindowHeight - 1, _y + 1));
    }

    private void AllapotPanelRajzolasa()
    {
        int sor = _menuNyitva ? 9 : 4;
        int sorkoz = _menuNyitva ? 1 : 2;
        int szinIndex = Array.IndexOf(_valaszthatoSzinek, _aktualisSzin);
        KiirPanelSort(sor, $"Pozíció: ({_x}, {_y})");
        KiirPanelSort(sor + sorkoz, $"Karakter: [{LathatoKarakter(_aktualisKarakter)}]");
        KiirPanelSort(sor + 2 * sorkoz, $"Szín: {_szinNevek[szinIndex]}");
        KiirPanelSort(sor + 3 * sorkoz, _eszkozLeteve ? "Eszköz: Letéve" : "Eszköz: Felemelve");

        if (_menuNyitva)
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
        for (int i = 0; i < _menuPontok.Length; i++)
        {
            Console.SetCursorPosition(0, 4 + i);
            string jel = i == _menuValasztas ? "> " : "  ";
            Console.ForegroundColor = i == _menuValasztas
                ? ConsoleColor.Yellow
                : ConsoleColor.Gray;
            Console.Write((jel + _menuPontok[i]).PadRight(PanelSzelesseg - 1));
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

    // 2-es tanuló – Menükezelés és választás.
    private bool MenuBillentyuKezelese(ConsoleKeyInfo billentyu)
    {
        ConsoleKey gomb = billentyu.Key;
        _hibaUzenet = "";

        if (gomb == ConsoleKey.UpArrow || gomb == ConsoleKey.DownArrow)
        {
            int irany = gomb == ConsoleKey.UpArrow ? -1 : 1;
            _menuValasztas = (_menuValasztas + irany + _menuPontok.Length) % _menuPontok.Length;
            _uzenet = $"Menüpont: {_menuPontok[_menuValasztas]}.";
        }
        else if (gomb == ConsoleKey.M || gomb == ConsoleKey.Escape)
        {
            _menuNyitva = false;
            _uzenet = "Menü bezárva. A rajzolás folytatható.";
        }
        else if (gomb != ConsoleKey.Enter)
        {
            Hiba("A menüben a Fel/Le, az Enter, az M és az Esc használható.");
        }
        else if (_menuValasztas == 3)
        {
            return false;
        }
        else if (_menuValasztas == 2)
        {
            AktualisCellaTorlese();
        }
        else
        {
            bool karaktertValaszt = _menuValasztas == 0;
            int index = karaktertValaszt
                ? Array.IndexOf(_valaszthatoKarakterek, _aktualisKarakter)
                : Array.IndexOf(_valaszthatoSzinek, _aktualisSzin);
            int darab = karaktertValaszt ? _valaszthatoKarakterek.Length : _valaszthatoSzinek.Length;

            while (true)
            {
                _uzenet = "Válassz a nyilakkal, majd nyomj Entert.";
                Megjelenites();
                KiirPanelSort(14, "Fel/Le: váltás");
                KiirPanelSort(15, "Enter: mentés");
                KiirPanelSort(16, "M vagy Esc: mégse");

                if (karaktertValaszt)
                {
                    KiirPanelSort(18, $"Jelölt: [{LathatoKarakter(_valaszthatoKarakterek[index])}]");
                }
                else if (Console.WindowHeight > 18)
                {
                    KiirPanelSort(18, "Jelölt:");
                    Console.SetCursorPosition(8, 18);
                    Console.ForegroundColor = _valaszthatoSzinek[index];
                    Console.Write(_szinNevek[index]);
                    Console.ResetColor();
                }

                gomb = Console.ReadKey(true).Key;
                _hibaUzenet = "";
                if (gomb == ConsoleKey.M || gomb == ConsoleKey.Escape)
                {
                    _uzenet = "A választást megszakítottad.";
                    break;
                }
                if (gomb == ConsoleKey.Enter)
                {
                    if (karaktertValaszt)
                    {
                        _aktualisKarakter = _valaszthatoKarakterek[index];
                        _uzenet = $"A rajzkarakter: [{LathatoKarakter(_aktualisKarakter)}].";
                    }
                    else
                    {
                        _aktualisSzin = _valaszthatoSzinek[index];
                        _uzenet = $"A rajzszín: {_szinNevek[index]}.";
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

            if (_menuNyitva)
            {
                fut = MenuBillentyuKezelese(billentyu);
            }
            else if (gomb == ConsoleKey.M || gomb == ConsoleKey.Escape)
            {
                _menuNyitva = true;
                _menuValasztas = 0;
                _hibaUzenet = "";
                _uzenet = "Menü megnyitva.";
            }
            else
            {
                // ----------------------------------------------------------------
                // 2-es tanuló:
                // ----------------------------------------------------------------

                int ujX = _x;
                int ujY = _y;
                if (gomb == ConsoleKey.UpArrow)
                    ujY--;
                else if (gomb == ConsoleKey.DownArrow)
                    ujY++;
                else if (gomb == ConsoleKey.LeftArrow)
                    ujX--;
                else if (gomb == ConsoleKey.RightArrow)
                    ujX++;

                if (ujX != _x || ujY != _y || gomb == ConsoleKey.Spacebar)
                {
                    if (ujX < 0 || ujX >= _rajzlapSzelesseg || ujY < 0 || ujY >= _rajzlapMagassag)
                    {
                        Hiba("A rajzlap szélén túl nem lehet lépni.");
                    }
                    else
                    {
                        _x = ujX;
                        _y = ujY;
                        _hibaUzenet = "";
                        _uzenet = "Kurzor mozgatva.";

                        if (gomb == ConsoleKey.Spacebar)
                        {
                            _eszkozLeteve = !_eszkozLeteve;
                            _uzenet = _eszkozLeteve ? "Az eszköz letéve." : "Az eszköz felemelve.";
                        }

                        if (_eszkozLeteve)
                        {
                            _karakterek[_y, _x] = _aktualisKarakter;
                            _szinek[_y, _x] = _aktualisSzin;
                        }
                    }
                }
            }

            if (!fut)
                break;

            Megjelenites();


            if (_menuNyitva)
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
            if (_x - sugar < 0 || _x + sugar >= _rajzlapSzelesseg ||
                _y - sugar < 0 || _y + sugar >= _rajzlapMagassag)
            {
                Hiba($"A {meret}×{meret} méretű nyomda nem fér el ezen a helyen.");
                Megjelenites();
                continue;
            }

            for (int dy = -sugar; dy <= sugar; dy++)
            {
                for (int dx = -sugar; dx <= sugar; dx++)
                {
                    int tavX = Math.Abs(dx);
                    int tavY = Math.Abs(dy);
                    bool jelolt = false;

                    if (alakzat == "X")
                        jelolt = tavX == tavY;
                    else if (alakzat == "kereszt")
                        jelolt = dx == 0 || dy == 0;
                    else if (alakzat == "négyzetkeret")
                        jelolt = tavX == sugar || tavY == sugar;
                    else if (alakzat == "rombuszkeret")
                        jelolt = tavX + tavY == sugar;

                    if (!jelolt)
                        continue;

                    int celX = _x + dx;
                    int celY = _y + dy;
                    _karakterek[celY, celX] = _aktualisKarakter;
                    _szinek[celY, celX] = _aktualisSzin;

                    Console.ForegroundColor = _aktualisSzin;
                    if (dx == 0 && dy == 0)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.SetCursorPosition(RajzlapKezdoOszlop + celX, celY + 1);
                    Console.Write(_aktualisKarakter);
                    Console.ResetColor();
                }
            }

            _hibaUzenet = "";
            _uzenet = $"Elhelyezve: {meret}×{meret} méretű {alakzat}.";
            Console.SetCursorPosition(0, Console.WindowHeight - AlsoSegedSorok + 1);
            Console.ForegroundColor = ConsoleColor.Gray;
            int statuszSzelesseg = Console.WindowWidth - 1;
            Console.Write(_uzenet.PadRight(statuszSzelesseg).Substring(0, statuszSzelesseg));
            Console.ResetColor();
            Console.SetCursorPosition(RajzlapKezdoOszlop + _x, _y + 1);
        }
    }
}

