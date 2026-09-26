using System;
using System.Windows.Forms;

namespace AplikasiKasirSMK4
{
    public partial class FormMenu : Form
    {
        public FormMenu()
        {
            InitializeComponent();
        }

        private void btnInputBarang_Click(object sender, EventArgs e)
        {
            // Membuka Form Barang
            FormBarang formBarang = new FormBarang();
            formBarang.Show();
        }

        private void btnKasir_Click(object sender, EventArgs e)
        {
            // Membuka Form Kasir
            FormKasir formKasir = new FormKasir();
            formKasir.Show();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Kembali ke layar Login
            Form1 formLogin = new Form1();
            formLogin.Show();
            this.Close();
        }
    }
}