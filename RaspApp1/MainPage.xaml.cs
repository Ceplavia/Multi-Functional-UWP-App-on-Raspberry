using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using WeatherNet.Clients;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace RaspApp1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.RequestedTheme = ElementTheme.Dark;
            DispatcherTimerSetup();

            
        }
        private void DispatcherTimerSetup()
        {
            var dispatcherTimer1Sec = new DispatcherTimer();
            var dispatcherTimer1Hour = new DispatcherTimer();
            dispatcherTimer1Sec.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer1Hour.Interval = new TimeSpan(0, 1, 0);
            dispatcherTimer1Sec.Tick += dispatcherTimer1Sec_Tick;
            dispatcherTimer1Hour.Tick += dispatcherTimer1Hour_Tick;
            dispatcherTimer1Sec.Start();
            //dispatcherTimer1Hour.Start();
        }
        private void dispatcherTimer1Sec_Tick(object sender, object e)
        {
            textBlock_nowTime.Text = DateTime.Now.ToString();
            textBlock_lunarDate.Text = GetChineseDateTime(DateTime.Now);
            //var result = CurrentWeather.GetByCityNameAsync("Stockholm", "Sweden", "en", "metric");
            //textBlock1.Text = result.ToString();
        }
        private async void dispatcherTimer1Hour_Tick(object sender, object e)
        {
            WeatherRootObject nowWeather = await OpenWeatherMapProxy.GetWeather("Shenzhen", "China");
            string icon = String.Format("http://openweathermap.org/img/w/{0}.png", nowWeather.weather[0].icon);
            WeatherImage.Source = new BitmapImage(new Uri(icon, UriKind.Absolute));
            textBlock1.Text = nowWeather.name + " - " + (int)(nowWeather.main.temp - 273.15) + "℃" + " - " + nowWeather.weather[0].description;
        }




        public static string GetChineseDateTime(DateTime datetime)
        {
            ChineseLunisolarCalendar cCalendar = new ChineseLunisolarCalendar();
            int lyear = cCalendar.GetYear(datetime);
            int lmonth = cCalendar.GetMonth(datetime);
            int lday = cCalendar.GetDayOfMonth(datetime);

            //获取闰月， 0 则表示没有闰月
            int leapMonth = cCalendar.GetLeapMonth(lyear,cCalendar.GetEra(datetime));
            bool isleap = false;

            if (leapMonth > 0)
            {
                if (leapMonth == lmonth)
                {
                    //闰月
                    isleap = true;
                    lmonth--;
                }
                else if (lmonth > leapMonth)
                {
                    lmonth--;
                }
            }
            return string.Concat(GetLunisolarYear(lyear), "年", isleap ? "閏" : string.Empty, GetLunisolarMonth(lmonth), "月", GetLunisolarDay(lday));
        }
        #region 农历年
        private static string[] tinGon = { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };
        private static string[] deiTsi = { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };
        private static string[] sangTsiu = { "鼠", "牛", "虎", "兔", "龍", "蛇", "馬", "羊", "猴", "雞", "狗", "豬" };
        public static string GetLunisolarYear(int year)
        {
            if (year > 3)
            {
                int tgIndex = (year - 4) % 10;
                int dzIndex = (year - 4) % 12;
                return string.Concat(tinGon[tgIndex], deiTsi[dzIndex], "[", sangTsiu[dzIndex], "]");
            }
            throw new ArgumentOutOfRangeException("無效年份!");
        }
        #endregion
        #region 农历月
        private static string[] months = { "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "腊" };
        public static string GetLunisolarMonth(int month)
        {
            if (month < 13 && month > 0)
            {
                return months[month - 1];
            }
            throw new ArgumentOutOfRangeException("無效月份!");
        }
        #endregion
        #region 农历日
        private static string[] days1 = { "初", "十", "廿", "三" };
        private static string[] days = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
        public static string GetLunisolarDay(int day)
        {
            if (day > 0 && day < 32)
            {
                if (day != 20 && day != 30)
                {
                    return string.Concat(days1[(day - 1) / 10], days[(day - 1) % 10]);
                }
                else
                {
                    return string.Concat(days[(day - 1) / 10], days1[1]);
                }
            }
            throw new ArgumentOutOfRangeException("無效日!");
        }
        #endregion

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            WeatherRootObject nowWeather = await OpenWeatherMapProxy.GetWeather("Shenzhen", "China");
            string icon = String.Format("http://openweathermap.org/img/w/{0}.png", nowWeather.weather[0].icon);
            WeatherImage.Source = new BitmapImage(new Uri(icon, UriKind.Absolute));
            textBlock1.Text = nowWeather.name + " - " + (int)(nowWeather.main.temp - 273.15) + "℃" + " - " + nowWeather.weather[0].description;
            DormFeeChecker nowDormFee = new DormFeeChecker("t41004");
            textBlock1_dormFee.Text = await nowDormFee.DataUpdate();
        }
    }
}
