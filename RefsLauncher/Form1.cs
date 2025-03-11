using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox1.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)

        {
            loadConfig();

            LoadDirectories();
            textBox1.Select();

        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void LoadDirectories()
        {
            string whdPath = Path.Combine(Application.StartupPath, textBox2.Text);
            if (Directory.Exists(whdPath))
            {
                string[] directories = Directory.GetDirectories(whdPath)
                                               .Select(Path.GetFileName)
                                               .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                                               .ToArray();
                listBox1.Items.Clear();
                listBox1.Items.AddRange(directories);
            }
            else
            {
                MessageBox.Show("WHD klasörü bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText)) return;

            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                string itemText = listBox1.Items[i].ToString().ToLower();
                if (itemText.Contains(searchText))
                {
                    listBox1.SelectedIndex = i;
                    listBox1.TopIndex = i;
                    return;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = !groupBox1.Visible;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveConfig();
            groupBox1.Visible = false;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            LoadDirectories();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            run();
        }

        private void run()
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedFolder = listBox1.SelectedItem.ToString();
                bool exitLauncher = checkBox1.Checked;
                replaceResolution();
                launch(selectedFolder, exitLauncher);
            }
            else
            {
                MessageBox.Show("Lütfen bir öğe seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

            private void launch(string folderName, bool exitLauncher)
        {
            string sysPath = Path.Combine(Application.StartupPath, "SYS", "S");
            string configFilePath = Path.Combine(sysPath, "User-Startup");
            
            try
            {
                // Yeni içerik oluştur
                                string newContent = "cd DH1:" + folderName + ";\n" +
                                    "WHDLoad " + folderName + ".slave PRELOAD;\n";
                
                // Dosyanın içeriğini güncelle
                File.WriteAllText(configFilePath, newContent);
                
                // WinUAE'yi başlat
                string wuaePath = Path.Combine(Application.StartupPath, "WUAE", "WinUae.exe");
                Process.Start(wuaePath, "-f RefLauncher.uae -portable");
                
                // exitLauncher true ise uygulamadan çık
                if (exitLauncher)
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Başlatma hatası"+ ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

            private void replaceResolution()
            {
                string configPath = Path.Combine(Application.StartupPath, "WUAE", "RefLauncher.uae");

                if (!File.Exists(configPath))
                {
                    MessageBox.Show("Config dosyası bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string resolutionText = textBox4.Text.Trim();
                string[] parts = resolutionText.Split('x');

                if (parts.Length != 2)
                {
                    MessageBox.Show("Geçersiz çözünürlük formatı! Lütfen '800x600' şeklinde girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int width, height;

                try
                {
                    width = int.Parse(parts[0].Trim());
                    height = int.Parse(parts[1].Trim());
                }
                catch
                {
                    MessageBox.Show("Geçersiz çözünürlük değerleri! Lütfen sayısal değerler girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string fullAmigaValue = chkFullAmiga.Checked ? "true" : "false";

                try
                {
                    string[] lines = File.ReadAllLines(configPath);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("gfx_width_fullscreen="))
                        {
                            lines[i] = "gfx_width_fullscreen=" + width;
                        }
                        else if (lines[i].StartsWith("gfx_height_fullscreen="))
                        {
                            lines[i] = "gfx_height_fullscreen=" + height;
                        }
                        else if (lines[i].StartsWith("gfx_fullscreen_amiga="))
                        {
                            lines[i] = "gfx_fullscreen_amiga=" + fullAmigaValue;
                        }
                    }

                    File.WriteAllLines(configPath, lines);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Çözünürlük güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }




            private void saveConfig()
            {
                string configPath = Path.Combine(Application.StartupPath, "WUAE", "launcher.ini");

                try
                {
                    using (StreamWriter writer = new StreamWriter(configPath))
                    {
                        writer.WriteLine("Resolution=" + textBox4.Text.Trim());
                        writer.WriteLine("Path=" + textBox2.Text.Trim());
                        writer.WriteLine("SomeSetting=" + textBox3.Text.Trim());
                        writer.WriteLine("FullAmiga=" + (chkFullAmiga.Checked ? "true" : "false"));
                        writer.WriteLine("ExitLauncher=" + (checkBox1.Checked ? "true" : "false"));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ayarlar kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private void loadConfig()
            {
                string configPath = Path.Combine(Application.StartupPath, "WUAE", "launcher.ini");

                if (!File.Exists(configPath))
                    return; // Dosya yoksa işlem yapma

                try
                {
                    string[] lines = File.ReadAllLines(configPath);

                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length != 2) continue;

                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        switch (key)
                        {
                            case "Resolution":
                                textBox4.Text = value;
                                break;
                            case "Path":
                                textBox2.Text = value;
                                break;
                            case "SomeSetting":
                                textBox3.Text = value;
                                break;
                            case "FullAmiga":
                                chkFullAmiga.Checked = value.ToLower() == "true";
                                break;
                            case "ExitLauncher":
                                checkBox1.Checked = value.ToLower() == "true";
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ayarlar yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private void button5_Click(object sender, EventArgs e)
            {
                MessageBox.Show("RefLauncher by Ref 2025 (c) retrojen.org  (Based On Ozay Turay's exe packer)");
            }

            private void textBox1_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) // Enter tuşuna basıldığını kontrol et
                {
                    e.SuppressKeyPress = true; // Enter tuşunun "ding" sesini engelle
                    button1.Focus(); // Button1'e odaklan
                }
            }


        
    }
}