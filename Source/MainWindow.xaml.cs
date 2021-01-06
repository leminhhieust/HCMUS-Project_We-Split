using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace We_Split
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    #region Assign essential classes
    public class Journey
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Image { get; set; }
        public BitmapImage BMPImg { get; set; }
        public ObservableCollection<Place> Places { get; set; }
        public ObservableCollection<Member> Members { get; set; }

        private ObservableCollection<Place> ReadPlaces(XElement places)
        {
            var res = new ObservableCollection<Place>();
            foreach (XElement place in places.Elements())
            {
                var place_res = new Place(place);
                res.Add(place_res);
            }
            return res;
        }

        private ObservableCollection<Member> ReadMembers(XElement members)
        {
            var res = new ObservableCollection<Member>();
            foreach (XElement member in members.Elements())
            {
                var member_res = new Member(member);
                res.Add(member_res);
            }
            return res;
        }

        public Journey(XElement journey)
        {
            Id = int.Parse(journey.Element("Id").Value);
            Status = journey.Element("Status").Value;
            Name = journey.Element("Name").Value;
            Description = journey.Element("Description").Value;
            StartDate = DateTime.Parse(journey.Element("StartDate").Value).Date;
            EndDate = DateTime.Parse(journey.Element("EndDate").Value).Date;
            Places = ReadPlaces(journey.Element("Places"));
            Members = ReadMembers(journey.Element("Members"));
            Image = journey.Element("Image").Value;

            BMPImg = new BitmapImage();
            BMPImg.BeginInit();
            BMPImg.CacheOption = BitmapCacheOption.OnLoad;
            BMPImg.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + $@"Images\{Image}", UriKind.RelativeOrAbsolute);
            BMPImg.EndInit();
        }

        public Journey()
        {
            Id = -1;
            Status = "";
            Name = "";
            Description = "";
            Image = "";
            BMPImg = new BitmapImage();
            StartDate = new DateTime().Date;
            EndDate = new DateTime().Date;
            Places = new ObservableCollection<Place>();
            Members = new ObservableCollection<Member>();
        }
    }

    public class Place
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public BitmapImage BMPImg { get; set; }
        public string Image { get; set; }
        public Place(XElement place)
        {
            Name = place.Element("Name").Value;
            Description = place.Element("Description").Value;
            Image = place.Element("Image").Value;
            BMPImg = new BitmapImage();
            BMPImg.BeginInit();
            BMPImg.CacheOption = BitmapCacheOption.OnLoad;
            BMPImg.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + $@"Images\{Image}", UriKind.RelativeOrAbsolute);
            BMPImg.EndInit();
        }

        public Place()
        {
            Name = "";
            Description = "";
            Image = "";
            BMPImg = new BitmapImage();
        }
    }

    public class Member
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Remain { get; set; }
        public ObservableCollection<Expense> Expenses { get; set; }

        private ObservableCollection<Expense> ReadExpenses(XElement expenses)
        {
            var res = new ObservableCollection<Expense>();
            foreach (XElement expense in expenses.Elements())
            {
                var expense_res = new Expense(expense);
                res.Add(expense_res);
            }
            return res;
        }

        public Member(XElement member)
        {
            Name = member.Element("Name").Value;
            Phone = member.Element("Phone").Value;
            Expenses = ReadExpenses(member.Element("Expenses"));
            Remain = member.Element("Remain").Value;
        }

        public Member()
        {
            Name = "";
            Phone = "";
            Remain = "";
            Expenses = new ObservableCollection<Expense>();
        }
    }

    public class Expense
    {
        public string Name { get; set; }
        public float Cost { get; set; }
        public Expense(XElement expense)
        {
            Name = expense.Element("Name").Value;
            Cost = float.Parse(expense.Element("Cost").Value);
        }
        public Expense()
        {
            Name = "";
            Cost = 0;
        }
    }

    public class Journeys
    {
        private static Journeys _instance = null;
        public ObservableCollection<Journey> Data { get; set; }

        public static Journeys Intance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Journeys();
                }
                return _instance;
            }
        }

        private Journeys()
        {
            Data = new ObservableCollection<Journey>();
            var DB = Database.Intance;
            var journeys = DB.Data.Element("root").Element("Journeys").Elements();

            foreach (XElement journey in journeys)
            {
                var journey_res = new Journey(journey);
                Data.Add(journey_res);
            }
        }

        public void Update()
        {
            Data.Clear();

            var DB = Database.Intance;
            var journeys = DB.Data.Element("root").Element("Journeys").Elements();

            foreach (XElement journey in journeys)
            {
                var journey_res = new Journey(journey);
                Data.Add(journey_res);
            }
        }
    }
    
    public class Database
    {
        private static Database _instance = null;
        public XDocument Data { get; set; }

        public static Database Intance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Database();
                }
                return _instance;
            }
        }

        private Database()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string XMLpath = path + @"\Data\Database.xml";

            string XMLtext = File.ReadAllText(XMLpath);
            this.Data = new XDocument(XDocument.Parse(XMLtext));
        }

        public void Update()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path += @"\Data\Database.xml";
            File.WriteAllText(path, Database.Intance.Data.ToString());
        }
    }

    public class Flag
    {
        private static Flag _instance = null;
        public static Flag Intance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Flag();
                }
                return _instance;
            }
        }

        public bool OnAdd { get; set; }
        public bool OnUpdate { get; set; }
        public bool OnDetail { get; set; }
        private Flag()
        {
            OnUpdate = false;
            OnAdd = false;
        }
    }
    #endregion

    public partial class MainWindow : Window
    {
        public ObservableCollection<Journey> Going { get; set; }
        public ObservableCollection<Journey> Complete { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }
        
        public T GetAncestorOfType<T>(FrameworkElement child) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent != null && !(parent is T))
                return (T)GetAncestorOfType<T>((FrameworkElement)parent);
            return (T)parent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadIcon();
            LoadGoingAndComplete();
        }

        private void LoadIcon()
        {
            var BMPImg = new BitmapImage();
            BMPImg.BeginInit();
            BMPImg.CacheOption = BitmapCacheOption.OnLoad;
            BMPImg.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + $@"Images\Icon.png", UriKind.RelativeOrAbsolute);
            BMPImg.EndInit();
            icon.Source = BMPImg;
        }

        public void LoadGoingAndComplete()
        {
            Going = new ObservableCollection<Journey>();
            Complete = new ObservableCollection<Journey>();
            for (int i = 0; i < Journeys.Intance.Data.Count; ++i)
            {
                if(Journeys.Intance.Data[i].Status == "Đang đi")
                {
                   Going.Add(Journeys.Intance.Data[i]);
                }
                else if (Journeys.Intance.Data[i].Status == "Đã hoàn thành")
                {
                    Complete.Add(Journeys.Intance.Data[i]);
                }
            }
            GoingListView.ItemsSource = Going;
            CompleteListView.ItemsSource = Complete;
        }

        private string NormalizeString(string str)
        {
            string result = str;

            result = result.ToLower();
            result = ToNonUnicode(result);

            return result;
        }

        private string ToNonUnicode(string str)
        {
            var result = new StringBuilder();

            string[] unicodeChars = {"á", "à", "ả", "ã", "ạ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ",
                        "đ",
                        "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ễ", "ể", "ệ",
                        "í", "ì", "ỉ", "ĩ", "ị", "ý", "ỳ", "ỷ", "ỹ", "ỵ",
                        "ó", "ò", "ỏ", "õ", "ọ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ",
                        "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự" };

            string[] nonUnicodeChars = { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                        "d",
                        "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
                        "i", "i", "i", "i", "i", "y", "y", "y", "y", "y",
                        "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o",
                        "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u" };

            for (var i = 0; i < str.Length; i++)
            {
                var index = FindIndexOfString(unicodeChars, str[i]);
                if (index != -1)
                {
                    result.Append(nonUnicodeChars[index]);
                }
                else
                {
                    result.Append(str[i]);
                }
            }

            return result.ToString();
        }

        private int FindIndexOfString(string[] unicodeChars, char character)
        {
            var index = -1;

            var i = 0;
            while (i < unicodeChars.Length)
            {
                if (character.ToString() == unicodeChars[i])
                {
                    index = i;
                    break;
                }
                i++;
            }

            return index;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button currentBtn = sender as Button;
            Flag.Intance.OnAdd = true;
            var AddScreen = new Add();
            if (currentBtn.Name == "AddBtn")
            {
                Flag.Intance.OnUpdate = false;
            }
            else if(currentBtn.Name == "UpdateBtn")
            {
                Flag.Intance.OnUpdate = true;
                var lv = GetAncestorOfType<ListView>(currentBtn);
                
                if(lv.Name == "GoingListView")
                {
                    Journey journey = GoingListView.SelectedItem as Journey;
                    AddScreen.currentJourney = journey;
                }
                else if(lv.Name == "CompleteListView")
                {
                    Journey journey = CompleteListView.SelectedItem as Journey;
                    AddScreen.currentJourney = journey;
                }
            }

            AddScreen.ShowDialog();
            while (Flag.Intance.OnAdd)
            {
                //wait
            }
            Journeys.Intance.Update();
            LoadGoingAndComplete();
        }

        private void DetailBtn_Click(object sender, RoutedEventArgs e)
        {
            Button currentBtn = sender as Button;
            var lv = GetAncestorOfType<ListView>(currentBtn);
            var DetailScreen = new Detail();
            if (lv.Name == "GoingListView")
            {
                Journey journey = GoingListView.SelectedItem as Journey;
                DetailScreen.currentJourney = journey;
            }
            else if (lv.Name == "CompleteListView")
            {
                Journey journey = CompleteListView.SelectedItem as Journey;
                DetailScreen.currentJourney = journey;
            }

            DetailScreen.ShowDialog();
            while (Flag.Intance.OnDetail)
            {
                //wait
            }
            LoadGoingAndComplete();
        }

        private void SearchBtnClick(object sender, RoutedEventArgs e)
        {
            string SearchValue = SearchQuery.Text;
            string NormalizedSearchValue = NormalizeString(SearchValue);
            Going = new ObservableCollection<Journey>();
            Complete = new ObservableCollection<Journey>();
            if (NormalizedSearchValue == "")
            {
                LoadGoingAndComplete();
            }
            else
            {
                if (PlaceRadioBtn.IsChecked == true)
                {
                    foreach (var journey in Journeys.Intance.Data)
                    {
                        foreach (var place in journey.Places)
                        {
                            if (NormalizeString(place.Name) == NormalizedSearchValue)
                            {
                                if (journey.Status == "Đang đi")
                                {
                                    Going.Add(journey);
                                }
                                else
                                {
                                    Complete.Add(journey);
                                }
                                break;
                            }
                        }

                    }
                }
                else
                {
                    foreach (var journey in Journeys.Intance.Data)
                    {
                        foreach (var member in journey.Members)
                        {
                            if (NormalizeString(member.Name) == NormalizedSearchValue)
                            {
                                if (journey.Status == "Đang đi")
                                {
                                    Going.Add(journey);
                                }
                                else
                                {
                                    Complete.Add(journey);
                                }
                                break;
                            }
                        }

                    }
                }
                GoingListView.ItemsSource = Going;
                CompleteListView.ItemsSource = Complete;
            }

            if(Going.Count == 0)
            {
                GoingSearchFail.Visibility = Visibility.Visible;
            }
            else
            {
                GoingSearchFail.Visibility = Visibility.Hidden;
            }

            if (Complete.Count == 0)
            {
                CompleteSearchFail.Visibility = Visibility.Visible;
            }
            else
            {
                CompleteSearchFail.Visibility = Visibility.Hidden;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void InforButton_Click(object sender, RoutedEventArgs e)
        {
            var infor = new Infor();
            infor.ShowDialog();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path += "\\Data\\Database.xml";
            var isShow = bool.Parse(Database.Intance.Data.Element("root").Element("AppSetting").Element("ShowSplashScreen").Value);
            if (isShow)
            {
                var mb = MessageBox.Show("Do you want to turn off Splash Screen?", "Splash screen Setting", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(mb == MessageBoxResult.No)
                {
                    return;
                }
                Database.Intance.Data.Element("root").Element("AppSetting").Element("ShowSplashScreen").Value = "false";
            }
            else
            {
                var mb = MessageBox.Show("Do you want to turn on Splash Screen?", "Splash screen Setting", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (mb == MessageBoxResult.No)
                {
                    return;
                }
                Database.Intance.Data.Element("root").Element("AppSetting").Element("ShowSplashScreen").Value = "true";
            }
            File.WriteAllText(path, Database.Intance.Data.ToString());
        }
    }
}