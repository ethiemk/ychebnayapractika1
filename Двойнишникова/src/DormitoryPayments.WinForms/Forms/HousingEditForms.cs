using DormitoryPayments.Application.Common;
using DormitoryPayments.Application.Housing;
using DormitoryPayments.Domain.Enums;
using DormitoryPayments.WinForms.UI;

namespace DormitoryPayments.WinForms.Forms;

internal sealed class DormitoryEditForm : EditDialogBase
{
    private readonly IHousingService _service;
    private readonly DormitoryInput _model;
    private readonly TextBox _name;
    private readonly TextBox _address;
    private readonly CheckBox _isActive;

    public DormitoryEditForm(IHousingService service, DormitoryInput? model)
        : base(
            model?.Id is null ? "Новое общежитие" : "Общежитие",
            "Основные сведения об объекте жилого фонда",
            new Size(650, 460))
    {
        _service = service;
        _model = model ?? new DormitoryInput(null, "", "");

        BodyPanel.Controls.Add(FieldLabel("Название *", 28, 24, 568));
        _name = TextField(28, 47, 568, _model.Name);
        BodyPanel.Controls.Add(_name);
        BodyPanel.Controls.Add(FieldLabel("Адрес *", 28, 111, 568));
        _address = TextField(28, 134, 568, _model.Address);
        BodyPanel.Controls.Add(_address);
        _isActive = new CheckBox
        {
            Text = "Общежитие доступно для работы",
            Checked = _model.IsActive,
            AutoSize = true,
            Location = new Point(30, 205),
            ForeColor = Theme.Text,
            Visible = _model.Id is not null
        };
        BodyPanel.Controls.Add(_isActive);
        SaveButton.Click += async (_, _) => await RunSaveAsync(() => _service.SaveDormitoryAsync(
            new DormitoryInput(_model.Id, _name.Text, _address.Text, _model.Id is null || _isActive.Checked)));
    }
}

internal sealed class RoomEditForm : EditDialogBase
{
    private readonly IHousingService _service;
    private readonly RoomInput _model;
    private readonly ComboBox _dormitory;
    private readonly TextBox _number;
    private readonly NumericUpDown _capacity;
    private readonly ComboBox _gender;
    private readonly CheckBox _isActive;

    public RoomEditForm(IHousingService service, RoomInput? model)
        : base(
            model?.Id is null ? "Новая комната" : "Комната",
            "Параметры комнаты и допустимое размещение",
            new Size(650, 520))
    {
        _service = service;
        _model = model ?? new RoomInput(null, 0, "", 2, RoomGender.Any);

        BodyPanel.Controls.Add(FieldLabel("Общежитие *", 28, 24, 568));
        _dormitory = ComboField(28, 47, 568);
        BodyPanel.Controls.Add(_dormitory);
        BodyPanel.Controls.Add(FieldLabel("Номер комнаты *", 28, 111, 270));
        _number = TextField(28, 134, 270, _model.Number);
        BodyPanel.Controls.Add(_number);
        BodyPanel.Controls.Add(FieldLabel("Количество мест *", 326, 111, 270));
        _capacity = new NumericUpDown
        {
            Location = new Point(326, 134),
            Size = new Size(270, 34),
            Minimum = 1,
            Maximum = 20,
            Value = Math.Clamp(_model.Capacity, 1, 20),
            Font = Theme.Font(10.5f)
        };
        BodyPanel.Controls.Add(_capacity);
        BodyPanel.Controls.Add(FieldLabel("Назначение комнаты", 28, 198, 568));
        _gender = ComboField(28, 221, 568);
        _gender.DataSource = new[]
        {
            new LookupItem<RoomGender>(RoomGender.Any, "Без ограничения"),
            new LookupItem<RoomGender>(RoomGender.Male, "Мужская"),
            new LookupItem<RoomGender>(RoomGender.Female, "Женская")
        };
        BodyPanel.Controls.Add(_gender);
        _isActive = new CheckBox
        {
            Text = "Комната доступна для заселения",
            Checked = _model.IsActive,
            AutoSize = true,
            Location = new Point(30, 286),
            ForeColor = Theme.Text,
            Visible = _model.Id is not null
        };
        BodyPanel.Controls.Add(_isActive);

        Load += async (_, _) =>
        {
            SaveButton.Enabled = false;
            var items = await _service.GetDormitoryOptionsAsync();
            _dormitory.DataSource = items.ToList();
            _dormitory.SelectedItem = items.FirstOrDefault(item => item.Id == _model.DormitoryId);
            _gender.SelectedItem = ((IEnumerable<LookupItem<RoomGender>>)_gender.DataSource)
                .First(item => item.Id == _model.Gender);
            SaveButton.Enabled = true;
        };
        SaveButton.Click += async (_, _) => await RunSaveAsync(SaveAsync);
    }

    private Task SaveAsync()
    {
        if (_dormitory.SelectedItem is not LookupItem<int> dormitory ||
            _gender.SelectedItem is not LookupItem<RoomGender> gender)
        {
            throw new InvalidOperationException("Выберите общежитие и назначение комнаты.");
        }
        return _service.SaveRoomAsync(new RoomInput(
            _model.Id,
            dormitory.Id,
            _number.Text,
            (int)_capacity.Value,
            gender.Id,
            _model.Id is null || _isActive.Checked));
    }
}

internal sealed class AccommodationEditForm : EditDialogBase
{
    private readonly IHousingService _service;
    private readonly AccommodationInput _model;
    private readonly ComboBox _student;
    private readonly ComboBox _room;
    private readonly DateTimePicker _startDate;
    private readonly DateTimePicker _endDate;
    private readonly CheckBox _hasEndDate;

    public AccommodationEditForm(IHousingService service, AccommodationInput? model)
        : base(
            model?.Id is null ? "Новое заселение" : "Период проживания",
            "Студент может одновременно проживать только в одной комнате",
            new Size(690, 570))
    {
        _service = service;
        var today = DateOnly.FromDateTime(DateTime.Today);
        _model = model ?? new AccommodationInput(null, 0, 0, today, null);

        BodyPanel.Controls.Add(FieldLabel("Студент *", 28, 24, 608));
        _student = ComboField(28, 47, 608);
        BodyPanel.Controls.Add(_student);
        BodyPanel.Controls.Add(FieldLabel("Комната *", 28, 111, 608));
        _room = ComboField(28, 134, 608);
        BodyPanel.Controls.Add(_room);
        BodyPanel.Controls.Add(FieldLabel("Дата заселения *", 28, 198, 290));
        _startDate = DateField(28, 221, 290, _model.StartDate);
        BodyPanel.Controls.Add(_startDate);
        BodyPanel.Controls.Add(FieldLabel("Дата выселения", 346, 198, 290));
        _endDate = DateField(346, 221, 290, _model.EndDate ?? today);
        _endDate.Enabled = _model.EndDate is not null;
        BodyPanel.Controls.Add(_endDate);
        _hasEndDate = new CheckBox
        {
            Text = "Выселение уже оформлено",
            Checked = _model.EndDate is not null,
            AutoSize = true,
            Location = new Point(348, 267),
            ForeColor = Theme.Text
        };
        _hasEndDate.CheckedChanged += (_, _) => _endDate.Enabled = _hasEndDate.Checked;
        BodyPanel.Controls.Add(_hasEndDate);

        var note = new RoundedPanel
        {
            Location = new Point(28, 315),
            Size = new Size(608, 72),
            CornerRadius = 12,
            BorderWidth = 0,
            BackColor = Theme.AccentSoft,
            Padding = new Padding(16)
        };
        note.Controls.Add(new Label
        {
            Text = "Перед сохранением система проверит пересечение периодов\nи наличие свободного места в комнате.",
            Dock = DockStyle.Fill,
            ForeColor = Theme.Success,
            BackColor = Color.Transparent,
            Font = Theme.Font(9),
            TextAlign = ContentAlignment.MiddleLeft
        });
        BodyPanel.Controls.Add(note);

        Load += async (_, _) =>
        {
            SaveButton.Enabled = false;
            var studentsTask = _service.GetStudentOptionsAsync();
            var roomsTask = _service.GetRoomOptionsAsync();
            await Task.WhenAll(studentsTask, roomsTask);
            var students = await studentsTask;
            var rooms = await roomsTask;
            _student.DataSource = students.ToList();
            _room.DataSource = rooms.ToList();
            _student.SelectedItem = students.FirstOrDefault(item => item.Id == _model.StudentId);
            _room.SelectedItem = rooms.FirstOrDefault(item => item.Id == _model.RoomId);
            SaveButton.Enabled = true;
        };
        SaveButton.Click += async (_, _) => await RunSaveAsync(SaveAsync);
    }

    private Task SaveAsync()
    {
        if (_student.SelectedItem is not LookupItem<int> student ||
            _room.SelectedItem is not LookupItem<int> room)
        {
            throw new InvalidOperationException("Выберите студента и комнату.");
        }
        return _service.SaveAccommodationAsync(new AccommodationInput(
            _model.Id,
            student.Id,
            room.Id,
            DateOnly.FromDateTime(_startDate.Value),
            _hasEndDate.Checked ? DateOnly.FromDateTime(_endDate.Value) : null));
    }
}
