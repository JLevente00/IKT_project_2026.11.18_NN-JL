# Konzolos rajzoló

A C# programban szabadon rajzolhatsz, alakzatokat helyezhetsz el, és szövegfájlból tölthetsz be mintát. A bal oldali panel a kurzor helyét, a karaktert, a színt és az eszköz állapotát mutatja.

## Futtatás

A programhoz .NET 10 SDK és legalább 76 oszlop széles, 23 sor magas interaktív konzol kell.

Visual Studióban nyisd meg az `IKT_Project_NN&JL.slnx` fájlt, válaszd ki az `IKT_Project_NN&JL` projektet, majd indítsd el a **Ctrl+F5** billentyűkkel.

PowerShellben a repó gyökérmappájából futtasd ezt.

```powershell
dotnet run --project '.\IKT_Project_NN&JL\IKT_Project_NN&JL.csproj'
```

## Vezérlés

| Billentyű | Művelet |
| --- | --- |
| Nyilak | A kurzor mozgatása. Letett eszközzel a célcellába kerül a kiválasztott jel. |
| Szóköz | Az eszköz letétele vagy felemelése. Letételkor az aktuális cellába is rajzol. |
| M vagy Esc | A menü megnyitása vagy bezárása. |
| Fel, Le és Enter a menüben | Menüpont vagy rajzbeállítás kiválasztása és jóváhagyása. |

| Alakzat | 3×3 | 5×5 | 7×7 |
| --- | --- | --- | --- |
| X | F1 | F2 | F3 |
| Kereszt | F4 | F5 | F6 |
| Négyzetkeret | F7 | F8 | F9 |
| Rombuszkeret | F10 | F11 | F12 |

A nyomda középpontja a kurzor helye. Ha az alakzat kilógna a rajzlapról, a program hibát jelez és megőrzi a korábbi rajzot. A **Cella törlése** menüpont a kurzor alatti jelet törli.

A **Minta betöltése** menüpontban add meg egy `.txt` fájl teljes útvonalát. A [mellékelt minta](Kezdoprojekt/minta.txt) a bal felső cellától jelenik meg a kiválasztott színnel. Sikeres betöltés után folytathatod a rajzolást. Hibás útvonal vagy túl nagy minta esetén megmarad az előző rajz. A programot az **M → Kilépés → Enter** sorrenddel zárhatod be.

## Commitok

| Commit | Változás |
| --- | --- |
| `4373069` | Létrejött a Git alapbeállítása. A `.gitattributes` és a `.gitignore` került a repóba. |
| `38adaa7` | Bekerült a Visual Studio-megoldás, a C# projektfájl és a konzolos rajzoló forráskódja. |
| `109bfff` | A feladatleírás, a kezdőprojekt, a szöveges minta, a projektnapló és a tesztjegyzőkönyv sablonja került a repóba. |
| `4b0f541` | A programrészek tanulónkénti jelölést kaptak. Az F1–F3 három méretű X-et rajzolt, és elkészült a projektnapló első kitöltése. A korábbi fájlbetöltő ekkor került ki. |
| `5294f0b` | A kereszt, a négyzetkeret és a rombuszkeret három méretben bekerült. Az F1–F12 kiosztás és a nyomdák helyének ellenőrzése egységes lett. |
| `de79c22` | A projektnapló munkamegosztási táblázatának fejléce rendezve lett. |
| `d992ad1` | A munkamegosztási táblázat a másik tanuló sorával egészült ki. |
| `993d409` | A nyilak és a szóköz kezelése egyszerű feltételekbe került. A program a rajz módosítása előtt ellenőrzi az új kurzorhelyet. |
| `a8a711e` | Egyesült a mozgáskezelés módosítása és a projektnapló másik ágon készült változata. |
| `4e95cfd` | A karakter és a szín közös menüvezérlést kapott. Bekerültek a magyar színnevek, az ékezetes feliratok és a nyitott menüben is látható állapotadatok. |
| `491d109` | Vonalas keretet és egyértelműbb változóneveket kapott a rajzlap. Visszakerült az ellenőrzött `.txt`-betöltés, és frissült a projektnapló. |

A tesztjegyzőkönyv a `491d109` commitot ellenőrzi. A teszten a cellatörlés elérhető volt, a második saját menüpont hiányzott.
