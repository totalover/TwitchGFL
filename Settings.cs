using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchGFL
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            if(string.IsNullOrEmpty(Properties.Settings.Default.FileSavePath))
            {
                textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
                textBox1.Text = Properties.Settings.Default.FileSavePath;
            }

            textBox2.Text = Properties.Settings.Default.FileName;



        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("Directory don't exist!!!");
                return;
            }

            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("No file name!");
                return;
            }


            Properties.Settings.Default.FileSavePath = textBox1.Text;
            Properties.Settings.Default.FileName = textBox2.Text;
            Properties.Settings.Default.Save();
            this.Close();


        }

        private void Settings_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox1.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
