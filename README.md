# ProjektReklamelyd – Agerbæk Reklamelyd

Eksamensprojekt, gruppe 8. Prototype på et program, der sammensætter og afspiller lydfiler fra en database.

## Funktion 1: Administration og kategorisering af lydfiler

Medarbejderen kan:

- se alle lydfiler i databasen og søge i dem (titel, type, genre eller emne)
- tilføje en ny lydfil: vælg filen, så bliver format og varighed udfyldt automatisk
- kategorisere lydfilen:
  - **Musik** får præcis én musikgenre
  - **Reklame** og **Taleindslag** får et eller flere emner
  - **Jingle** har ingen kategori
- redigere, slette og afspille en lydfil
- oprette nye genrer og emner

## Sådan kører I prototypen

1. Kør `Database/Database.sql` mod `(localdb)\MSSQLLocalDB`.
   I Visual Studio: *View → SQL Server Object Explorer* → højreklik på `(localdb)\MSSQLLocalDB` → *New Query* → indsæt scriptet → *Execute*.
   Scriptet opretter databasen `ReklamelydDB`, tabellerne og testdata.
2. Bruger I en anden database, så ret connection string i `appsettings.json`.
3. Start programmet (F5), og klik på **Administration og kategorisering af lydfiler**.

Testdata bruger Windows' egne lyde fra `C:\Windows\Media`, så de kan afspilles på alle Windows-pc'er.

## Opbygning

| Mappe | Indhold |
| --- | --- |
| `Models` | Klasserne fra domænemodellen: `Lydfil` (abstrakt), `Musik`, `Reklame`, `Taleindslag`, `Jingle`, `Musikgenre`, `Emne` |
| `Repositories` | SQL mod databasen med ADO.NET (`SqlConnection`, `SqlCommand`) |
| `Services` | Reglerne, fx at musik skal have en genre, og at den samme fil ikke må registreres to gange |
| `View` | Vinduet til funktion 1 (`LydfilWindow`) |
| `Database` | SQL-script med tabeller og testdata |

Alle fire typer ligger i én tabel, `Lydfil`. Kolonnen `LydfilType` fortæller, om rækken er Musik, Reklame, Taleindslag eller Jingle. Tabellen `LydfilEmne` kobler reklamer og taleindslag til deres emner.
