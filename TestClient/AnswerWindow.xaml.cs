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

namespace TestClient
{
    /// <summary>
    /// Логика взаимодействия для AnswerWindow.xaml
    /// </summary>
    public partial class AnswerWindow : Window
    {
        public AnswerWindow()
        {
            InitializeComponent();


        }
        public void answer_Check(object sender, EventArgs e)//добавляет ответы в список ответов для отправки
        {
            System.Windows.Controls.RadioButton tag = sender as System.Windows.Controls.RadioButton;

            ((MainWindow)this.Owner).answer_Checked(tag, null);
        }
        public void answer_unCheck(object sender, EventArgs e)//удаляет ответы в списке ответов для отправки
        {
            System.Windows.Controls.RadioButton tag = sender as System.Windows.Controls.RadioButton;

            ((MainWindow)this.Owner).answer_Unchecked(tag, null);
        }
    }
}
