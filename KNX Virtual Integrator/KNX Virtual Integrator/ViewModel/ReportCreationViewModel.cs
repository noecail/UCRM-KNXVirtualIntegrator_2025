﻿namespace KNX_Virtual_Integrator.ViewModel;



public partial class MainViewModel
{
    

    private string _pdfPath = "";

    public string PdfPath
    {
        get=>_pdfPath;
        set
        {
            _pdfPath = value;
            WhenPropertyChanged(nameof(PdfPath));
        }
    }
}