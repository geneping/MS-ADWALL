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
using IntelligentKioskSample.Views;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Media.Playback;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IntelligentKioskSample.Controls
{
    public sealed partial class OverallStatsControl : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(
            "HeaderText",
            typeof(string),
            typeof(OverallStatsControl),
            new PropertyMetadata("Total Faces")
            );

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, (string)value); }
        }

        public static readonly DependencyProperty SubHeaderTextProperty =
            DependencyProperty.Register(
            "SubHeaderText",
            typeof(string),
            typeof(OverallStatsControl),
            new PropertyMetadata("")
            );

        public string SubHeaderText
        {
            get { return (string)GetValue(SubHeaderTextProperty); }
            set { SetValue(SubHeaderTextProperty, (string)value); }
        }

        public static readonly DependencyProperty SubHeaderVisibilityProperty =
            DependencyProperty.Register(
            "SubHeaderVisibility",
            typeof(Visibility),
            typeof(OverallStatsControl),
            new PropertyMetadata(Visibility.Collapsed)
            );

        public Visibility SubHeaderVisibility
        {
            get { return (Visibility)GetValue(SubHeaderVisibilityProperty); }
            set { SetValue(SubHeaderVisibilityProperty, (Visibility)value); }
        }

        public OverallStatsControl()
        {
            this.InitializeComponent();
        }

        public void UpdateData(DemographicsData data)
        {
           
            thistime = DateTime.Now.Second;
            this.facesProcessedTextBlock.Text = data.Visitors.Sum(v => v.Count).ToString();
            this.uniqueFacesCountTextBlock.Text = data.Visitors.Count.ToString();
            //在這裡判斷同一人停留時間(粗略估計)
            //if (thistime - lasttime > 10) count = 0;
            if ((data.Visitors.Sum(v => v.Count) - saveTotal) > (data.Visitors.Count - saveUnique))
            {
                count++;
                if (count >= 3)
                {
                    //執行廣告
                    
                    adControl = 1;
                    //this.facesProcessedTextBlock.Text = "到達次數";
                    //this.uniqueFacesCountTextBlock.Text = "count="+count.ToString();
                }
            }
            saveTotal = data.Visitors.Sum(v => v.Count);
            saveUnique = data.Visitors.Count;
            lasttime = DateTime.Now.Second;
        }
        public int ADinit()
        {
            return adControl;
        }
        public void ADcancel()
        {
            adControl = 0;
            count = 0;
        }
        private int adControl;
        private int count=0;
        private int saveTotal;
        private int saveUnique;
        private int lasttime;
        private int thistime;
    }
}
