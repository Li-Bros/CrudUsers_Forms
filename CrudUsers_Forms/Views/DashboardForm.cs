using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrudUsers_Forms.Data;
using CrudUsers_Forms.Models;

namespace CrudUsers_Forms.Views;

public class DashboardForm : Form
{
    private readonly UserRecord _currentUser;
    private readonly UserRepository _repository;
    private readonly BindingList<UserRecord> _users = new();

    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly Label _lblStatus = new() { Dock = DockStyle.Top, Padding = new Padding(10) };
    private readonly Button _btnEdit = new() { Text = "Editar", Height = 40 };
    private readonly Button _btnDelete = new() { Text = "Eliminar", Height = 40 };
    private readonly Button _btnBlock = new() { Text = "Bloquear", Height = 40 };

    public DashboardForm(UserRecord currentUser, UserRepository repository)
    {
        _currentUser = currentUser;
        _repository = repository;

        Text = "Panel de administración";
        StartPosition = FormStartPosition.CenterParent;
        Width = 1040;
        Height = 720;

        ConfigureGrid();

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var headerPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        headerPanel.Controls.Add(new Label { Text = $"Sesión: {_currentUser.DisplayName} ({_currentUser.RoleDescription})", AutoSize = true, Padding = new Padding(10) });
        var btnLogout = new Button { Text = "Cerrar sesión", Height = 40 };
        btnLogout.Click += (_, _) => Close();
        headerPanel.Controls.Add(btnLogout);

        mainLayout.Controls.Add(headerPanel, 0, 0);
        mainLayout.Controls.Add(_grid, 0, 1);

        var actionsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(10) };
        var btnAdd = new Button { Text = "Agregar", Height = 40 };
        btnAdd.Click += (_, _) => OpenEditor(null);
        _btnEdit.Click += (_, _) => OpenEditor(GetSelectedUser());
        _btnDelete.Click += (_, _) => DeleteSelected();
        _btnBlock.Click += (_, _) => ToggleBlock();
        var btnRefresh = new Button { Text = "Actualizar", Height = 40 };
        btnRefresh.Click += (_, _) => LoadUsers();

        actionsPanel.Controls.AddRange(new Control[] { btnAdd, _btnEdit, _btnDelete, _btnBlock, btnRefresh });
        var bottomPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        bottomPanel.Controls.Add(actionsPanel, 0, 0);
        _lblStatus.TextAlign = ContentAlignment.MiddleRight;
        _lblStatus.Dock = DockStyle.Fill;
        bottomPanel.Controls.Add(_lblStatus, 1, 0);
        mainLayout.Controls.Add(bottomPanel, 0, 2);

        Controls.Add(mainLayout);

        Load += (_, _) => LoadUsers();
        _grid.SelectionChanged += (_, _) => UpdateButtons();
    }

    private void ConfigureGrid()
    {
        _grid.DataSource = _users;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Username", HeaderText = "Usuario" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DisplayName", HeaderText = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "Correo", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoleDescription", HeaderText = "Rol" });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "IsBlocked", HeaderText = "Bloqueado" });
    }

    private void LoadUsers()
    {
        try
        {
            _users.Clear();
            var data = _repository.GetUsers();
            if (_currentUser.Role == UserRole.Admin)
            {
                data = data.Where(u => u.Role == UserRole.Conventional).ToList();
            }
            foreach (var user in data)
            {
                _users.Add(user);
            }
            _lblStatus.Text = $"Usuarios cargados: {_users.Count}";
            UpdateButtons();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No fue posible cargar los usuarios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private UserRecord? GetSelectedUser() => _grid.CurrentRow?.DataBoundItem as UserRecord;

    private void UpdateButtons()
    {
        var selected = GetSelectedUser();
        var canManage = selected != null && CanManage(selected);
        _btnEdit.Enabled = canManage;
        _btnDelete.Enabled = canManage;
        _btnBlock.Enabled = canManage;
        if (selected != null)
        {
            _btnBlock.Text = selected.IsBlocked ? "Desbloquear" : "Bloquear";
        }
    }

    private bool CanManage(UserRecord user)
    {
        if (user == null || user.Id == _currentUser.Id)
        {
            return false;
        }

        return _currentUser.Role switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => user.Role == UserRole.Conventional,
            _ => false
        };
    }

    private void OpenEditor(UserRecord? target)
    {
        if (target != null && !CanManage(target))
        {
            MessageBox.Show("No tiene permisos para editar este usuario.");
            return;
        }

        var allowedRoles = GetAllowedRoles();
        using var editor = new UserEditorForm(target, allowedRoles);
        if (editor.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            if (target == null)
            {
                _repository.CreateUser(editor.UserData, editor.Password ?? throw new InvalidOperationException("Contraseña requerida"), _currentUser.Id);
            }
            else
            {
                _repository.UpdateUser(editor.UserData, editor.Password);
            }
            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private IEnumerable<UserRole> GetAllowedRoles()
    {
        if (_currentUser.Role == UserRole.SuperAdmin)
        {
            return new[] { UserRole.SuperAdmin, UserRole.Admin, UserRole.Conventional };
        }
        return new[] { UserRole.Conventional };
    }

    private void DeleteSelected()
    {
        var selected = GetSelectedUser();
        if (selected == null || !CanManage(selected))
        {
            return;
        }
        if (MessageBox.Show($"¿Eliminar a {selected.Username}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }
        try
        {
            _repository.DeleteUser(selected.Id);
            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ToggleBlock()
    {
        var selected = GetSelectedUser();
        if (selected == null || !CanManage(selected))
        {
            return;
        }
        try
        {
            _repository.SetBlocked(selected.Id, !selected.IsBlocked);
            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
