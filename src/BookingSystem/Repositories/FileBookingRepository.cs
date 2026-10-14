using System.Text.Json;
using BookingSystem.Models;

namespace BookingSystem.Repositories;

public class FileBookingRepository(string filePath) : IBookingRepository
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    private readonly List<Booking> _bookings = Load(filePath);

    public IReadOnlyList<Booking> GetAll() => _bookings;

    public void Add(Booking booking) => _bookings.Add(booking);

    public void Save()
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_bookings, _options);
        File.WriteAllText(filePath, json);
    }

    private static List<Booking> Load(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Booking>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
