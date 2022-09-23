using System;

public class ITAM {
	
	private string _icto;
    private string _name;
    private string _adm;
    private string _admEmail;
    private string _admVertreter;
    private string _admVertreterEmail;
    private string _organisation;
    private string _bdl;
    private string _edl;
    private string _wdl;
    private string _verteiler;


    public ITAM(string icto) {_icto = icto;}

	public string getIcto() {return _icto;}
    public string getName() {return _name;}
    public void setName(string name) {_name = name;}
    public string getADM() { return _adm; }
    public void setADM(string adm) { _adm = adm; }
    public string getADMEmail() { return _admEmail; }
    public void setADMEmail(string email) { _adm = email; }
    public string getADMVertreter() { return _admVertreter; }
    public void setADMVertreter(string admVertreter) { _admVertreter = admVertreter; }
    public string getADMVertreterEmail() { return _admVertreterEmail; }
    public void setADMVertreterEmail(string email) { _admVertreter = email; }
    public string getOrganisation() { return _organisation; }
    public void setOrganisation(string organisation) { _organisation = organisation; }
    public string getBDL() { return _bdl; }
    public void setBDL(string bdl) { _bdl = bdl; }
    public string getEDL() { return _edl; }
    public void setEDL(string edl) { _edl = edl; }
    public string getWDL() { return _wdl; }
    public void setWDL(string wdl) { _wdl = wdl; }
    public string getVerteiler() { return _verteiler; }
    public void setVerteiler(string verteiler) { _verteiler = verteiler; }
}
