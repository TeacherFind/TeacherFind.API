using System;

namespace TeacherFind.Mobile.Features.Profile.Models;

public class ProfileMenuItemModel
{
    public string Id { get; set; }        // Menüyü ayırt etmek için benzersiz kimlik (Örn: "Panel", "IlanVer")
    public string Title { get; set; }     // Ekranda yazacak metin
    public string Icon { get; set; }      // Solundaki ikon (Emoji)
    public Type TargetPage { get; set; }  // Tıklandığında açılacak sayfanın C# tipi
}