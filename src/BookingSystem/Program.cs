using BookingSystem.ConsoleUI;
using BookingSystem.Repositories;
using BookingSystem.Services;

var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "bookings.json");
var repository = new FileBookingRepository(dataPath);
var service = new BookingService(repository);
var console = new BookingConsole(service);

console.Run();
