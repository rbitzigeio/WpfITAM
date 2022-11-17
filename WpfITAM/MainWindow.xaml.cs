using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System;
using System.IO;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Navigation;

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
        SortedDictionary<string, ITAM>     _mITAM       = null;
        Dictionary<string, string>         _mNameToIcto = new Dictionary<string, string>();
        Dictionary<string, string>         _mEmail      = new Dictionary<string, string>();
        Dictionary<string, VerteilerInfo>  _mADMInfo    = new Dictionary<string, VerteilerInfo>();
        string                             _lastName    = "";
        string                             _dataDir     = "";

        public MainWindow() {
            loadData(); 
            InitializeComponent();
        }
        /*-------------------------------------------------------------------------
         * Lesen der IT-AM Stammdaten aus diversen CSV Dateien.
         * Füllen der Map mit Stammdaten auf Basis der ICTO-Nummer der IT-Projekte.
         */
        private void loadData() {
            _dataDir = getDataPath();
            // Read E-Mail Accounts Mapping
            loadITAMEmails(_dataDir, "IT-AM-EMail.csv");
            // Load IT-AM Data from csv file
            loadITAM(_dataDir, "Azure-IT-AM.csv");
            // Read IT-AM-Application data
            loadITAMApplication(_dataDir, "IT-AM-Applications.csv");
            // Read IT-AM-Zuständigkeiten data
            loadITAMResponsibility(_dataDir, "IT-AM-Zuständigkeiten.csv");
            // Read ADM Verteiler Info
            loadITAMADMInfo(_dataDir, "IT-AM-ADMAbfrage.csv");
        }

        private void loadITAMADMInfo(string dataDir, string v) {
            VerteilerInfo vi;
            //ICTO;Projekt;LS (PI);LS (NPI);ADM;ADM Postfach;Incident Postfach (ADM DPDHL Postfach);Incident Postfach (BDL Postfach);
            foreach (string line in System.IO.File.ReadLines(dataDir + v)) {
                string[] s = line.Split(";");
                if (s[0] != null && s[0].Length > 0) {
                    vi = new VerteilerInfo(s[0]);
                    _mADMInfo[s[0]] = vi;
                } else {
                    vi = null;
                }
               
                if (vi != null) {
                    if (s[1] != null && s[1].Length > 0) { vi.Project = s[1];      }
                    if (s[2] != null && s[2].Length > 0) { vi.LS_PI = s[2];        }
                    if (s[3] != null && s[3].Length > 0) { vi.LS_NPI = s[3];       }
                    if (s[4] != null && s[4].Length > 0) { vi.ADM = s[4];          }
                    if (s[5] != null && s[5].Length > 0) { vi.ADMPostfach = s[5];  }
                    if (s[6] != null && s[6].Length > 0) { vi.DPDHLPostfach = s[6];}
                    if (s[7] != null && s[7].Length > 0) { vi.BDLPostfach = s[7];  }

                    // Check missing info in ITAM
                    if (_mITAM.ContainsKey(s[0])) {
                        ITAM itam = _mITAM[s[0]];
                        if (itam.getVerteiler() == null) {
                            itam.setVerteiler(vi.DPDHLPostfach);
                        }
                        if (itam.getBDL() == null) {
                            itam.setBDL(vi.BDLPostfach);
                        }
                    }
                }
            }
        }

        private void loadITAM(string dataDir, string v) {
            ITAM icto;
            // Read Azure-IT-AM data
            Dictionary<string, ITAM> unsorted = new Dictionary<string, ITAM>();
            foreach (string line in System.IO.File.ReadLines(dataDir + v))
            {
                //Trace.WriteLine(line);
                icto = new ITAM(line.Trim());
                unsorted.Add(line, icto);
            }
            _mITAM = new SortedDictionary<string, ITAM>(unsorted);
        }

        private void loadITAMApplication(string dataDir, string v) {
            foreach (string line in System.IO.File.ReadLines(dataDir + v)) {
                //Trace.WriteLine(line);
                string[] s = line.Split(";");
                if (s.Length > 2 && _mITAM.ContainsKey(s[2])) {
                    ITAM itam = _mITAM[s[2]];
                   
                    itam.setName(s[1]);
                    _mNameToIcto.Add(s[1], s[2]);
                    if (s.Length > 5)
                    {
                        itam.setOrganisation(s[5]);
                        itam.setADM(s[6]);
                    }
                }
            }
        }

        private void loadITAMResponsibility(string dataDir, string v){
            foreach (string line in System.IO.File.ReadLines(dataDir + v)) {
                //Trace.WriteLine(line);
                string[] s = line.Split(";");
                if (s.Length == 17) {
                    if (_mITAM.ContainsKey(s[3])) {
                        ITAM itam = _mITAM[s[3]];
                        switch (s[10]) {
                            case "BDL AP":             itam.setBDL(getEmail(s[6]));           break;
                            case "EDL AP":             itam.setEDL(getEmail(s[6]));           break;
                            case "WDL AP":             itam.setWDL(getEmail(s[6]));           break;
                            case "ADM-Vertreter":      itam.setADMVertreter(getEmail(s[11])); break;
                            case "BenachrichtigungCh": itam.setVerteiler(getEmail(s[11]));    break;
                            default: break;
                        }
                    }
                }
            }
        }

        private string getEmail(string s) {
            string s1 = s;
            if (!s.Contains("@") && _mEmail.ContainsKey(s)) {
                    s1 = _mEmail[s];
            } 
            return s1;
        }

        private void loadITAMEmails(string dataDir, string v) {
            foreach (string line in System.IO.File.ReadLines(dataDir + v))
            {
                //Trace.WriteLine(line);
                string[] s = line.Split(";");
                string name = null;
                string email = null;
                if (s.Length == 2) {
                    name          = s[0];
                    email         = s[1];
                    _mEmail[name] = email;
                }
            }
            foreach (var entry in _mEmail) {
                Trace.WriteLine(entry.Key + " / " + entry.Value);
            }
        }

        /*-------------------------------------------------------------------------
* Erfragen der Environmentvariablen des OS und Lesen der Properties des 
* Projekts aus dem Homedirectory des Users. 
*/
        private string getDataPath() {
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
            item.Header = "IT-AM Systems";
            // Insert tree items to root item and get names of IT-Systems
            string name = "";
            List<string> names = new List<string>();
            foreach (var entry in _mITAM) {
                TreeViewItem tvi = new TreeViewItem();
                tvi.Header = entry.Key;
                item.Items.Add(tvi);
                name = entry.Value.getName();
                if (name != null && name.Length > 0) {
                    names.Add(name);
                }
            }
            // Second Item
            TreeViewItem item2 = new TreeViewItem();
            item2.Header = "IT-AM Names";
            List<string> sortedNames = names.OrderBy(name => name).ToList();
            foreach (var entry in sortedNames) {
                item2.Items.Add(entry);
            }
            // Get TreeView reference and add items.
            var tree = sender as TreeView;
            if (tree != null) {
                tree.Items.Add(item);
                tree.Items.Add(item2);
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
            var item = (e.NewValue as TreeViewItem);
            if (item != null) {
                string icto = item.Header.ToString();
                if (icto != null) { 
                    tbLog.Text  = icto;
                    updateITSystemView(icto);
                }
            } else {
                string name = (e.NewValue as string);
                if (name != null && name.Length>0) {
                    tbLog.Text = name;
                    string icto = getICTOByName(name);
                    if (icto != null) {
                        tbLog.Text = icto;
                        updateITSystemView(icto);
                    }
                }
            }
        }

        private string getICTOByName(string name) {
            string icto = null;
            foreach (var entry in _mITAM) {
                string itname = entry.Value.getName();
                if (itname != null && itname.Equals(name)) {
                    icto = entry.Key;
                    break;
                }
            }
            return icto;
        }

        private void updateITSystemView(string icto) {
            if (_mITAM.ContainsKey(icto)) {
                ITAM itam = _mITAM[icto];
                lICTO.Content = icto;
                //lName.Content         = itam.getName();
                tbName.Text = itam.getName();
                lADM.Content = itam.getADM();
                lADMVertreter.Content = itam.getADMVertreter();
                lOrganisation.Content = itam.getOrganisation();
                lVerteiler.Content = itam.getVerteiler();
                lBDL.Content = itam.getBDL();
                lEDL.Content = itam.getEDL();
                lWDL.Content = itam.getWDL();
                if (_mADMInfo.ContainsKey(icto)) {
                    VerteilerInfo vi = _mADMInfo[icto];
                    if (lVerteiler.Content == null) { lVerteiler.Content = vi.DPDHLPostfach; }
                    if (lBDL.Content == null) { lBDL.Content = vi.BDLPostfach; }
                }
            }
        }

        /*--------------------------------------------------------------------
* Extrahieren der gefundenen Daten in eine CSV Datei, die an Dienstleister
* übergeben werden kann.
*/
        private void BtnExtractClick(object sender, RoutedEventArgs e) {
            tbLog.Text = "Extract Application";
            Dictionary<string, string> mEmail = new Dictionary<string, string>();
            string line = null;
            string path = _dataDir + "IT-AM-Systeme.csv";
            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
                string s = "ICTO; Name; ADM; ADM-Vertreter; Organisation; Change Verteiler; Betriebs-DL; Entwicklungs-DL; Wartungs-DL \n";
                Byte[] bs = new UTF8Encoding(true).GetBytes(s);
                fs.Write(bs, 0, bs.Length);
                foreach (var itam in _mITAM) {
                    line = itam.Value.getIcto() + ";" +
                           itam.Value.getName() + ";" +
                           itam.Value.getADM() + ";" +
                           itam.Value.getADMVertreter() + ";" +
                           itam.Value.getOrganisation() + ";" +
                           itam.Value.getVerteiler() + ";" +
                           itam.Value.getBDL() + ";" +
                           itam.Value.getEDL() + ";" +
                           itam.Value.getWDL() + "\n";
                    Byte[] info = new UTF8Encoding(true).GetBytes(line);
                    fs.Write(info, 0, info.Length);
                    string adm = itam.Value.getADM();
                    if (adm != null && adm.Length > 0) {
                        string email = _mEmail[adm];
                        if (!mEmail.ContainsKey(email)) {
                            mEmail.Add(email, email);
                        }
                    }
                    if (itam.Value.getADMVertreter() != null && itam.Value.getADMVertreter().Length > 0) {
                        if (!mEmail.ContainsKey(itam.Value.getADMVertreter())) {
                            mEmail.Add(itam.Value.getADMVertreter(), itam.Value.getADMVertreter());
                        }
                    }
                    if (itam.Value.getVerteiler() != null && itam.Value.getVerteiler().Length > 0){
                        if (!mEmail.ContainsKey(itam.Value.getVerteiler())) {
                            mEmail.Add(itam.Value.getVerteiler(), itam.Value.getVerteiler());
                        }
                    }
                    if (itam.Value.getBDL() != null && itam.Value.getBDL().Length > 0) {
                        if (!mEmail.ContainsKey(itam.Value.getBDL())) {
                            mEmail.Add(itam.Value.getBDL(), itam.Value.getBDL());
                        }
                    }
                    if (itam.Value.getEDL() != null && itam.Value.getEDL().Length > 0) {
                        if (!mEmail.ContainsKey(itam.Value.getEDL())) {
                            mEmail.Add(itam.Value.getEDL(), itam.Value.getEDL());
                        }
                    }
                    if (itam.Value.getWDL() != null && itam.Value.getWDL().Length > 0) {
                        if (!mEmail.ContainsKey(itam.Value.getWDL())) {
                            mEmail.Add(itam.Value.getWDL(), itam.Value.getWDL());
                        }
                    }
                }
                fs.Flush();
                fs.Close();
            }
            path = _dataDir + "IT-AM-MMS.csv";
            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
                foreach (var email in mEmail) {
                    line = email.Value + "\n";
                    Byte[] bs = new UTF8Encoding(true).GetBytes(line);
                    fs.Write(bs, 0, bs.Length);
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
                newTVI.Header             = "IT-AM Systems";
                string name               = "";
                List<string> names        = new List<string>();
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
                    name = itam.Value.getName();
                    if (name != null && name.Length>0) {
                        names.Add(name);
                    }
                }
                TreeViewItem tviNames = new TreeViewItem();
                tviNames.Header = "IT-AM Names";
                List<string> sortedNames = names.OrderBy(name => name).ToList();
                foreach (var entry in sortedNames) {
                    tviNames.Items.Add(entry);
                }
                tvIcto.Items.Add(tviNames);
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