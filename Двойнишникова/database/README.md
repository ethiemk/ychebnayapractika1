# База данных DormitoryPaymentsDb

## Установка через SQL Server Management Studio

1. Подключитесь к Microsoft SQL Server 2019 или новее.
2. Откройте и выполните `01_schema.sql`. Скрипт создаст базу `DormitoryPaymentsDb`, таблицы, ключи, индексы и ограничения.
3. Откройте и выполните `02_seed.sql`. Скрипт добавит роли, демонстрационных пользователей, студентов, комнаты, начисления и платежи.
4. При необходимости измените строку `ConnectionStrings:DefaultConnection` в `appsettings.json`.

Оба скрипта идемпотентны: повторный запуск не создаёт дублирующиеся записи.

## Демонстрационные учётные записи

| Роль | Логин | Пароль |
|---|---|---|
| Администратор | `admin` | `Admin123!` |
| Бухгалтер | `accountant` | `Buh123!` |
| Комендант | `commandant` | `Kom123!` |
| Просмотр | `viewer` | `View123!` |

Пароли хранятся в базе только как PBKDF2-SHA256-хэши с индивидуальной солью и 210 000 итераций.

## Работа с миграциями

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update `
  --project "src\DormitoryPayments.Infrastructure\DormitoryPayments.Infrastructure.csproj" `
  --startup-project "src\DormitoryPayments.Infrastructure\DormitoryPayments.Infrastructure.csproj"
```

Строку подключения можно переопределить переменной среды `DORMITORY_DB_CONNECTION`.
