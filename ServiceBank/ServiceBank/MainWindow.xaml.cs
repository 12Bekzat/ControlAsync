using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ServiceBank
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string currentAccount = String.Empty;
        private static Func<int, int> action;

        public MainWindow()
        {
            InitializeComponent();
            using (var context = new Context())
            {
                var accounts = new List<Account>
                {
                    new Account
                    {
                        Name = "Бекзат",
                        Sum = 5000
                     },
                    new Account
                    {
                        Name = "Борис",
                        Sum = 3000
                     },
                    new Account
                    {
                        Name = "Наиль",
                        Sum = 3500
                     },
                    new Account
                    {
                        Name = "Алихан",
                        Sum = 9999
                     },
                    new Account
                    {
                        Name = "Нурым",
                        Sum = 8000
                     },
                    new Account
                    {
                        Name = "Даулет",
                        Sum = 6543
                     },
                };

                var setter = context.Set<Account>();
                foreach (var user in context.Accounts)
                {
                    if(user == null)
                    {
                        break;
                    }
                    for(int i = 0; i < accounts.Count; i++)
                    {
                        if(Guid.Equals(accounts[i].Id, user.Id))
                        {
                            continue;
                        }
                        accounts.Remove(accounts[i]);
                    }
                }

                setter.AddRange(accounts);
                context.SaveChanges();

                foreach (var user in context.Accounts)
                {
                    users.Items.Add(user.Name + ":" + user.Id.ToString());
                }
            }
        }

        private void WithDraw(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(summa.Text))
            {
                if (String.IsNullOrWhiteSpace(currentAccount))
                {
                    MessageBox.Show("Выберите баноквский счет!(чтобы выбрать нажмите два раза)");
                }
                else
                {
                    if (int.TryParse(summa.Text, out var sum))
                    {
                        action = new Func<int, int>(ChangeAccount);
                        action.BeginInvoke(sum, SendMessage, null);
                    }
                    else
                    {
                        MessageBox.Show("Введдите корректную сумму!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Введите сумму!");
            }
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(summa.Text))
            {
                if (String.IsNullOrWhiteSpace(currentAccount))
                {
                    MessageBox.Show("Выберите баноквский счет!(чтобы выбрать нажмите два раза)");
                }
                else
                {
                    if (int.TryParse(summa.Text, out var sum))
                    {
                        action = new Func<int, int>(ChangeAccount);
                        action.BeginInvoke(sum, SendMessage, null);
                    }
                    else
                    {
                        MessageBox.Show("Введдите корректную сумму!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Введите сумму!");
            }
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            summa.Text = String.Empty;
        }

        private static int ChangeAccount(int number)
        {
            using (var context = new Context())
            {
                string[] tokens = currentAccount.Split(':');
                if (Guid.TryParse(tokens[1], out var idCheck))
                {
                    foreach (var account in context.Accounts)
                    {
                        if (Guid.Equals(account.Id, idCheck))
                        {
                            var lastAccount = account;
                            account.Sum += number;
                            var set = context.Set<Account>();
                            set.Remove(lastAccount);
                            context.SaveChanges();
                            set.Add(account);
                            context.SaveChanges();
                            return number;
                        }
                    }
                }
            }
            return 0;
        }

        private static void SendMessage(IAsyncResult result)
        {
            var sum = action.EndInvoke(result);
            if (sum != 0)
            {
                const string accountSid = "AC72eab5c9bc0bd97ed11b1bbb38666350";
                const string authToken = "56afd5e30952054631405de6e68b55a9";

                TwilioClient.Init(accountSid, authToken);

                var message = MessageResource.Create(
                    body: String.Format("{0} {1}тг", sum < 0 ? "С вашего счета снято" : "На ваш счет поступило", sum < 0 ? sum * (-1) : sum),
                    from: new Twilio.Types.PhoneNumber("+13343263032"),
                    to: new Twilio.Types.PhoneNumber("+77774213007")
                );

                var str = message.Sid;
            }
        }

        private void SelectListUser(object sender, MouseButtonEventArgs e)
        {
            var current = ((sender as ListBox).SelectedItem as ListBoxItem);
            
        }

        private void Select(object sender, SelectionChangedEventArgs e)
        {
            var current = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as ListBox) as ListBoxItem;
            if (current != null)
            {
                currentAccount = current.Content.ToString();
                MessageBox.Show("Пользователь выбран!");
            }
        }
    }
}
