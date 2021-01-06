using System;
using System.Collections.Generic;
using System.IO;
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
using LiveCharts;
using LiveCharts.Wpf;

namespace We_Split
{
    /// <summary>
    /// Interaction logic for Detail.xaml
    /// </summary>

    public partial class Detail : Window
    {
        public Journey currentJourney { get; set; }
        public int currentPlace = 0;

        public Detail()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Flag.Intance.OnDetail = true;
            this.DataContext = currentJourney;
            LoadFirstPlace();
            LoadSumAndAvg();
            LoadChart();
            lvMembers.ItemsSource = currentJourney.Members;
            lvExpenses.ItemsSource = currentJourney.Members;
        }

        public void LoadFirstPlace()
        {
            Place firstPlace = currentJourney.Places[0];
            PlaceName.Text = firstPlace.Name;
            PlaceImg.Source = firstPlace.BMPImg;
            PlaceDes.Text = firstPlace.Description;
        }

        public void LoadSumAndAvg()
        {
            float sum = 0;
            foreach(var member in currentJourney.Members)
            {
                foreach (var expense in member.Expenses)
                {
                    sum += expense.Cost;
                }
            }
            Sum.Text = sum.ToString(("#,##0.00đ"));
            float avg = (sum / (currentJourney.Members.Count));
            Avg.Text = avg.ToString(("#,##0.00đ"));

            foreach (var member in currentJourney.Members)
            {
                float total = 0;
                foreach (var expense in member.Expenses)
                {
                    total += expense.Cost;
                }
                member.Remain = (total - avg).ToString("#,##0.00đ");
            }
        }

        private void LoadChart()
        {
            foreach(var member in currentJourney.Members)
            {
                float sum = 0;
                foreach(var expense in member.Expenses)
                {
                    sum += expense.Cost;

                    columnChart.Series.Add(new ColumnSeries
                    {
                        Title = expense.Name,
                        Values = new ChartValues<float> { expense.Cost}
                    }) ;

                }

                pieChart.Series.Add(new PieSeries
                {
                    Title = member.Name,
                    Values = new ChartValues<float> {sum}
                });
            }
        }

        private void Chart_OnDataClick(object sender, ChartPoint chartPoint)
        {
            var chart = chartPoint.ChartView as PieChart;
            foreach(PieSeries series in chart.Series)
            {
                series.PushOut = 0;
            }
            var selectedSeries = chartPoint.SeriesView as PieSeries;
            selectedSeries.PushOut = 15;
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            Flag.Intance.OnDetail = false;
            this.Close();
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            currentPlace = (currentPlace - 1 + currentJourney.Places.Count) % currentJourney.Places.Count;
            Place place = currentJourney.Places[currentPlace];
            PlaceName.Text = place.Name;
            PlaceImg.Source = place.BMPImg;
            PlaceDes.Text = place.Description;
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            currentPlace = (currentPlace + 1) % currentJourney.Places.Count;
            Place place = currentJourney.Places[currentPlace];
            PlaceName.Text = place.Name;
            PlaceImg.Source = place.BMPImg;
            PlaceDes.Text = place.Description;
        }

        private void delBtn_Click(object sender, RoutedEventArgs e)
        {
            var MessageBoxBtn = MessageBox.Show("Do you really want to delete this recipe?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (MessageBoxBtn == MessageBoxResult.No)
            {
                return;
            }
            var journeys = Database.Intance.Data.Root.Element("Journeys").Elements();
            foreach (var journey in journeys)
            {
                if (int.Parse(journey.Element("Id").Value) == currentJourney.Id)
                {
                    journey.Remove();
                }
            }
            var SystemPath = AppDomain.CurrentDomain.BaseDirectory + $@"Images\";
            File.Delete(SystemPath + currentJourney.Image);
            foreach(var place in currentJourney.Places)
            {
                File.Delete(SystemPath + place.Image);
            }
            Journeys.Intance.Update();
            WriteDownDatabase();
            Flag.Intance.OnDetail = false;
            this.Close();
        }

        private void WriteDownDatabase()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path += @"\Data\Database.xml";

            File.WriteAllText(path, Database.Intance.Data.ToString());
        }
    }
}
