using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Shared.Components;

namespace TeacherFind.Mobile;

public partial class MainApp : global::Microsoft.Maui.Controls.Application
{
    public MainApp(MainShellPage mainShellPage)
    {
        InitializeComponent();

        // Tertemiz menülü omurgayý uygulamanýn kalbine koyduk
        MainPage = mainShellPage;
    }
}