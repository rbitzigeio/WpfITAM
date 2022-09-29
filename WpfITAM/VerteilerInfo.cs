using System;

public class VerteilerInfo
{
	private string _icto             = "";
    private string _project          = "";
    private string _ls_pi            = "";
    private string _ls_npi           = "";
    private string _adm              = "";
    private string _admPostfach      = "";
    private string _admDpdhlPostfach = "";
    private string _bdlPostfach      = "";

	public VerteilerInfo(string icto) { _icto = icto; }
    public string Icto          { get => _icto;             set => _icto             = value; }
    public string Project       { get => _project;          set => _project          = value; }
    public string LS_PI         { get => _ls_pi;            set => _ls_pi            = value; }
    public string LS_NPI        { get => _ls_npi;           set => _ls_npi           = value; }
    public string ADM           { get => _adm;              set => _adm              = value; }
    public string ADMPostfach   { get => _admPostfach;      set => _admPostfach      = value; }
    public string DPDHLPostfach { get => _admDpdhlPostfach; set => _admDpdhlPostfach = value; }
    public string BDLPostfach   { get => _bdlPostfach;      set => _bdlPostfach      = value; }
}
