namespace KNX_Virtual_Integrator.ViewModel;



public partial class MainViewModel
{
    /// <summary>
    /// The file path to which the pdf analysis report should be saved.
    /// </summary>
    private string _pdfPath = "";

    /// <summary>
    /// Gets or sets the file path to which the pdf analysis report should be saved.
    /// </summary>
    public string PdfPath
    {
        get=>_pdfPath;
        set
        {
            _pdfPath = value;
            WhenPropertyChanged(nameof(PdfPath));
        }
    }
    
    /// <summary>
    /// The name of the author of the Pdf.
    /// </summary>
    private string _authorName = "";
    
    /// <summary>
    /// Gets or sets the name of the author of the Pdf.
    /// </summary>
    public string AuthorName
    {
        get => _authorName;
        set
        {
            _authorName = value;
            WhenPropertyChanged(nameof(AuthorName));
        }
    }
}