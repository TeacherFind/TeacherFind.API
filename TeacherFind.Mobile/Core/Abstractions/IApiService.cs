using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Mobile.Core.Abstractions
{
    public interface IApiService
    {
        // VERİTABANINDAN VERİ ÇEKMEK İÇİN GEREKEN METOT TANIMI
        // api/teachers gibi bir endpoint yolu alır ve geriye User listesi döner.
        Task<T> GetAsync<T>(string endpoint);

    }
}
