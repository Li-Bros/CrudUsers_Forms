using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CrudUsers_Forms.Models;

namespace CrudUsers_Forms.Views;

public class UserEditorForm : Form
{
    private readonly bool _isNew;
    private readonly TextBox _txtUsername = new();
    private readonly TextBox _txtDisplayName = new();
    private readonly TextBox _txtEmail = new();
    private readonly ComboBox _cmbRole = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtPassword = new() { UseSystemPasswordChar = true };
    private readonly TextBox _txtConfirm = new() { UseSystemPasswordChar = true };
    private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick };

    public UserRecord UserData { get; }
    public string? Password => string.IsNullOrWhiteSpace(_txtPassword.Text) ? null : _txtPassword.Text;

    public UserEditorForm(UserRecord? user, IEnumerable<UserRole> allowedRoles)
    {
        _isNew = user == null;
        UserData = user == null ? new UserRecord() : new UserRecord
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role
        };

        Text = _isNew ? "Crear usuario" : "Editar usuario";
        StartPosition = FormStartPosition.CenterParent;
        Width = 420;
        Height = 420;

        foreach (var role in allowedRoles)
        {
            _cmbRole.Items.Add(new ComboBoxItem(role));
        }

        if (_cmbRole.Items.Count > 0)
        {
            _cmbRole.SelectedIndex = 0;
        }

        InitializeLayout();
        BindUser();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(15)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        AddField(layout, "Usuario", _txtUsername, 0);
        AddField(layout, "Nombre completo", _txtDisplayName, 1);
        AddField(layout, "Correo", _txtEmail, 2);
        AddField(layout, "Rol", _cmbRole, 3);
        AddField(layout, "Contraseña", _txtPassword, 4);
        AddField(layout, "Confirmar", _txtConfirm, 5);

        layout.Controls.Add(_lblMessage, 0, 6);
        layout.SetColumnSpan(_lblMessage, 2);

        var panelButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft };
        var btnSave = new Button { Text = "Guardar", Height = 40 };
        btnSave.Click += OnSave;
        var btnCancel = new Button { Text = "Cancelar", Height = 40 };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        panelButtons.Controls.Add(btnSave);
        panelButtons.Controls.Add(btnCancel);

        Controls.Add(layout);
        Controls.Add(panelButtons);
        AcceptButton = btnSave;
    }

    private void AddField(TableLayoutPanel panel, string label, Control control, int row)
    {
        panel.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row);
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, 1, row);
    }

    private void BindUser()
    {
        _txtUsername.Text = UserData.Username;
        _txtUsername.Enabled = _isNew;
        _txtDisplayName.Text = UserData.DisplayName;
        _txtEmail.Text = UserData.Email;

        var idx = -1;
        for (var i = 0; i < _cmbRole.Items.Count; i++)
        {
            if (_cmbRole.Items[i] is ComboBoxItem item && item.Role == UserData.Role)
            {
                idx = i;
                break;
            }
        }
        if (idx >= 0)
        {
            _cmbRole.SelectedIndex = idx;
        }
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblMessage.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtDisplayName.Text))
        {
            _lblMessage.Text = "Usuario y nombre son obligatorios";
            return;
        }

        if (_isNew && _txtPassword.Text.Length < 6)
        {
            _lblMessage.Text = "La contraseña debe tener al menos 6 caracteres";
            return;
        }

        if (!string.IsNullOrEmpty(_txtPassword.Text) && _txtPassword.Text != _txtConfirm.Text)
        {
            _lblMessage.Text = "Las contraseñas no coinciden";
            return;
        }

        UserData.Username = _txtUsername.Text.Trim();
        UserData.DisplayName = _txtDisplayName.Text.Trim();
        UserData.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();
        if (_cmbRole.SelectedItem is ComboBoxItem roleItem)
        {
            UserData.Role = roleItem.Role;
        }

        DialogResult = DialogResult.OK;
    }

    private sealed class ComboBoxItem
    {
        public ComboBoxItem(UserRole role) => Role = role;
        public UserRole Role { get; }
        public override string ToString() => roleText.TryGetValue(Role, out var text) ? text : Role.ToString();
        private static readonly Dictionary<UserRole, string> roleText = new()
        {
            [UserRole.SuperAdmin] = "Super Administrador",
            [UserRole.Admin] = "Administrador",
            [UserRole.Conventional] = "Usuario"
        };
    }
}
