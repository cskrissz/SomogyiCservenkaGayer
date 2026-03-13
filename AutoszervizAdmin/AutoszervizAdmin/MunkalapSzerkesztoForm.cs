using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoszervizAdmin
{
    public class MunkalapSzerkesztoForm : Form
    {
        public Munkalap UjMunkalap { get; private set; }
        private TextBox txtRendszam, txtSzerelo, txtHiba, txtFeladatok, txtAlkatreszek;
        private NumericUpDown numIdo;
        private ComboBox cmbStatusz;

        public MunkalapSzerkesztoForm()
        {
            this.Text = "Munkalap Rögzítése";
            this.Size = new Size(400, 550);
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            int y = 20;
            AddControl("Rendszám:", txtRendszam = new TextBox { Width = 180, CharacterCasing = CharacterCasing.Upper }, ref y);
            AddControl("Szerelő:", txtSzerelo = new TextBox { Width = 180 }, ref y);
            AddControl("Hiba:", txtHiba = new TextBox { Width = 180, Multiline = true, Height = 50 }, ref y); y += 30;
            AddControl("Feladatok:", txtFeladatok = new TextBox { Width = 180, Multiline = true, Height = 50 }, ref y); y += 30;
            AddControl("Alkatrészek:", txtAlkatreszek = new TextBox { Width = 180 }, ref y);
            AddControl("Idő (óra):", numIdo = new NumericUpDown { Width = 60, Minimum = 1 }, ref y);

            cmbStatusz = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusz.Items.AddRange(new string[] { "Nyitott", "Folyamatban", "Kész", "Számlázva" });
            cmbStatusz.SelectedIndex = 0;
            AddControl("Státusz:", cmbStatusz, ref y);

            Button btnOk = new Button { Text = "MENTÉS", Location = new Point(100, y + 20), Size = new Size(200, 40), BackColor = Color.FromArgb(0, 122, 204), FlatStyle = FlatStyle.Flat };
            btnOk.Click += (s, e) => {
                UjMunkalap = new Munkalap { Rendszam = txtRendszam.Text, Szerelo = txtSzerelo.Text, Hibajelenseg = txtHiba.Text, Feladatok = txtFeladatok.Text, Alkatreszek = txtAlkatreszek.Text, Munkaido = (int)numIdo.Value, Statusz = cmbStatusz.Text };
                this.DialogResult = DialogResult.OK;
            };
            this.Controls.Add(btnOk);
        }

        private void AddControl(string txt, Control c, ref int y)
        {
            this.Controls.Add(new Label { Text = txt, Location = new Point(20, y), AutoSize = true });
            c.Location = new Point(150, y); this.Controls.Add(c); y += 40;
        }
    }
}