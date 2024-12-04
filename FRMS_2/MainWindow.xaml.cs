using EasyJet.FRAMModel.Engine;
using EasyJet.FRAMModel.Engine.ExternalContract;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            //this.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Images/app.ico"));
        }

        private void buttonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void buttonMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                maximizeImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/maximize.png"));
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                maximizeImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restore.png"));
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        OpenFileDialog openFileDialog = new OpenFileDialog();
        //Create and populate DataTable
        DataTable dt;
        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {

            bool? result = openFileDialog.ShowDialog();
            if (!(bool)result)
            {
                return;
            }
            var fileName = string.Empty;

            fileName = openFileDialog.FileName;
            string[] textData = System.IO.File.ReadAllLines(fileName);
            string[] headers = (textData[0]).Split(',');

            dt = new DataTable();
            foreach (string header in headers)
                dt.Columns.Add(header, typeof(string), null);
            for (int i = 1; i < textData.Length; i++)
                dt.Rows.Add(textData[i].Split(','));
            FramScoreGrid.ItemsSource = null;
            GenTxt.Text = string.Empty;
            GenTxt.Visibility = Visibility.Collapsed;
            FramInputGrid.ItemsSource = dt.DefaultView;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            //ExecutionTimeTxt.Visibility = Visibility.Collapsed;
            //progressBar.Visibility = Visibility.Visible;
            GenTxt.Visibility = Visibility.Visible;
            GenTxt.Text = "Generating score.... ";

            Task.Delay(1000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    var request = new FRMModelRequest();
                    var dutyBlockDutyCountList = new List<int>();
                    var dutyPeriodOfDutyBlockList = new List<int>();
                    var operationalSectorCountList = new List<int>();
                    var isaHomeStandbyFlagList = new List<int>();
                    var startDateLocalTimeList = new List<string>();
                    var startTimeLocalTimeList = new List<string>();
                    var endDateLocalTimeList = new List<string>();
                    var endTimeLocalTimeList = new List<string>();
                    var endDateCrewReferenceTimeList = new List<string>();
                    var endTimeCrewReferenceTimeList = new List<string>();
                    var startDateTimeZuluList = new List<string>();
                    var endDateTimeZuluList = new List<string>();
                    var dutyLengthList = new List<string>();
                    var isDutyMorningStartList = new List<int>();
                    var isDutyEveningFinishList = new List<int>();
                    var isDutyNightFinishList = new List<int>();
                    var isDutyElongatedList = new List<int>();
                    var isDutyHighSectorList = new List<int>();
                    var hoursBetweenMidnightList = new List<string>();
                    var isContactableList = new List<string>();
                    var isStandbyList = new List<string>();
                    var commuteTimeList = new List<string>();
                    var sbyCalloutList = new List<string>();
                    //var crewIdList = new List<int>();

                    foreach (DataRow dr in dt.Rows)
                    {
                        dutyPeriodOfDutyBlockList.Add(int.Parse(dr.ItemArray[0].ToString()));
                        operationalSectorCountList.Add(int.Parse(dr.ItemArray[1].ToString()));
                        isaHomeStandbyFlagList.Add(int.Parse(dr.ItemArray[2].ToString()));
                        startDateLocalTimeList.Add(dr.ItemArray[3].ToString());
                        startTimeLocalTimeList.Add(dr.ItemArray[4].ToString());
                        endDateLocalTimeList.Add(dr.ItemArray[5].ToString());
                        endTimeLocalTimeList.Add(dr.ItemArray[6].ToString());
                        endDateCrewReferenceTimeList.Add(dr.ItemArray[7].ToString());
                        endTimeCrewReferenceTimeList.Add(dr.ItemArray[8].ToString());
                        startDateTimeZuluList.Add(dr.ItemArray[9].ToString());
                        endDateTimeZuluList.Add(dr.ItemArray[10].ToString());
                        dutyLengthList.Add(dr.ItemArray[11].ToString());
                        isDutyMorningStartList.Add(int.Parse(dr.ItemArray[12].ToString()));
                        isDutyEveningFinishList.Add(int.Parse(dr.ItemArray[13].ToString()));
                        isDutyNightFinishList.Add(int.Parse(dr.ItemArray[14].ToString()));
                        isDutyElongatedList.Add(int.Parse(dr.ItemArray[15].ToString()));
                        isDutyHighSectorList.Add(int.Parse(dr.ItemArray[16].ToString()));
                        hoursBetweenMidnightList.Add(dr.ItemArray[17].ToString());
                        isContactableList.Add(dr.ItemArray[18].ToString());
                        isStandbyList.Add(dr.ItemArray[19].ToString());
                        commuteTimeList.Add(dr.ItemArray[20].ToString());
                        sbyCalloutList.Add(dr.ItemArray[21].ToString());
                        //crewIdList.Add(int.Parse(dr.ItemArray[22].ToString()));
                    }


                    request.IdxInBlock = dutyPeriodOfDutyBlockList.ToArray();
                    request.OperationalSectorCount = operationalSectorCountList.ToArray();
                    request.IsaHomeStandbyFlag = isaHomeStandbyFlagList.ToArray();
                    request.StartDateLocalTime = startDateLocalTimeList.ToArray();
                    request.StartTimeLocalTime = startTimeLocalTimeList.ToArray();
                    request.EndDateLocalTime = endDateLocalTimeList.ToArray();
                    request.EndTimeLocalTime = endTimeLocalTimeList.ToArray();
                    request.EndDateCrewReferenceTime = endDateCrewReferenceTimeList.ToArray();
                    request.EndTimeCrewReferenceTime = endTimeCrewReferenceTimeList.ToArray();
                    request.StartDateTimeZulu = startDateTimeZuluList.ToArray();
                    request.EndDateTimeZulu = endDateTimeZuluList.ToArray();
                    request.DutyLength = dutyLengthList.ToArray();
                    request.IsDutyMorningStart = isDutyMorningStartList.ToArray();
                    request.IsDutyEveningFinish = isDutyEveningFinishList.ToArray();
                    request.IsDutyNightFinish = isDutyNightFinishList.ToArray();
                    request.IsDutyElongated = isDutyElongatedList.ToArray();
                    request.IsDutyHighSector = isDutyHighSectorList.ToArray();
                    request.HoursBetweenMidnight = hoursBetweenMidnightList.ToArray();
                    request.IsContactable = isContactableList.ToArray();
                    request.IsStandby = isStandbyList.ToArray();
                    request.CommuteTime = commuteTimeList.ToArray();
                    request.SbyCallout = sbyCalloutList.ToArray();
                    ScoreGenerator scoreGenerator = new ScoreGenerator();
                    DateTime startTime = DateTime.Now;
                    IFRMModelResponse response = scoreGenerator.Generate(request);
                    TimeSpan timeSpan = DateTime.Now - startTime;
                    GenTxt.Visibility = Visibility.Collapsed;
                    //ExecutionTimeTxt.Text = timeSpan.ToString();
                    //ExecutionTimeTxt.Visibility = Visibility.Visible;
                    //progressBar.Visibility = Visibility.Collapsed;
                    BindData(response);
                });
            });
        }

        private void BindData(IFRMModelResponse response)
        {
            if (String.IsNullOrEmpty(response.ErrorDescription))
            {
                FramScoreGrid.ItemsSource = response.FRMScore;
            }
            else
            {
                GenTxt.Text = response.ErrorDescription;
                GenTxt.Visibility = Visibility.Visible;
            }

        }
    }
}