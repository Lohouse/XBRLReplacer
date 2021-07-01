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
using System.Xml.Linq;

namespace XBRLReplacer
{
    public partial class Form1 : Form
    {
        private XNamespace xbrli = "http://www.xbrl.org/2003/instance";
        private List<string> xbrlFiles = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    foreach (var f in files)
                    {
                        Console.WriteLine(Path.GetExtension(f));
                        if (Path.GetExtension(f) == ".xbrl")
                        {
                            xbrlFiles.Add(f);
                        }
                    }

                    //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");

                    labelSelected.Text = "Folder: " + fbd.SelectedPath + " (contains " + xbrlFiles.Count.ToString() + " XBRL files)";
                    labelAction.Text = "";
                    progressReplace.Value = 0;
                }
            }
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (xbrlFiles.Count <= 0)
            {
                System.Windows.Forms.MessageBox.Show("ERROR: No XBRL files found in the given folder!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFind.Text) || string.IsNullOrWhiteSpace(txtReplace.Text))
            {
                System.Windows.Forms.MessageBox.Show("ERROR: Find or replace field does not contain valid text!");
                return;
            }

            if (txtFind.Text.Equals(txtReplace.Text))
            {
                System.Windows.Forms.MessageBox.Show("ERROR: Find and replace are equal!");
                return;
            }

            btnReplace.Enabled = false;

            int filesReplaced = 0;
            int amountReplaced = 0;
            progressReplace.Maximum = xbrlFiles.Count;

            foreach (var f in xbrlFiles)
            {
                filesReplaced++;

                string[] fParts = f.Split('\\');
                labelAction.Text = "[" + filesReplaced.ToString() + "/" + xbrlFiles.Count.ToString() + "] " + fParts[fParts.Length - 1];
                labelAction.Refresh();

                XDocument doc = XDocument.Load(f);

                foreach (var element in doc.Descendants(xbrli + "identifier"))
                {
                    // TODO: Also do contains, startwith, endswith ?
                    if (element.Value == txtFind.Text)
                    {
                        element.SetValue(txtReplace.Text);
                        amountReplaced++;
                    }
                }

                doc.Save(f);

                progressReplace.Value = filesReplaced;
            }

            labelAction.Text = "Replaced " + amountReplaced.ToString() + " IDs in " + filesReplaced.ToString() + " XBRL files";
            btnReplace.Enabled = true;
        }
    }
}
