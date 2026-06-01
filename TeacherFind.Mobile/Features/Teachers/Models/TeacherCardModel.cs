using System.Collections.ObjectModel;

namespace TeacherFind.Mobile.Features.Teachers.Models;

// Eğitmen kartındaki küçük etiketler için (Örn: Çevrimiçi, Yüz Yüze)
public class BadgeModel
{
    public string Text { get; set; }
}

public class TeacherCardModel
{
    public string AvatarUrl { get; set; }
    public string Name { get; set; }
    public string Rating { get; set; }
    public string ReviewCount { get; set; }
    public string Price { get; set; }
    public bool IsOnline { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ObservableCollection<BadgeModel> Badges { get; set; }
}