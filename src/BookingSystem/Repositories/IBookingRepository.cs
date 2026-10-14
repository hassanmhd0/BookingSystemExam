using BookingSystem.Models;

namespace BookingSystem.Repositories;

public interface IBookingRepository
{
    IReadOnlyList<Booking> GetAll();
    void Add(Booking booking);
    void Save();
}
