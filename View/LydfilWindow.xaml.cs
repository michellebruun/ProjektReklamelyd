using Microsoft.Win32;
using ProjektReklamelyd.Models;
using ProjektReklamelyd.Services;
using ProjektReklamelyd.Services.Interfaces;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjektReklamelyd.View
{
    /// <summary>
    /// Funktion 1: Administration og kategorisering af lydfiler.
    /// Her kan medarbejderen tilføje, kategorisere, redigere, slette og afspille lydfiler.
    /// </summary>
    public partial class LydfilWindow : Window
    {
        private ILydfilService _lydfilService;

        // Alle lydfiler fra databasen. Vi gemmer dem her, så vi kan søge i dem.
        private List<Lydfil> _alleLydfiler = new List<Lydfil>();

        // Id på den lydfil, der er i formularen. 0 betyder "ny lydfil".
        private int _valgtLydfilId = 0;

        // MediaPlayer kan åbne lydfiler.
        // Den ene bruger vi til at aflæse varigheden, den anden til at afspille lyden.
        private MediaPlayer _varighedsLaeser = new MediaPlayer();
        private MediaPlayer _afspiller = new MediaPlayer();

        public LydfilWindow()
        {
            InitializeComponent();
            _lydfilService = new LydfilService();

            // Metoderne der skal kaldes, når MediaPlayer er færdig med at åbne en fil (eller fejler)
            _varighedsLaeser.MediaOpened += VarighedsLaeser_MediaOpened;
            _varighedsLaeser.MediaFailed += VarighedsLaeser_MediaFailed;
            _afspiller.MediaEnded += Afspiller_MediaEnded;
            _afspiller.MediaFailed += Afspiller_MediaFailed;

            // De fire typer fra domænemodellen
            comboType.ItemsSource = new List<string> { "Musik", "Reklame", "Taleindslag", "Jingle" };

            try
            {
                VisLydfiler();
                VisMusikgenrer();
                VisEmner();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kunne ikke hente data fra databasen:\n\n" + ex.Message +
                    "\n\nHar I kørt Database/Database.sql, og er connection string i appsettings.json rigtig?",
                    "Databasefejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            NulstilFormular();
        }

        // =====================================================================
        //  Listen med lydfiler
        // =====================================================================

        // Henter alle lydfiler fra databasen og viser dem i listen
        private void VisLydfiler()
        {
            _alleLydfiler = _lydfilService.GetAllLydfiler();
            VisSoegeresultat();
        }

        // Viser de lydfiler, der passer til søgeteksten (titel, type, genre eller emner)
        private void VisSoegeresultat()
        {
            string soegetekst = txtSoeg.Text.Trim().ToLower();
            List<Lydfil> resultat = new List<Lydfil>();

            foreach (Lydfil lydfil in _alleLydfiler)
            {
                if (lydfil.Titel.ToLower().Contains(soegetekst) ||
                    lydfil.Type.ToLower().Contains(soegetekst) ||
                    lydfil.Kategori.ToLower().Contains(soegetekst))
                {
                    resultat.Add(lydfil);
                }
            }

            dataGridLydfiler.ItemsSource = resultat;
            txtAntal.Text = "Viser " + resultat.Count + " af " + _alleLydfiler.Count + " lydfiler";
        }

        private void TxtSoeg_TextChanged(object sender, TextChangedEventArgs e)
        {
            VisSoegeresultat();
        }

        // Når man klikker på en lydfil i listen, bliver den vist i formularen, så den kan redigeres
        private void DataGridLydfiler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Lydfil? lydfil = dataGridLydfiler.SelectedItem as Lydfil;
            if (lydfil == null)
            {
                return;
            }

            _afspiller.Stop();
            _valgtLydfilId = lydfil.LydfilId;
            txtOverskrift.Text = "Redigér lydfil";
            txtFilsti.Text = lydfil.Filsti;
            txtFormat.Text = lydfil.Format;
            txtVarighed.Text = lydfil.Varighed.ToString();
            txtTitel.Text = lydfil.Titel;
            comboType.SelectedItem = lydfil.Type;

            // Fjern de gamle valg, og vælg lydfilens egen genre eller emner
            comboGenre.SelectedItem = null;
            listEmner.SelectedItems.Clear();

            if (lydfil is Musik musik && musik.Genre != null)
            {
                VaelgGenre(musik.Genre.MusikgenreId);
            }
            else if (lydfil is Reklame reklame)
            {
                VaelgEmner(reklame.Emner);
            }
            else if (lydfil is Taleindslag taleindslag)
            {
                VaelgEmner(taleindslag.Emner);
            }

            knapSlet.IsEnabled = true;
            txtStatus.Text = "";
        }

        // =====================================================================
        //  Formularen
        // =====================================================================

        // Tømmer formularen, så man kan tilføje en ny lydfil
        private void NulstilFormular()
        {
            _afspiller.Stop();
            _valgtLydfilId = 0;
            dataGridLydfiler.SelectedItem = null;

            txtOverskrift.Text = "Ny lydfil";
            txtFilsti.Text = "";
            txtFormat.Text = "";
            txtVarighed.Text = "";
            txtTitel.Text = "";
            comboType.SelectedItem = null;
            comboGenre.SelectedItem = null;
            listEmner.SelectedItems.Clear();
            txtNyGenre.Text = "";
            txtNytEmne.Text = "";
            knapSlet.IsEnabled = false;
        }

        private void KnapNy_Click(object sender, RoutedEventArgs e)
        {
            NulstilFormular();
            txtStatus.Text = "";
        }

        // Viser kun de kategori-felter, der passer til den valgte type
        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? type = comboType.SelectedItem as string;

            // Først skjuler vi alle tre ...
            panelGenre.Visibility = Visibility.Collapsed;
            panelEmner.Visibility = Visibility.Collapsed;
            txtJingleInfo.Visibility = Visibility.Collapsed;

            // ... og så viser vi den, der passer til typen
            if (type == "Musik")
            {
                panelGenre.Visibility = Visibility.Visible;     // Musik har én genre
            }
            else if (type == "Reklame" || type == "Taleindslag")
            {
                panelEmner.Visibility = Visibility.Visible;     // Reklamer og taleindslag har emner
            }
            else if (type == "Jingle")
            {
                txtJingleInfo.Visibility = Visibility.Visible;  // Jingler har ingen kategori
            }
        }

        // Medarbejderen vælger en lydfil på computeren
        private void KnapVaelgFil_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Vælg en lydfil";
            dialog.Filter = "Lydfiler (mp3, wav, wma, m4a)|*.mp3;*.wav;*.wma;*.m4a";

            if (dialog.ShowDialog() == true)
            {
                string filsti = dialog.FileName;
                txtFilsti.Text = filsti;

                // Formatet er filens endelse uden punktum, fx "mp3"
                txtFormat.Text = Path.GetExtension(filsti).Replace(".", "").ToLower();

                // Er der ikke skrevet en titel, foreslår vi filnavnet
                if (txtTitel.Text.Trim() == "")
                {
                    txtTitel.Text = Path.GetFileNameWithoutExtension(filsti);
                }

                // MediaPlayer åbner filen. Når den er klar, kaldes VarighedsLaeser_MediaOpened.
                txtVarighed.Text = "";
                txtStatus.Text = "Aflæser varigheden ...";
                _varighedsLaeser.Open(new Uri(filsti));
            }
        }

        // Kaldes af MediaPlayer, når lydfilen er åbnet - nu kender vi varigheden
        private void VarighedsLaeser_MediaOpened(object? sender, EventArgs e)
        {
            if (_varighedsLaeser.NaturalDuration.HasTimeSpan)
            {
                double sekunder = _varighedsLaeser.NaturalDuration.TimeSpan.TotalSeconds;
                txtVarighed.Text = ((int)Math.Round(sekunder)).ToString();
                txtStatus.Text = "Varigheden er aflæst.";
            }
            else
            {
                txtStatus.Text = "Varigheden kunne ikke aflæses. Skriv den selv i sekunder.";
            }
            _varighedsLaeser.Close();
        }

        // Kaldes, hvis filen ikke kunne åbnes - så må medarbejderen selv skrive varigheden
        private void VarighedsLaeser_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            txtStatus.Text = "Varigheden kunne ikke aflæses. Skriv den selv i sekunder.";
            _varighedsLaeser.Close();
        }

        // Gem lydfilen (ny eller redigeret)
        private void KnapGem_Click(object sender, RoutedEventArgs e)
        {
            string? type = comboType.SelectedItem as string;
            if (type == null)
            {
                MessageBox.Show("Vælg hvilken type lydfilen er: Musik, Reklame, Taleindslag eller Jingle.",
                    "Lydfilen kan ikke gemmes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1) Lav et objekt af den rigtige klasse ud fra typen
            Lydfil lydfil;
            if (type == "Musik")
            {
                Musik musik = new Musik();
                musik.Genre = comboGenre.SelectedItem as Musikgenre;
                lydfil = musik;
            }
            else if (type == "Reklame")
            {
                Reklame reklame = new Reklame();
                reklame.Emner = HentValgteEmner();
                lydfil = reklame;
            }
            else if (type == "Taleindslag")
            {
                Taleindslag taleindslag = new Taleindslag();
                taleindslag.Emner = HentValgteEmner();
                lydfil = taleindslag;
            }
            else
            {
                lydfil = new Jingle();
            }

            // 2) Udfyld det, som alle lydfiler har (fra klassen Lydfil)
            lydfil.LydfilId = _valgtLydfilId;
            lydfil.Titel = txtTitel.Text.Trim();
            lydfil.Filsti = txtFilsti.Text;
            lydfil.Format = txtFormat.Text;

            int varighed;
            if (int.TryParse(txtVarighed.Text.Trim(), out varighed))
            {
                lydfil.Varighed = varighed;
            }

            try
            {
                // 3) Tjek reglerne (fx at musik skal have en genre)
                string fejl = _lydfilService.ValidateLydfil(lydfil);
                if (fejl != "")
                {
                    MessageBox.Show(fejl, "Lydfilen kan ikke gemmes", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 4) Gem i databasen og vis den opdaterede liste
                _lydfilService.SaveLydfil(lydfil);
                VisLydfiler();
                NulstilFormular();
                txtStatus.Text = "\"" + lydfil.Titel + "\" er gemt.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lydfilen kunne ikke gemmes:\n\n" + ex.Message, "Databasefejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Slet den valgte lydfil
        private void KnapSlet_Click(object sender, RoutedEventArgs e)
        {
            if (_valgtLydfilId == 0)
            {
                return;
            }

            string titel = txtTitel.Text;
            MessageBoxResult svar = MessageBox.Show("Vil du slette \"" + titel + "\"?\n\n" +
                "Lydfilen fjernes kun fra programmet. Selve filen på computeren bliver ikke slettet.",
                "Slet lydfil", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (svar == MessageBoxResult.Yes)
            {
                try
                {
                    _lydfilService.DeleteLydfil(_valgtLydfilId);
                    VisLydfiler();
                    NulstilFormular();
                    txtStatus.Text = "\"" + titel + "\" er slettet.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lydfilen kunne ikke slettes:\n\n" + ex.Message, "Databasefejl",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // =====================================================================
        //  Afspil (så man kan lytte til filen, før man kategoriserer den)
        // =====================================================================

        private void KnapAfspil_Click(object sender, RoutedEventArgs e)
        {
            string filsti = txtFilsti.Text;
            if (filsti == "" || !File.Exists(filsti))
            {
                MessageBox.Show("Der er ingen lydfil at afspille. Vælg en lydfil, der findes på computeren.",
                    "Afspil", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _afspiller.Open(new Uri(filsti));
            _afspiller.Play();
            txtStatus.Text = "Afspiller \"" + txtTitel.Text + "\" ...";
        }

        private void KnapStop_Click(object sender, RoutedEventArgs e)
        {
            _afspiller.Stop();
            txtStatus.Text = "";
        }

        // Kaldes, når lyden er spillet færdig
        private void Afspiller_MediaEnded(object? sender, EventArgs e)
        {
            _afspiller.Stop();
            txtStatus.Text = "";
        }

        // Kaldes, hvis lydfilen ikke kan afspilles
        private void Afspiller_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            txtStatus.Text = "";
            MessageBox.Show("Lydfilen kunne ikke afspilles:\n\n" + e.ErrorException.Message,
                "Afspil", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Stop lyden, når vinduet bliver lukket
        private void Window_Closed(object sender, EventArgs e)
        {
            _afspiller.Close();
            _varighedsLaeser.Close();
        }

        // =====================================================================
        //  Genrer og emner (kategorier)
        // =====================================================================

        private void VisMusikgenrer()
        {
            comboGenre.ItemsSource = _lydfilService.GetAllMusikgenrer();
        }

        private void VisEmner()
        {
            listEmner.ItemsSource = _lydfilService.GetAllEmner();
        }

        // Vælger genren med det givne id i genre-listen
        private void VaelgGenre(int musikgenreId)
        {
            foreach (Musikgenre genre in comboGenre.Items)
            {
                if (genre.MusikgenreId == musikgenreId)
                {
                    comboGenre.SelectedItem = genre;
                }
            }
        }

        // Sætter flueben ved de emner, der står i listen "emner"
        private void VaelgEmner(List<Emne> emner)
        {
            foreach (Emne emneIListen in listEmner.Items)
            {
                foreach (Emne emne in emner)
                {
                    if (emneIListen.EmneId == emne.EmneId && !listEmner.SelectedItems.Contains(emneIListen))
                    {
                        listEmner.SelectedItems.Add(emneIListen);
                    }
                }
            }
        }

        // Laver en liste med de emner, der har flueben
        private List<Emne> HentValgteEmner()
        {
            List<Emne> valgteEmner = new List<Emne>();
            foreach (Emne emne in listEmner.SelectedItems)
            {
                valgteEmner.Add(emne);
            }
            return valgteEmner;
        }

        // Opret en ny genre, hvis den mangler i listen
        private void KnapTilfoejGenre_Click(object sender, RoutedEventArgs e)
        {
            string navn = txtNyGenre.Text.Trim();
            if (navn == "")
            {
                MessageBox.Show("Skriv navnet på den nye genre.", "Ny genre",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Musikgenre genre = _lydfilService.AddMusikgenre(navn);
                VisMusikgenrer();
                VaelgGenre(genre.MusikgenreId);
                txtNyGenre.Text = "";
                txtStatus.Text = "Genren \"" + genre.Navn + "\" er valgt.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Genren kunne ikke gemmes:\n\n" + ex.Message, "Databasefejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Opret et nyt emne, hvis det mangler i listen
        private void KnapTilfoejEmne_Click(object sender, RoutedEventArgs e)
        {
            string navn = txtNytEmne.Text.Trim();
            if (navn == "")
            {
                MessageBox.Show("Skriv navnet på det nye emne.", "Nyt emne",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Husk de emner, der allerede har flueben, før listen bliver hentet igen
                List<Emne> valgteEmner = HentValgteEmner();

                Emne emne = _lydfilService.AddEmne(navn);
                valgteEmner.Add(emne);

                VisEmner();
                VaelgEmner(valgteEmner);
                txtNytEmne.Text = "";
                txtStatus.Text = "Emnet \"" + emne.Navn + "\" er valgt.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Emnet kunne ikke gemmes:\n\n" + ex.Message, "Databasefejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
