using LateCat.CefPlayer;
using System.Globalization;
using System.IO;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LateCat", "cef", "logfile.txt"));
        }
        catch
        {

        }

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DefaultForm());
    }
}