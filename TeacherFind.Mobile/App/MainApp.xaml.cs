using TeacherFind.Mobile.Shell;

namespace TeacherFind.Mobile; // Namespace BU OLMALI

public partial class MainApp : Application // Sýnýf adý BU OLMALI
{
    public MainApp()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}