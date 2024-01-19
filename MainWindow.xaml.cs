using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MMOCalculator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class CopyableHyperlink : Hyperlink {
        public CopyableHyperlink() {
            MouseLeftButtonDown += CopyableHyperlink_MouseLeftButtonDown;
        }

        private void CopyableHyperlink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            try {
                if (Inlines.FirstInline is Run run) {
                    Clipboard.SetText(run.Text);
                }
            }
            catch (System.Runtime.InteropServices.COMException) {
                MessageBox.Show("Hiba történt a vágólap használata közben!");
            }
        }
    }

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        FlowDocument flowDocument = new FlowDocument();
        Paragraph paragraph = new Paragraph();
        CopyableHyperlink hyperlink = new CopyableHyperlink();

        private void btnConvert_Click(object sender, RoutedEventArgs e) {

            // Adatok beolvasása az input felületről
            string range = new TextRange(inputOldData.Document.ContentStart, inputOldData.Document.ContentEnd).Text.Trim();
            List<string> dataList = range.Split('\n').ToList();

            // Kivételek kezelése
            if (inputName.Text.Trim() == "") {
                MessageBox.Show($"A név mező üres!");
                return;
            }

            if (range.Trim() == "") {
                MessageBox.Show($"A beviteli mező üres!");
                return;
            }

            // Az időbélyegző kivágása a sorokból, a nem kellő sorok törlése
            for (int i = 0; i < dataList.Count; i++) {

                if (!dataList[i].Trim().EndsWith(")")) {
                    dataList.RemoveAt(i);
                    i--;
                    continue;
                }
                dataList[i] = dataList[i].Split(']')[1];
            }

            // [SkillName,SkillExp]
            string[,] skills = new string[dataList.Count, 2];

            for (int i = 0; i < dataList.Count; i++) { 

                skills[i, 0] = dataList[i].Split(':')[0]; // SkillName 
                if (skills[i, 0] == "Szelídítés") skills[i, 0] = "Szelidítés"; // Elírás javítása, mert aki fordította egy fogalmatlan cigány

                skills[i, 1] = dataList[i].Substring(dataList[i].IndexOf('(') + 1, dataList[i].IndexOf('/') - (dataList[i].IndexOf('(') + 1)).Replace(",", ""); // SkillExp

                // Skill level kinyerése, xp számítása
                int SkillLevel = int.Parse(dataList[i].Substring(dataList[i].IndexOf(':') + 2, (dataList[i].IndexOf('X') - 1) - (dataList[i].IndexOf(':') + 2)).Replace(",", ""));
                int xp = 1020;

                for (int j = 0; j < SkillLevel; j++) {
                    skills[i, 1] = (int.Parse(skills[i, 1]) + xp).ToString();   
                    xp += 20;
                }
            }

            // Parancsok generálása
            for (int i = 0; i < skills.Length/2; i++) {

                hyperlink.Inlines.Add(new Run($"/addxp {inputName.Text} {skills[i, 0]} {skills[i, 1]}\n"));

                paragraph.Inlines.Add(hyperlink);
                flowDocument.Blocks.Add(paragraph);
                outputCommands.Document = flowDocument;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "Név...") {
                textBox.Text = "";
            }
        }

        private void RichTextBox_GotFocus(object sender, RoutedEventArgs e) {
            RichTextBox richTextBox = (RichTextBox)sender;
            if (richTextBox.Document.Blocks.FirstBlock != null && richTextBox.Document.Blocks.FirstBlock is Paragraph paragraph) {
                Run run = (Run)paragraph.Inlines.FirstInline;
                if (run.Text == "Régi MCMMO adatok..." || run.Text == "Parancsok...") {
                    run.Text = "";
                }
            }
        }
    }
}
