using System;
using System.Collections.Generic;
using System.Windows;
using System.Net.Sockets;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;

namespace TestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string logText;
        public ObservableCollection<Tests> testsList { get; set; } = new ObservableCollection<Tests>();//коллекция листов
        public ObservableCollection<Questions> qList { get; set; } = new ObservableCollection<Questions>(); //вопросов
        public ObservableCollection<Answer> aList { get; set; } = new ObservableCollection<Answer>();//ответов
        public List<string> testTempList = new List<string>();//промежуточные
        public List<string> questionsTempList = new List<string>();
        public List<string> answersTempList = new List<string>();
        public List<string> answersChekedList = new List<string>();
        public int studentAnswers = 0;
        public MainWindow()
        {
            InitializeComponent();
           
        }
        public void answer_Checked(object sender, RoutedEventArgs e)//те функции из первого окна в списке
        {
            System.Windows.Controls.RadioButton tag = sender as System.Windows.Controls.RadioButton;
            answersChekedList.Add(tag.Content.ToString());
            LogsCreator(tag.Content.ToString() + " добавлен.");
        }
        public void answer_Unchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton tag = sender as System.Windows.Controls.RadioButton;
            answersChekedList.Remove(tag.Content.ToString());
            LogsCreator(tag.Content.ToString() + " удален");
        }
        public async void ConnectButton_Click(object sender, EventArgs e) // подключится кнопка
        {

           DataStream(userName.Text + " подключился");
            LogsCreator(userName.Text + " подключился. Отправлено.");
          
        }
        public async void OpenTest_Click(object sender, EventArgs e) // открыть тест
        {
            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;
            questionsTempList.Clear();
            DataStream(tag.Tag.ToString() + "questions");
        }
        public async void StartTest_Click(object sender, EventArgs e) //начать тест
        {
            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;
            DataStream(tag.Tag.ToString() + "answer");
            TestWindow testAnswer = new TestWindow();
            Window parentWindow = Window.GetWindow(tag);
            parentWindow.Close();

        }
      
        public async void NextTest_Click(object sender, EventArgs e) //следующие вопросы
        {
            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;
            DataStream(tag.Tag.ToString() + "answer");
            studentAnswers += 1;
            TestWindow testAnswer = new TestWindow();
            Window parentWindow = Window.GetWindow(tag);
            parentWindow.Close();
            

        }
        public async void EndTest_Click(object sender, EventArgs e) //завершить тест
        {
            StringBuilder sA = new StringBuilder();
            System.Windows.Controls.Button tag = sender as System.Windows.Controls.Button;
            studentAnswers += 1;
            sA.Append(userName.Text + "*");
            foreach (var answer in answersChekedList)
            {
                sA.Append(answer + "*");
            }
            answersChekedList.Clear();
            DataStream(sA.ToString()+ "aStudent");
            Window parentWindow = Window.GetWindow(tag);
            parentWindow.Close();

        }
        public async void DataStream(string message)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(serverIP.Text), 8888);
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(serverIP.Text, 8888);
            using var stream = new NetworkStream(socket);
            try
            {
                // кодируем его в массив байт
                var data = Encoding.UTF8.GetBytes(message);
                // отправляем массив байт на сервер 
                await stream.WriteAsync(data);
                while (true)
                {
                    // буфер для получения данных
                    var responseData = new byte[512];
                    // получаем данные
                    var bytes = await stream.ReadAsync(responseData);
                    // преобразуем полученные данные в строку
                    string response = Encoding.UTF8.GetString(responseData, 0, bytes);
                    List<string> testsNameList = new List<string>();
                    if (response.Contains("test"))// получаем тесты
                    {
                        testsList.Clear();
                        testsNameList = response.ToString().Replace("test", "").Split('*').ToList();

                        foreach (var testTemp in testsNameList)
                        {
                            Tests tests = new Tests
                            {
                                testName = testTemp
                            };
                            testsList.Add(tests);
                        }
                        testsList.RemoveAt(testsList.Count - 1);
                        testsOut.ItemsSource = testsList;
                    }
                    if (response.Contains("question"))//получаем вопросы
                    {
                        qList.Clear();
                        questionsTempList = response.ToString().Replace("question", "").Split('*').ToList();
                        foreach (var questTemp in questionsTempList)
                        {
                            Questions quetionsTemp = new Questions
                            {
                                qName = questTemp
                            };
                            qList.Add(quetionsTemp);
                        }
                        qList.RemoveAt(qList.Count - 1);
                        TestWindow testWindow = new TestWindow();
                        testWindow.Title = message.Replace("questions","");
                        testWindow.Owner = this;
                        testWindow.Show();
                        testWindow.questionsOut.ItemsSource = qList;
                        testWindow.startTestButton.Tag = qList[0].qName;
                        testWindow.startTestButton.Click += StartTest_Click;
                        studentAnswers = 0;
                    }
                    if (response.Contains("right")) //сообщение о том что прошли
                    {
                        System.Windows.MessageBox.Show("Поздравляем вы ответили правильно на " + response.ToString().Replace("right", "") + "%");
                    }
                    if (response.Contains("answer"))
                    {
                       
                        aList.Clear();
                        answersTempList = response.ToString().Replace("answer", "").Split('*').ToList();
                        foreach (var answerTemps in answersTempList)
                        {
                            Answer answerTemp = new Answer
                            {
                                aName = answerTemps
                            };
                            aList.Add(answerTemp);
                        }
                        aList.RemoveAt(aList.Count - 1);
                        AnswerWindow answerWindow = new AnswerWindow();
                        answerWindow.Owner = this;
                        answerWindow.Title = qList[studentAnswers].qName;
                        answerWindow.questionsNameBox.Text = qList[studentAnswers].qName;
                        answerWindow.Show();
                        answerWindow.answersOut.ItemsSource = aList;
                        
                        if (studentAnswers < qList.Count - 1)
                        {
                            answerWindow.nextTestButton.Content = "Далее";
                            answerWindow.nextTestButton.Tag = qList[studentAnswers + 1].qName;
                            answerWindow.nextTestButton.Click += NextTest_Click;
                        }
                       else
                       {
                            answerWindow.nextTestButton.Content = "Завершить";
                            answerWindow.nextTestButton.Tag = qList[studentAnswers-1].qName;
                            answerWindow.nextTestButton.Click += EndTest_Click;
                           
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task LogsCreator(string textLog)
        {
                logText += textLog + "\n";
                Log.Text = logText;
        }
    }
    
    
}
public class Tests
    {
    public string testName { get; set; }
}
public class Questions
{
    public string qName { get; set; }

}
public class Answer
{
    public string aName { get; set; }

}