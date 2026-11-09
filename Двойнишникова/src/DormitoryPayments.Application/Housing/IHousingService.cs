using DormitoryPayments.Application.Common;

namespace DormitoryPayments.Application.Housing;

public interface IHousingService
{
    Task<IReadOnlyList<DormitoryRow>> GetDormitoriesAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<DormitoryInput?> GetDormitoryAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveDormitoryAsync(DormitoryInput input, CancellationToken cancellationToken = default);

    Task ArchiveDormitoryAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomRow>> GetRoomsAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task<RoomInput?> GetRoomAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveRoomAsync(RoomInput input, CancellationToken cancellationToken = default);

    Task ArchiveRoomAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccommodationRow>> GetAccommodationsAsync(
        string? search,
        bool activeOnly,
        CancellationToken cancellationToken = default);

    Task<AccommodationInput?> GetAccommodationAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveAccommodationAsync(
        AccommodationInput input,
        CancellationToken cancellationToken = default);

    Task CloseAccommodationAsync(
        int id,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItem<int>>> GetDormitoryOptionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItem<int>>> GetStudentOptionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItem<int>>> GetRoomOptionsAsync(CancellationToken cancellationToken = default);
}
