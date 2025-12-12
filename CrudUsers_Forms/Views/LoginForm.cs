using System;
using System.Drawing;
using System.Windows.Forms;
using CrudUsers_Forms.Data;
using CrudUsers_Forms.Models;

namespace CrudUsers_Forms.Views;

public class LoginForm : Form
{
    private readonly UserRepository _repository = new();

    private readonly TextBox _txtUsername = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtPassword = new() { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
    private readonly Label _lblMessage = new() { ForeColor = Color.Firebrick, Dock = DockStyle.Fill };

    public LoginForm()
    {
        Text = "Control de usuarios";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 520;
        Height = 360;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(15),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        layout.Controls.Add(new Label { Text = "Usuario", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
        layout.Controls.Add(_txtUsername, 1, 0);
        layout.Controls.Add(new Label { Text = "Contraseña", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
        layout.Controls.Add(_txtPassword, 1, 1);

        var btnLogin = new Button { Text = "Ingresar", Dock = DockStyle.Fill, Height = 40 };
        btnLogin.Click += OnLoginClicked;
        layout.Controls.Add(btnLogin, 0, 2);

        var btnRegister = new Button { Text = "Crear usuario", Dock = DockStyle.Fill, Height = 40 };
        btnRegister.Click += (_, _) => ShowRegisterDialog();
        layout.Controls.Add(btnRegister, 1, 2);

        layout.Controls.Add(_lblMessage, 0, 3);
        layout.SetColumnSpan(_lblMessage, 2);

        var btnExit = new Button { Text = "Salir", Dock = DockStyle.Fill, Height = 40 };
        btnExit.Click += (_, _) => Close();
        layout.Controls.Add(btnExit, 0, 4);
        layout.SetColumnSpan(btnExit, 2);

        Controls.Add(layout);
        AcceptButton = btnLogin;
    }

    private void OnLoginClicked(object? sender, EventArgs e)
    {
        _lblMessage.Text = string.Empty;
        var username = _txtUsername.Text.Trim();
        var password = _txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _lblMessage.Text = "Capture usuario y contraseña";
            return;
        }

        try
        {
            var user = _repository.Authenticate(username, password);
            if (user == null)
            {
                _lblMessage.Text = "Usuario o contraseña inválidos";
                return;
            }

            if (user.IsBlocked)
            {
                _lblMessage.Text = "El usuario está bloqueado";
                return;
            }

            _repository.UpdateLastLogin(user.Id);
            Hide();
            if (user.Role == UserRole.Conventional)
            {
                using var welcome = new WelcomeForm(user);
                welcome.ShowDialog(this);
            }
            else
            {
                using var dashboard = new DashboardForm(user, _repository);
                dashboard.ShowDialog(this);
            }
        }
        catch (Exception ex)
        {
            _lblMessage.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _txtPassword.Clear();
            Show();
            Activate();
        }
    }

    private void ShowRegisterDialog()
    {
        using var register = new RegisterForm(_repository);
        register.ShowDialog(this);
    }
}
