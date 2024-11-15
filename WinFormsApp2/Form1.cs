
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;

namespace WinFormsApp2
{
    public partial class Forma : Form
    {
        private byte[] ccbpmBytes;
        public Forma()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CCBPM files (*.ccbpm)|*.ccbpm";
            openFileDialog.Title = "Selecione o arquivo eventBattle.ccbpm";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string ccbpmPath = openFileDialog.FileName;
                ccbpmBytes = File.ReadAllBytes(ccbpmPath); 

                
                openFileDialog.Filter = "CCTXT files (*.cctxt)|*.cctxt";
                openFileDialog.Title = "Selecione o arquivo eventBattle.cctxt";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string cctxtPath = openFileDialog.FileName;
                    LoadAndDisplayEvents(ccbpmPath, cctxtPath);
                }
            }
        }

        public class CharacterConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
              
                return new StandardValuesCollection(new[]
                {
        "Desativado", "Asuma", "Choji", "Chyo", "Deidara", "Gaara", "Guy", "Hinata", "Hidan", "Itachi",
        "Ino", "Jiraya", "Jugo", "Kabuto", "Kabuto (SHIPPUDEN)", "Kakashi", "Kakashi jovem",
        "Karin", "Kankuro", "Kisame", "Konan", "Kurenei", "Minato - Quarto Hokage",
        "Naruto", "Naruto (Manto de 4 Caudas)", "Naruto estudante", "Neji", "Obito",
        "Orochimaru", "Pain 1", "Pain 2", "Pain 3", "Rock Lee", "Sai", "Sakura",
        "Sasori", "Sasori (Hiruko)", "Sasuke", "Sasuke estudante", "Shikamaru",
        "Shino", "Shizune", "Tenten", "Temari", "Tobi", "Tsunade", "Yamato", "nwz",
        "sazori transformação", "sugeitsu"
    });
            }
        }


        private void LoadAndDisplayEvents(string ccbpmPath, string cctxtPath)
        {
            try
            {
                const int blockSize = 0x52; 

                List<string> missionNames = LoadMissionNames(cctxtPath);

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    Title = "Selecione o arquivo de tradução"
                };

                List<string> translationTexts = new List<string>();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string translationPath = openFileDialog.FileName;
                    translationTexts = File.ReadAllLines(translationPath).ToList();
                }

                byte[] ccbpmBytes = File.ReadAllBytes(ccbpmPath);
                int missionCount = ccbpmBytes.Length / blockSize;

                treeView1.Nodes.Clear();

                for (int i = 0; i < missionCount; i++)
                {
                    string missionName = i < missionNames.Count ? missionNames[i] : $"Missão {i + 1} (Nome não encontrado)";
                    if (i < translationTexts.Count)
                    {
                        missionName = translationTexts[i]; 
                    }

                    int blockStart = i * blockSize;
                    string character = GetCharacterName(ccbpmBytes, blockStart);
                    string player2Character = GetCharacterName(ccbpmBytes, blockStart + 0x0A); 
                    string player3Character = GetCharacterName(ccbpmBytes, blockStart + 0x14); 
                    string player4Character = GetCharacterName(ccbpmBytes, blockStart + 0x1E); 
                    string fightTime = GetFightTime(ccbpmBytes, blockStart);

                    string player2Team = GetFriendOrEnemy(ccbpmBytes, blockStart + 0x0C);
                    string player3Team = GetFriendOrEnemy(ccbpmBytes, blockStart + 0x16);
                    string player4Team = GetFriendOrEnemy(ccbpmBytes, blockStart + 0x20); 
                    string map = GetMap(ccbpmBytes, blockStart + 0x2A);

                    string friendOrEnemy = GetFriendOrEnemy(ccbpmBytes, blockStart + 0x0C);

                    EventInfo eventInfo = new EventInfo
                    {
                        Character = character,
                        FightTime = fightTime,
                        Player2 = player2Character, 
                        Player2Team = friendOrEnemy, 
                        Player3 = player3Character,
                        Player3Team = player3Team, 
                        Player4 = player4Character, 
                        Player4Team = player4Team, 
                        Map = map,
                        HexData = ccbpmBytes.Skip(blockStart).Take(blockSize).ToArray()
                    };

                    TreeNode missionNode = new TreeNode(missionName)
                    {
                        Tag = eventInfo
                    };

                    treeView1.Nodes.Add(missionNode);
                }

                treeView1.AfterSelect += TreeView1_AfterSelect;


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar os arquivos: {ex.Message}");
            }
        }



        private string GetFightTime(byte[] data, int start)
        {
            if (data.Length >= start + 0x50 + 1)
            {
                int fightTime = data[start + 0x50];
                int fightTimeDecimal = fightTime;
                return $"{fightTimeDecimal}";
            }
            return "Tempo de Luta: Indisponível";
        }

        private string GetCharacterName(byte[] data, int start)
        {
            if (data.Length >= start + 1)
            {
                string hexValue = BitConverter.ToString(data, start, 1).Replace("-", " ");

                return hexValue switch
                {
                    "39" => "Naruto",
                    "3A" => "Sakura",
                    "3B" => "Gaara",
                    "3C" => "Kankuro",
                    "3D" => "Temari",
                    "3E" => "Chyo",
                    "3F" => "Sasori",
                    "40" => "Deidara",
                    "41" => "Neji",
                    "42" => "Tenten",
                    "43" => "Rock Lee",
                    "44" => "Shikamaru",
                    "45" => "Guy",
                    "46" => "Kakashi",
                    "47" => "Itachi",
                    "48" => "Kisame",
                    "49" => "Naruto (Manto de 4 Caudas)",
                    "4A" => "nwz",
                    "4B" => "Sasori transformação",
                    "4C" => "Sasori (Hiruko)",
                    "4D" => "Kiba",
                    "4E" => "Shino",
                    "4F" => "Hinata",
                    "51" => "Choji",
                    "52" => "Ino",
                    "53" => "Jiraya",
                    "54" => "Tsunade",
                    "55" => "Shizune",
                    "56" => "Asuma",
                    "57" => "Kurenei",
                    "58" => "Kabuto",
                    "59" => "Orochimaru",
                    "5A" => "Kabuto (SHIPPUDEN)",
                    "5B" => "Yamato",
                    "5C" => "Sai",
                    "5D" => "Sasuke",
                    "5E" => "Hidan",
                    "5F" => "Kakuzu",
                    "60" => "Sugeitsu",
                    "61" => "Jugo",
                    "62" => "Karin",
                    "63" => "Tobi",
                    "64" => "Konan",
                    "65" => "Pain 1",
                    "66" => "Pain 2",
                    "67" => "Pain 3",
                    "68" => "Minato - Quarto Hokage",
                    "69" => "Kakashi jovem",
                    "6A" => "Obito",
                    "6B" => "Naruto estudante",
                    "6C" => "Sasuke estudante",

                    "FF" => "Desativado",

                    _ => $"Outro personagem: {hexValue}"
                };
            }
            return "Desconhecido";
        }

        private void UpdateCharacter(byte[] data, int start, string characterChoice)
        {
            byte[] characterBytes = characterChoice switch
            {
                "Desativado" => new byte[] { 0xFF, 0xFF }, 
                "Naruto" => new byte[] { 0x39, 0x00 },      
                "Sakura" => new byte[] { 0x3A, 0x00 },
                "Gaara" => new byte[] { 0x3B, 0x00 },
                "Kankuro" => new byte[] { 0x3C, 0x00 },
                "Temari" => new byte[] { 0x3D, 0x00 },
                "Chyo" => new byte[] { 0x3E, 0x00 },
                "Sasori" => new byte[] { 0x3F, 0x00 },
                "Deidara" => new byte[] { 0x40, 0x00 },
                "Neji" => new byte[] { 0x41, 0x00 },
                "Tenten" => new byte[] { 0x42, 0x00 },
                "Rock Lee" => new byte[] { 0x43, 0x00 },
                "Shikamaru" => new byte[] { 0x44, 0x00 },
                "Guy" => new byte[] { 0x45, 0x00 },
                "Kakashi" => new byte[] { 0x46, 0x00 },
                "Itachi" => new byte[] { 0x47, 0x00 },
                "Kisame" => new byte[] { 0x48, 0x00 },
                "Naruto (Manto de 4 Caudas)" => new byte[] { 0x49, 0x00 },
                "nwz" => new byte[] { 0x4A, 0x00 },
                "Sasori transformação" => new byte[] { 0x4B, 0x00 },
                "Sasori (Hiruko)" => new byte[] { 0x4C, 0x00 },
                "Kiba" => new byte[] { 0x4D, 0x00 },
                "Shino" => new byte[] { 0x4E, 0x00 },
                "Hinata" => new byte[] { 0x4F, 0x00 },
                "Choji" => new byte[] { 0x51, 0x00 },
                "Ino" => new byte[] { 0x52, 0x00 },
                "Jiraya" => new byte[] { 0x53, 0x00 },
                "Tsunade" => new byte[] { 0x54, 0x00 },
                "Shizune" => new byte[] { 0x55, 0x00 },
                "Asuma" => new byte[] { 0x56, 0x00 },
                "Kurenei" => new byte[] { 0x57, 0x00 },
                "Kabuto" => new byte[] { 0x58, 0x00 },
                "Orochimaru" => new byte[] { 0x59, 0x00 },
                "Kabuto (SHIPPUDEN)" => new byte[] { 0x5A, 0x00 },
                "Yamato" => new byte[] { 0x5B, 0x00 },
                "Sai" => new byte[] { 0x5C, 0x00 },
                "Sasuke" => new byte[] { 0x5D, 0x00 },
                "Hidan" => new byte[] { 0x5E, 0x00 },
                "Kakuzu" => new byte[] { 0x5F, 0x00 },
                "Sugeitsu" => new byte[] { 0x60, 0x00 },
                "Jugo" => new byte[] { 0x61, 0x00 },
                "Karin" => new byte[] { 0x62, 0x00 },
                "Tobi" => new byte[] { 0x63, 0x00 },
                "Konan" => new byte[] { 0x64, 0x00 },
                "Pain 1" => new byte[] { 0x65, 0x00 },
                "Pain 2" => new byte[] { 0x66, 0x00 },
                "Pain 3" => new byte[] { 0x67, 0x00 },
                "Minato - Quarto Hokage" => new byte[] { 0x68, 0x00 },
                "Kakashi jovem" => new byte[] { 0x69, 0x00 },
                "Obito" => new byte[] { 0x6A, 0x00 },
                "Naruto estudante" => new byte[] { 0x6B, 0x00 },
                "Sasuke estudante" => new byte[] { 0x6C, 0x00 },

                _ => new byte[] { 0xFF, 0xFF }  
            };

          
            if (data.Length >= start + 2)
            {
                data[start] = characterBytes[0];
                data[start + 1] = characterBytes[1];
            }
        }






        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is EventInfo eventInfo)
            {
        
                propertyGrid1.SelectedObject = eventInfo;

    
                if (eventInfo.HexData != null)
                {
                    StringBuilder hexDisplay = new StringBuilder();
                    for (int i = 0; i < eventInfo.HexData.Length; i += 16) 
                    {
                        int length = Math.Min(16, eventInfo.HexData.Length - i);
                        string hexLine = BitConverter.ToString(eventInfo.HexData, i, length).Replace("-", " ");
                        hexDisplay.AppendLine(hexLine);
                    }

                }
            }
        }




        private List<string> LoadMissionNames(string cctxtPath)
        {
            var missionNames = new List<string>();

            try
            {
                byte[] fileBytes = File.ReadAllBytes(cctxtPath);
                if (fileBytes.Length > 401)
                {
                    fileBytes = fileBytes[401..];
                    var texts = Encoding.UTF8.GetString(fileBytes).Split('\0');

                    foreach (string text in texts)
                    {
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            missionNames.Add(text.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao ler o arquivo cctxt: {ex.Message}");
            }

            return missionNames;
        }

        private string GetMap(byte[] data, int start)
        {
            if (data.Length >= start + 1)
            {
                byte value = data[start];
                return value switch
                {
                    0x00 => "Village Hidden in the Leaves (Mapa 1)",
                    0x06 => "Training Field (Mapa 2)",
                    0x01 => "Village Hidden in the Sand (Mapa 5)",
                    0x04 => "Wilderness of Running Water (Mapa 6)",
                    0x05 => "Cliff of Morning Mist (Mapa 7)",
                    0x0C => "Tenchi Bridge (Mapa 9)",
                    0x02 => "Orochimaru Lab (Mapa 11)",
                    _ => $"Mapa desconhecido: {value:X2}"
                };
            }
            return "Mapa: Indisponível";
        }

        private void UpdateFriendOrEnemy(byte[] data, int start, string teamChoice)
        {
            byte value = teamChoice == "Yes" ? (byte)0x00 : (byte)0x01;  
            data[start] = value;
        }

        public class YesNoConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { "Yes", "No" });
            }
        }


        public class EventInfo
        {

            [Category("Battle Settings")]
            public string FightTime { get; set; }

            [Category("Battle Settings")]
            public string Map { get; set; }

            [Category("Informações do Evento")]
            [TypeConverter(typeof(CharacterConverter))] 
            public string Character { get; set; }



            [Category("Informações do Evento")]
            [TypeConverter(typeof(CharacterConverter))] 
            public string Player2 { get; set; }

            [Category("Informações do Evento")]
            [TypeConverter(typeof(CharacterConverter))]
            public string Player3 { get; set; }

            [Category("Informações do Evento")]
            [TypeConverter(typeof(CharacterConverter))] 
            public string Player4 { get; set; }

            [Category("Team Settings")]
            [TypeConverter(typeof(YesNoConverter))]
            public string Player2Team { get; set; }

            [Category("Team Settings")]
            [TypeConverter(typeof(YesNoConverter))]
            public string Player3Team { get; set; }

            [Category("Team Settings")]
            [TypeConverter(typeof(YesNoConverter))]
            public string Player4Team { get; set; }


            [Category("zHex View")]
            public byte[] HexData { get; set; } 
        }
        private string GetFriendOrEnemy(byte[] data, int start)
        {
            if (data.Length >= start + 1)
            {
                byte value = data[start];
                return value == 0x01 ? "No" : "Yes";
            }
            return "Indisponível";
        }






        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                Title = "Abrir arquivo de texto"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    string[] lines = File.ReadAllLines(filePath);
                    treeView1.Nodes.Clear();

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            TreeNode node = new TreeNode(line);
                            treeView1.Nodes.Add(node);
                        }
                    }

                    MessageBox.Show("Texto importado com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao importar o arquivo de texto: {ex.Message}");
                }
            }
        }

        private void exportTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                Title = "Salvar arquivo de texto"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    List<string> treeViewTexts = new List<string>();
                    foreach (TreeNode node in treeView1.Nodes)
                    {
                        treeViewTexts.Add(node.Text);
                    }

                    File.WriteAllLines(filePath, treeViewTexts);
                    MessageBox.Show("Texto exportado com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao exportar o arquivo de texto: {ex.Message}");
                }
            }
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CCBPM files (*.ccbpm)|*.ccbpm",
                Title = "Salvar Arquivo eventBattle.ccbpm"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    
                    foreach (TreeNode node in treeView1.Nodes)
                    {
                        if (node.Tag is EventInfo eventInfo)
                        {
                            int blockStart = treeView1.Nodes.IndexOf(node) * 0x52;  

                            

                            UpdateFriendOrEnemy(ccbpmBytes, blockStart + 0x0C, eventInfo.Player2Team);
                            UpdateFriendOrEnemy(ccbpmBytes, blockStart + 0x16, eventInfo.Player3Team);
                            UpdateFriendOrEnemy(ccbpmBytes, blockStart + 0x20, eventInfo.Player4Team);

                   
                            UpdateCharacter(ccbpmBytes, blockStart + 0x00, eventInfo.Character);  
                            UpdateCharacter(ccbpmBytes, blockStart + 0x0A, eventInfo.Player2); 
                            UpdateCharacter(ccbpmBytes, blockStart + 0x14, eventInfo.Player3);  
                            UpdateCharacter(ccbpmBytes, blockStart + 0x1E, eventInfo.Player4);  
                        }
                    }

           
                    File.WriteAllBytes(saveFileDialog.FileName, ccbpmBytes);
                    MessageBox.Show("Arquivo salvo com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar o arquivo: {ex.Message}");
                }
            }
        }

    }
}
