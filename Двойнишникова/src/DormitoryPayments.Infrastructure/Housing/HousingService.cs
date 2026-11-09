using DormitoryPayments.Application.Common;
using DormitoryPayments.Application.Housing;
using DormitoryPayments.Application.Security;
using DormitoryPayments.Domain.Entities;
using DormitoryPayments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryPayments.Infrastructure.Housing;

public sealed class HousingService(
    IDbContextFactory<DormitoryPaymentsDbContext> contextFactory,
    IUserSession session) : IHousingService
{
    public async Task<IReadOnlyList<DormitoryRow>> GetDormitoriesAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var term = search?.Trim();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Dormitories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(item => item.Name.Contains(term) || item.Address.Contains(term));
        }

        return await query
            .OrderBy(item => item.Name)
            .Select(item => new DormitoryRow(item.Id, item.Name, item.Address, item.Rooms.Count, item.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<DormitoryInput?> GetDormitoryAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Dormitories.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new DormitoryInput(item.Id, item.Name, item.Address, item.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int> SaveDormitoryAsync(DormitoryInput input, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Address))
        {
            throw new ValidationException("Укажите название и адрес общежития.");
        }

        var name = input.Name.Trim();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        if (await context.Dormitories.AnyAsync(item => item.Name == name && item.Id != input.Id, cancellationToken))
        {
            throw new ValidationException("Общежитие с таким названием уже существует.");
        }

        Dormitory entity;
        var action = "DormitoryCreated";
        if (input.Id is null)
        {
            entity = new Dormitory();
            context.Dormitories.Add(entity);
        }
        else
        {
            entity = await context.Dormitories.SingleOrDefaultAsync(item => item.Id == input.Id, cancellationToken)
                ?? throw new ValidationException("Общежитие не найдено.");
            action = "DormitoryUpdated";
        }
        entity.Name = name;
        entity.Address = input.Address.Trim();
        entity.IsActive = input.IsActive;
        AddAudit(context, action, $"Общежитие: {entity.Name}.");
        await context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task ArchiveDormitoryAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.Dormitories.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new ValidationException("Общежитие не найдено.");
        if (await context.Rooms.AnyAsync(item => item.DormitoryId == id && item.IsActive, cancellationToken))
        {
            throw new ValidationException("Нельзя архивировать общежитие, пока в нём есть активные комнаты.");
        }
        entity.IsActive = false;
        AddAudit(context, "DormitoryArchived", $"Общежитие {entity.Name} переведено в архив.");
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoomRow>> GetRoomsAsync(
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var term = search?.Trim();
        var today = DateOnly.FromDateTime(DateTime.Today);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Rooms.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(item => item.Number.Contains(term) || item.Dormitory.Name.Contains(term));
        }

        return await query
            .OrderBy(item => item.Dormitory.Name)
            .ThenBy(item => item.Number)
            .Select(item => new RoomRow(
                item.Id,
                item.DormitoryId,
                item.Dormitory.Name,
                item.Number,
                item.Capacity,
                item.Accommodations.Count(accommodation => accommodation.StartDate <= today &&
                    (accommodation.EndDate == null || accommodation.EndDate >= today)),
                item.Gender,
                item.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomInput?> GetRoomAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Rooms.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new RoomInput(
                item.Id,
                item.DormitoryId,
                item.Number,
                item.Capacity,
                item.Gender,
                item.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int> SaveRoomAsync(RoomInput input, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        if (input.DormitoryId <= 0 || string.IsNullOrWhiteSpace(input.Number))
        {
            throw new ValidationException("Выберите общежитие и укажите номер комнаты.");
        }
        if (input.Capacity is < 1 or > 20)
        {
            throw new ValidationException("Вместимость комнаты должна быть от 1 до 20 мест.");
        }

        var number = input.Number.Trim();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        if (!await context.Dormitories.AnyAsync(item => item.Id == input.DormitoryId && item.IsActive, cancellationToken))
        {
            throw new ValidationException("Выбранное общежитие недоступно.");
        }
        if (await context.Rooms.AnyAsync(
                item => item.DormitoryId == input.DormitoryId && item.Number == number && item.Id != input.Id,
                cancellationToken))
        {
            throw new ValidationException("Комната с таким номером уже существует в выбранном общежитии.");
        }

        Room entity;
        var action = "RoomCreated";
        if (input.Id is null)
        {
            entity = new Room();
            context.Rooms.Add(entity);
        }
        else
        {
            entity = await context.Rooms.SingleOrDefaultAsync(item => item.Id == input.Id, cancellationToken)
                ?? throw new ValidationException("Комната не найдена.");
            var today = DateOnly.FromDateTime(DateTime.Today);
            var occupied = await context.Accommodations.CountAsync(
                item => item.RoomId == input.Id && item.StartDate <= today &&
                    (item.EndDate == null || item.EndDate >= today),
                cancellationToken);
            if (input.Capacity < occupied)
            {
                throw new ValidationException($"В комнате сейчас проживает {occupied} чел.; уменьшить вместимость нельзя.");
            }
            action = "RoomUpdated";
        }

        entity.DormitoryId = input.DormitoryId;
        entity.Number = number;
        entity.Capacity = input.Capacity;
        entity.Gender = input.Gender;
        entity.IsActive = input.IsActive;
        AddAudit(context, action, $"Комната {number}, вместимость {input.Capacity}.");
        await context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task ArchiveRoomAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.Rooms.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new ValidationException("Комната не найдена.");
        if (await context.Accommodations.AnyAsync(item => item.RoomId == id && item.EndDate == null, cancellationToken))
        {
            throw new ValidationException("Нельзя архивировать комнату, пока в ней проживают студенты.");
        }
        entity.IsActive = false;
        AddAudit(context, "RoomArchived", $"Комната {entity.Number} переведена в архив.");
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccommodationRow>> GetAccommodationsAsync(
        string? search,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var term = search?.Trim();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Accommodations.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(item => item.EndDate == null);
        }
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(item =>
                item.Student.StudentNumber.Contains(term) ||
                item.Student.LastName.Contains(term) ||
                item.Student.FirstName.Contains(term) ||
                item.Room.Number.Contains(term) ||
                item.Room.Dormitory.Name.Contains(term));
        }

        return await query
            .OrderByDescending(item => item.EndDate == null)
            .ThenBy(item => item.Student.LastName)
            .Select(item => new AccommodationRow(
                item.Id,
                item.StudentId,
                item.Student.LastName + " " + item.Student.FirstName,
                item.Student.StudentNumber,
                item.RoomId,
                item.Room.Dormitory.Name,
                item.Room.Number,
                item.StartDate,
                item.EndDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<AccommodationInput?> GetAccommodationAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Accommodations.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new AccommodationInput(
                item.Id,
                item.StudentId,
                item.RoomId,
                item.StartDate,
                item.EndDate))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int> SaveAccommodationAsync(
        AccommodationInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        if (input.StudentId <= 0 || input.RoomId <= 0)
        {
            throw new ValidationException("Выберите студента и комнату.");
        }
        if (input.EndDate < input.StartDate)
        {
            throw new ValidationException("Дата выселения не может быть раньше даты заселения.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var room = await context.Rooms.SingleOrDefaultAsync(
                item => item.Id == input.RoomId && item.IsActive,
                cancellationToken)
            ?? throw new ValidationException("Выбранная комната недоступна.");
        if (!await context.Students.AnyAsync(item => item.Id == input.StudentId && item.IsActive, cancellationToken))
        {
            throw new ValidationException("Выбранный студент недоступен.");
        }

        var intervalEnd = input.EndDate ?? DateOnly.MaxValue;
        var overlapsStudent = await context.Accommodations.AnyAsync(
            item => item.StudentId == input.StudentId && item.Id != input.Id &&
                item.StartDate <= intervalEnd && (item.EndDate == null || item.EndDate >= input.StartDate),
            cancellationToken);
        if (overlapsStudent)
        {
            throw new ValidationException("У студента уже есть заселение, пересекающееся с выбранным периодом.");
        }

        var overlappingResidents = await context.Accommodations.CountAsync(
            item => item.RoomId == input.RoomId && item.Id != input.Id &&
                item.StartDate <= intervalEnd && (item.EndDate == null || item.EndDate >= input.StartDate),
            cancellationToken);
        if (overlappingResidents >= room.Capacity)
        {
            throw new ValidationException("На выбранный период в комнате нет свободных мест.");
        }

        Accommodation entity;
        var action = "AccommodationCreated";
        if (input.Id is null)
        {
            entity = new Accommodation();
            context.Accommodations.Add(entity);
        }
        else
        {
            entity = await context.Accommodations.SingleOrDefaultAsync(item => item.Id == input.Id, cancellationToken)
                ?? throw new ValidationException("Запись о заселении не найдена.");
            action = "AccommodationUpdated";
        }
        entity.StudentId = input.StudentId;
        entity.RoomId = input.RoomId;
        entity.StartDate = input.StartDate;
        entity.EndDate = input.EndDate;
        AddAudit(context, action, $"Заселение студента ID {input.StudentId} в комнату ID {input.RoomId}.");
        await context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task CloseAccommodationAsync(
        int id,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.Accommodations.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new ValidationException("Запись о заселении не найдена.");
        if (entity.EndDate is not null)
        {
            throw new ValidationException("Выселение уже оформлено.");
        }
        if (endDate < entity.StartDate)
        {
            throw new ValidationException("Дата выселения не может быть раньше даты заселения.");
        }
        entity.EndDate = endDate;
        AddAudit(context, "AccommodationClosed", $"Оформлено выселение по записи ID {id}.");
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItem<int>>> GetDormitoryOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Dormitories.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.Name)
            .Select(item => new LookupItem<int>(item.Id, item.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItem<int>>> GetStudentOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Students.AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.LastName)
            .ThenBy(item => item.FirstName)
            .Select(item => new LookupItem<int>(item.Id,
                item.LastName + " " + item.FirstName + " (" + item.StudentNumber + ")"))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItem<int>>> GetRoomOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Rooms.AsNoTracking()
            .Where(item => item.IsActive && item.Dormitory.IsActive)
            .OrderBy(item => item.Dormitory.Name)
            .ThenBy(item => item.Number)
            .Select(item => new LookupItem<int>(item.Id,
                item.Dormitory.Name + ", комната " + item.Number + " (мест: " + item.Capacity + ")"))
            .ToListAsync(cancellationToken);
    }

    private void EnsureCanManage()
    {
        if (!session.HasPermission(Permission.ManageHousing))
        {
            throw new UnauthorizedAccessException("Недостаточно прав для изменения жилого фонда.");
        }
    }

    private void AddAudit(DormitoryPaymentsDbContext context, string action, string details) =>
        context.AuditLogs.Add(new AuditLog
        {
            UserId = session.CurrentUser?.Id,
            TimestampUtc = DateTime.UtcNow,
            Level = "Information",
            Action = action,
            Details = details
        });
}
