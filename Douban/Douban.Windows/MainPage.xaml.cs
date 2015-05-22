using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SQLite;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Search;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Douban
{
    /*	如何使用sqlite数据库？
    安装SQlite for Windows Runtime(Win8.1)插件，并将其添加到项目的引用中，然后再安装sqlite-net，
    成功之后便会在系统中自动添加SQLite.cs 和 SQLiteAsnync.cs 两个文件，这样便可利用他们来对sqlite数据库进行操作了。
    但由于sqlite不支持any CPU来编译，所以需要将debug环境配置为x64。又由于设计视图不可用于ARM和x64目标平台，
    所以开发过程中还是需要使用any CPU环境， 编译时再换为x64便可（注：这样的缺陷是无法使用Blend开发）
    参考链接：http://www.devdiv.com/metro%E4%B8%ADsqlite%E5%AE%89%E8%A3%85%E5%BF%83%E5%BE%97-weblog-252306-13068.html   
     */


    public sealed partial class MainPage : Page
    {
        private readonly Task _initializingTask; 
        //public static MainPage Current;
        ViewModel viewModel;
        int totalMovie;
        public MainPage()
        {
            //参考链接：http://stackoverflow.com/a/18661151
            _initializingTask = CopyDatabase(); //将数据库从项目路径复制到安装路径
            this.InitializeComponent();
            //注册搜索自动提示事件
            SearchPane.GetForCurrentView().SuggestionsRequested += searchPane_SuggestionsRequested;
            viewModel = new ViewModel();
            totalMovie = 125; //输入要展示的电影歩数
        }

        //复制数据库函数，参考链接：http://wp.qmatteoq.com/import-an-already-existing-sqlite-database-in-a-windows-8-application/
        private async Task CopyDatabase()
        {
            /*bool isDatabaseExisting = false;

            try
            {
                StorageFile storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync("doubanmovie.sqlite");
                isDatabaseExisting = true;
            }
            catch
            {
                isDatabaseExisting = false;
            }

            if (!isDatabaseExisting)
            {*/
                StorageFile databaseFile = await Package.Current.InstalledLocation.GetFileAsync("doubanmovie.sqlite");
                await databaseFile.CopyAsync(ApplicationData.Current.LocalFolder);
            //}
        }

        //用于搜索
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            viewModel = (ViewModel)e.Parameter;
            var dbpath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "doubanmovie.sqlite");
            var db = new SQLite.SQLiteConnection(dbpath);
            List<object> list = db.Query(new TableMapping(typeof(Top250)), "select * from Top250 ");
            int mindex = 0; 
                foreach (Top250 item in list)
                {
                    mindex++;
                    item.mindex = mindex;
                    item.poster = "Assets/movie_poster/" + item.id + ".jpg";
                    if (viewModel.StoreDataSampleList.Count < totalMovie)
                        viewModel.StoreDataSampleList.Add(item);
                }
            imgInfor.ItemsSource = viewModel.StoreDataSampleList;
            imgInfor.Visibility = Visibility.Visible;
            viewModel.PropertyChanged += (sender, e1) =>
            {
                if (e1.PropertyName == "SelectedItemIndex")
                {
                    imgInfor.SelectedIndex = viewModel.SelectedItemIndex;
                    imgInfor.ScrollIntoView(imgInfor.SelectedItem);
                }
            };       
        }

        //搜索建议
        void searchPane_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            foreach (Top250 storeDataSampleItem in viewModel.StoreDataSampleList)
            {
                string MaybeRight = storeDataSampleItem.name;

                if (MaybeRight.StartsWith(args.QueryText, StringComparison.CurrentCultureIgnoreCase))
                {
                    args.Request.SearchSuggestionCollection.AppendQuerySuggestion(MaybeRight);
                }
                if (args.Request.SearchSuggestionCollection.Size > 5)
                {
                    break;
                }
            }
        }

        //选中电影时，将其详细信息展示到右半栏中
        private void imgInfor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Top250 item = (Top250)imgInfor.SelectedItem;
            if (item != null)
            {
                m_director.Text = item.director;
                m_name.Text = item.name;
                m_actor.Text = item.actor;
                m_score.Text = item.score.ToString();
                m_year.Text = item.year.ToString();
                m_classification.Text = item.classification;
                m_abstracts.Text = item.abstracts;
                m_image.Source = new BitmapImage(new Uri("ms-appx:///" + "Assets/movie_poster/" + item.id + ".jpg"));
                Top250_intro.Visibility = Visibility.Collapsed;
                movie_detail.Visibility = Visibility.Visible;
            }
            popin.Begin();
        }


        //根据提供的SQL语句来对GridView中的内容进行排序输出
        private void SortTheList(String query)
        {
            var dbpath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "doubanmovie.sqlite");
            var db = new SQLite.SQLiteConnection(dbpath);
            List<object> list = db.Query(new TableMapping(typeof(Top250)), query);
            int mindex = 0;
            foreach (Top250 item in list)
            {
                mindex++;
                item.mindex = mindex;
                item.poster = "Assets/movie_poster/" + item.id + ".jpg";
            }
            imgInfor.ItemsSource = list;
            imgInfor.Visibility = Visibility.Visible;
        }
        private void DefaultSort_Click(object sender, RoutedEventArgs e)
        {
            string query = "select * from Top250";
            SortTheList( query);
            reposition.Begin();
        }

        //BottomAppBar中按钮事件，将电影按照分数由高到低排序
        private void ScoreDesc_Click(object sender, RoutedEventArgs e)
        {
            string query = "select * from Top250 order by score desc";
            SortTheList( query);
            reposition.Begin();//动画效果开始
        }

        private void ScoreAsc_Click(object sender, RoutedEventArgs e)
        {
            string query = "select * from Top250 order by score asc";
            SortTheList( query);
            reposition.Begin();
        }

        private void YearDesc_Click(object sender, RoutedEventArgs e)
        {
            string query = "select * from Top250 order by year desc";
            SortTheList( query);
            reposition.Begin();
        }

        private void YearAsc_Click(object sender, RoutedEventArgs e)
        {
            string query = "select * from Top250 order by year asc";
            SortTheList( query);
            reposition.Begin();
        }

        //显示或者隐藏排序方法
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondBar.Visibility == Visibility.Collapsed)
                SecondBar.Visibility = Visibility.Visible;
            else
                SecondBar.Visibility = Visibility.Collapsed;
        }

        private void HotComments_Click(object sender, RoutedEventArgs e)
        {
            popout.Begin();
            //将viewModel参数传递到另一页面，保存下来，切回这个页面时再传回来。否则会导致搜索失效
            this.Frame.Navigate(typeof(MovieReview), viewModel);
        }

    }

    //Top250的电影类，注意名字要与数据库中相同
    class Top250 
    {

        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        public string name {get; set;}
        public string director {get; set;}
        public string actor {get; set; }
        public string classification {get; set;}
        public float score {get;set;}
        public int year {get; set;}
        public string abstracts {get; set; }
        public string poster { get; set; } 
        public int mindex { get; set; } //用于显示实际排序情况

    }

    //用于搜索，实现搜索还要在App.xaml.cs中重载OnSearchActivated函数，并给navigate函数传递一个viewModel参数
    class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Top250> storeDataSampleList;
        private int selectedItemIndex;
        public ViewModel()
        {
            storeDataSampleList = new ObservableCollection<Top250>();
            selectedItemIndex = -1;
        }
        public int SelectedItemIndex
        {
            get
            {
                return selectedItemIndex;
            }
            set
            {
                selectedItemIndex = value; NotifyPropertyChanged("SelectedItemIndex");
            }
        }
        public ObservableCollection<Top250> StoreDataSampleList
        {
            get
            {
                return storeDataSampleList;
            }
            
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string SomeAttri)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(SomeAttri));
            }
        }
        //  搜索的功能代码
        public void Search(string searchTerm)
        {
            int selectedItemIndex = -1;
            for (int i = 0; i < StoreDataSampleList.Count; i++)
            {
                if (StoreDataSampleList[i].name.ToLower().Contains(searchTerm.ToLower()))
                {
                    selectedItemIndex = i;
                }
            }
            SelectedItemIndex = selectedItemIndex;
        }
    }
}
