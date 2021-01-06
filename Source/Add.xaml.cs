using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Xml.Linq;

namespace We_Split
{
    /// <summary>
    /// Interaction logic for Add.xaml
    /// </summary>

    public partial class SelectedImage
    {
        public string Path { get; set; }
        public string Name { get; set; }

        public SelectedImage()
        {
            //Do nothing
        }
    }

    public partial class Add : Window
    {
        public Journey currentJourney { get; set; }
        public SelectedImage JourneyImage { get; set; }
        public List<SelectedImage> PlaceImage { get; set; }

        public Add()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Check Update or Add
            if (!Flag.Intance.OnUpdate)
            {
                currentJourney = new Journey();
                currentJourney.StartDate = DateTime.Now.Date;
                currentJourney.EndDate = DateTime.Now.Date;

                PlaceImage = new List<SelectedImage>();
                PlaceImage.Add(new SelectedImage());
                currentJourney.Places.Add(new Place());
                Member newMem = new Member();
                newMem.Expenses.Add(new Expense());
                currentJourney.Members.Add(newMem);
            }
            else
            {
                PlaceImage = new List<SelectedImage>();
                JourneyImage = new SelectedImage();

                var SystemPath = AppDomain.CurrentDomain.BaseDirectory + $@"Images\";

                JourneyImage.Name = currentJourney.Image;
                JourneyImage.Path = SystemPath + JourneyImage.Name;

                foreach (var place in currentJourney.Places)
                {
                    SelectedImage placeImg = new SelectedImage();
                    placeImg.Name = place.Image;
                    placeImg.Path = SystemPath + placeImg.Name;
                    PlaceImage.Add(placeImg);
                }
            } 
            #endregion

            if(currentJourney.Id != -1)
            {
                AddTitle.Text = "CẬP NHẬT CHUYẾN ĐI";
            }
            AddWindow.DataContext = currentJourney;
            MemberListView.ItemsSource = currentJourney.Members.ToList();
            PlaceListView.ItemsSource = currentJourney.Places.ToList();
        }

        private string GetFileName(string path)
        {
            char delim = '\\';
            var tokens = path.Split(delim);

            string result = tokens[tokens.Count() - 1];

            return result;
        }

        private string GetFileExtension(string name)
        {
            char delim = '.';

            var tokens = name.Split(delim);
            string result = "." + tokens[tokens.Count() - 1];

            return result;
        }

        private BitmapImage LoadImage(string Path)
        {
            var BMPImg = new BitmapImage();
            BMPImg.BeginInit();
            BMPImg.CacheOption = BitmapCacheOption.OnLoad;
            BMPImg.UriSource = new Uri(Path, UriKind.RelativeOrAbsolute);
            BMPImg.EndInit();
            return BMPImg;
        }

        private void BrowseImage(object sender, RoutedEventArgs e)
        {
            var index = PlaceListView.SelectedIndex;
            string name = ((Button)sender).Name;
            SelectedImage currentImage = new SelectedImage();

            var openfileDialog = new OpenFileDialog
            {
                Title = "Pick an image"
            };

            if (openfileDialog.ShowDialog() == true)
            {
                currentImage.Path = openfileDialog.FileName;
                currentImage.Name = GetFileName(currentImage.Path);
                if (name == "JourneyBrowse")
                {
                    JourneyImage = currentImage;
                    currentJourney.BMPImg = LoadImage(JourneyImage.Path);
                    JourneyImg.Source = currentJourney.BMPImg;
                }
                else if (name == "PlaceBrowse")
                {                 
                    PlaceImage[index] = currentImage;
                    currentJourney.Places[index].BMPImg = LoadImage(PlaceImage[index].Path);
                    PlaceListView.ItemsSource = currentJourney.Places.ToList();
                }
            }
        }

        private void AddPlaceMember_Click(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Name;
            
            if(name == "AddPlace")
            {
                Place newPlace = new Place();
                currentJourney.Places.Add(newPlace);
                PlaceImage.Add(new SelectedImage());
                PlaceListView.ItemsSource = currentJourney.Places;
            }
            else if(name == "AddMember")
            {
                Member newMember = new Member();
                newMember.Expenses.Add(new Expense());
                currentJourney.Members.Add(newMember);
                MemberListView.ItemsSource = currentJourney.Members;
            }
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            Member member = MemberListView.SelectedItem as Member;
            member.Expenses.Add(new Expense());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var journeys = Database.Intance.Data.Root.Element("Journeys").Elements();
            var SystemPath = AppDomain.CurrentDomain.BaseDirectory + $@"Images\";

            int currentID = 0;
            if (currentJourney.Id != -1)
            {
                currentID = currentJourney.Id;
            }
            else
            {
                for (int i = 0; i < Journeys.Intance.Data.Count; ++i)
                {
                    bool isIn = false;

                    foreach (var journey in Journeys.Intance.Data)
                    {
                        if (i == journey.Id)
                        {
                            isIn = true;
                            break;
                        }
                    }

                    if (!isIn)
                    {
                        currentID = i;
                        break;
                    }
                }
            }

            foreach (var journey in journeys)
            {
                if (int.Parse(journey.Element("Id").Value) == currentJourney.Id)
                {
                    journey.Remove();
                }
            }


            #region Handling Journey's Image
            if (JourneyImage.Name != null)
            {
                if(File.Exists(SystemPath + currentJourney.Image) && (SystemPath + currentJourney.Image) != JourneyImage.Path)
                {
                    File.Delete(SystemPath + currentJourney.Image);
                }

                if(!File.Exists(SystemPath + currentJourney.Image))
                {
                    var extension = GetFileExtension(JourneyImage.Name);
                    JourneyImage.Name = $"{currentID}{extension}";
                    var DestinationPath = SystemPath + JourneyImage.Name;
                    File.Copy(JourneyImage.Path, DestinationPath);
                }
            }
            #endregion

            ComboBoxItem ComboItem = (ComboBoxItem)Status.SelectedItem;
            Database.Intance.Data.Root.Element("Journeys").Add(new XElement("Journey",
                                                        new XElement("Id",currentID),
                                                        new XElement("Name", currentJourney.Name),
                                                        new XElement("Description", currentJourney.Description),
                                                        new XElement("Status", ComboItem.Content),
                                                        new XElement("StartDate", currentJourney.StartDate.ToShortDateString()),
                                                        new XElement("EndDate", currentJourney.EndDate.ToShortDateString()),
                                                        new XElement("Image", JourneyImage.Name),
                                                        new XElement("Places"),
                                                        new XElement("Members")
                                                          ));

            #region Add Places
            var PlacesElement = (Database.Intance.Data.Root.Element("Journeys").LastNode as XElement).Element("Places");
            for (int i = 0; i < currentJourney.Places.Count; ++i)
            {
                Place place = currentJourney.Places[i];
                if (File.Exists(SystemPath + place.Image) && (SystemPath + place.Image) != PlaceImage[i].Path)
                {
                    File.Delete(SystemPath + place.Image);
                }

                if (!File.Exists(SystemPath + place.Image))
                {
                    var extension = GetFileExtension(PlaceImage[i].Name);
                    PlaceImage[i].Name = $"{currentID}.{i+1}{extension}";
                    var DestinationPath = SystemPath + PlaceImage[i].Name;
                    File.Copy(PlaceImage[i].Path, DestinationPath);
                }

                var temp = new XElement("Place",
                    new XElement("Name", place.Name),
                    new XElement("Description", place.Description),
                    new XElement("Image", PlaceImage[i].Name));

                PlacesElement.Add(temp);
            }
            #endregion

            #region Add Members
            var MembersElement = (Database.Intance.Data.Root.Element("Journeys").LastNode as XElement).Element("Members");
            for (int i = 0; i < currentJourney.Members.Count; ++i)
            {
                Member member = currentJourney.Members[i];
                var temp = new XElement("Member",
                    new XElement("Name", member.Name),
                    new XElement("Phone", member.Phone),
                    new XElement("Remain"),
                    new XElement("Expenses"));

                for (int j = 0; j < member.Expenses.Count; ++j)
                {
                    Expense expense = member.Expenses[j];
                    var temp_ex = new XElement("Expense",
                    new XElement("Name", expense.Name),
                    new XElement("Cost", expense.Cost));
                    temp.Element("Expenses").Add(temp_ex);
                }
                MembersElement.Add(temp);
            }
            #endregion

            Journeys.Intance.Update();
            WriteDownDatabase();

            if (currentJourney.Id == -1)
            {
                MessageBox.Show("Thêm thành công!");
            }
            else
            {
                MessageBox.Show("Cập nhật thành công!");
            }
            Flag.Intance.OnAdd = false;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var MessageBoxBtn = MessageBox.Show("If you close this window, all data will be lost.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (MessageBoxBtn == MessageBoxResult.Cancel)
            {
                return;
            }
            Flag.Intance.OnAdd = false;
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