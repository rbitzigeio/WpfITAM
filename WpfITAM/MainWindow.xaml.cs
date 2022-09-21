using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System;
using System.IO;
using System.Text;

namespace WpfITAM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// Stammdaten der IT-Systeme in der Azure Cloud werden aus diversen CSV Dateien gelen und 
    /// in einer MAP verwaltet.
    /// Ziel ist die Extraktion dieser Daten in eine CSV Datei, so dass Dienstleister auf Basis 
    /// der Dienste die verantwortlichen Personen für Betriebs-, Wartungs- und 
    /// Entwicklungsdienstleistungen benachrichtigen können. 
    public partial class MainWindow : Window
    {
        SortedDictionary<string, ITAM> _mITAM       = null;
        Dictionary<string, string>     _mNameToIcto = new Dictionary<string, string>();
        string                         _lastName    = null;
        string                         _dataDir     = null;

        public MainWindow() {
            loadData(); 
            InitializeComponent();
        }
        /*-------------------------------------------------------------------------
         * Lesen der IT-AM Stammdaten aus diversen CSV Dateien.
         * Füllen der Map mit Stammdaten auf Basis der ICTO-Nummer der IT-Projekte.
         */
        private void loadData() {
            // Load IT-AM Data from csv file
            _dataDir = getDataPath();
            ITAM icto;
            // Read Azure-IT-AM data
            Dictionary<string, ITAM> unsorted = new Dictionary<string, ITAM>();
            foreach (string line in System.IO.File.ReadLines(_dataDir + "Azure-IT-AM.csv")) {
                //Trace.WriteLine(line);
                icto = new ITAM(line.Trim());
                unsorted.Add(line, icto);
            }
            _mITAM = new SortedDictionary<string, ITAM>(unsorted);
            // Read IT-AM-Application data
            foreach (string line in System.IO.File.ReadLines(_dataDir + "IT-AM-Applications.csv")) {
                //Trace.WriteLine(line);
                string[] s = line.Split(";");
                if (s.Length > 2 && _mITAM.ContainsKey(s[2])) {
                    ITAM itam = _mITAM[s[2]];
                    itam.setName(s[1]);
                    _mNameToIcto.Add(s[1], s[2]);
                    if (s.Length > 5){
                        itam.setOrganisation(s[5]);
                        itam.setADM(s[6]);
                    }
                }
            }
            // Read IT-AM-Zuständigkeiten data
            foreach (string line in System.IO.File.ReadLines(_dataDir + "IT-AM-Zuständigkeiten.csv")) {
                //Trace.WriteLine(line);
                string[] s = line.Split(";");
                if (s.Length == 17) {
                    if (_mITAM.ContainsKey(s[3])) {
                        ITAM itam = _mITAM[s[3]];
                        switch (s[10]) {
                            case "BDL AP":             itam.setBDL(s[6]);           break;
                            case "ADM-Vertreter":      itam.setADMVertreter(s[11]); break;
                            case "BenachrichtigungCh": itam.setVerteiler(s[11]);    break;
                            default: break;
                        }
                    }
                }
            }
        }
        /*-------------------------------------------------------------------------
         * Erfragen der Environmentvariablen des OS und Lesen der Properties des 
         * Projekts aus dem Homedirectory des Users. 
         */
        private string getDataPath()
        {
            string homedrive = Environment.GetEnvironmentVariable("Homedrive");
            string homepath  = Environment.GetEnvironmentVariable("Homepath");
            string pathsep   = "\\";
            string path      = homedrive + homepath + pathsep;
            string dir       = null;

            foreach (string line in System.IO.File.ReadLines(path + ".wpfitam.properties")) {
                string[] s = line.Split("=");
                switch(s[0]) {
                    case "datadir": dir = s[1]; break;
                }
            }
            return dir;
        }

        /*-------------------------------------------------------
         * Füllen der graphischen Oberflächen mit den Daten der 
         * IT-Systeme in das Oberflächenelement TreeView.  
         */
        private void TVLoader(object sender, RoutedEventArgs e) {
            tbLog.Text = "Count of IT-AM objects : " + _mITAM.Count().ToString();
            // Create a TreeViewItem.
            TreeViewItem item = new TreeViewItem();
            item.Header = "IT-AM Sytem";
            // Insert tree items to root item
            foreach (var entry in _mITAM) {
                TreeViewItem tvi = new TreeViewItem();
                tvi.Header = entry.Key;
                item.Items.Add(tvi);
            }
            // Get TreeView reference and add  items.
            var tree = sender as TreeView;
            if (tree != null){
                tree.Items.Add(item);
            }
        }

        /*-------------------------------------------------------------
         * Verlassen der Applikation
         */
        private void BtnExitClick(object sender, RoutedEventArgs e) {
            tbLog.Text = "Exit Application";
            Application.Current.MainWindow.Close();
        }

        /*--------------------------------------------------------------------
         * Aktualisierung der Daten des IT-Systems bei Änderung der Auswahl im Baum
         */
        private void selectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var   item = (e.NewValue as TreeViewItem);
            if (item != null) {
                string icto = item.Header.ToString();
                if (icto != null) { 
                    tbLog.Text  = icto;
                    if (_mITAM.ContainsKey(icto)) {
                        ITAM itam = _mITAM[icto];
                        lICTO.Content         = icto;
                        //lName.Content         = itam.getName();
                        tbName.Text           = itam.getName();
                        lADM.Content          = itam.getADM();
                        lADMVertreter.Content = itam.getADMVertreter();
                        lOrganisation.Content = itam.getOrganisation();
                        lVerteiler.Content    = itam.getVerteiler();
                        lBDL.Content          = itam.getBDL();
                    }
                }
            }
        }
        /*--------------------------------------------------------------------
         * Extrahieren der gefundenen Daten in eine CSV Datei, die an Dienstleister
         * übergeben werden kann.
         */
        private void BtnExtractClick(object sender, RoutedEventArgs e) {
            tbLog.Text  = "Extract Application";
            string line = null;
            string path = _dataDir + "IT-AM-Systeme.csv";
            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
                string s  = "ICTO; Name; ADM; ADM-Vertreter; Organisation; Change Verteiler; Betriebs-DL \n";
                Byte[] bs = new UTF8Encoding(true).GetBytes(s);
                fs.Write(bs, 0, bs.Length);
                foreach (var itam in _mITAM) {
                    line = itam.Value.getIcto() + ";" +
                           itam.Value.getName() + ";" +
                           itam.Value.getADM() + ";" +
                           itam.Value.getADMVertreter() + ";" +
                           itam.Value.getOrganisation() + ";" +
                           itam.Value.getVerteiler() + ";" +
                           itam.Value.getBDL() + "\n";
                    Byte[] info = new UTF8Encoding(true).GetBytes(line);
                    fs.Write(info, 0, info.Length);
                }
                fs.Flush();
                fs.Close();
            }
        }
        /*--------------------------------------------------------------------
         * Das Textfeld mit dem Namen des IT-Systems kann auch zur Suche genutzt werden.
         * Nach Verlassen des Textfeldes, z.B. über <TAB> wird diese Methode aufgerufen
         * und sucht über den IT-Systemname nach der ICTO Nummer. Diesewird bei erfolgreiche 
         * Suche im Baum aktiviert und in den Textfeldern aktualisiert. 
         * Wenn die Suche über den IT-System Namen keine ICTO Nummer findet,
         * so erfolgt kein Update der Maske und die Daten der letzten korrekten 
         * Suche werden unverändert angezeigt. 
        */
        private void tbName_LostFocus(object sender, RoutedEventArgs e) {
            var tb = e.Source as TextBox;
            if (_mNameToIcto.ContainsKey(tb.Text)) {
                tvIcto.Items.Clear();
                string icto               = _mNameToIcto[tb.Text];
                tbLog.Text                = "Focus Lost. New ICTO : " + tb.Text + " / " + icto;
                TreeViewItem newTVI       = new TreeViewItem();
                newTVI.Header             = "IT-AM Sytem";
                newTVI.IsExpanded         = true;
                TreeViewItem selectedItem = new TreeViewItem();
                tvIcto.Items.Add(newTVI);
                foreach (var itam in _mITAM) {
                    TreeViewItem item2 = new TreeViewItem();
                    item2.Header = itam.Value.getIcto();
                    newTVI.Items.Add(item2);
                    if (itam.Value.getIcto().Equals(icto)) {
                        Trace.WriteLine("Icto found : " + icto);
                        selectedItem = item2;
                        
                    }
                }
                selectedItem.SetValue(TreeViewItem.IsSelectedProperty, true);
                tvIcto.UpdateLayout();
            } else {
                tbLog.Text = "ICTO for " + tb.Text + " not found.";
                if (_lastName != null) { 
                    tb.Text = _lastName; 
                }
            }  
            
        }
        
        /*--------------------------------------------------------------------
         * Sichern des Namens des zuletzt korrekt ausgewählten IT-Systems
         * Wenn die Suche über den IT-System Namen keine ICTO Nummer findet,
         * so erfolgt kein Update der Maske und die Daten der letzten korrekten 
         * Suche werden unverändert angezeigt. 
        */
        private void tbName_GotFocus(object sender, RoutedEventArgs e) {
            var tb = e.Source as TextBox;
            _lastName = tb.Text;
        }
    }
}