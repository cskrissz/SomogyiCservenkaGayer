using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions; // <<<--- ÚJ: Szükséges a Regex validációhoz ---<<<
using System.Windows.Forms;

// A projekt névtér, ahova az alkalmazás tartozik.
namespace AutoszervizAdmin
{
    // Az Idõpont osztály, ami egy-egy foglalást reprezentál.
    // Ez tartalmazza a foglaláshoz tartozó összes adatot.
    public class Idopont
    {
        public int Id { get; set; }
        public DateTime Datum { get; set; }
        public string UgyfelNeve { get; set; }
        public string Telefonszam { get; set; }
        public string Rendszam { get; set; }
        public string MunkaTipusa { get; set; }
        public string Statusz { get; set; } // Pl: "Foglalt", "Folyamatban", "Befejezett", "Lemondva"
    }

    // A fõ ablak (Form) osztálya. Itt építjük fel a felületet és kezeljük az eseményeket.
    public partial class Form1 : Form
    {
        // === VEZÉRLÕK DEFINÍCIÓJA ===
        // Itt deklaráljuk az összes vizuális elemet, amit használni fogunk.
        private DataGridView dgvIdopontok;
        private Label lblIdopontok, lblReszletek;
        private Label lblUgyfelNeve, lblTelefonszam, lblRendszam, lblDatum, lblMunka, lblStatusz;
        private TextBox txtUgyfelNeve, txtTelefonszam, txtRendszam;
        private DateTimePicker dtpDatum;
        private ComboBox cmbMunkaTipusa, cmbStatusz;
        private Button btnUjIdopont, btnMentes, btnTorles;
        private Button btnRegisztraltIdopontok, btnMunkalapok;
        private Panel pnlReszletek, pnlHeader;
        private Label lblHeaderTitle;
        private Button btnClose, btnMinimize;
        private Button btnKijelentkezes;
        private Button btnTeljesKepernyo;

        // <<<--- ÚJ VEZÉRLÕK A KERESÉSHEZ ÉS SZÛRÉSHEZ ---<<<
        private Label lblKereses, lblStatuszSzuro;
        private TextBox txtKereses;
        private ComboBox cmbStatuszSzuro;

        // Adattárolás: egy lista, ami az Idopont objektumokat tárolja.
        private List<Idopont> idopontok;

        // Egy segédváltozó az ablak mozgatásához.
        private bool isDragging = false;
        private Point dragStartPoint;

        // === KONSTRUKTOR ===
        public Form1()
        {
            // A felület elemeinek létrehozása és beállítása.
            FelületÉpítése();

            // Mintaadatok betöltése, hogy ne legyen üres az alkalmazás.
            LoadSampleData(); // <<<--- MÓDOSÍTOTT: Mintaadatok a teszteléshez ---<<<

            // Az adatok megjelenítése a táblázatban.
            BindDataToGrid();
        }

        // === FELÜLETÉPÍTÕ METÓDUS ===
        private void FelületÉpítése()
        {
            // === ALAP FORM BEÁLLÍTÁSOK ===
            this.Text = "Autószerviz Admin Felület";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(1000, 600);
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.FormClosing += Form1_FormClosing;

            // === EGYEDI FEJLÉC ===
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(63, 63, 70) };
            pnlHeader.MouseDown += Header_MouseDown;
            pnlHeader.MouseMove += Header_MouseMove;
            pnlHeader.MouseUp += Header_MouseUp;

            lblHeaderTitle = new Label { Text = "Autószerviz Idõpontkezelõ", Location = new Point(10, 8), ForeColor = Color.Gainsboro, AutoSize = true };

            btnClose = new Button { Text = "X", Dock = DockStyle.Right, Width = 45, FlatStyle = FlatStyle.Flat, BackColor = pnlHeader.BackColor, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
            btnClose.Click += (s, e) => this.Close();

            btnMinimize = new Button { Text = "—", Dock = DockStyle.Right, Width = 45, FlatStyle = FlatStyle.Flat, BackColor = pnlHeader.BackColor, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 85);
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnTeljesKepernyo = new Button { Text = "Teljes képernyõ", Dock = DockStyle.Right, Width = 110, FlatStyle = FlatStyle.Flat, BackColor = pnlHeader.BackColor, ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 9F) };
            btnTeljesKepernyo.FlatAppearance.BorderSize = 0;
            btnTeljesKepernyo.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 85);
            btnTeljesKepernyo.Click += BtnTeljesKepernyo_Click;

            btnKijelentkezes = new Button { Text = "Kijelentkezés", Dock = DockStyle.Right, Width = 110, FlatStyle = FlatStyle.Flat, BackColor = pnlHeader.BackColor, ForeColor = Color.Gainsboro, Font = new Font("Segoe UI", 9F) };
            btnKijelentkezes.FlatAppearance.BorderSize = 0;
            btnKijelentkezes.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 85);
            btnKijelentkezes.Click += BtnKijelentkezes_Click;

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(btnKijelentkezes);
            pnlHeader.Controls.Add(btnTeljesKepernyo);
            pnlHeader.Controls.Add(btnMinimize);
            pnlHeader.Controls.Add(btnClose);

            // === CÍMKÉK ===
            lblIdopontok = new Label { Text = "Regisztrált Idõpontok", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 50) };
            lblReszletek = new Label { Text = "Kiválasztott idõpont részletei", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(18, 1) };

            // <<<--- ÚJ: KERESÕ ÉS SZÛRÕ VEZÉRLÕK LÉTREHOZÁSA ---<<<
            lblKereses = new Label { Text = "Keresés:", Location = new Point(20, 85), AutoSize = true, ForeColor = Color.Gainsboro };
            txtKereses = new TextBox { Location = new Point(80, 82), Width = 200, BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            txtKereses.TextChanged += SzuresAlkalmazasa;

            lblStatuszSzuro = new Label { Text = "Státusz szûrõ:", Location = new Point(300, 85), AutoSize = true, ForeColor = Color.Gainsboro };
            cmbStatuszSzuro = new ComboBox { Location = new Point(400, 82), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            cmbStatuszSzuro.Items.AddRange(new string[] { "Minden státusz", "Foglalt", "Folyamatban", "Befejezett", "Lemondva" });
            cmbStatuszSzuro.SelectedIndex = 0; // Alapértelmezett a "Minden státusz"
            cmbStatuszSzuro.SelectedIndexChanged += SzuresAlkalmazasa;

            // === TÁBLÁZAT (DataGridView) ===
            dgvIdopontok = new DataGridView
            {
                // <<<--- MÓDOSÍTOTT: Elhelyezés és méret a szûrõk miatt ---<<<
                Location = new Point(20, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(740, 520),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(51, 51, 55),
                BorderStyle = BorderStyle.None,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Padding = new Padding(5) },
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.Gainsboro, SelectionBackColor = Color.FromArgb(0, 122, 204), SelectionForeColor = Color.White, Padding = new Padding(5) },
                RowTemplate = new DataGridViewRow { Height = 35 },
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(70, 70, 75)
            };
            dgvIdopontok.SelectionChanged += DgvIdopontok_SelectionChanged;
            // <<<--- ÚJ: Eseménykezelõ a sorszínezéshez ---<<<
            dgvIdopontok.CellFormatting += DgvIdopontok_CellFormatting;

            // === RÉSZLETEK PANEL ===
            pnlReszletek = new Panel
            {
                // <<<--- MÓDOSÍTOTT: Elhelyezés és méret a szûrõk miatt ---<<<
                Location = new Point(780, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                Size = new Size(380, 520),
                BackColor = Color.FromArgb(51, 51, 55),
                Padding = new Padding(20)
            };

            // === BEVITELI MEZÕK ÉS CÍMKÉIK A RÉSZLETEK PANELEN ===
            int yPos = 30;
            int spacing = 45;

            lblUgyfelNeve = CreateLabel("Ügyfél neve:", yPos);
            txtUgyfelNeve = CreateTextBox(yPos);
            yPos += spacing;

            lblTelefonszam = CreateLabel("Telefonszám:", yPos);
            txtTelefonszam = CreateTextBox(yPos);
            yPos += spacing;

            lblRendszam = CreateLabel("Rendszám:", yPos);
            txtRendszam = CreateTextBox(yPos);
            txtRendszam.CharacterCasing = CharacterCasing.Upper;
            yPos += spacing;

            lblDatum = CreateLabel("Dátum és idõ:", yPos);
            dtpDatum = new DateTimePicker
            {
                Location = new Point(140, yPos - 3),
                Width = 220,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy. MM. dd. HH:mm"
            };
            yPos += spacing;

            lblMunka = CreateLabel("Munka típusa:", yPos);
            cmbMunkaTipusa = CreateComboBox(yPos);
            cmbMunkaTipusa.Items.AddRange(new string[] { "Általános szerviz", "Mûszaki vizsga", "Gumiszerelés", "Motorjavítás", "Fékrendszer javítás", "Diagnosztika" });
            yPos += spacing;

            lblStatusz = CreateLabel("Státusz:", yPos);
            cmbStatusz = CreateComboBox(yPos);
            cmbStatusz.Items.AddRange(new string[] { "Foglalt", "Folyamatban", "Befejezett", "Lemondva" });
            yPos += spacing + 20;

            // === GOMBOK ===


            // === VEZÉRLÕK HOZZÁADÁSA A FELÜLETEKHEZ ===
            pnlReszletek.Controls.Add(lblReszletek);
            pnlReszletek.Controls.Add(lblUgyfelNeve);
            pnlReszletek.Controls.Add(txtUgyfelNeve);
            pnlReszletek.Controls.Add(lblTelefonszam);
            pnlReszletek.Controls.Add(txtTelefonszam);
            pnlReszletek.Controls.Add(lblRendszam);
            pnlReszletek.Controls.Add(txtRendszam);
            pnlReszletek.Controls.Add(lblDatum);
            pnlReszletek.Controls.Add(dtpDatum);
            pnlReszletek.Controls.Add(lblMunka);
            pnlReszletek.Controls.Add(cmbMunkaTipusa);
            pnlReszletek.Controls.Add(lblStatusz);
            pnlReszletek.Controls.Add(cmbStatusz);
            pnlReszletek.Controls.Add(btnUjIdopont);
            pnlReszletek.Controls.Add(btnMentes);
            pnlReszletek.Controls.Add(btnTorles);
            pnlReszletek.Controls.Add(btnRegisztraltIdopontok);
            pnlReszletek.Controls.Add(btnMunkalapok);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(lblIdopontok);
            // <<<--- ÚJ: Szûrõk hozzáadása a Form-hoz ---<<<
            this.Controls.Add(lblKereses);
            this.Controls.Add(txtKereses);
            this.Controls.Add(lblStatuszSzuro);
            this.Controls.Add(cmbStatuszSzuro);
            this.Controls.Add(dgvIdopontok);
            this.Controls.Add(pnlReszletek);
        }

        #region Segédmetódusok a vezérlõk létrehozásához
        private Label CreateLabel(string text, int y)
        {
            return new Label { Text = text, Location = new Point(20, y), AutoSize = true, ForeColor = Color.Gainsboro };
        }
        private TextBox CreateTextBox(int y)
        {
            return new TextBox { Location = new Point(140, y - 3), Width = 220, BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        }
        private ComboBox CreateComboBox(int y)
        {
            return new ComboBox { Location = new Point(140, y - 3), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        }
        private Button CreateButton(string text, int y, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = new Point(20, y),
                Size = new Size(340, 30),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
        }
        #endregion

        #region Adatkezelõ metódusok
        // Minta adatok generálása az induláshoz
        private void LoadSampleData()
        {
            idopontok = new List<Idopont>
            {
                // <<<--- ÚJ: Mintaadatok a színezés és szûrés teszteléséhez ---<<<
                new Idopont { Id = 1, Datum = DateTime.Now.AddDays(1), UgyfelNeve = "TeszT Elek", Telefonszam = "0630123456", Rendszam = "ABC-123", MunkaTipusa = "Általános szerviz", Statusz = "Foglalt" },
                new Idopont { Id = 2, Datum = DateTime.Now.AddDays(2), UgyfelNeve = "Nagy János", Telefonszam = "0670987654", Rendszam = "DEF-456", MunkaTipusa = "Gumiszerelés", Statusz = "Folyamatban" },
                new Idopont { Id = 3, Datum = DateTime.Now.AddHours(2), UgyfelNeve = "Kiss Mária", Telefonszam = "0620111222", Rendszam = "GHI-789", MunkaTipusa = "Motorjavítás", Statusz = "Befejezett" },
                new Idopont { Id = 4, Datum = DateTime.Now.AddDays(3), UgyfelNeve = "Próba Béla", Telefonszam = "0630555666", Rendszam = "JKL-012", MunkaTipusa = "Mûszaki vizsga", Statusz = "Lemondva" },
                new Idopont { Id = 5, Datum = DateTime.Now.AddDays(1).AddHours(3), UgyfelNeve = "Fehér István", Telefonszam = "0630444333", Rendszam = "ABCD-123", MunkaTipusa = "Fékrendszer javítás", Statusz = "Foglalt" }
            };
        }

        // <<<--- MÓDOSÍTOTT: Adatok kötése a táblázathoz (Szûréssel) ---<<<
        private void BindDataToGrid()
        {
            dgvIdopontok.DataSource = null;

            var szurtLista = idopontok.AsEnumerable();

            // 1. Keresés szöveg alapján (ha a vezérlõ már létezik)
            if (txtKereses != null && !string.IsNullOrEmpty(txtKereses.Text))
            {
                string kereses = txtKereses.Text.ToLower().Trim();
                szurtLista = szurtLista.Where(i =>
                    i.UgyfelNeve.ToLower().Contains(kereses) ||
                    i.Rendszam.ToLower().Contains(kereses) ||
                    i.MunkaTipusa.ToLower().Contains(kereses)
                );
            }

            // 2. Szûrés státusz alapján (ha a vezérlõ már létezik és nem a "Minden" van kiválasztva)
            if (cmbStatuszSzuro != null && cmbStatuszSzuro.SelectedIndex > 0)
            {
                string statusz = cmbStatuszSzuro.SelectedItem.ToString();
                szurtLista = szurtLista.Where(i => i.Statusz == statusz);
            }

            // A szûrt lista bekötése a DataGridView-ba
            dgvIdopontok.DataSource = szurtLista.OrderBy(i => i.Datum).ToList();

            // Oszlopok elrejtése vagy átnevezése
            if (dgvIdopontok.Columns["Id"] != null) dgvIdopontok.Columns["Id"].Visible = false;
            if (dgvIdopontok.Columns["Telefonszam"] != null) dgvIdopontok.Columns["Telefonszam"].Visible = false;
            if (dgvIdopontok.Columns["Datum"] != null) dgvIdopontok.Columns["Datum"].HeaderText = "Dátum";
            if (dgvIdopontok.Columns["UgyfelNeve"] != null) dgvIdopontok.Columns["UgyfelNeve"].HeaderText = "Ügyfél neve";
            if (dgvIdopontok.Columns["Rendszam"] != null) dgvIdopontok.Columns["Rendszam"].HeaderText = "Rendszám";
            if (dgvIdopontok.Columns["MunkaTipusa"] != null) dgvIdopontok.Columns["MunkaTipusa"].HeaderText = "Munka";
            if (dgvIdopontok.Columns["Statusz"] != null) dgvIdopontok.Columns["Statusz"].HeaderText = "Státusz";
        }
        #endregion

        #region Eseménykezelõk

        // <<<--- ÚJ ESEMÉNYKEZELÕ A KERESÉSHEZ/SZÛRÉSHEZ ---<<<
        private void SzuresAlkalmazasa(object sender, EventArgs e)
        {
            // Bármelyik szûrõ változik, frissítjük a táblázatot
            BindDataToGrid();
        }

        // <<<--- ÚJ ESEMÉNYKEZELÕ A TÁBLÁZAT SORSZÍNEZÉSÉHEZ ---<<<
        private void DgvIdopontok_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Csak adatsorokat formázunk, a fejlécet nem
            if (e.RowIndex < 0 || e.RowIndex >= dgvIdopontok.Rows.Count) return;

            var idopont = dgvIdopontok.Rows[e.RowIndex].DataBoundItem as Idopont;
            if (idopont == null) return;

            // Alap sötét téma színek (RGB)
            Color backColor = Color.FromArgb(60, 60, 65); // Alapértelmezett

            switch (idopont.Statusz)
            {
                case "Befejezett":
                    backColor = Color.FromArgb(60, 100, 60); // Sötétzöld
                    break;
                case "Folyamatban":
                    backColor = Color.FromArgb(100, 100, 60); // Sötétsárga/Okker
                    break;
                case "Lemondva":
                    backColor = Color.FromArgb(100, 60, 60); // Sötétpiros
                    break;
                case "Foglalt":
                    backColor = Color.FromArgb(60, 60, 100); // Sötétkék
                    break;
            }

            // Beállítjuk a cella háttérszínét
            e.CellStyle.BackColor = backColor;

            // Biztosítjuk, hogy a kijelölés színe felülírja a sorszínt
            if (dgvIdopontok.Rows[e.RowIndex].Selected)
            {
                e.CellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
                e.CellStyle.SelectionForeColor = Color.White;
            }
            else
            {
                e.CellStyle.SelectionBackColor = backColor; // Ha nincs kijelölve, a selection backcolor is ez legyen
                e.CellStyle.SelectionForeColor = Color.Gainsboro;
            }
        }


        private void BtnTeljesKepernyo_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                btnTeljesKepernyo.Text = "Teljes képernyõ";
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                btnTeljesKepernyo.Text = "Normál méret";
            }
        }

        private void BtnKijelentkezes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.Retry)
            {
                DialogResult result = MessageBox.Show(
                    "Biztosan ki szeretne lépni az alkalmazásból?",
                    "Kilépés megerõsítése",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void DgvIdopontok_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvIdopontok.SelectedRows.Count > 0)
            {
                var kivalasztottIdopont = dgvIdopontok.SelectedRows[0].DataBoundItem as Idopont;
                if (kivalasztottIdopont != null)
                {
                    txtUgyfelNeve.Text = kivalasztottIdopont.UgyfelNeve;
                    txtTelefonszam.Text = kivalasztottIdopont.Telefonszam;
                    txtRendszam.Text = kivalasztottIdopont.Rendszam;
                    dtpDatum.Value = kivalasztottIdopont.Datum;
                    cmbMunkaTipusa.SelectedItem = kivalasztottIdopont.MunkaTipusa;
                    cmbStatusz.SelectedItem = kivalasztottIdopont.Statusz;
                }
            }
        }

        private void ClearForm()
        {
            dgvIdopontok.ClearSelection();
            txtUgyfelNeve.Clear();
            txtTelefonszam.Clear();
            txtRendszam.Clear();
            dtpDatum.Value = DateTime.Now;
            cmbMunkaTipusa.SelectedIndex = -1;
            cmbStatusz.SelectedIndex = -1;
            txtUgyfelNeve.Focus();
        }

        private void BtnUjIdopont_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // <<<--- MÓDOSÍTOTT: Mentés gomb validációval ---<<<
        private void BtnMentes_Click(object sender, EventArgs e)
        {
            // Alap validáció (kötelezõ mezõk)
            if (string.IsNullOrWhiteSpace(txtUgyfelNeve.Text) || string.IsNullOrWhiteSpace(txtRendszam.Text) || cmbMunkaTipusa.SelectedItem == null || cmbStatusz.SelectedItem == null)
            {
                MessageBox.Show("Kérlek, töltsd ki az összes kötelezõ mezõt (Név, Rendszám, Munka, Státusz)!", "Hiányzó adatok", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // <<<--- ÚJ: Fejlettebb validáció (Rendszám) ---<<<
            // Elfogadja: ABC-123 és ABCD-123. A CharacterCasing.Upper már be van állítva a TextBoxon.
            string rendszamPattern = @"^[A-Z]{3,4}-\d{3}$";
            if (!Regex.IsMatch(txtRendszam.Text, rendszamPattern))
            {
                MessageBox.Show("A rendszám formátuma érvénytelen. Várt formátum: ABC-123 vagy ABCD-123.", "Érvénytelen rendszám", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRendszam.Focus();
                return;
            }

            // <<<--- ÚJ: Fejlettebb validáció (Telefonszám) ---<<<
            // Opcionális. Ha van, csak szám, +, / és szóköz lehet benne.
            string telefon = txtTelefonszam.Text;
            if (!string.IsNullOrEmpty(telefon) && !telefon.All(c => char.IsDigit(c) || c == '+' || c == '/' || c == ' '))
            {
                MessageBox.Show("A telefonszám érvénytelen karaktereket tartalmaz. Csak számok, '+', '/' és szóköz engedélyezett.", "Érvénytelen telefonszám", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelefonszam.Focus();
                return;
            }


            // Módosítás vagy új elem létrehozása
            if (dgvIdopontok.SelectedRows.Count > 0)
            {
                // Módosítás
                var modositando = dgvIdopontok.SelectedRows[0].DataBoundItem as Idopont;
                if (modositando != null) // Biztonsági ellenõrzés
                {
                    modositando.UgyfelNeve = txtUgyfelNeve.Text;
                    modositando.Telefonszam = txtTelefonszam.Text;
                    modositando.Rendszam = txtRendszam.Text;
                    modositando.Datum = dtpDatum.Value;
                    modositando.MunkaTipusa = cmbMunkaTipusa.SelectedItem.ToString();
                    modositando.Statusz = cmbStatusz.SelectedItem.ToString();

                    MessageBox.Show("Az idõpont sikeresen módosítva.", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // Új elem
                var ujIdopont = new Idopont
                {
                    Id = idopontok.Any() ? idopontok.Max(i => i.Id) + 1 : 1,
                    UgyfelNeve = txtUgyfelNeve.Text,
                    Telefonszam = txtTelefonszam.Text,
                    Rendszam = txtRendszam.Text,
                    Datum = dtpDatum.Value,
                    MunkaTipusa = cmbMunkaTipusa.SelectedItem.ToString(),
                    Statusz = cmbStatusz.SelectedItem.ToString()
                };
                idopontok.Add(ujIdopont);
                MessageBox.Show("Az új idõpont sikeresen rögzítve.", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // A táblázat frissítése és a mezõk kiürítése
            BindDataToGrid();
            ClearForm();
        }

        private void BtnTorles_Click(object sender, EventArgs e)
        {
            if (dgvIdopontok.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Biztosan törölni szeretnéd a kiválasztott idõpontot?", "Törlés megerõsítése", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var torlendo = dgvIdopontok.SelectedRows[0].DataBoundItem as Idopont;
                    idopontok.Remove(torlendo);
                    BindDataToGrid();
                    ClearForm();
                    MessageBox.Show("Az idõpont sikeresen törölve.", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Nincs kiválasztott idõpont a törléshez.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Ablak mozgatásához szükséges eseménykezelõk
        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.dragStartPoint.X, p.Y - this.dragStartPoint.Y);
            }
        }

        private void Header_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }
        #endregion
    }
}
