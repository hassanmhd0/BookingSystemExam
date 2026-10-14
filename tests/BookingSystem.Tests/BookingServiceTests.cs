using BookingSystem.Models;
using BookingSystem.Services;

namespace BookingSystem.Tests;

public class BookingServiceTests
{
    [Fact]
    public void Create_WhenCustomerNameIsEmpty_ReturnsFailure()
    {
        var service = new BookingService(new InMemoryBookingRepository());

        var result = service.Create(CreateBooking(customerName: ""));

        Assert.False(result.IsSuccessful);
        Assert.Equal("Customer name is required.", result.Message);
    }

    [Fact]
    public void Create_WhenStartTimeIsAfterEndTime_ReturnsFailure()
    {
        var service = new BookingService(new InMemoryBookingRepository());
        var booking = CreateBooking(
            startTime: new TimeOnly(14, 0),
            endTime: new TimeOnly(12, 0));

        var result = service.Create(booking);

        Assert.False(result.IsSuccessful);
        Assert.Equal(
            "The start time must be earlier than the end time.",
            result.Message);
    }

    [Fact]
    public void Create_WhenParticipantCountExceedsCapacity_ReturnsFailure()
    {
        var service = new BookingService(new InMemoryBookingRepository());

        var result = service.Create(
            CreateBooking(roomName: "Aspen", participants: 5));

        Assert.False(result.IsSuccessful);
    }

    [Theory]
    [InlineData(8, 0, 10, 0)]
    [InlineData(10, 0, 12, 0)]
    [InlineData(9, 30, 10, 30)]
    [InlineData(8, 0, 12, 0)]
    public void Create_WhenBookingOverlapsExistingBooking_ReturnsFailure(
        int startHour,
        int startMinute,
        int endHour,
        int endMinute)
    {
        var existingBooking = CreateBooking(
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(11, 0));
        var service = new BookingService(
            new InMemoryBookingRepository(existingBooking));
        var newBooking = CreateBooking(
            startTime: new TimeOnly(startHour, startMinute),
            endTime: new TimeOnly(endHour, endMinute));

        var result = service.Create(newBooking);

        Assert.False(result.IsSuccessful);
        Assert.Equal(
            "The room is already booked during the selected time.",
            result.Message);
    }

    [Fact]
    public void Create_WhenBookingStartsWhenExistingBookingEnds_ReturnsSuccess()
    {
        var existingBooking = CreateBooking(
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(11, 0));
        var service = new BookingService(
            new InMemoryBookingRepository(existingBooking));
        var newBooking = CreateBooking(
            startTime: new TimeOnly(11, 0),
            endTime: new TimeOnly(12, 0));

        var result = service.Create(newBooking);

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Create_WhenBookingIsValid_AddsAndSavesBooking()
    {
        var repository = new InMemoryBookingRepository();
        var service = new BookingService(repository);

        var result = service.Create(CreateBooking());

        Assert.True(result.IsSuccessful);
        Assert.Single(repository.GetAll());
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public void Search_WhenSearchTermHasDifferentCasing_ReturnsBooking()
    {
        var service = new BookingService(
            new InMemoryBookingRepository(CreateBooking()));

        var result = service.Search("avanoria");

        Assert.Single(result);
        Assert.Equal("Avanoria AB", result[0].CustomerName);
    }

    [Fact]
    public void Search_WhenBookingIsCancelled_DoesNotReturnBooking()
    {
        var cancelledBooking = CreateBooking();
        cancelledBooking.IsCancelled = true;
        var service = new BookingService(
            new InMemoryBookingRepository(cancelledBooking));

        var result = service.Search("Avanoria");

        Assert.Empty(result);
    }

    [Fact]
    public void GetActiveBookings_ReturnsOnlyActiveBookingsSortedByDateAndStartTime()
    {
        var lateBooking = CreateBooking(
            customerName: "Late booking",
            date: new DateOnly(2026, 10, 21),
            startTime: new TimeOnly(13, 0),
            endTime: new TimeOnly(14, 0));
        var firstBooking = CreateBooking(
            customerName: "First booking",
            date: new DateOnly(2026, 10, 20),
            startTime: new TimeOnly(9, 0),
            endTime: new TimeOnly(10, 0));
        var secondBooking = CreateBooking(
            customerName: "Second booking",
            date: new DateOnly(2026, 10, 20),
            startTime: new TimeOnly(11, 0),
            endTime: new TimeOnly(12, 0));
        var cancelledBooking = CreateBooking(
            customerName: "Cancelled booking",
            date: new DateOnly(2026, 10, 19));
        cancelledBooking.IsCancelled = true;
        var service = new BookingService(
            new InMemoryBookingRepository(
                lateBooking,
                secondBooking,
                cancelledBooking,
                firstBooking));

        var result = service.GetActiveBookings();

        Assert.Collection(
            result,
            booking => Assert.Equal("First booking", booking.CustomerName),
            booking => Assert.Equal("Second booking", booking.CustomerName),
            booking => Assert.Equal("Late booking", booking.CustomerName));
    }

    [Fact]
    public void Cancel_WhenBookingExists_CancelsAndSavesBooking()
    {
        var booking = CreateBooking(id: 1042);
        var repository = new InMemoryBookingRepository(booking);
        var service = new BookingService(repository);

        var result = service.Cancel(1042);

        Assert.True(result.IsSuccessful);
        Assert.True(booking.IsCancelled);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public void Cancel_WhenBookingDoesNotExist_ReturnsFailureWithoutSaving()
    {
        var repository = new InMemoryBookingRepository(CreateBooking(id: 1042));
        var service = new BookingService(repository);

        var result = service.Cancel(9999);

        Assert.False(result.IsSuccessful);
        Assert.Equal(0, repository.SaveCount);
    }

    private static Booking CreateBooking(
        int id = 0,
        string customerName = "Avanoria AB",
        string roomName = "Aspen",
        int participants = 4,
        DateOnly? date = null,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null)
    {
        return new Booking
        {
            Id = id,
            CustomerName = customerName,
            RoomName = roomName,
            Date = date ?? new DateOnly(2026, 10, 20),
            StartTime = startTime ?? new TimeOnly(9, 0),
            EndTime = endTime ?? new TimeOnly(11, 0),
            Participants = participants
        };
    }
}
