// ============================================================================
// KONZOLOS RAJZOLO
// 1-es tanuló – Felület, kurzor, menü, szabadkézi rajzolás és cellatörlés.
//
// Fontos: a Console.SetCursorPosition parameterei (oszlop, sor) sorrendben
// ertendoek. A sajat rajzlapon beluli koordinatak viszont (x, y), ahol
// a bal felso cella (0, 0).
// ============================================================================

using System;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// A programnak valodi, interaktiv konzolra van szuksege.
if (Console.IsInputRedirected || Console.IsOutputRedirected)
{
    Console.WriteLine("A program interaktiv konzolablakban futtathato.");
    return;
}

// A rajzlap es a menu elrendezese miatt minimalis ablakmeretet kerunk.
const int minimalisSzelesseg = 76;
const int minimalisMagassag = 23;

if (Console.WindowWidth < minimalisSzelesseg ||
    Console.WindowHeight < minimalisMagassag)
{
    Console.WriteLine(
        $"Nagyitsd a konzolablakot legalabb {minimalisSzelesseg} oszloposra " +
        $"es {minimalisMagassag} sorosra, majd inditsd ujra a programot.");
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
    // Akkor is visszaallitjuk a konzol allapotat, ha futas kozben hiba tortenik.
    Console.ResetColor();
    Console.CursorVisible = true;
    Console.Clear();
    Console.WriteLine("A konzolos rajzolo bezarult.");
}

// ============================================================================
// A Rajzolo osztaly tartalmazza a program teljes mukodeset.
// ============================================================================
sealed class Rajzolo
{
    // A bal oldali panel szelessege. A rajzlap a konzol fennmarado reszet kapja.
    private const int PanelSzelesseg = 24;
    private const int RajzlapKezdoOszlop = PanelSzelesseg;
    private const int AlsoSegedSorok = 3;

    // A rajzlap minden cellajahoz eltároljuk a karaktert es a szinet.
    private char[,] _karakterek;
    private ConsoleColor[,] _szinek;

    private int _rajzlapSzelesseg;
    private int _rajzlapMagassag;

    // A kurzor koordinataja a rajzlapon belul, nullatol indexelve.
    private int _x;
    private int _y;

    private char _aktualisKarakter = '#';
    private ConsoleColor _aktualisSzin = ConsoleColor.Cyan;

    // true = az eszkoz le van teve, mozgas kozben rajzol.
    // A feladat szerint indulaskor felemelt allapotban van.
    private bool _eszkozLeteve = false;

    private bool _menuNyitva = false;
    private int _menuValasztas = 0;
    private string _hibaUzenet = "";
    private string _uzenet = "M: menu | Nyilak: mozgas | Szokoz: eszkoz | F1-F12: nyomdak";

    private readonly string[] _menuPontok =
    {
        "Karakter valasztasa",
        "Szin valasztasa",
        "Aktualis cella torlese",
        "Kilepes"
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

    public Rajzolo()
    {
        // A rajzlap legalabb a konzol ket harmadat foglalja el:
        // 76 oszlopnal 52 oszlopos rajzlap keszul.
        // A pontos szelesseg a konzol aktualis meretetol fugg.
        MeretekFrissitese();

        _karakterek = new char[_rajzlapMagassag, _rajzlapSzelesseg];
        _szinek = new ConsoleColor[_rajzlapMagassag, _rajzlapSzelesseg];

        RajzTorlese();
        _x = _rajzlapSzelesseg / 2;
        _y = _rajzlapMagassag / 2;
    }

    // A rajzlap a teljes konzolmagassagbol a fejlecnek es az also
    // billentyusavnak fenntartott sorok utan megmarado teruletet hasznalja.
    private void MeretekFrissitese()
    {
        _rajzlapSzelesseg = Console.WindowWidth - RajzlapKezdoOszlop - 1;
        _rajzlapMagassag = Console.WindowHeight - 5;

        if (_rajzlapSzelesseg < 1 || _rajzlapMagassag < 1)
            throw new InvalidOperationException("Tul kicsi a konzolablak.");
    }

    // ------------------------------------------------------------------------
    // Rajzolas kozbeni billentyuk kezelese
    // ------------------------------------------------------------------------
    private bool RajzBillentyuKezelese(ConsoleKeyInfo billentyu)
    {
        switch (billentyu.Key)
        {
            case ConsoleKey.UpArrow:
                Mozgas(0, -1);
                break;

            case ConsoleKey.DownArrow:
                Mozgas(0, 1);
                break;

            case ConsoleKey.LeftArrow:
                Mozgas(-1, 0);
                break;

            case ConsoleKey.RightArrow:
                Mozgas(1, 0);
                break;

            case ConsoleKey.Spacebar:
                // Az eszkoz letetelekor az aktualis cellaba is rajzolunk.
                _eszkozLeteve = !_eszkozLeteve;

                if (_eszkozLeteve)
                    CellabaIr(_x, _y, _aktualisKarakter, _aktualisSzin);

                _uzenet = _eszkozLeteve
                    ? "Az eszkoz LETEVE: a mozgas rajzol."
                    : "Az eszkoz FELEMELVE: a mozgas nem rajzol.";
                break;

            case ConsoleKey.M:
            case ConsoleKey.Escape:
                _menuNyitva = true;
                _menuValasztas = 0;
                _uzenet = "Fel/Le: valasztas | Enter: megnyitas | Esc: vissza";
                break;
        }

        return true;
    }

    // Egyetlen helyen kezeljuk a kurzormozgatast, igy a negy irany
    // hatarellenorzese es a szabadkezes rajzolas is azonosan mukodik.
    private void Mozgas(int dx, int dy)
    {
        int ujX = _x + dx;
        int ujY = _y + dy;

        // A celcellat meg a kurzor vagy a rajz megvaltoztatasa elott
        // ellenorizzuk. Hataron kivul semmi nem valtozhat.
        if (!ErvenyesKoordinata(ujX, ujY))
        {
            Hiba("A rajzlap szelen tul nem lehet lepni.");
            return;
        }

        _x = ujX;
        _y = ujY;

        // Letett eszkoznel minden ervenyes lepes a celcellaba rajzol.
        if (_eszkozLeteve)
            CellabaIr(_x, _y, _aktualisKarakter, _aktualisSzin);

        _uzenet = "Kurzor mozgatva.";
        _hibaUzenet = "";
    }

    // ------------------------------------------------------------------------
    // Menu
    // ------------------------------------------------------------------------
    private bool MenuBillentyuKezelese(ConsoleKeyInfo billentyu)
    {
        switch (billentyu.Key)
        {
            case ConsoleKey.UpArrow:
                _menuValasztas =
                    (_menuValasztas - 1 + _menuPontok.Length) % _menuPontok.Length;
                break;

            case ConsoleKey.DownArrow:
                _menuValasztas =
                    (_menuValasztas + 1) % _menuPontok.Length;
                break;

            case ConsoleKey.Escape:
                _menuNyitva = false;
                _hibaUzenet = "";
                _uzenet = "Rajzolas: nyilak | Szokoz: eszkoz | F1-F12: nyomdak";
                break;

            case ConsoleKey.Enter:
                return MenuPontVegrehajtasa();

            default:
                // Hibás menübillentyűre is hangjelzést adunk.
                Hiba("A menuben a Fel/Le nyilak, Enter es Esc hasznalhato.");
                break;
        }

        return true;
    }

    private bool MenuPontVegrehajtasa()
    {
        switch (_menuValasztas)
        {
            case 0:
                KarakterValasztasa();
                break;

            case 1:
                SzinValasztasa();
                break;

            case 2:
                AktualisCellaTorlese();
                break;

            case 3:
                return false;
        }

        return true;
    }

    // A karaktervalasztas kulon kis billentyuzetkepernyo.
    // Fel/Le billentyuvel lehet valtani, Enterrel elfogadni, Esc-pel kilepni.
    private void KarakterValasztasa()
    {
        int index = Array.IndexOf(_valaszthatoKarakterek, _aktualisKarakter);
        if (index < 0) index = 0;

        while (true)
        {
            Megjelenites();
            KiirPanelSort(10, "KARAKTER VALASZTASA");
            KiirPanelSort(11, "Fel/Le: valt | Enter: OK");
            KiirPanelSort(12, "Esc: vissza");
            KiirPanelSort(14, $"Aktualis: [{LathatoKarakter(_valaszthatoKarakterek[index])}]");

            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape)
                break;
            if (key == ConsoleKey.Enter)
            {
                _aktualisKarakter = _valaszthatoKarakterek[index];
                _uzenet = $"A rajzkarakter: [{LathatoKarakter(_aktualisKarakter)}].";
                break;
            }
            if (key == ConsoleKey.UpArrow)
                index = (index - 1 + _valaszthatoKarakterek.Length) % _valaszthatoKarakterek.Length;
            else if (key == ConsoleKey.DownArrow)
                index = (index + 1) % _valaszthatoKarakterek.Length;
            else
                Hiba("Itt a Fel/Le, Enter vagy Esc billentyu hasznalhato.");
        }
    }

    private void SzinValasztasa()
    {
        int index = Array.IndexOf(_valaszthatoSzinek, _aktualisSzin);
        if (index < 0) index = 0;

        while (true)
        {
            Megjelenites();
            KiirPanelSort(10, "SZIN VALASZTASA");
            KiirPanelSort(11, "Fel/Le: valt | Enter: OK");
            KiirPanelSort(12, "Esc: vissza");
            KiirPanelSort(14, "Aktualis szin:");
            Console.SetCursorPosition(2, 15);
            Console.ForegroundColor = _valaszthatoSzinek[index];
            Console.Write(_valaszthatoSzinek[index].ToString().PadRight(20));
            Console.ResetColor();

            ConsoleKey key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape)
                break;
            if (key == ConsoleKey.Enter)
            {
                _aktualisSzin = _valaszthatoSzinek[index];
                _uzenet = $"A rajzszin: {_aktualisSzin}.";
                break;
            }
            if (key == ConsoleKey.UpArrow)
                index = (index - 1 + _valaszthatoSzinek.Length) % _valaszthatoSzinek.Length;
            else if (key == ConsoleKey.DownArrow)
                index = (index + 1) % _valaszthatoSzinek.Length;
            else
                Hiba("Itt a Fel/Le, Enter vagy Esc billentyu hasznalhato.");
        }
    }

    // ------------------------------------------------------------------------
    // Aktualis cella torlese
    // ------------------------------------------------------------------------
    private void AktualisCellaTorlese()
    {
        // Csak a kurzor alatti cellat toroljuk, a kurzor es az eszkoz allapota
        // valtozatlan marad.
        CellabaIr(_x, _y, ' ', ConsoleColor.Gray);
        _uzenet = "Az aktualis cella torolve.";
        _hibaUzenet = "";
    }

    // ------------------------------------------------------------------------
    // Rajzlap es cellak kezelese
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

        // Windows alatt valodi hangjelzes, mas rendszeren konzolcsengo.
        try
        {
            if (OperatingSystem.IsWindows())
                Console.Beep(850, 140);
            else
                Console.Write('\a');
        }
        catch
        {
            // Ha a terminal nem tamogatja a hangjelzest, a szoveges hibauzenet
            // akkor is megjelenik.
        }
    }

    // ------------------------------------------------------------------------
    // Kepernyo kirajzolasa
    // ------------------------------------------------------------------------
    private void Megjelenites()
    {
        // A konzolablak futas kozbeni atmeretezeset is kezeljuk.
        int ujSzelesseg = Console.WindowWidth - RajzlapKezdoOszlop - 1;
        int ujMagassag = Console.WindowHeight - 5;

        if (ujSzelesseg != _rajzlapSzelesseg ||
            ujMagassag != _rajzlapMagassag)
        {
            // Atmeretezesnel a korabbi tartalom kozos reszet megorizzuk.
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

        // Fejlec.
        Console.SetCursorPosition(0, 0);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("KONZOLOS RAJZOLO".PadRight(PanelSzelesseg));
        Console.ResetColor();
        Console.WriteLine("RAJZLAP");

        // Bal oldali panel.
        if (_menuNyitva)
            MenuPanelRajzolasa();
        else
            AllapotPanelRajzolasa();

        // Jobb oldali rajzlap: minden cellat a tarolt karakterrel es szinnel rajzolunk ki.
        for (int y = 0; y < _rajzlapMagassag; y++)
        {
            for (int x = 0; x < _rajzlapSzelesseg; x++)
            {
                Console.SetCursorPosition(RajzlapKezdoOszlop + x, y + 1);

                // A kurzor helyet forditott szinekkel mutatjuk.
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

        // Also informacios sorok.
        int alsoSor = Console.WindowHeight - 3;
        Console.SetCursorPosition(0, alsoSor);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('─', Math.Max(1, Console.WindowWidth - 1)));
        Console.ResetColor();

        Console.SetCursorPosition(0, alsoSor + 1);
        string statusz = _hibaUzenet.Length > 0
            ? "HIBA: " + _hibaUzenet
            : _uzenet;
        Console.ForegroundColor = _hibaUzenet.Length > 0
            ? ConsoleColor.Red
            : ConsoleColor.Gray;
        Console.Write(statusz.PadRight(Math.Max(1, Console.WindowWidth - 1))
            .Substring(0, Math.Max(1, Console.WindowWidth - 1)));
        Console.ResetColor();

        Console.SetCursorPosition(0, alsoSor + 2);
        Console.Write("A kilepeshez: M, majd Kilepes.  A rajzlap bal felso cellaja: (0, 0).");

        // A Console kurzorpoziciojat a rajzlap kurzorcellajara tesszuk.
        // A tenylegesen lathato kurzor a forditott hatteru cella.
        Console.SetCursorPosition(
            Math.Min(Console.WindowWidth - 1, RajzlapKezdoOszlop + _x),
            Math.Min(Console.WindowHeight - 1, _y + 1));
    }

    private void AllapotPanelRajzolasa()
    {
        KiirPanelSort(2, "M  Menu megnyitasa");
        KiirPanelSort(4, $"Pozicio: ({_x}, {_y})");
        KiirPanelSort(6, $"Karakter: [{LathatoKarakter(_aktualisKarakter)}]");
        KiirPanelSort(8, $"Szin: {_aktualisSzin}");
        KiirPanelSort(10, _eszkozLeteve ? "Eszkoz: LETEVE" : "Eszkoz: FELEMELVE");
        KiirPanelSort(13, "Nyilak: mozgas");
        KiirPanelSort(14, "Szokoz: eszkoz");
        KiirPanelSort(15, "F1-F3: X");
        KiirPanelSort(16, "F4-F6: Kereszt");
        KiirPanelSort(17, "F7-F9: Negyzetkeret");
        KiirPanelSort(18, "F10-F12: Rombuszkeret");
        KiirPanelSort(20, "M: menu");
    }

    private void MenuPanelRajzolasa()
    {
        KiirPanelSort(2, "MENU");
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

        KiirPanelSort(12, "Fel/Le: valasztas");
        KiirPanelSort(13, "Enter: megnyitas");
        KiirPanelSort(14, "Esc: vissza");
        KiirPanelSort(17, $"Poz.: ({_x}, {_y})");
        KiirPanelSort(18, _eszkozLeteve ? "Eszkoz: LETEVE" : "Eszkoz: FELEMELVE");
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
        return karakter == ' ' ? "szokoz" : karakter.ToString();
    }

    public void Futtatas()
    {
        Megjelenites();

        bool fut = true;
        while (fut)
        {
            ConsoleKeyInfo billentyu = Console.ReadKey(true);

            if (_menuNyitva)
            {
                fut = MenuBillentyuKezelese(billentyu);
            }
            else
            {
                fut = RajzBillentyuKezelese(billentyu);
            }

            if (!fut)
                break;

            Megjelenites();

            // ----------------------------------------------------------------
            // 2-es tanuló:
            // ----------------------------------------------------------------
            if (_menuNyitva)
                continue;

            ConsoleKey gomb = billentyu.Key;
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
                alakzat = "negyzetkeret";
            else if (gomb == ConsoleKey.F10 || gomb == ConsoleKey.F11 || gomb == ConsoleKey.F12)
                alakzat = "rombuszkeret";

            int sugar = meret / 2;
            if (_x - sugar < 0 || _x + sugar >= _rajzlapSzelesseg ||
                _y - sugar < 0 || _y + sugar >= _rajzlapMagassag)
            {
                Hiba($"A {meret}x{meret}-es nyomda nem fer el ezen a helyen.");
                Megjelenites();
                continue;
            }

            for (int dy = -sugar; dy <= sugar; dy++)
            {
                for (int dx = -sugar; dx <= sugar; dx++)
                {
                    // Hány cellára van a pont a kurzortól vízszintesen és függőlegesen?
                    int tavX = Math.Abs(dx);
                    int tavY = Math.Abs(dy);
                    bool jelolt = false;

                    if (alakzat == "X")
                        jelolt = tavX == tavY;
                    else if (alakzat == "kereszt")
                        jelolt = dx == 0 || dy == 0;
                    else if (alakzat == "negyzetkeret")
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
            _uzenet = $"{meret}x{meret}-es {alakzat} nyomda elhelyezve.";
            Console.SetCursorPosition(0, Console.WindowHeight - AlsoSegedSorok + 1);
            Console.ForegroundColor = ConsoleColor.Gray;
            int statuszSzelesseg = Console.WindowWidth - 1;
            Console.Write(_uzenet.PadRight(statuszSzelesseg).Substring(0, statuszSzelesseg));
            Console.ResetColor();
            Console.SetCursorPosition(RajzlapKezdoOszlop + _x, _y + 1);
        }
    }
}

