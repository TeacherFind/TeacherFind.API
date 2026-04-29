using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Reports;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Reports;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
        => _reportRepository = reportRepository;

    public async Task CreateReportAsync(Guid reporterId, CreateReportDto dto)
    {
        if (dto.TargetListingId == null && dto.TargetUserId == null)
            throw new Exception("Şikayet bir ilana veya kullanıcıya yönelik olmalıdır.");

        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new Exception("Şikayet sebebi boş olamaz.");

        var report = new Report
        {
            ReporterId = reporterId,
            TargetListingId = dto.TargetListingId,
            TargetUserId = dto.TargetUserId,
            Reason = dto.Reason.Trim(),
            Description = dto.Description?.Trim()
        };

        await _reportRepository.AddAsync(report);
        await _reportRepository.SaveChangesAsync();
    }

    public async Task<List<ReportDto>> GetAllReportsAsync()
    {
        var reports = await _reportRepository.GetAllAsync();

        return reports.Select(r => new ReportDto
        {
            Id = r.Id,
            ReporterId = r.ReporterId,
            ReporterName = r.Reporter?.FullName ?? "Bilinmiyor",
            TargetListingId = r.TargetListingId,
            TargetListingTitle = r.TargetListing?.Title,
            TargetUserId = r.TargetUserId,
            TargetUserName = r.TargetUser?.FullName,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.Status,
            AdminNote = r.AdminNote,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt
        }).ToList();
    }

    public async Task<bool> ResolveReportAsync(Guid reportId, ResolveReportDto dto)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null) return false;

        if (dto.Status != "Resolved" && dto.Status != "Dismissed")
            throw new Exception("Geçersiz durum. Sadece 'Resolved' veya 'Dismissed' kabul edilir.");

        report.Status = dto.Status;
        report.AdminNote = dto.AdminNote?.Trim();
        report.ResolvedAt = DateTime.UtcNow;

        await _reportRepository.SaveChangesAsync();
        return true;
    }
}