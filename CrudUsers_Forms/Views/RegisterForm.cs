using System;
using System.Drawing;
using System.Windows.Forms;
using CrudUsers_Forms.Data;
using CrudUsers_Forms.Models;

namespace CrudUsers_Forms.Views;

public class RegisterForm : Form
{
    private readonly UserRepository _repository;

    private readonly TextBox _txtUsername = new();
    private readonly TextBox _txtDisplayName = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPassword = new() { UseSystemPasswordChar = true };
    private readonly TextBox _txtConfirm = new() { UseSystemPasswordChar = true };
    private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick };

    public RegisterForm(UserRepository repository)
    {
        _repository = repository;
        Text = "Registro de usuario";
        StartPosition = FormStartPosition.CenterParent;
        Width = 520;
        Height = 440;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(15)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

        AddField(layout, "Usuario", _txtUsername, 0);
        AddField(layout, "Nombre completo", _txtDisplayName, 1);
        AddField(layout, "Correo", _txtEmail, 2);
        AddField(layout, "Contraseña", _txtPassword, 3);
        AddField(layout, "Confirmar", _txtConfirm, 4);

        layout.Controls.Add(_lblMessage, 0, 5);
        layout.SetColumnSpan(_lblMessage, 2);

        var btnSave = new Button { Text = "Registrar", Dock = DockStyle.Fill, Height = 40 };
        btnSave.Click += OnSave;
        layout.Controls.Add(btnSave, 0, 6);
        layout.SetColumnSpan(btnSave, 2);

        Controls.Add(layout);
        AcceptButton = btnSave;
    }

    private void AddField(TableLayoutPanel panel, string label, Control control, int row)
    {
        panel.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row);
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, 1, row);
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _lblMessage.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtDisplayName.Text))
        {
            _lblMessage.Text = "Usuario y nombre son obligatorios";
            return;
        }

        if (_txtPassword.Text.Length < 6)
        {
            _lblMessage.Text = "La contraseña debe tener al menos 6 caracteres";
            return;
        }

        if (_txtPassword.Text != _txtConfirm.Text)
        {
            _lblMessage.Text = "Las contraseñas no coinciden";
            return;
        }

        try
        {
            var user = new UserRecord
            {
                Username = _txtUsername.Text.Trim(),
                DisplayName = _txtDisplayName.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
                Role = UserRole.Conventional
            };

            _repository.CreateUser(user, _txtPassword.Text, null);
            MessageBox.Show("Usuario creado. Puede iniciar sesión.", "Registro", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            _lblMessage.Text = ex.Message;
        }
    }
}
