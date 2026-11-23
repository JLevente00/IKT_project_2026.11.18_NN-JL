# Projektnapló – Konzolos rajzoló

## Csapat és munkamegosztás

| Név vagy GitHub-felhasználónév | Vállalt feladatok és szerep | Saját menüfunkció |
| --- | --- | --- |
| Jákób Levente | Alakzatok rajzolása és ellenőrzése, szövegfájl betöltése | A Dusza Árpád Országos Programozói Emlékversenyre való tekintettel a saját menüfunkcióra nem jutott időm. |
| Nagy Nándor | Kurzor és rajzolás, Konzolos felület kialakítása, Menürendszer|  |

## Közös megállapodások

- A rajzlap mérete és helye: Jobb oldalon, körülbelül az ablak 2/3-a (JL)
- A két munkarész kapcsolódási pontja: Nándor hétvégén otthon elkészítette a grafikus felület kódját, majd odaadta nekem. A közösen használt változóneveket magam alakítottam ki az AI segítségével, és ezekhez igazítva próbáltam elkészíteni a projekt rám eső részét. (JL)
- A módosítások egyeztetése: Az összes commitot én töltöttem fel, Nándor részét is. A kész kódját e-mailben küldte el. Az ellenőrzés és a változónevek áttekintése után a kapott változatot töltöttem fel. A saját kódomat közvetlenül a `master` ágra töltöttem fel, commitonként. A módosításokat a commitüzenetekben részletesen leírtam. (JL)

## Munkanapló

| Dátum | Ki | Mi készült el vagy miben állapodtunk meg? | Következő lépés vagy commit |
| --- | --- | --- | --- |
| 2026. 11. 18. | Nagy Nándor | A konzolos rajzolóprogram menüje | Levente készítse el a hozzá tartozó logikai kódot. Commit: `109bfff`. |
| 2026. 11. 22. | Jákób Levente | Az alapvető működés átgondolása és megértése. Az AI segítségével felmértem a kiinduló lépéseket és a kívánt végeredményt. | A menüfunkciók megírása és a szövegfájl betöltése. Commit nem készült. |
| 2026. 11. 23. | Jákób Levente | A menüfunkciók ellenőrzése és tesztelése, a fájlbeolvasás elkészítése és tesztelése, valamint a mozgás módosítása. Nándor adott egy kész kódot a mozgáshoz. Ezt eleinte nehezen értettem, ezért az AI segítségével tanulmányoztam, majd a saját tudásom alapján elkészítettem a megoldásomat. | Az F1–F3 billentyűk működése, a kód felosztása, a projektnapló első változata és az AI-használat leírása (`4b0f541`). Kereszt, négyzetkeret és rombusz rajzolása, valamint a projektnapló kiegészítése (`5294f0b`). |


## Másik pár tesztje

- Tesztelt commit: `109bfff` (JL)
- Visszajelzés és saját javítás: A program különösebb hiba nélkül lefut. A logikáját részben értem, a működését megfelelőnek tartom. A rajzlap szélét még jelölöm a kirajzolásnál. (JL)
- Újrateszt eredménye: A program megfelelően működik, és a menü felülete számomra átláthatóbb lett. (JL)

## Otthoni AI-használat

Aki használt AI-t, írja le a nevét, miben kért segítséget, és mit tanult meg belőle.

- Név: Jákób Levente
- Miben kértem segítséget: A másik fél munkájának ellenőrzésében és megértésében, a feladatok felosztásában, a változónevek megválasztásában és a kód olvashatóságának ellenőrzésében. Mindkét fájlban kértem segítséget a helyesírási hibák és az elgépelések javításához. A szövegek átformálása/átfogalmazása/helysiírásellenőrzés mind a projectnaplo.md-ben mint a programban, a fajbeolvasás megértéséhez szükséges tudás elsajátítása.
- Mit tanultam meg: A munkát két fő részre lehet osztani. Az egyik ember a felületért és a megjelenésért, a másik a kód logikájáért felel. Megértettem, melyik feladat melyik részhez tartozik. Az `if` szerkezetben a kapcsos zárójelek elhagyhatók, ha az adott ágban egyetlen utasítás van. A fajl beolvasás alapjai C#-ben

