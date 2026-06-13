using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Utilities;
using TeacherFind.Mobile.Features.Teachers.Models;

namespace TeacherFind.Mobile.Features.Teachers.ViewModels;

public class TeacherListViewModel : BindableObject
{
    private readonly IApiService _apiService;
    private bool _isBusy;

    public ObservableCollection<TeacherCardModel> Teachers { get; set; }

    // Liste güncellendiğinde arayüzdeki sayının da değişmesi için ufak bir tetikleyici ekledik
    public string TotalCountText => $"{Teachers?.Count ?? 0} Eğitmen Listeleniyor";

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public TeacherListViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Teachers = new ObservableCollection<TeacherCardModel>();
    }

    // Arayüz yüklendiğinde tetiklenecek metodumuz
    public async Task LoadTeachersAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Teachers.Clear();

            var apiTeachers = await _apiService.GetAsync<PagedResultDto<TutorListItemDto>>(
                "api/tutors?page=1&pageSize=20&sort=rating");

            if (apiTeachers?.Items != null && apiTeachers.Items.Any())
            {
                foreach (var teacher in apiTeachers.Items)
                {
                    Teachers.Add(new TeacherCardModel
                    {
                        Id = teacher.Id,
                        Name = DisplayValueHelper.ToPlainText(teacher.TeacherName),
                        AvatarUrl = DisplayValueHelper.ResolveTutorImageUrl(
                            _apiService,
                            teacher.Photos),
                        Description = DisplayValueHelper.TruncatePlainText(teacher.Description, 140),
                        Rating = teacher.Rating.ToString("0.0"),
                        ReviewCount = $"({teacher.ReviewCount} Değerlendirme)",
                        Price = $"₺{teacher.Price:N0}/saat",
                        IsOnline = true,
                        Title = DisplayValueHelper.ToPlainText(teacher.Title),

                        Badges = new ObservableCollection<BadgeModel>
                        {
                            new BadgeModel { Text = DisplayValueHelper.ToPlainText(teacher.ServiceType) },
                            new BadgeModel { Text = DisplayValueHelper.ToPlainText(teacher.Subject ?? "Ders") }
                        }
                    });
                }
            }

            // Veriler dolduktan sonra "X Eğitmen Listeleniyor" yazısını güncelliyoruz
            OnPropertyChanged(nameof(TotalCountText));
        }
        catch (Exception ex)
        {
            // İleride hata mesajı basılabilir
        }
        finally
        {
            IsBusy = false;
        }
    }
}
