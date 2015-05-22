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
using Windows.Data.Xml.Dom;
using Windows.UI.Core;
using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Douban
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MovieReview : Page
    {
        private string getUrl = "http://www.douban.com/feed/review/movie";
        private List<MovieItem> movieItems;
        ViewModel viewModel;
        public Rect WindowsRect = Window.Current.Bounds;
        int index;
        public MovieReview()
        {
            viewModel = new ViewModel();
            Window.Current.SizeChanged += sizechanged;
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
            this.InitializeComponent();
            Get();          
        }

        //保存MainPage中传过来的viewModel
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            viewModel = (ViewModel)e.Parameter;
        }

        //获取RSS的xml文件，解析并展示出来
        private async void Get()
        {
            try
            {
                status.Text = "正在获取最新的热门影评，请您耐心等候 ...";
                HttpClient httpClient = new HttpClient();
                var headers = httpClient.DefaultRequestHeaders;
                // 添加HTTP头
                //headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.2; WOW64; rv:25.0) Gecko/20100101 Firefox/25.0");
                headers.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_3) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.54 Safari/536.5");

                string url = string.Format(getUrl);
                // 使用get方法请求url
                HttpResponseMessage response = await httpClient.GetAsync(url);
                // 响应失败将抛出异常
                response.EnsureSuccessStatusCode();
                status.Text = response.StatusCode + " " + response.ReasonPhrase + Environment.NewLine;
                // 获取返回内容
                string rescontent = await response.Content.ReadAsStringAsync();
                XmlDocument xmlDoc = new XmlDocument();
                // 对返回回来的xml进行解析
                xmlDoc.LoadXml(rescontent);
                XmlElement root = xmlDoc.DocumentElement;
                XmlNodeList pubDate = root.SelectNodes("/rss/channel/pubDate");
                XmlNodeList titles = root.SelectNodes("/rss/channel/item/title");
                XmlNodeList links = root.SelectNodes("/rss/channel/item/link");
                XmlNodeList descriptions = root.SelectNodes("/rss/channel/item/description");
                XmlNodeList pubDates = root.SelectNodes("/rss/channel/item/pubDate");

                string publishDate;
                if (pubDate.Length != 0)
                    publishDate = pubDate[0].InnerText;
                else
                    publishDate = "Ask God For The Publish Date";
                VpubDate.Text = publishDate;

                movieItems = new List<MovieItem>();
                for (int i = 0; i < titles.Length; i++)
                {
                    movieItems.Add(new MovieItem { title = titles[i].InnerText, link = links[i].InnerText, description = descriptions[i].InnerText, pubDate = pubDates[i].InnerText });
                }

                //将list作为VmovieItem template的数据绑定源
                VmovieItems.ItemsSource = movieItems;
                //用于响应式布局中的另一布局
                Limginfo.ItemsSource = movieItems;
                status.Visibility = Visibility.Collapsed;
            }
            catch (HttpRequestException hre)
            {
                //status.Text = hre.ToString();
                status.Text = "啊哦，网络好像有点儿不太给力哦，试一下右键，在弹出的上方栏中点击一下“刷新”按钮吧" + "\n" + "错误代码：" + hre.ToString(); ;
            }
            catch (Exception ex)
            {
                //status.Text = ex.ToString();
                status.Text = "啊哦，网络好像有点儿不太给力哦，试一下右键，在弹出的上方栏中点击一下“刷新”按钮吧" + "\n" + "错误代码："+ ex.ToString();
            }
        }

        //共享数据设置
        void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {            
            var defl = args.Request.GetDeferral();
            // 设置数据包
            DataPackage dp = new DataPackage();
            dp.Properties.Title = "豆瓣最新影评分享";
            dp.Properties.Description = "——来自我的Win8.1客户端";
            MovieItem item = (MovieItem)VmovieItems.SelectedItem;
            dp.SetText(item.title + "\n" + item.link + "\n" + item.description + "\n" + item.pubDate );
            args.Request.Data = dp;
            // 报告操作完成
            defl.Complete();
        }

        //点击Grid view中item时，调用分享
        private void VmovieItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        //注意切换页面时需回传viewModel
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), viewModel);
        }

        //依据窗口的长宽的不同提供不同的布局
        private void sizechanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs edge)
        {
            double width = edge.Size.Width;
            double heigh = edge.Size.Height;
            if (width < heigh)
            {
                index = VmovieItems.SelectedIndex;
                VmovieItems.Visibility = Visibility.Collapsed;
                Limginfo.Visibility = Visibility.Visible;
                Limginfo.SelectedIndex = index;
            }
            else
            {
                index = Limginfo.SelectedIndex;
                Limginfo.Visibility = Visibility.Collapsed;
                VmovieItems.Visibility = Visibility.Visible;
                VmovieItems.SelectedIndex = index;
            }
        }

        //重新调用Get函数，实现刷新作用
        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            Get();
        }


    }

    class MovieItem
    {
        public String title { get; set; }
        public String link { get; set; }
        public String description { get; set; }
        public String pubDate { get; set; }
    }
}
