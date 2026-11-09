namespace DormitoryPayments.Application.Common;

public sealed record LookupItem<T>(T Id, string DisplayName)
{
    public override string ToString() => DisplayName;
}
