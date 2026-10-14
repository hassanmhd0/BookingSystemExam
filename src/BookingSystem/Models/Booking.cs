namespace BookingSystem.Models;

public class Booking
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Participants { get; set; }
    public bool IsCancelled { get; set; }
}
