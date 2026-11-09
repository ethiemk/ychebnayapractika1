namespace DormitoryPayments.Domain.Entities;

public sealed class Accommodation
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int RoomId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public Student Student { get; set; } = null!;
    public Room Room { get; set; } = null!;

    public bool IsActiveOn(DateOnly date) => StartDate <= date && (EndDate is null || EndDate >= date);
}
