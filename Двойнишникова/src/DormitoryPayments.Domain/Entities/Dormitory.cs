namespace DormitoryPayments.Domain.Entities;

public sealed class Dormitory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    public override string ToString() => Name;
}
