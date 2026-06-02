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
    /// Логика взаимодействия для WindowAdmin.xaml
    /// </summary>
    public partial class WindowAdmin : Window
    {
        public WindowAdmin()
        {
            InitializeComponent();
            AppConnect.modelBD = DBXlebobylochnoeEntities.GetContext();
            LoadUsers();
            cbRole.ItemsSource = AppConnect.modelBD.role.ToList();
        }

        private void LoadUsers()
        {
            dgUsers.ItemsSource = AppConnect.modelBD.users
                .Include("role")
                .ToList();
        }

        // ➕ ДОБАВИТЬ ПОЛЬЗОВАТЕЛЯ
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AdminWindow.WindowAddEditUser();
            addWindow.Owner = this;
            addWindow.ShowDialog();

            if (addWindow.IsSaved)
            {
                LoadUsers(); // Обновляем таблицу
            }
        }

        // Редактирование пользователя
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя из таблицы", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            users selectedUser = dgUsers.SelectedItem as users;

            var editWindow = new AdminWindow.WindowAddEditUser(selectedUser);
            editWindow.Owner = this;
            editWindow.ShowDialog();

            if (editWindow.IsSaved)
            {
                LoadUsers();
            }
        }

        // Снятие блокировки
        private void BtnUnblock_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для разблокировки", "Внимание");
                return;
            }

            users selectedUser = dgUsers.SelectedItem as users;

            if (selectedUser.is_blocked == true)
            {
                selectedUser.is_blocked = false;
                selectedUser.failed_attempts = 0;
                AppConnect.modelBD.SaveChanges();
                MessageBox.Show($"Пользователь '{selectedUser.login}' разблокирован", "Успех");
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Этот пользователь не заблокирован", "Информация");
            }
        }

        // Выход на окно авторизацции
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var windowAuthorization = new AuthorizationWindow.WindowAuthorization();
            windowAuthorization.Show();
            this.Close();
        }
    }
}
