-- =====================================================================
--  Agerbæk Reklamelyd - Funktion 1: Administration og kategorisering af lydfiler
--
--  Sådan bruger I scriptet:
--   1. Forbind til (localdb)\MSSQLLocalDB
--      (i Visual Studio: View -> SQL Server Object Explorer)
--   2. Kør hele scriptet. Det opretter databasen ReklamelydDB,
--      tabellerne og noget testdata.
--
--  OBS: Scriptet sletter tabellerne og opretter dem igen,
--       så alle data starter forfra, hver gang scriptet køres.
-- =====================================================================

-- Opret databasen, hvis den ikke findes i forvejen.
-- (Bruger I en anden database, fx på skolens server, så slet de næste
--  fire linjer og vælg jeres egen database, før I kører scriptet.)
IF DB_ID('ReklamelydDB') IS NULL
    CREATE DATABASE ReklamelydDB;
GO
USE ReklamelydDB;
GO

-- Slet tabellerne, hvis de findes.
-- Rækkefølgen er vigtig på grund af fremmednøglerne.
IF OBJECT_ID('dbo.LydfilEmne', 'U') IS NOT NULL DROP TABLE dbo.LydfilEmne;
IF OBJECT_ID('dbo.Lydfil', 'U') IS NOT NULL DROP TABLE dbo.Lydfil;
IF OBJECT_ID('dbo.Emne', 'U') IS NOT NULL DROP TABLE dbo.Emne;
IF OBJECT_ID('dbo.Musikgenre', 'U') IS NOT NULL DROP TABLE dbo.Musikgenre;
GO

-- Musikgenre: fx Pop, Rock og Jazz
CREATE TABLE dbo.Musikgenre
(
    MusikgenreId INT IDENTITY(1,1) NOT NULL,
    Navn NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_Musikgenre PRIMARY KEY (MusikgenreId),
    CONSTRAINT UQ_Musikgenre_Navn UNIQUE (Navn)
);

-- Emne: fx Shopping, Sport og Nyheder
CREATE TABLE dbo.Emne
(
    EmneId INT IDENTITY(1,1) NOT NULL,
    Navn NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_Emne PRIMARY KEY (EmneId),
    CONSTRAINT UQ_Emne_Navn UNIQUE (Navn)
);

-- Lydfil: én tabel til alle fire typer (Musik, Reklame, Taleindslag og Jingle).
-- Kolonnen LydfilType fortæller, hvilken klasse rækken skal blive til i C#.
CREATE TABLE dbo.Lydfil
(
    LydfilId INT IDENTITY(1,1) NOT NULL,
    Titel NVARCHAR(100) NOT NULL,
    Filsti NVARCHAR(400) NOT NULL,
    Varighed INT NOT NULL,              -- i sekunder
    Format NVARCHAR(10) NOT NULL,       -- fx mp3 eller wav
    LydfilType NVARCHAR(20) NOT NULL,   -- Musik, Reklame, Taleindslag eller Jingle
    MusikgenreId INT NULL,              -- kun Musik har en genre (ellers NULL)

    CONSTRAINT PK_Lydfil PRIMARY KEY (LydfilId),
    CONSTRAINT FK_Lydfil_Musikgenre FOREIGN KEY (MusikgenreId) REFERENCES dbo.Musikgenre (MusikgenreId),
    CONSTRAINT CK_Lydfil_LydfilType CHECK (LydfilType IN ('Musik', 'Reklame', 'Taleindslag', 'Jingle'))
);

-- LydfilEmne: hvilke emner en reklame eller et taleindslag handler om.
-- Det er en mange-til-mange-tabel: en lydfil kan have flere emner,
-- og et emne kan bruges af flere lydfiler.
CREATE TABLE dbo.LydfilEmne
(
    LydfilId INT NOT NULL,
    EmneId INT NOT NULL,

    CONSTRAINT PK_LydfilEmne PRIMARY KEY (LydfilId, EmneId),
    CONSTRAINT FK_LydfilEmne_Lydfil FOREIGN KEY (LydfilId) REFERENCES dbo.Lydfil (LydfilId),
    CONSTRAINT FK_LydfilEmne_Emne FOREIGN KEY (EmneId) REFERENCES dbo.Emne (EmneId)
);
GO

-- =====================================================================
--  Testdata
--  Lydfilerne er Windows' egne lyde fra C:\Windows\Media,
--  så de kan afspilles på alle Windows-pc'er.
--  Id'erne bliver 1, 2, 3 ... i den rækkefølge, rækkerne indsættes.
-- =====================================================================

INSERT INTO dbo.Musikgenre (Navn) VALUES
(N'Pop'),          -- 1
(N'Rock'),         -- 2
(N'Jazz'),         -- 3
(N'Klassisk'),     -- 4
(N'Hiphop'),       -- 5
(N'Elektronisk'),  -- 6
(N'Country'),      -- 7
(N'Julemusik');    -- 8

INSERT INTO dbo.Emne (Navn) VALUES
(N'Shopping'),       -- 1
(N'Tilbud'),         -- 2
(N'Mad og drikke'),  -- 3
(N'Mode'),           -- 4
(N'Sport'),          -- 5
(N'Nyheder'),        -- 6
(N'Vejret'),         -- 7
(N'Sundhed'),        -- 8
(N'Jul'),            -- 9
(N'Lokalt');         -- 10

INSERT INTO dbo.Lydfil (Titel, Filsti, Varighed, Format, LydfilType, MusikgenreId) VALUES
(N'Sommerhit (testlyd)',                   N'C:\Windows\Media\Ring05.wav',        13, N'wav', N'Musik', 1),          -- 1
(N'Rolig morgenjazz (testlyd)',            N'C:\Windows\Media\Ring08.wav',        11, N'wav', N'Musik', 3),          -- 2
(N'Rockriff (testlyd)',                    N'C:\Windows\Media\Ring09.wav',        10, N'wav', N'Musik', 2),          -- 3
(N'Klaverstykke (testlyd)',                N'C:\Windows\Media\Ring03.wav',         9, N'wav', N'Musik', 4),          -- 4
(N'Julemelodi (testlyd)',                  N'C:\Windows\Media\Alarm05.wav',        8, N'wav', N'Musik', 8),          -- 5
(N'Black Friday i storcentret (testlyd)',  N'C:\Windows\Media\Ring10.wav',         8, N'wav', N'Reklame', NULL),     -- 6
(N'Den lokale bager (testlyd)',            N'C:\Windows\Media\Ring04.wav',         8, N'wav', N'Reklame', NULL),     -- 7
(N'Julegaver til hele familien (testlyd)', N'C:\Windows\Media\Alarm06.wav',        7, N'wav', N'Reklame', NULL),     -- 8
(N'Dagens vejr (testlyd)',                 N'C:\Windows\Media\Alarm07.wav',        6, N'wav', N'Taleindslag', NULL), -- 9
(N'Sportsnyt (testlyd)',                   N'C:\Windows\Media\Alarm01.wav',        6, N'wav', N'Taleindslag', NULL), -- 10
(N'Stationsjingle (testlyd)',              N'C:\Windows\Media\tada.wav',           2, N'wav', N'Jingle', NULL),      -- 11
(N'Kort jingle (testlyd)',                 N'C:\Windows\Media\chimes.wav',         1, N'wav', N'Jingle', NULL),      -- 12
(N'Velkomstjingle (testlyd)',              N'C:\Windows\Media\Windows Logon.wav',  4, N'wav', N'Jingle', NULL);      -- 13

-- Reklamer og taleindslag får deres emner: (LydfilId, EmneId)
INSERT INTO dbo.LydfilEmne (LydfilId, EmneId) VALUES
(6, 1), (6, 2),     -- Black Friday: Shopping, Tilbud
(7, 3), (7, 10),    -- Den lokale bager: Mad og drikke, Lokalt
(8, 1), (8, 9),     -- Julegaver: Shopping, Jul
(9, 7),             -- Dagens vejr: Vejret
(10, 5), (10, 6);   -- Sportsnyt: Sport, Nyheder
GO
