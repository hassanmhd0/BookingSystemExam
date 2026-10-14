namespace BookingSystem.Services;

public record BookingResult(bool IsSuccessful, string Message)
{
    public static BookingResult Success(string message) => new(true, message);
    public static BookingResult Failure(string message) => new(false, message);
}
