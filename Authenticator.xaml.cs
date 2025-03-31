using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Immutable;

namespace ReactorSimulator
{
    public partial class Authenticator : Window
    {
        private const string filePath = "F:\\Computing\\ReactorSimulator\\data\\users.json";
        private Dictionary<string, UserData> users = new Dictionary<string, UserData>();
        private Menu menuWindow;

        public Authenticator() // Constructor
        {
            InitializeComponent();
            loadUsers();
            this.Topmost = true;
        }

        private void loadUsers() // Method to load the users from the JSON file.
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var loadedUsers = JsonSerializer.Deserialize<Dictionary<string, UserData>>(json);
                    if (loadedUsers != null)
                    {
                        users = loadedUsers;
                    }
                }
                catch (JsonException ex) // If the file is corrupt it will make a new one.
                {
                    MessageBox.Show($"Error deserializing users.json: {ex.Message}. The file may be corrupted. Creating a new user file.", "Load User Method Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    users = new Dictionary<string, UserData>();
                    saveUsers();
                }
                catch (IOException ex) // Incase there are issues with file permissions, added this as it was mucking about but not sure if it's why but good to have.
                {
                    MessageBox.Show($"Error reading users.json: {ex.Message}. Please check file permissions.", "Load User Method Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    users = new Dictionary<string, UserData>();
                }
            }
        }

        public void saveUsers() // Method to save new users to the users.json file.
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(users, options);
            try
            {
                File.WriteAllText(filePath, json);
            }
            catch (IOException ex) // Same as other IOException in loadUsers.
            {
                MessageBox.Show($"Error writing to users.json: {ex.Message}. Please check file permissions.", "Save User Method Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string encryptPassword(string password) // Method that uses SHA256 to hash the password.
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower(); // The hash value is always lowercase without dashes. Looks nicer thats the only reason why :)
            }
        }

        private void login(object sender, EventArgs e) // Method that is called from the login button, handles the log in.
        {
            string userName = userNameBox.Text;
            string password = passwordBox.Password;

            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Please enter a username.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password.", "LoginError", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (users.ContainsKey(userName))
            {
                string hashedPassword = encryptPassword(password);
                if (users[userName].password == hashedPassword)
                {
                    MessageBox.Show("Login successful.", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
                    string userID = users[userName].ID;
                    menuWindow = new Menu(userName, userID);
                    SettingsManager.loadUserSettings(userID, menuWindow, null, null);
                    menuWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect password. Please try again.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Account not found. Please register instead.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void register(object sender, EventArgs e) // Method that is called from the register button, handles registration and then calls the login method.
        {
            string userName = userNameBox.Text;
            string password = passwordBox.Password;

            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Please enter a username.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (users.ContainsKey(userName))
            {
                MessageBox.Show("Username already taken. Please choose another.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("This username is not valid, please try another.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("This password is not valid, please try another.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string userID = getUserID();
                string hashedPassword = encryptPassword(password);
                users[userName] = new UserData {ID = userID, password = hashedPassword};
                saveUsers();
                MessageBox.Show("Account created successfully. Attempting login.", "New Account", MessageBoxButton.OK, MessageBoxImage.Information);
                login(null, null);
            }
        }

        private string getUserID() // Used in registration, takes the number of files and adds 1 for the new user ID.
        {
            if (users.Count == 0)
            {
                return "1";
            }
            else
            {
                int maxId = 0;
                foreach (var user in users.Values)
                {
                    if (user != null && !string.IsNullOrEmpty(user.ID))
                    {
                        int currentId = int.Parse(user.ID);
                        if (currentId > maxId)
                        {
                            maxId = currentId;
                        }
                    }
                }
                return (maxId + 1).ToString();
            }
        }

        private void changePassword(object sender, EventArgs e) // Method called from the change password button to open the window to allow a user to change their password.
        {
            this.Topmost = false;
            ChangePassword changePassword = new ChangePassword();
            changePassword.Show();
        }

        public class UserData
        {
            public string ID { get; set; }
            public string password { get; set; }
        }

        public Dictionary<string, UserData> Users
        {
            get { return users; }
            set { users = value; }
        }
    }
}