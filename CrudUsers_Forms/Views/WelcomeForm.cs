using System;
using System.Drawing;
using System.Windows.Forms;
using CrudUsers_Forms.Models;

namespace CrudUsers_Forms.Views;

public class WelcomeForm : Form
{
    public WelcomeForm(UserRecord user)
    {
        Text = "Bienvenido";
        StartPosition = FormStartPosition.CenterParent;
        Width = 420;
        Height = 240;
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(20)
        };

        layout.Controls.Add(new Label
        {
            Text = $"Hola {user.DisplayName}",
            Dock = DockStyle.Fill,
            Font = new Font(Font.FontFamily, 12, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 0);

        layout.Controls.Add(new Label
        {
            Text = "No tiene acciones disponibles en este módulo.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 1);

        var btnClose = new Button { Text = "Cerrar sesión", Dock = DockStyle.Top, Height = 40 };
        btnClose.Click += (_, _) => Close();
        layout.Controls.Add(btnClose, 0, 2);

        Controls.Add(layout);
    }
}
