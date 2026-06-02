using _7_Xlebobylochnoe.ApplicationDateBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _7_Xlebobylochnoe.AdminWindow
{
    /// <summary>
    /// Логика взаимодействия для WindowAddEditUser.xaml
    /// </summary>
    public partial class WindowAddEditUser : Window
    {
        private users _editingUser;
        private bool _isSaved = false;

        public WindowAddEditUser(users userToEdit = null)
        {
            InitializeComponent();
            _editingUser = userToEdit;

            // Загрузка ролей
            cbRole.ItemsSource = AppConnect.modelBD.role.ToList();

            if (_editingUser != null)
            {
                // Режим редактирования
                lblTitle.Text = "Редактирование пользователя";
                tbLogin.Text = _editingUser.login;
                pbPassword.Password = _editingUser.password;
                cbRole.SelectedValue = _editingUser.role_id;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text.Trim();
            string password = pbPassword.Password;

            // Валидация
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || cbRole.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? editingUserId = _editingUser?.user_id;
            int roleId = Convert.ToInt32(cbRole.SelectedValue);

            // Проверка уникальности логина
            bool loginExists;

            if (editingUserId == null)
            {
                // Добавление нового пользователя
                loginExists = AppConnect.modelBD.users.Any(u => u.login == login);
            }
            else
            {
                // Редактирование существующего
                loginExists = AppConnect.modelBD.users
                    .Any(u => u.login == login && u.user_id != editingUserId.Value);
            }

            if (loginExists)
            {
                MessageBox.Show($"Логин '{login}' уже занят", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_editingUser == null)
            {
                // Добавление
                users newUser = new users
                {
                    login = login,
                    password = password,
                    role_id = roleId,
                    is_blocked = false,
                    failed_attempts = 0,
                    created_date = DateTime.Now,
                    customer_id = "000000001" // значение по умолчанию
                };
                AppConnect.modelBD.users.Add(newUser);
            }
            else
            {
                // Редактирование
                _editingUser.login = login;
                _editingUser.password = password;
                _editingUser.role_id = roleId;
            }

            AppConnect.modelBD.SaveChanges();
            _isSaved = true;
            MessageBox.Show("Данные сохранены", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public bool IsSaved => _isSaved;
    }
}
