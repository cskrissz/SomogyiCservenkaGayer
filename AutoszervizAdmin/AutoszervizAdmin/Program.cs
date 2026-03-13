using System;
using System.Windows.Forms;

namespace AutoszervizAdmin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ciklus az alkalmazás futtatásához
            while (true)
            {
                // Bejelentkezési ablak megjelenítése dialógusként
                using (LoginForm loginForm = new LoginForm())
                {
                    // Ha a bejelentkezés NEM sikeres (a user bezárja az ablakot),
                    // akkor szakítsuk meg a ciklust és lépjünk ki az alkalmazásból.
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        break; // Kilépés a ciklusból -> alkalmazás vége
                    }
                }

                // Ha idáig eljutott a kód, a bejelentkezés sikeres volt.
                // Indítsuk el a fõablakot.
                using (Form1 mainForm = new Form1())
                {
                    // A fõablak megjelenítése dialógusként. Ez blokkolja a végrehajtást,
                    // amíg a mainForm be nem záródik.
                    mainForm.ShowDialog();

                    // Ellenõrizzük, hogy a fõablak a "Kijelentkezés" miatt záródott-e be.
                    // A "Kijelentkezés" gomb DialogResult.Retry-t ad vissza (ezt mi definiáljuk).
                    // Ha nem ez történt, az azt jelenti, hogy a user az X-szel lépett ki,
                    // tehát az egész alkalmazásból ki akar lépni.
                    if (mainForm.DialogResult != DialogResult.Retry)
                    {
                        break; // Kilépés a ciklusból -> alkalmazás vége
                    }
                    // Ha a DialogResult Retry volt, a ciklus újraindul, és a login form ismét megjelenik.
                }
            }
        }
    }
}