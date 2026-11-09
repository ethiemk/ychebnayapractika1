using System.ComponentModel;
using DormitoryPayments.Application.Housing;
using DormitoryPayments.Application.Security;
using DormitoryPayments.Domain.Enums;
using DormitoryPayments.WinForms.Forms;
using DormitoryPayments.WinForms.UI;

namespace DormitoryPayments.WinForms.Views;

internal sealed class HousingView : UserControl
{
    private enum Section
    {
        Dormitories,
        Rooms,
        Accommodations
    }

    private readonly IHousingService _service;
    private readonly bool _canManage;
    private readonly DataGridView _grid = new();
    private readonly TextBox _search = new();
    private readonly CheckBox _filterCheckBox;
    private readonly Label _summary;
    private readonly RoundedButton _addButton;
    private readonly RoundedButton _editButton;
    private readonly RoundedButton _archiveButton;
    private readonly Dictionary<Section, RoundedButton> _sectionButtons = [];
    private readonly System.Windows.Forms.Timer _searchTimer;
    private Section _section = Section.Dormitories;

    public HousingView(IHousingService service, IUserSession session)
    {
        _service = service;
        _canManage = session.HasPermission(Permission.ManageHousing);
        Dock = DockStyle.Fill;
        BackColor = Theme.Canvas;

        var card = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            CornerRadius = 16,
            BorderColor = Theme.Border,
            Padding = Padding.Empty
        };
        Controls.Add(card);

        var sections = new Panel
        {
            Dock = DockStyle.Top,
            Height = 66,
            BackColor = Theme.Surface,
            Padding = new Padding(22, 13, 22, 11)
        };
        card.Controls.Add(sections);
        var sectionFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 520,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Theme.Surface
        };
        sections.Controls.Add(sectionFlow);
        AddSectionButton(sectionFlow, Section.Dormitories, "Общежития");
        AddSectionButton(sectionFlow, Section.Rooms, "Комнаты");
        AddSectionButton(sectionFlow, Section.Accommodations, "Заселения");

        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = Color.FromArgb(251, 252, 254),
            Padding = new Padding(22, 15, 22, 15)
        };
        card.Controls.Add(toolbar);
        toolbar.BringToFront();
        var searchPanel = UiFactory.CreateSearchPanel(_search, 250);
        searchPanel.Location = new Point(22, 15);
        toolbar.Controls.Add(searchPanel);
        _filterCheckBox = new CheckBox
        {
            Text = "Показывать архив",
            AutoSize = true,
            Location = new Point(290, 26),
            ForeColor = Theme.MutedText,
            Font = Theme.Font(9)
        };
        _filterCheckBox.CheckedChanged += async (_, _) => await LoadDataAsync();
        toolbar.Controls.Add(_filterCheckBox);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = _canManage ? 420 : 100,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = toolbar.BackColor
        };
        toolbar.Controls.Add(actions);
        var refresh = UiFactory.CreateSecondaryButton("Обновить", 90);
        refresh.Click += async (_, _) => await LoadDataAsync();
        actions.Controls.Add(refresh);
        _archiveButton = UiFactory.CreateDangerButton("В архив", 90);
        _archiveButton.Visible = _canManage;
        _archiveButton.Click += ArchiveButton_Click;
        actions.Controls.Add(_archiveButton);
        _editButton = UiFactory.CreateSecondaryButton("Изменить", 90);
        _editButton.Visible = _canManage;
        _editButton.Click += EditButton_Click;
        actions.Controls.Add(_editButton);
        _addButton = UiFactory.CreatePrimaryButton("+  Добавить", 112);
        _addButton.Visible = _canManage;
        _addButton.Click += AddButton_Click;
        actions.Controls.Add(_addButton);

        Theme.ConfigureGrid(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.CellDoubleClick += (_, eventArgs) =>
        {
            if (_canManage && eventArgs.RowIndex >= 0)
            {
                EditButton_Click(this, EventArgs.Empty);
            }
        };
        card.Controls.Add(_grid);
        _grid.BringToFront();

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            BackColor = Theme.Surface,
            Padding = new Padding(22, 10, 22, 10)
        };
        _summary = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Theme.MutedText,
            Font = Theme.Font(9)
        };
        footer.Controls.Add(_summary);
        card.Controls.Add(footer);
        footer.BringToFront();

        _searchTimer = new System.Windows.Forms.Timer { Interval = 350 };
        _search.TextChanged += (_, _) =>
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        };
        _searchTimer.Tick += async (_, _) =>
        {
            _searchTimer.Stop();
            await LoadDataAsync();
        };

        UpdateSectionAppearance();
        ConfigureColumns();
        Load += async (_, _) => await LoadDataAsync();
    }

    private void AddSectionButton(FlowLayoutPanel host, Section section, string text)
    {
        var button = UiFactory.CreateSecondaryButton(text, 144);
        button.Tag = section;
        button.Click += async (_, _) =>
        {
            _section = section;
            UpdateSectionAppearance();
            ConfigureColumns();
            await LoadDataAsync();
        };
        host.Controls.Add(button);
        _sectionButtons[section] = button;
    }

    private void UpdateSectionAppearance()
    {
        foreach (var pair in _sectionButtons)
        {
            var selected = pair.Key == _section;
            pair.Value.NormalColor = selected ? Theme.PrimarySoft : Color.FromArgb(241, 245, 249);
            pair.Value.HoverColor = selected ? Theme.PrimarySoft : Color.FromArgb(226, 232, 240);
            pair.Value.ForeColor = selected ? Theme.Primary : Theme.Text;
        }
        _filterCheckBox.Text = _section == Section.Accommodations ? "Только проживающие" : "Показывать архив";
        _filterCheckBox.Checked = _section == Section.Accommodations;
        _archiveButton.Text = _section == Section.Accommodations ? "Выселить" : "В архив";
        _addButton.Text = _section == Section.Accommodations ? "+  Заселить" : "+  Добавить";
    }

    private void ConfigureColumns()
    {
        _grid.DataSource = null;
        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", Visible = false });
        switch (_section)
        {
            case Section.Dormitories:
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = "Name", FillWeight = 110 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Адрес", DataPropertyName = "Address", FillWeight = 210 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Комнат", DataPropertyName = "RoomCount", FillWeight = 55 });
                _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Активно", DataPropertyName = "IsActive", FillWeight = 55 });
                break;
            case Section.Rooms:
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Общежитие", DataPropertyName = "DormitoryName", FillWeight = 130 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Комната", DataPropertyName = "Number", FillWeight = 65 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Занято", DataPropertyName = "Occupancy", FillWeight = 70 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Назначение", DataPropertyName = "Gender", FillWeight = 80 });
                _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Активна", DataPropertyName = "IsActive", FillWeight = 55 });
                break;
            case Section.Accommodations:
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Студент", DataPropertyName = "StudentName", FillWeight = 130 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "№ билета", DataPropertyName = "StudentNumber", FillWeight = 75 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Общежитие", DataPropertyName = "DormitoryName", FillWeight = 105 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Комната", DataPropertyName = "RoomNumber", FillWeight = 55 });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "С", DataPropertyName = "StartDate", FillWeight = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "По", DataPropertyName = "EndDate", FillWeight = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy", NullValue = "—" } });
                _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = "Status", FillWeight = 70 });
                break;
        }
    }

    private async Task LoadDataAsync()
    {
        Enabled = false;
        try
        {
            switch (_section)
            {
                case Section.Dormitories:
                    var dormitories = await _service.GetDormitoriesAsync(_search.Text, _filterCheckBox.Checked);
                    _grid.DataSource = new BindingList<DormitoryRow>(dormitories.ToList());
                    _summary.Text = $"Общежитий: {dormitories.Count}";
                    break;
                case Section.Rooms:
                    var rooms = await _service.GetRoomsAsync(_search.Text, _filterCheckBox.Checked);
                    _grid.DataSource = new BindingList<RoomDisplayRow>(rooms.Select(RoomDisplayRow.From).ToList());
                    _summary.Text = $"Комнат: {rooms.Count}  •  Мест: {rooms.Sum(item => item.Capacity)}  •  Занято: {rooms.Sum(item => item.OccupiedPlaces)}";
                    break;
                case Section.Accommodations:
                    var accommodations = await _service.GetAccommodationsAsync(_search.Text, _filterCheckBox.Checked);
                    _grid.DataSource = new BindingList<AccommodationRow>(accommodations.ToList());
                    _summary.Text = $"Записей о проживании: {accommodations.Count}";
                    break;
            }
        }
        catch (Exception exception)
        {
            UiFactory.ShowError(this, exception);
        }
        finally
        {
            Enabled = true;
        }
    }

    private async void AddButton_Click(object? sender, EventArgs eventArgs)
    {
        try
        {
            DialogResult result;
            switch (_section)
            {
                case Section.Dormitories:
                    using (var form = new DormitoryEditForm(_service, null))
                    {
                        result = form.ShowDialog(this);
                    }
                    break;
                case Section.Rooms:
                    using (var form = new RoomEditForm(_service, null))
                    {
                        result = form.ShowDialog(this);
                    }
                    break;
                default:
                    using (var form = new AccommodationEditForm(_service, null))
                    {
                        result = form.ShowDialog(this);
                    }
                    break;
            }
            if (result == DialogResult.OK)
            {
                await LoadDataAsync();
            }
        }
        catch (Exception exception)
        {
            UiFactory.ShowError(this, exception);
        }
    }

    private async void EditButton_Click(object? sender, EventArgs eventArgs)
    {
        var id = UiFactory.SelectedId(_grid);
        if (id is null)
        {
            MessageBox.Show(this, "Выберите запись в таблице.", "Campus Pay", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            DialogResult result;
            switch (_section)
            {
                case Section.Dormitories:
                    var dormitory = await _service.GetDormitoryAsync(id.Value)
                        ?? throw new InvalidOperationException("Общежитие не найдено.");
                    using (var form = new DormitoryEditForm(_service, dormitory))
                    {
                        result = form.ShowDialog(this);
                    }
                    break;
                case Section.Rooms:
                    var room = await _service.GetRoomAsync(id.Value)
                        ?? throw new InvalidOperationException("Комната не найдена.");
                    using (var form = new RoomEditForm(_service, room))
                    {
                        result = form.ShowDialog(this);
                    }
                    break;
                default:
                    var accommodation = await _service.GetAccommodationAsync(id.Value)
                        ?? throw new InvalidOperationException("Запись о заселении не найдена.");
                    using (var form = new AccommodationEditForm(_service, accommodation))
                    {
                        result = form.ShowDialog(this);
                    }
                    break;
            }
            if (result == DialogResult.OK)
            {
                await LoadDataAsync();
            }
        }
        catch (Exception exception)
        {
            UiFactory.ShowError(this, exception);
        }
    }

    private async void ArchiveButton_Click(object? sender, EventArgs eventArgs)
    {
        var id = UiFactory.SelectedId(_grid);
        if (id is null)
        {
            MessageBox.Show(this, "Выберите запись в таблице.", "Campus Pay", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var message = _section == Section.Accommodations
            ? "Оформить выселение сегодняшней датой?"
            : "Перевести выбранную запись в архив?";
        if (MessageBox.Show(this, message, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }
        try
        {
            switch (_section)
            {
                case Section.Dormitories:
                    await _service.ArchiveDormitoryAsync(id.Value);
                    break;
                case Section.Rooms:
                    await _service.ArchiveRoomAsync(id.Value);
                    break;
                case Section.Accommodations:
                    await _service.CloseAccommodationAsync(id.Value, DateOnly.FromDateTime(DateTime.Today));
                    break;
            }
            await LoadDataAsync();
        }
        catch (Exception exception)
        {
            UiFactory.ShowError(this, exception);
        }
    }

    private sealed record RoomDisplayRow(
        int Id,
        string DormitoryName,
        string Number,
        string Occupancy,
        string Gender,
        bool IsActive)
    {
        public static RoomDisplayRow From(RoomRow room) => new(
            room.Id,
            room.DormitoryName,
            room.Number,
            room.Occupancy,
            room.Gender switch
            {
                RoomGender.Male => "Мужская",
                RoomGender.Female => "Женская",
                _ => "Без ограничения"
            },
            room.IsActive);
    }
}
