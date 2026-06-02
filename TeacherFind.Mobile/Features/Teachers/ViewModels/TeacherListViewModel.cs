using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Features.Teachers.Models;
using TeacherFind.Domain.Entities; // Veritabanı User/Teacher entity'niz

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

            // 1. GERÇEK API'DEN VERİYİ ÇEKİYORUZ
            var apiTeachers = await _apiService.GetAsync<List<User>>("api/teachers");

            if (apiTeachers != null && apiTeachers.Any())
            {
                // 2. VERİTABANINDAN GELEN GERÇEK VERİYİ ARAYÜZE BAĞLIYORUZ
                foreach (var teacher in apiTeachers)
                {
                    Teachers.Add(new TeacherCardModel
                    {
                        // ARTIK DOĞRUDAN VERİTABANINDAKİ SÜTUNLARI ÇEKİYORUZ
                        Name = teacher.FullName ?? "İsimsiz Eğitmen",
                        AvatarUrl = teacher.ProfileImageUrl ?? "dotnet_bot.png", // Veritabanında resim yoksa varsayılan resim
                        Description = teacher.Bio ?? "Eğitmen açıklaması bulunmuyor.", // Biyografiyi de bağladık!

                        // NOT: Puan, Fiyat ve Etiketler senin 'User' sınıfında değil, 
                        // muhtemelen TeacherProfile veya TeacherListings tablosunda tutuluyor.
                        // Onları API'den "Include" ile çektiğinde buraya bağlayacağız. Şimdilik tasarımsal olarak kalıyorlar:
                        Rating = "4.8",
                        ReviewCount = "(12 Değerlendirme)",
                        Price = "₺5000/saat",
                        IsOnline = teacher.IsActive, // Çevrimiçi durumunu aktiflikten aldık
                        Title = "Uzman Eğitmen",

                        Badges = new ObservableCollection<BadgeModel>
                        {
                            new BadgeModel { Text = "Çevrimiçi" },
                            new BadgeModel { Text = "Matematik" }
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