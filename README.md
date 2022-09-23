# WpfITAM
Sammeln der Stammdaten aus IT-AM von Azure Projekten.

Herkunft der Daten:

IT-AM-Applications 
-> aus IT-AM
   Extrakt der Web-Seite https://itm.prg-dc.dhl.com/sites/ITR/SiteAssets/Prod/ITR/index.aspx#/home?$clusters=&$apps=&$search=
   Aktion "Tabelle exportieren"
   Ergebnis ist eine Excelliste, diese Liste im Dateityp "CSV UTF-8 (durch Trennzeichen getrennt)" speichern

IT-AM-Zustaendigkeiten
-> aus IT-AM
   Extrakt der iShare Liste https://itm.prg-dc.dhl.com/sites/opk/_layouts/15/start.aspx#/Lists/ITApplicationKontakt/AllItems.aspx
   Aktion "Nach Excel exportieren"
   Ergebnis ist eine Excelliste, diese Liste im Dateityp "CSV UTF-8 (durch Trennzeichen getrennt)" speichern
   
Azure-IT-AM
-> aus GO Programm "IT-AM-Azure" (s.GitHub)
   Ergebnis ist eine Liste mit ICTO bzw. ITR Nummern der Azure Programme
   
IT-AM-EMail.csv
-> Mappingliste zum Ermitteln der E-Mailadressen von Personen mit Postkennung
   Postkennung ist "<Name>, <Vornamenskürzel>. <Abteilung>, <Dienstsitz>"
   Diese Datei muss ggf. iterativ erweitert werden, da immer wieder neue Personen aus IT-AM hinzukommen.
   
IT-AM-Systeme
-> Ergebnisdatei aus dem C# Programm "WpfITAM" (s.GitHub)
   Datei beinhaltet im ersten Entwurf:
   ICTO; Name; ADM; ADM-Vertreter; Organisation; Change Verteiler; Betriebs-DL 
   
IT-AM-MMS
-> Ergebnisdatei aus dem C# Programm "WpfITAM" (s.GitHub)
   Datei beinhaltet im ersten Entwurf Liste der bei einem Azure Change durch die MMS zu informierende Empfänger.
   Die Empfängerliste soll auch Funktionspostfächer enthalten
