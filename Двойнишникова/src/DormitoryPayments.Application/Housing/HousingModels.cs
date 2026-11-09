using DormitoryPayments.Domain.Enums;

namespace DormitoryPayments.Application.Housing;

public sealed record DormitoryRow(
    int Id,
    string Name,
    string Address,
    int RoomCount,
    bool IsActive);

public sealed record DormitoryInput(
    int? Id,
    string Name,
    string Address,
    bool IsActive = true);

public sealed record RoomRow(
    int Id,
    int DormitoryId,
    string DormitoryName,
    string Number,
    int Capacity,
    int OccupiedPlaces,
    RoomGender Gender,
    bool IsActive)
{
    public string Occupancy => $"{OccupiedPlaces} из {Capacity}";
}

public sealed record RoomInput(
    int? Id,
    int DormitoryId,
    string Number,
    int Capacity,
    RoomGender Gender,
    bool IsActive = true);

public sealed record AccommodationRow(
    int Id,
    int StudentId,
    string StudentName,
    string StudentNumber,
    int RoomId,
    string DormitoryName,
    string RoomNumber,
    DateOnly StartDate,
    DateOnly? EndDate)
{
    public string Status => EndDate is null ? "Проживает" : "Выселен";
}

public sealed record AccommodationInput(
    int? Id,
    int StudentId,
    int RoomId,
    DateOnly StartDate,
    DateOnly? EndDate);
