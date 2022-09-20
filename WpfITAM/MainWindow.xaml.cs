using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Windows.Shapes;
using System;

namespace WpfITAM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SortedDictionary<string, ITAM> mITAM       = null;
        Dictionary<string, string>     mNameToIcto = new Dictionary<string, string>();
        string lastName = null;

        public MainWindow() {
            loadData(); 
            InitializeComponent();
        }

        private void loadData() {
            // Load IT-AM Data from csv file
            string dir = getDataPath();
            ITAM icto;
            // Read Azure-IT-AM data
            Dictionary<string, ITAM> unsorted = new Dictionary<string, ITAM>();
            foreach (string line in System.IO.File.ReadLines(dir + "Azure-IT-AM.csv")) {
                //Trace.WriteLine(line);
                icto = new ITAM(line.Trim());
                unsorted.Add(line, icto);
            }
            mITAM = new SortedDictionary<string, ITAM>(unsorted);
            // Read IT-AM-Application data
            foreach (string line in System.IO.File.ReadLines(dir + "IT-AM-Applications.csv")) {
                //Trace.WriteLine(line);
                string[] s = line.Split(";");
                if (s.Length > 2 && mITAM.ContainsKey(s[2])) {
                    ITAM itam = mITAM[s[2]];
                    itam.setName(s[1]);
                    mNameToIcto.Add(s[1], s[2]);
                    if (s.Length > 5){
                        itam.setOrganisation(s[5]);
                        itam.setADM(s[6]);
                    }
                }
            }
            // Read IT-AM-Zuständigkeiten data
            foreach (string line in System.IO.File.ReadLines(dir + "IT-AM-Zuständigkeiten.csv")) {
                //Trace.WriteLine(line);
                string[] s = line.Split(";");
                if (s.Length == 17) {
                    if (mITAM.ContainsKey(s[3])) {
                        ITAM itam = mITAM[s[3]];
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

        private string getDataPath()
        {
            string homedrive = Environment.GetEnvironmentVariable("Homedrive");
            string homepath = Environment.GetEnvironmentVariable("Homepath");
            string pathsep  = "\\";
            string path     = homedrive + homepath + pathsep;
            string dir      = null;

            foreach (string line in System.IO.File.ReadLines(path + ".wpfitam.properties")) {
                string[] s = line.Split("=");
                switch(s[0]) {
                    case "datadir": dir = s[1]; break;
                }
            }
            return dir;
        }

        private void TVLoader(object sender, RoutedEventArgs e) {
            tbLog.Text = "Count of IT-AM objects : " + mITAM.Count().ToString();
            // Create a TreeViewItem.
            TreeViewItem item = new TreeViewItem();
            item.Header = "IT-AM Sytem";
            // Insert tree items to root item
            foreach (var entry in mITAM) {
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
                    if (mITAM.ContainsKey(icto)) {
                        ITAM itam = mITAM[icto];
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
            tbLog.Text = "Extract Application";
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
            if (mNameToIcto.ContainsKey(tb.Text)) {
                tvIcto.Items.Clear();
                string icto               = mNameToIcto[tb.Text];
                tbLog.Text                = "Focus Lost. New ICTO : " + tb.Text + " / " + icto;
                TreeViewItem newTVI       = new TreeViewItem();
                newTVI.Header             = "IT-AM Sytem";
                newTVI.IsExpanded         = true;
                TreeViewItem selectedItem = new TreeViewItem();
                tvIcto.Items.Add(newTVI);
                foreach (var itam in mITAM) {
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
                if (lastName != null) { 
                    tb.Text = lastName; 
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
            lastName = tb.Text;
        }
    }
}