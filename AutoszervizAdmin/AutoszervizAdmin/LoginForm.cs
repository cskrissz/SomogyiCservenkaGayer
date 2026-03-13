using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace AutoszervizAdmin
{
    public class LoginForm : Form
    {
        // === VEZÉRLŐK DEFINÍCIÓJA ===
        private Label lblHeaderTitle, lblFelhasznalonev, lblJelszo;
        private TextBox txtFelhasznalonev, txtJelszo;
        private Button btnBejelentkezes, btnClose;
        private Panel pnlHeader;

        // Segédváltozók az ablak mozgatásához
        private bool isDragging = false;
        private Point dragStartPoint;

        // A helyes adminisztrátori adatok (ezt később adatbázisból vagy konfigurációs fájlból is betöltheted)
        private const string AdminUser = "admin";
        private const string AdminPass = "admin";

        // === KONSTRUKTOR ===
        public LoginForm()
        {
            FelületÉpítése();
        }

        private void FelületÉpítése()
        {
            // === ALAP FORM BEÁLLÍTÁSOK ===
            this.Text = "Bejelentkezés";
            this.Size = new Size(400, 250);
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // Keret nélküli ablak

            // Eseménykezelő hozzáadása a form bezárásához
            this.FormClosing += LoginForm_FormClosing; // <<<--- ÚJ SOR

            // === EGYEDI FEJLÉC ===
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(63, 63, 70) };
            pnlHeader.MouseDown += Header_MouseDown;
            pnlHeader.MouseMove += Header_MouseMove;
            pnlHeader.MouseUp += Header_MouseUp;

            lblHeaderTitle = new Label { Text = "Admin Bejelentkezés", Location = new Point(10, 8), ForeColor = Color.Gainsboro, AutoSize = true };

            btnClose = new Button { Text = "X", Dock = DockStyle.Right, Width = 45, FlatStyle = FlatStyle.Flat, BackColor = pnlHeader.BackColor, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
            btnClose.Click += (s, e) => this.Close(); // Ez a sor változatlan marad, a FormClosing esemény elkapja ezt a hívást

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(btnClose);

            // === CÍMKÉK ÉS BEVITELI MEZŐK ===
            lblFelhasznalonev = new Label { Text = "Felhasználónév:", Location = new Point(40, 70), AutoSize = true };
            txtFelhasznalonev = new TextBox { Location = new Point(160, 67), Width = 180, BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            lblJelszo = new Label { Text = "Jelszó:", Location = new Point(40, 110), AutoSize = true };
            txtJelszo = new TextBox { Location = new Point(160, 107), Width = 180, BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, PasswordChar = '●' };

            // === GOMB ===
            btnBejelentkezes = new Button
            {
                Text = "Bejelentkezés",
                Location = new Point(40, 160),
                Size = new Size(300, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnBejelentkezes.Click += BtnBejelentkezes_Click;

            // === VEZÉRLŐK HOZZÁADÁSA A FORMHOZ ===
            this.Controls.Add(pnlHeader);
            this.Controls.Add(lblFelhasznalonev);
            this.Controls.Add(txtFelhasznalonev);
            this.Controls.Add(lblJelszo);
            this.Controls.Add(txtJelszo);
            this.Controls.Add(btnBejelentkezes);

            // Enter gomb lenyomására is működjön a bejelentkezés
            this.AcceptButton = btnBejelentkezes;
        }

        // === ESEMÉNYKEZELŐK ===
        private void BtnBejelentkezes_Click(object sender, EventArgs e)
        {
            if (txtFelhasznalonev.Text == AdminUser && txtJelszo.Text == AdminPass)
            {
                // Ha a bejelentkezés sikeres, a DialogResult-ot OK-ra állítjuk.
                // A Program.cs ez alapján fogja tudni, hogy megnyithatja a főablakot.
                this.DialogResult = DialogResult.OK;
                this.Close(); // Bezárjuk a login ablakot
            }
            else
            {
                MessageBox.Show("Hibás felhasználónév vagy jelszó!", "Sikertelen bejelentkezés", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtJelszo.Clear();
                txtFelhasznalonev.Focus();
                txtFelhasznalonev.SelectAll();
            }
        }

        // <<<--- ÚJ ESEMÉNYKEZELŐ A BEZÁRÁS MEGERŐSÍTÉSÉHEZ ---<<<
        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Csak akkor kérdezünk rá, ha a bezárás nem a sikeres bejelentkezés miatt történik.
            // A sikeres bejelentkezés a DialogResult-ot OK-ra állítja.
            if (this.DialogResult != DialogResult.OK)
            {
                DialogResult result = MessageBox.Show(
                    "Biztosan be szeretné zárni az alkalmazást?",
                    "Megerősítés",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                // Ha a felhasználó a "Nem"-re kattint, megszakítjuk a bezárási folyamatot.
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        #region Ablak mozgatásához szükséges eseménykezelők
        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDragging = true; dragStartPoint = new Point(e.X, e.Y); }
        }
        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging) { Point p = PointToScreen(e.Location); Location = new Point(p.X - this.dragStartPoint.X, p.Y - this.dragStartPoint.Y); }
        }
        private void Header_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }
        #endregion
    }
}