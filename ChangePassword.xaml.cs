using System;
using System.Windows;

namespace ReactorSimulator
{
    public partial class ChangePassword : Window
    {
        private Authenticator authenticator;

        public ChangePassword()
        {
            InitializeComponent();
            this.Topmost = true;
            authenticator = new Authenticator();
        }

        private void changePassword(object sender, EventArgs e) // Method called when the change password button is pressed, does the actual password changing.
        {
            string username = userNameBox.Text;
            string oldPassword = passwordBox.Password;
            string newPassword = newPasswordBox.Password;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter your username.", "Change Password Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(oldPassword))
            {
                MessageBox.Show("Please enter your old password.", "Change Password Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Please enter your new password.", "Change Password Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Authenticator.UserData user = null;

            if (authenticator.Users.ContainsKey(username))
            {
                user = authenticator.Users[username];
            }
            else
            {
                MessageBox.Show($"User {username} does not exist.", "Change Password Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (authenticator.encryptPassword(oldPassword) == user.password)
            {
                user.password = authenticator.encryptPassword(newPassword);
                authenticator.saveUsers();
                MessageBox.Show($"Successfully changed password for the account {username}.", "Password Changed Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("The old password is incorrect.", "Change Password Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void back(object sender, EventArgs e) // I don't think this really needs explanation.
        {
            this.Close();
        }
    }
}