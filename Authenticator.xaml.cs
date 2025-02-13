using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.IO;
using System.Security.Cryptography;

namespace ReactorSimulator
{
    public partial class Authenticator : Window
    {
        private const string filePath = "D:\\Computing\\ReactorSimulator\\data\\users.json";
        private Dictionary<string, string> users;
        private Simulation simulation;

        public Authenticator() // Constructor
        {
            Simulation simulation = new Simulation();

            InitializeComponent();
            loadUsers();
            this.Topmost = true;
        }

        private void loadUsers() // Method to load the username and hashed password to memory.
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<userData>(json);
                users = data?.users ?? new Dictionary<string, string>();
            }
            else
            {
                users = new Dictionary<string, string>();
            }
        }

        private void saveUsers() // Method to save username and password to file.
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            var data = new userData { users = users };
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(filePath, json);
        }

        private string encryptPassword(string password) // Uses SHA256 to hash the password.
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower(); // The hash value is always lowercase without dashes. Looks nicer thats the only reason why :)
            }
        }

        private void login(object sender, EventArgs e) // Method called when the login button is pressed.
        {
            string userName = userNameBox.Text;
            string password = passwordBox.Password;

            if (users.ContainsKey(userName)) // Checks that username is in the file.
            {
                string hashedPassword = encryptPassword(password);
                if (users[userName] == hashedPassword)
                {
                    MessageBox.Show("Login successful.", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
                    Menu menu = new Menu(userName);
                    menu.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect password. Please try again.", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Account not found. Please register instead.", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void register(object sender, RoutedEventArgs e) // Method called when the register button is called.
        {
            string userName = userNameBox.Text;
            string password = passwordBox.Password;

            if (users.ContainsKey(userName))
            {
                MessageBox.Show("Username already taken. Please choose another.", "New Account", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (userName == null || userName == "")
            {
                MessageBox.Show("This username is not valid, please try another.", "New Account", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (password == null || password == "")
            {
                MessageBox.Show("This password is not valid, please try another.", "New Account", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                users[userName] = encryptPassword(password);
                saveUsers();
                MessageBox.Show("Account created successfully. Attempting login.", "New Account", MessageBoxButton.OK, MessageBoxImage.Information);
                login(null, null);
            }
        }

        private void resetPassword(object sender, EventArgs e) // Overwrites the existing data, yes this is not at all secure as no user verification but a cool feature to have!
        {
            string userName = userNameBox.Text;
            string password = passwordBox.Password;

            if (!users.ContainsKey(userName))
            {
                MessageBox.Show("Please enter the username of an existing account you want to reset the password for.", "Reset Password", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (password == null || password == "")
            {
                MessageBox.Show("Please enter the new password in the password box.", "Reset Password", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (encryptPassword(password) == users[userName])
            {
                MessageBox.Show("Cannot set the password to the existing password.", "Reset Password", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                users[userName] = encryptPassword(password);
                saveUsers();
                MessageBox.Show("Password changed successfully. Attempting login.", "Reset Password", MessageBoxButton.OK, MessageBoxImage.Information);
                login(null, null);
            }
        }

        public class userData
        {
            public Dictionary<string, string> users { get; set; } = new();
        }
    }
}