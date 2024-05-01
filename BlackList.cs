using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace TwitchGFL
{
    public partial class BlackList : Form
    {
        public BlackList()
        {
            InitializeComponent();


            try
            {
                var bannedList = System.IO.File.ReadAllText("bans.json");
                var bans = JsonConvert.DeserializeObject<List<string>>(bannedList);
                textBox1.Lines = bans.ToArray();
            }
            catch { }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var bannedJson = new List<string>();
            try
            {
                foreach (var item in textBox1.Lines)
                {
                    bannedJson.Add(item.ToUpper().Trim());
                }
            }
            catch
            {
                MessageBox.Show("Error while parsing bans!");
                return;
            }


            System.IO.File.WriteAllText("bans.json", JsonConvert.SerializeObject(bannedJson, Formatting.Indented));
            this.DialogResult = DialogResult.OK;

        }
    }
}
