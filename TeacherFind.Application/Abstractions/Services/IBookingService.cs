using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Bookings;

namespace TeacherFind.Application.Abstractions.Services;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(
        Guid studentUserId,
        CreateBookingRequestDto request);

    Task<List<BookingDto>> GetMyBookingsAsync(
        Guid currentUserId);

    Task<List<BookingDto>> GetTutorBookingsAsync(
        Guid tutorUserId);

    Task<bool> ApproveAsync(
        Guid bookingId,
        Guid tutorUserId);

    Task<bool> RejectAsync(
        Guid bookingId,
        Guid tutorUserId,
        RejectBookingRequestDto request);

    Task<bool> CancelAsync(
        Guid bookingId,
        Guid currentUserId,
        CancelBookingRequestDto request);

    Task<bool> CompleteAsync(
        Guid bookingId,
        Guid tutorUserId);
}