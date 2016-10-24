// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using ServiceHelpers;
using IntelligentKioskSample.Controls;
using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Reflection;
using Windows.UI.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IntelligentKioskSample.Views
{
    class TimeCounter
    {
        public int count = 0;
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [KioskExperience(Title = "Realtime Crowd Insights", ImagePath = "ms-appx:/Assets/realtime.png", ExperienceType = ExperienceType.Kiosk)]
    public sealed partial class RealTimeDemo : Page, IRealTimeDataProvider
    {
        private Task processingLoopTask;
        private bool isProcessingLoopInProgress;
        private bool isProcessingPhoto;

        private IEnumerable<Emotion> lastEmotionSample;
        private IEnumerable<Face> lastDetectedFaceSample;
        private IEnumerable<Tuple<Face, IdentifiedPerson>> lastIdentifiedPersonSample;
        private IEnumerable<SimilarFaceMatch> lastSimilarPersistedFaceSample;

        private DemographicsData demographics;
        private Dictionary<Guid, Visitor> visitors = new Dictionary<Guid, Visitor>();
        private TimeCounter timeCounter = new TimeCounter();
        private Timer t1;
        public RealTimeDemo()
        {
            this.InitializeComponent();

            this.DataContext = this;

            Window.Current.Activated += CurrentWindowActivationStateChanged;
            this.cameraControl.SetRealTimeDataProvider(this);
            this.cameraControl.FilterOutSmallFaces = true;
            this.cameraControl.HideCameraControls();
            this.cameraControl.CameraAspectRatioChanged += CameraControl_CameraAspectRatioChanged;

        }

        private void CameraControl_CameraAspectRatioChanged(object sender, EventArgs e)
        {
            this.UpdateCameraHostSize();
        }

        private void StartProcessingLoop()
        {
            this.isProcessingLoopInProgress = true;

            if (this.processingLoopTask == null || this.processingLoopTask.Status != TaskStatus.Running)
            {
                this.processingLoopTask = Task.Run(() => ProcessingLoop());
                //cameraControl.Visibility = Visibility.Collapsed;
                
            }
        }
        private string adName,adchose,adtitle;
        private int nopeople=0,adcount=0;
        private int cat1,cat2,cat3,cat4,cat5;
        private int sound=0,adtime;
        private int musictime,countup;
        private async void ProcessingLoop()
        {
            while (this.isProcessingLoopInProgress)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    time.Text = DateTime.Now.ToString();
                    status.Text = ImageAnalyzer.state;
                });

                if (sound == 0)
                {
                    musictime = (DateTime.Now.Hour) * 3600 + (DateTime.Now.Minute) * 60 + (DateTime.Now.Second);
                    BackgroundMediaPlayer.Current.SetUriSource(new Uri("ms-appx:///Musics/At_The_Fair.mp3"));
                    BackgroundMediaPlayer.Current.Play();
                    sound = 1;
                }
                if (musictime + 123 <= (DateTime.Now.Hour) * 3600 + (DateTime.Now.Minute) * 60 + (DateTime.Now.Second))
                {
                    BackgroundMediaPlayer.Current.SetUriSource(new Uri("ms-appx:///Musics/At_The_Fair.mp3"));
                    BackgroundMediaPlayer.Current.Play();
                    musictime = (DateTime.Now.Hour) * 3600 + (DateTime.Now.Minute) * 60 + (DateTime.Now.Second);
                }

                //當偵測到可以開始執行廣告時
                if (this.overallStatsControl.ADinit() == 1)
                {
                    countup++;
                }
                    
                if (countup==30)
                {
                    countup = 0;
                    sound = 0;
                    BackgroundMediaPlayer.Current.SetUriSource(new Uri("ms-appx:///Musics/Friction_Looks.mp3"));
                    BackgroundMediaPlayer.Current.Volume = 0.5;
                    BackgroundMediaPlayer.Current.Play();

                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Title.Text = "";
                        Subtitle.Text = "";
                        Hint.Visibility = Visibility.Visible;
                        Hint.Text = "Analyzing...";

                        cameraControl.Height = 120;
                        cameraControl.Width = 200;
                        cameraControl.Margin = new Thickness(100, 250, -250, -235);
                        cameraControl.HorizontalAlignment = HorizontalAlignment.Right;
                        cameraControl.VerticalAlignment = VerticalAlignment.Bottom;
                        belowcontent.Visibility = Visibility.Collapsed;
                    });
                    if (Boy0To18 + Boy19To30 + Boy31To44 + Boy45To64 + Boy65To100 + Girl0To18 + Girl19To30 + Girl31To44 + Girl45To64 + Girl65To100 == 0)
                        nopeople = 1;
                    if (nopeople == 0)
                    {
                        int total_boy = Boy0To18 + Boy19To30 + Boy31To44 + Boy45To64 + Boy65To100;
                        int total_girl = Girl0To18 + Girl19To30 + Girl31To44 + Girl45To64 + Girl65To100;
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            Hint.Text = string.Format("I see {0} children, {1} women and {2} men" + "\n" + "\n" + "\n", (Boy0To18 + Girl0To18), (total_girl - Girl0To18), (total_boy - Boy0To18));

                        });
                        for (int i = 0; i < 2; i++)
                        {
                            await getnewage();
                            await Task.Delay(1000);
                        }
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            subhint.Visibility = Visibility.Visible;
                            subhint.Text = "Am I smart?";
                            subhint.Opacity = 0.1;
                        });
                        
                        for(int i = 2; i < 11; i++)
                        {
                            await Task.Delay(150);
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                subhint.Opacity = i*0.1;
                            });
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            await getnewage();
                            await Task.Delay(1000);
                        }
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            subhint.Visibility = Visibility.Collapsed;
                            int[] AgeDist = { Boy0To18 + Girl0To18, Boy19To30 + Girl19To30, Boy31To44 + Girl31To44,
                                Boy45To64 + Girl45To64, Boy65To100 + Girl65To100 };
                            Hint.Text = string.Format("I think you belongs to the age group of 20 below" + "\n" + "\n" + "\n");

                            for (int i = 1; i < 5; i++)
                            {
                                if (AgeDist[i] > AgeDist[0])
                                {
                                    if (i == 1) Hint.Text = string.Format("I think you belongs to the age group of 21-30！" + "\n" + "\n" + "\n");
                                    else if (i == 2) Hint.Text = string.Format("I think you belongs to the age group of 31-44！" + "\n" + "\n" + "\n");
                                    else if (i == 3) Hint.Text = string.Format("I think you belongs to the age group of 45-64！" + "\n" + "\n" + "\n");
                                    else if (i == 4) Hint.Text = string.Format("I think you belongs to the age group of 65 above！" + "\n" + "\n" + "\n");
                                }
                            }
                        });
                        await loopHint();

                        await Task.Delay(300);
                        //挑選影片(根據年齡)
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            subhint.Visibility = Visibility.Collapsed;
                            Hint.Visibility = Visibility.Collapsed;
                            spin.Visibility = Visibility.Visible;

                            bord1.Visibility = Visibility.Visible;
                            bigbord.Visibility = Visibility.Visible;
                            bigbord_up.Visibility = Visibility.Visible;
                            yearAD.Visibility = Visibility.Visible;
                            ad1.Visibility = Visibility.Visible;
                            ad1name.Visibility = Visibility.Visible;
                            ad2.Visibility = Visibility.Visible;
                            ad2name.Visibility = Visibility.Visible;
                            ad3.Visibility = Visibility.Visible;
                            ad3name.Visibility = Visibility.Visible;

                            int[] AgeDist = { Boy0To18 + Girl0To18, Boy19To30 + Girl19To30, Boy31To44 + Girl31To44,
                                Boy45To64 + Girl45To64, Boy65To100 + Girl65To100 };
                            int sortcat = 0;
                            for (int i = 1; i < 5; i++)
                            {
                                if (AgeDist[i] > AgeDist[sortcat]) sortcat = i;
                            }
                            if (sortcat == 0)//0-18
                            {
                                //yearAD.Text = "For Teanagers";
                                //adchose = "who are less than 20 years-old"; // 螢幕上方文字
                                adcount = cat1;
                                ad1name.Text = "Windows 10"; ad2name.Text = "Windows10 - Tean-astronaut"; ad3name.Text = "Microsoft HoloLens - Young Conker";
                                ad1.Source = new Uri("ms-appx:///Videos/2. Windows/2. Windows 10 未來始於現在 - 30秒篇.mp4");
                                ad2.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/太空人.png", UriKind.Absolute));
                                ad3.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/conker.png", UriKind.Absolute));
                                if (cat1 == 0) { mediaElement.Source = new Uri("ms-appx:///Videos/2. Windows/2. Windows 10 未來始於現在 - 30秒篇.mp4"); adName = "Windows 10"; adtitle = "Windows 10"; adtime = 30000; }
                                else if (cat1 == 1) { mediaElement.Source = new Uri("ms-appx:///Videos/2. Windows/3. Windows 10 未來始於現在 - 小小太空人篇.mp4"); adName = "Windows10 - Tean-astronaut"; adtitle = "Windows 10"; adtime = 30000; }
                                else if (cat1 == 2) { mediaElement.Source = new Uri("ms-appx:///Videos/5. HoloLens/9. Microsoft HoloLens- Young Conker.mp4"); adName = "Microsoft HoloLens- Young Conker"; adtitle = "HoloLens"; adtime = 129000; }
                                cat1++;
                                cat1 = cat1 % 3;
                            }
                            else if (sortcat == 1)//19-30
                            {
                                ad1name.Text = "Windows"; ad2name.Text = "Office 365"; ad3name.Text = "A Surface day";
                                ad1.Source = new Uri("ms-appx:///Videos/2. Windows/4. 盧廣仲 Windows.mp4");
                                ad2.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/IT.png", UriKind.Absolute));
                                ad3.Source = new Uri("ms-appx:///Videos/7. Surface/1. 劉軒 x Surface 的一天.mp4");

                                //yearAD.Text = "For young man/woman";
                                //adchose = "who are between 20-30 years-old"; // 螢幕上方文字
                                adcount = cat2;
                                if (cat2 == 0) { mediaElement.Source = new Uri("ms-appx:///Videos/2. Windows/4. 盧廣仲 Windows.mp4"); adName = "Windows"; adtitle = "Windows"; adtime = 138000; }
                                else if (cat2 == 1) { mediaElement.Source = new Uri("ms-appx:///Videos/3. Office 365/3. IT不再背黑鍋！有微軟 Office 365，行動辦公更容易！.mp4"); adName = "Office 365"; adtitle = "Office 365"; adtime = 86000; }
                                else if (cat2 == 2) { mediaElement.Source = new Uri("ms-appx:///Videos/7. Surface/1. 劉軒 x Surface 的一天.mp4"); adName = "Surface"; adtitle = "A Surface day"; adtime = 62000; }
                                cat2++;
                                cat2 = cat2 % 3;
                            }
                            else if (sortcat == 2)//31-44
                            {
                                ad1name.Text = "Xbox Play Anywhere"; ad2name.Text = "Microsoft Cloud x Temenos"; ad3name.Text = "The most powerful games are in Xbox One";
                                ad1.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/xbox1.png", UriKind.Absolute));
                                ad2.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/31-45(2).png", UriKind.Absolute));
                                ad3.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/xbox3.png", UriKind.Absolute));

                                //yearAD.Text = "For mature";
                                //adchose = "who are between 31-44 years-old"; // 螢幕上方文字
                                adcount = cat3;
                                if (cat3 == 0) { mediaElement.Source = new Uri("ms-appx:///Videos/6. Xbox/1. Xbox Play Anywhere - Anywhere is a great place to play.mp4"); adName = "Xbox Play Anywhere"; adtitle = "Xbox"; adtime = 100000; }
                                else if (cat3 == 1) { mediaElement.Source = new Uri("ms-appx:///Videos/4. 其他/3. Microsoft Cloud 幫助 Temenos 將金融服務帶給偏鄉數百萬的人民 [社會]"); adName = "Xbox one"; adtitle = "Microsoft Cloud"; adtime = 68000; }
                                else if (cat3 == 2) { mediaElement.Source = new Uri("ms-appx:///Videos/6. Xbox/2. 最強大的遊戲陣容 都在Xbox One.mp4"); adName = "Xbox one"; adtitle = "Xbox One"; adtime = 61000; }
                                cat3++;
                                cat3 = cat3 % 3;
                            }
                            else if (sortcat == 3)//45-64
                            {
                                ad1name.Text = "Pepper x Microsoft Azure"; ad2name.Text = "The Worthy assistant - Windows"; ad3name.Text = "The latest creative ways to share - Sway";
                                ad1.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/pepper.png", UriKind.Absolute));
                                ad2.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/windows幫手.png", UriKind.Absolute));
                                ad3.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/sway.png", UriKind.Absolute));

                                //yearAD.Text = "For Prime";
                                //adchose = "who are between 45-64 years-old"; // 螢幕上方文字
                                adcount = cat4;
                                if (cat4 == 0) { mediaElement.Source = new Uri("ms-appx:///Videos/1. AzureSurface Pro 4/1. Pepper x Microsoft Azure 實現未來購物新體驗.mp4"); adName = "Pepper x Microsoft Azure"; adtitle = "AzureSurface Pro"; adtime = 167000; }
                                else if (cat4 == 1) { mediaElement.Source = new Uri("ms-appx:///Videos/2. Windows/1. 它是我的幫手，它是Windows.mp4"); adName = "The Worthy assistant - Windows"; adtitle = "Windows"; adtime = 84000; }
                                else if (cat4 == 2) { mediaElement.Source = new Uri("ms-appx:///Videos/3. Office 365/2. Sway-體驗最新分享創意的方式.mp4"); adName = "The latest creative ways to share - Sway"; adtitle = "Sway"; adtime = 111000; }
                                cat4++;
                                cat4 = cat4 % 3;
                            }
                            else//65-
                            {
                                ad1name.Text = "Microsoft HoloLens x Skype"; ad2name.Text = "Microsoft Cognitive Services"; ad3name.Text = "記得我怕黑";
                                ad1.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/skype.png", UriKind.Absolute));
                                ad2.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/cognitive.png", UriKind.Absolute));
                                ad3.PosterSource = new BitmapImage(new Uri("ms-appx:///Assets/怕黑.png", UriKind.Absolute));

                                //yearAD.Text = "For Senior";
                                //adchose = "who are more than 65 years-old"; // 螢幕上方文字
                                adcount = cat5;
                                if (cat5 == 0) { mediaElement.Source = new Uri("ms-appx:///Videos/5. HoloLens/8. Microsoft HoloLens- Skype - Brings Us Together.mp4"); adName = "Microsoft HoloLens x Skype"; adtitle = "HoloLens"; adtime = 35000; }
                                else if (cat5 == 1) { mediaElement.Source = new Uri("ms-appx:///Videos/5. HoloLens/1. Microsoft Cognitive Services- Introducing the Seeing AI project.mp4"); adName = "Microsoft Cognitive Services"; adtitle = "HoloLens"; adtime = 164000; }
                                else if (cat5 == 2) { mediaElement.Source = new Uri("ms-appx:///Videos/4. 其他/1. 記得我怕黑 – 關懷失親兒童.mp4"); adName = "記得我怕黑 – 關懷失親兒童"; adtitle = "Microsoft"; adtime = 75000; }
                                cat5++;
                                cat5 = cat5 % 3;
                            }

                            cameraControl.Margin = new Thickness(100, 250, -250, -340);

                            spin.Text = string.Format("I guess you may like -");

                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                            bord2.Visibility = Visibility.Visible;
                            bord1.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                            bord3.Visibility = Visibility.Visible;
                            bord2.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                            bord1.Visibility = Visibility.Visible;
                            bord3.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                            bord2.Visibility = Visibility.Visible;
                            bord1.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                            bord3.Visibility = Visibility.Visible;
                            bord2.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                            bord1.Visibility = Visibility.Visible;
                            bord3.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                            bord2.Visibility = Visibility.Visible;
                            bord1.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                            bord3.Visibility = Visibility.Visible;
                            bord2.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                            bord1.Visibility = Visibility.Visible;
                            bord3.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                            bord2.Visibility = Visibility.Visible;
                            bord1.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                            bord3.Visibility = Visibility.Visible;
                            bord2.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                            bord1.Visibility = Visibility.Visible;
                            bord3.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                            bord2.Visibility = Visibility.Visible;
                            bord1.Visibility = Visibility.Collapsed;
                        });
                        ////////////////
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                            bord3.Visibility = Visibility.Visible;
                            bord2.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                            bord1.Visibility = Visibility.Visible;
                            bord3.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");

                            bord2.Visibility = Visibility.Visible;
                            bord1.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like /");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like |");
                        });
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like \\");
                        });
                        /////////////////
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format("I guess you may like -");
                        });
                        await Task.Delay(100);

                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            bord3.Visibility = Visibility.Visible;
                            bord2.Visibility = Visibility.Collapsed;
                        });
                        await Task.Delay(100);
                      
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            spin.Text = string.Format(" {0} !", adName);
                            if (adcount == 0)
                            {
                                bord1.Visibility = Visibility.Visible;
                                bord3.Visibility = Visibility.Collapsed;
                            }
                            else if (adcount == 1)
                            {
                                bord2.Visibility = Visibility.Visible;
                                bord3.Visibility = Visibility.Collapsed;
                            }
                        });
                        BackgroundMediaPlayer.Current.SetUriSource(new Uri("ms-appx:///Videos/laugh.mp3"));
                        BackgroundMediaPlayer.Current.Volume = 1;
                        BackgroundMediaPlayer.Current.Play();
                        for (int i = 0; i < 4; i++)
                        {
                            await getnewage();
                            await Task.Delay(1000);
                        }
                        BackgroundMediaPlayer.Current.Pause();
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {

                            bord1.Visibility = Visibility.Collapsed;
                            bord2.Visibility = Visibility.Collapsed;
                            bord3.Visibility = Visibility.Collapsed;
                            bigbord.Visibility = Visibility.Collapsed;
                            bigbord_up.Visibility = Visibility.Collapsed;
                            yearAD.Visibility = Visibility.Collapsed;
                            cameraControl.Margin = new Thickness(100, 250, -250, -40);
                            ad1.Visibility = Visibility.Collapsed;
                            ad1name.Visibility = Visibility.Collapsed;
                            ad2.Visibility = Visibility.Collapsed;
                            ad2name.Visibility = Visibility.Collapsed;
                            ad3.Visibility = Visibility.Collapsed;
                            ad3name.Visibility = Visibility.Collapsed;
                            spin.Visibility = Visibility.Collapsed;

                            mediaElement.Visibility = Visibility.Visible;
                            mediaElement.Play();
                            Title.Visibility = Visibility.Visible;
                            Title.Text = adtitle;
                        });

                        //fullversion AD
                        while (adtime > 0)
                        {
                            adtime -= 1000;
                            await getnewage();
                            await Task.Delay(1000);
                        }
                        /*10sec AD function
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            debugText.Visibility = Visibility.Visible;
                            backad.Visibility = Visibility.Visible;
                        });
                        for(int i = 10; i > 0; i--)
                        {
                            await getnewage();
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                debugText.Text = Convert.ToString(i) + " seconds left";
                            });
                            await Task.Delay(1000);
                        }
                        */
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            mediaElement.Visibility = Visibility.Collapsed;
                            mediaElement.Pause();
                            Title.Visibility = Visibility.Collapsed;
                            Hint.Visibility = Visibility.Visible;
                            Hint.Text = "Isn't it incredible?" + "\n" + "\n" + "Let cognitive services reshape our world!";
                       
                            cameraControl.Margin = new Thickness(100, 250, -250, -290);
                        });
                        await Task.Delay(5000);
                    }
                    else
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            Hint.Text = "Hey guys, don't be shy!";
                        });
                        //await Task.Delay(2000);
                        BackgroundMediaPlayer.Current.Pause();
                    }

                    nopeople = 0;
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        cameraControl.Margin = new Thickness(0, 0, 0, 0);
                        Title.Visibility = Visibility.Visible;
                        //Title.Text = "帥哥美女看過來";
                        Subtitle.Visibility = Visibility.Visible;
                        Title.Text = "Real Time Face Analytics";
                        Subtitle.Text = "Let me read your mind...";
                        //Subtitle.Text = "請對著鏡頭停留五秒，擺出你(們)最想被看到的表情";
                        Hint.Visibility = Visibility.Collapsed;
                        cameraControl.Height = Double.NaN;
                        cameraControl.Width = Double.NaN;
                        cameraControl.HorizontalAlignment = HorizontalAlignment.Center;
                        cameraControl.VerticalAlignment = VerticalAlignment.Center;
                        cameraControl.Visibility = Visibility.Visible;
                        belowcontent.Visibility = Visibility.Visible;
                        debugText.Visibility = Visibility.Collapsed;
                    });
                    this.overallStatsControl.ADcancel();
                }

                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    if (!this.isProcessingPhoto)
                    {
                        //每天清除demographics(介面左下)的數據

                        if (DateTime.Now.Day != this.demographics.StartTime.Day)
                        {
                            // We have been running through the hour. Reset the data...
                            await this.ResetDemographicsData();
                            this.UpdateDemographicsUI();
                        }

                        this.isProcessingPhoto = true;

                        //鏡頭前沒有人時
                        if (this.cameraControl.NumFacesOnLastFrame == 0)
                        {
                            Boy0To18 = 0; Boy19To30 = 0; Boy31To44 = 0; Boy45To64 = 0; Boy65To100 = 0;
                            Girl0To18 = 0; Girl19To30 = 0; Girl31To44 = 0; Girl45To64 = 0; Girl65To100 = 0;
                            await this.ProcessCameraCapture(null);
                        }
                        //鏡頭前有人
                        else
                        {
                            await this.ProcessCameraCapture(await this.cameraControl.TakeAutoCapturePhoto());
                        }

                    }
                });
                await Task.Delay(1000);//每一秒上傳照片一次
            }
        }
        private async Task loopHint()
        {
            int count = 0;
            while (count < 20)
            {
                String s = "";
                if (count % 4 == 0) s = "";
                else if (count % 4 == 1) s = ".";
                else if (count % 4 == 2) s = "..";
                else if (count % 4 == 3) s = "...";
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    subhint.Visibility = Visibility.Visible;
                    subhint.Text = "Humm, let me find something interesting for you" + s;
                });
                await Task.Delay(300);
                count++;
            }
        }
        private async Task getnewage()
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (!this.isProcessingPhoto)
                {
                    this.isProcessingPhoto = true;
                    if (this.cameraControl.NumFacesOnLastFrame == 0)
                    {
                        await this.ProcessCameraCapture(null);
                    }
                    else
                    {
                        await this.ProcessCameraCapture(await this.cameraControl.TakeAutoCapturePhoto());
                    }
                }
            });
        }

        private async void CurrentWindowActivationStateChanged(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if ((e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.CodeActivated ||
                e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.PointerActivated) &&
                this.cameraControl.CameraStreamState == Windows.Media.Devices.CameraStreamState.Shutdown)
            {
                // When our Window loses focus due to user interaction Windows shuts it down, so we 
                // detect here when the window regains focus and trigger a restart of the camera.
                await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: true);
            }
        }

        private async Task ProcessCameraCapture(ImageAnalyzer e)
        {
            if (e == null)
            {
                this.lastDetectedFaceSample = null;
                this.lastIdentifiedPersonSample = null;
                this.lastSimilarPersistedFaceSample = null;
                this.lastEmotionSample = null;
                this.debugText.Text = "";

                this.isProcessingPhoto = false;
                return;
            }

            DateTime start = DateTime.Now;

            // Compute Emotion, Age and Gender
            await Task.WhenAll(e.DetectEmotionAsync(), e.DetectFacesAsync(detectFaceAttributes: true));

            if (!e.DetectedEmotion.Any())
            {
                this.lastEmotionSample = null;
                this.ShowTimelineFeedbackForNoFaces();
            }
            else
            {
                this.lastEmotionSample = e.DetectedEmotion;

                Scores averageScores = new Scores
                {
                    Happiness = e.DetectedEmotion.Average(em => em.Scores.Happiness),
                    Anger = e.DetectedEmotion.Average(em => em.Scores.Anger),
                    Sadness = e.DetectedEmotion.Average(em => em.Scores.Sadness),
                    Contempt = e.DetectedEmotion.Average(em => em.Scores.Contempt),
                    Disgust = e.DetectedEmotion.Average(em => em.Scores.Disgust),
                    Neutral = e.DetectedEmotion.Average(em => em.Scores.Neutral),
                    Fear = e.DetectedEmotion.Average(em => em.Scores.Fear),
                    Surprise = e.DetectedEmotion.Average(em => em.Scores.Surprise)
                };

                this.emotionDataTimelineControl.DrawEmotionData(averageScores);
            }

            if (e.DetectedFaces == null || !e.DetectedFaces.Any())
            {
                this.lastDetectedFaceSample = null;
            }
            else
            {
                this.lastDetectedFaceSample = e.DetectedFaces;
            }

            // Compute Face Identification and Unique Face Ids
            await Task.WhenAll(e.IdentifyFacesAsync(), e.FindSimilarPersistedFacesAsync());

            if (!e.IdentifiedPersons.Any())
            {
                this.lastIdentifiedPersonSample = null;
            }
            else
            {
                this.lastIdentifiedPersonSample = e.DetectedFaces.Select(f => new Tuple<Face, IdentifiedPerson>(f, e.IdentifiedPersons.FirstOrDefault(p => p.FaceId == f.FaceId)));
            }

            if (!e.SimilarFaceMatches.Any())
            {
                this.lastSimilarPersistedFaceSample = null;
            }
            else
            {
                this.lastSimilarPersistedFaceSample = e.SimilarFaceMatches;
            }

            this.UpdateDemographics(e);

            //this.debugText.Text = string.Format("Latency: {0}ms", (int)(DateTime.Now - start).TotalMilliseconds);

            this.isProcessingPhoto = false;
        }

        private void ShowTimelineFeedbackForNoFaces()
        {
            this.emotionDataTimelineControl.DrawEmotionData(new Scores { Neutral = 1 });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            EnterKioskMode();

            if (string.IsNullOrEmpty(SettingsHelper.Instance.EmotionApiKey) || string.IsNullOrEmpty(SettingsHelper.Instance.FaceApiKey))
            {
                await new MessageDialog("Missing Face or Emotion API Key. Please enter a key in the Settings page.", "Missing API Key").ShowAsync();
            }
            else
            {
                await FaceListManager.Initialize();

                await ResetDemographicsData();
                this.UpdateDemographicsUI();

                await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: true);
                this.StartProcessingLoop();
            }

            base.OnNavigatedTo(e);
        }
        private int youngest;
        private int Boy0To18, Boy19To30, Boy31To44, Boy45To64, Boy65To100;
        private int Girl0To18, Girl19To30, Girl31To44, Girl45To64, Girl65To100;
        private void UpdateDemographics(ImageAnalyzer img)
        {
            
            if (this.lastSimilarPersistedFaceSample != null)
            {
                bool demographicsChanged = false;
                Boy0To18 = 0; Boy19To30 = 0; Boy31To44 = 0; Boy45To64 = 0; Boy65To100 = 0;
                Girl0To18 = 0; Girl19To30 = 0; Girl31To44 = 0; Girl45To64 = 0; Girl65To100 = 0;
                youngest = 100;
                // Update the Visitor collection (either add new entry or update existing)
                foreach (var item in this.lastSimilarPersistedFaceSample)
                {
                    Visitor visitor;
                    if (item.Face.FaceAttributes.Age < youngest) youngest = Convert.ToInt32(item.Face.FaceAttributes.Age);
                    //count this_time boy num & girl num
                    if (item.Face.FaceAttributes.Gender == "male")
                    {
                        if (item.Face.FaceAttributes.Age <= 20) Boy0To18++;
                        else if (item.Face.FaceAttributes.Age <= 30) Boy19To30++;
                        else if (item.Face.FaceAttributes.Age <= 44) Boy31To44++;
                        else if (item.Face.FaceAttributes.Age <= 64) Boy45To64++;
                        else Boy65To100++;
                    }
                    else
                    {
                        if (item.Face.FaceAttributes.Age <= 18) Girl0To18++;
                        else if (item.Face.FaceAttributes.Age <= 30) Girl19To30++;
                        else if (item.Face.FaceAttributes.Age <= 44) Girl31To44++;
                        else if (item.Face.FaceAttributes.Age <= 64) Girl45To64++;
                        else Girl65To100++;
                    }

                    if (this.visitors.TryGetValue(item.SimilarPersistedFace.PersistedFaceId, out visitor))
                    {
                        visitor.Count++;
                    }
                    else
                    {
                        demographicsChanged = true;
                        
                        visitor = new Visitor { UniqueId = item.SimilarPersistedFace.PersistedFaceId, Count = 1 };
                        this.visitors.Add(visitor.UniqueId, visitor);
                        this.demographics.Visitors.Add(visitor);

                        // Update the demographics stats. We only do it for new visitors to avoid double counting. 
                        AgeDistribution genderBasedAgeDistribution = null;
                        if (string.Compare(item.Face.FaceAttributes.Gender, "male", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.demographics.OverallMaleCount++;
                            genderBasedAgeDistribution = this.demographics.AgeGenderDistribution.MaleDistribution;
                        }
                        else
                        {
                            this.demographics.OverallFemaleCount++;
                            genderBasedAgeDistribution = this.demographics.AgeGenderDistribution.FemaleDistribution;
                        }

                        if (item.Face.FaceAttributes.Age < 16)
                        {
                            genderBasedAgeDistribution.Age0To15++;
                        }
                        else if (item.Face.FaceAttributes.Age < 20)
                        {
                            genderBasedAgeDistribution.Age16To19++;
                        }
                        else if (item.Face.FaceAttributes.Age < 30)
                        {
                            genderBasedAgeDistribution.Age20s++;
                        }
                        else if (item.Face.FaceAttributes.Age < 40)
                        {
                            genderBasedAgeDistribution.Age30s++;
                        }
                        else if (item.Face.FaceAttributes.Age < 50)
                        {
                            genderBasedAgeDistribution.Age40s++;
                        }
                        else
                        {
                            genderBasedAgeDistribution.Age50sAndOlder++;
                        }
                    }
                }

                if (demographicsChanged)
                {
                    this.ageGenderDistributionControl.UpdateData(this.demographics);
                }

                this.overallStatsControl.UpdateData(this.demographics);
            }
        }

        private void UpdateDemographicsUI()
        {
            this.ageGenderDistributionControl.UpdateData(this.demographics);
            this.overallStatsControl.UpdateData(this.demographics);
        }

        //private async Task ResetDemographicsData()
        private async Task ResetDemographicsData()
        {
            this.initializingUI.Visibility = Visibility.Visible;
            this.initializingProgressRing.IsActive = true;

            this.demographics = new DemographicsData
            {
                StartTime = DateTime.Now,
                AgeGenderDistribution = new AgeGenderDistribution { FemaleDistribution = new AgeDistribution(), MaleDistribution = new AgeDistribution() },
                Visitors = new List<Visitor>()
            };

            //this.visitors.Clear();
            //await FaceListManager.ResetFaceLists();

            this.initializingUI.Visibility = Visibility.Collapsed;
            this.initializingProgressRing.IsActive = false;
        }

        public async Task HandleApplicationShutdownAsync()
        {
           await  ResetDemographicsData();
        }

        private void EnterKioskMode()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode)
            {
                view.TryEnterFullScreenMode();
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            this.isProcessingLoopInProgress = false;
            Window.Current.Activated -= CurrentWindowActivationStateChanged;
            this.cameraControl.CameraAspectRatioChanged -= CameraControl_CameraAspectRatioChanged;

            await this.ResetDemographicsData();

            await this.cameraControl.StopStreamAsync();
            base.OnNavigatingFrom(e);
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateCameraHostSize();
        }

        private void UpdateCameraHostSize()
        {
            this.cameraHostGrid.Width = this.cameraHostGrid.ActualHeight * (this.cameraControl.CameraAspectRatio != 0 ? this.cameraControl.CameraAspectRatio : 1.777777777777);
        }

        public Scores GetLastEmotionForFace(BitmapBounds faceBox)
        {
            if (this.lastEmotionSample == null || !this.lastEmotionSample.Any())
            {
                return null;
            }

            return this.lastEmotionSample.OrderBy(f => Math.Abs(faceBox.X - f.FaceRectangle.Left) + Math.Abs(faceBox.Y - f.FaceRectangle.Top)).First().Scores;
        }

        public Face GetLastFaceAttributesForFace(BitmapBounds faceBox)
        {
            if (this.lastDetectedFaceSample == null || !this.lastDetectedFaceSample.Any())
            {
                return null;
            }

            return Util.FindFaceClosestToRegion(this.lastDetectedFaceSample, faceBox);
        }

        public IdentifiedPerson GetLastIdentifiedPersonForFace(BitmapBounds faceBox)
        {
            if (this.lastIdentifiedPersonSample == null || !this.lastIdentifiedPersonSample.Any())
            {
                return null;
            }

            Tuple<Face, IdentifiedPerson> match =
                this.lastIdentifiedPersonSample.Where(f => Util.AreFacesPotentiallyTheSame(faceBox, f.Item1.FaceRectangle))
                                               .OrderBy(f => Math.Abs(faceBox.X - f.Item1.FaceRectangle.Left) + Math.Abs(faceBox.Y - f.Item1.FaceRectangle.Top)).FirstOrDefault();
            if (match != null)
            {
                return match.Item2;
            }

            return null;
        }

        public SimilarPersistedFace GetLastSimilarPersistedFaceForFace(BitmapBounds faceBox)
        {
            if (this.lastSimilarPersistedFaceSample == null || !this.lastSimilarPersistedFaceSample.Any())
            {
                return null;
            }

            SimilarFaceMatch match =
                this.lastSimilarPersistedFaceSample.Where(f => Util.AreFacesPotentiallyTheSame(faceBox, f.Face.FaceRectangle))
                                               .OrderBy(f => Math.Abs(faceBox.X - f.Face.FaceRectangle.Left) + Math.Abs(faceBox.Y - f.Face.FaceRectangle.Top)).FirstOrDefault();

            return match?.SimilarPersistedFace;
        }

        
    }

    [XmlType]
    public class Visitor
    {
        [XmlAttribute]
        public Guid UniqueId { get; set; }

        [XmlAttribute]
        public int Count { get; set; }
    }

    [XmlType]
    public class AgeDistribution
    {
        public int Age0To15 { get; set; }
        public int Age16To19 { get; set; }
        public int Age20s { get; set; }
        public int Age30s { get; set; }
        public int Age40s { get; set; }
        public int Age50sAndOlder { get; set; }
    }

    [XmlType]
    public class AgeGenderDistribution
    {
        public AgeDistribution MaleDistribution { get; set; }
        public AgeDistribution FemaleDistribution { get; set; }
    }

    [XmlType]
    [XmlRoot]
    public class DemographicsData
    {
        public DateTime StartTime { get; set; }

        public AgeGenderDistribution AgeGenderDistribution { get; set; }

        public int OverallMaleCount { get; set; }

        public int OverallFemaleCount { get; set; }

        [XmlArrayItem]
        public List<Visitor> Visitors { get; set; }
    }
}