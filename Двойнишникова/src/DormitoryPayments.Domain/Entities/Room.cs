using DormitoryPayments.Domain.Enums;

namespace DormitoryPayments.Domain.Entities;

public sealed class Room
{
    public int Id { get; set; }
    public int DormitoryId { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public RoomGender Gender { get; set; }
    public bool IsActive { get; set; } = true;

    public Dormitory Dormitory { get; set; } = null!;
    public ICollection<Accommodation> Accommodations { get; set; } = new List<Accommodation>();

    public override string ToString() => $"{Dormitory?.Name}, комната {Number}";
}
