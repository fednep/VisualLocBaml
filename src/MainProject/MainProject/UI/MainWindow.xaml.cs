/*
    Copyright (c) 2013-2014 Fedir Nepyivoda <fednep@gmail.com>

    This file is part of VisualLocBaml project
    http://visuallocbaml.com

    VisualLocBaml is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    VisualLocBaml is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with VisualLocBaml. If not, see <http://www.gnu.org/licenses/>

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using System.Threading.Tasks;
using MainProject.UI;
using MainProject.UI.DocumentPages;
using System.Windows.Media.Animation;
using System.Threading;
using System.ComponentModel;

namespace MainProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private Document __Document;
        private const double TRANSITION_TIME = 0.5;
        private const double ANIMATION_DISTANCE = 0.05; // в % от размера кадра

        private Stack<NavigationStackItem> __NavigationStack;

        private bool __IsMessageBoxVisible;

        private AutoResetEvent __ButtonPressEvent;
        private MessageBoxResult __MessageBoxResult;
        private MessageBoxButton __MessageBoxButton;


        public MainWindow()
        {
            InitializeComponent();
            BindCommands();
            __ButtonPressEvent = new AutoResetEvent(false);

            Closing += MainWindow_Closing;
            MouseDown += MainWindow_MouseDown;

            // При запуске, если приложение запущено без параметров, 
            // то считать что документа у нас нет
            __Document = null;
            DataContext = this;

            __NavigationStack = new Stack<NavigationStackItem>();

            DocumentFrame.NavigationService.Navigated += NavService_Navigated;
        }

        void NavService_Navigated(object sender, NavigationEventArgs e)
        {

        }

        public async Task<object> ExecuteLongTask(LongTask task, bool allowCancellation = false)
        {
            p_Progress progress = new p_Progress();
            progress.Cancellable = allowCancellation;

            p_Document document = MainFrame.Content as p_Document;
            if (document != null)
                document.Header.IsMenuEnabled = false;
            try
            {
                AnimatedNavigate(MiddleLayerFrame, progress, NavigationAnimation.FadeToLeft, true);
                var result = await task(progress, progress.CancellationToken);

                return result;
            } finally
            {
                if (document != null)
                    document.Header.IsMenuEnabled = true;
            }
        }

        public void ToggleMaximize()
        {
            if (App.Current.MainWindow.WindowState == System.Windows.WindowState.Maximized)
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
            else
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Maximized;
        }

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();

            e.Handled = true;
        }

        private bool __CloseWithoutAsking = false;
        private bool __MessageBoxShown = false;
        async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (__CloseWithoutAsking)
                return;

            e.Cancel = true;
            __MessageBoxShown = false;

            if (await CloseProject())
            {
                if (__MessageBoxShown)
                {
                    __CloseWithoutAsking = true;
                    Close();
                } else
                {
                    e.Cancel = false;
                }
            }
        }

        private void BindCommands()
        {
            CommandBindings.Add(new CommandBinding(Commands.New, new ExecutedRoutedEventHandler(New_Cmd)));
            CommandBindings.Add(new CommandBinding(Commands.SaveAs, new ExecutedRoutedEventHandler(SaveAs_Cmd),
                                                                    new CanExecuteRoutedEventHandler(SaveAs_CanCmd)));

            CommandBindings.Add(new CommandBinding(Commands.Close, new ExecutedRoutedEventHandler(Close_Cmd),
                                                                   new CanExecuteRoutedEventHandler(Close_CanCmd)));

            CommandBindings.Add(new CommandBinding(Commands.Exit, new ExecutedRoutedEventHandler(Exit_Cmd)));

            CommandBindings.Add(new CommandBinding(Commands.OpenBinariesPath, new ExecutedRoutedEventHandler(OpenBinariesPath_Cmd)));

            CommandBindings.Add(new CommandBinding(Commands.GoBack, new ExecutedRoutedEventHandler(GoBack_Cmd)));
        }        

        public async void New_Cmd(object sender, RoutedEventArgs e)
        {
            if (Document != null)
            {                
                if (await ShowMessage( StringUtils.String("MB_CloseCurrentProject"), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Commands.Close.Execute(null, this);
            }

            if (Document != null)
                return;

            Document newDocument = null;
            DoAsync(() => newDocument = new Document());
            Document = newDocument;
        }

        public async void SaveAs_Cmd(object sender, ExecutedRoutedEventArgs e)
        {
            await SaveDocumentAs();
        }

        private async Task<bool> SaveDocumentAs()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = StringUtils.String("OpenSave_Filter");
            string errorMessage = "";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (DoAsync(() => Document.Save(dialog.FileName)) as bool? == true)
                        return true;
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
                await ShowMessage(StringUtils.String("MB_ErrorSavingFile1", errorMessage), MessageBoxButton.OK);

            return false;
        }

        public void SaveAs_CanCmd(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;

            if (Document == null)
                e.CanExecute = false;
        }

        public void Close_CanCmd(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;

            if (__Document == null)
                e.CanExecute = false;
        }

        public async void Close_Cmd(object sender, RoutedEventArgs e)
        {
            await CloseProject();
        }

        private async Task<bool> CloseProject()
        {
            if (Document == null)
                return true;

            if (!await SaveOrDiscardUnsavedDocument())
                return false;

            Document = null;
            return true;
        }

        private async Task<bool> SaveOrDiscardUnsavedDocument()
        {
            if (!Document.IsUntitled)
                return true;

            var messageResult = await ShowMessage(StringUtils.String("MB_CloseSaveQuestion"), MessageBoxButton.YesNoCancel);
            if (messageResult == MessageBoxResult.No)
            {
                __Document.Close();
                return true;
            }

            else if (messageResult == MessageBoxResult.Yes)
            {
                if (!await SaveDocumentAs())
                    return false;

                __Document.Close();
                return true;
            }

            return false;
        }

        public void Exit_Cmd(object sender, RoutedEventArgs e)
        {
            Close();
        }        

        public void LoadDocumentPage()
        {
            if (__Document == null)
            {
                AnimatedNavigate(MainFrame, p_StartPage.Instance, NavigationAnimation.FadeToRight);
            }
            else
            {
                p_Document documentPage = new p_Document();

                if (String.IsNullOrWhiteSpace(__Document.PathToBinaries))
                {
                    p_EmptyProject emptyProject = new p_EmptyProject();
                    documentPage.DocumentFrame.Navigate(emptyProject);
                }

                AnimatedNavigate(MainFrame, documentPage);
            }
        }

        /// <summary>
        /// Сейчас функция реализована как синхронная.
        /// 
        /// Если операция будет выполняться асинхронно, то исключения возникшие в асинхронном потоке 
        /// перебрасываются в потоке который вызвал DoAsync.
        /// </summary>
        /// <param name="action">Длинное действие, которое должно выполняться асинхронно</param>
        /// <returns>резултат выполнения action</returns>
        public static object DoAsync(AsyncCall action)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            var result = action();
            Mouse.OverrideCursor = null;
            return result;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            DocumentFrame.NavigationService.RemoveBackEntry();
        }

        private void OpenBinariesPath_Cmd(object sender, ExecutedRoutedEventArgs e)
        {
            UrlHelper.OpenUrl(__Document.PathToBinaries);
        }

        private void ReturnFromProgressPage(Task arg)
        {

        }

        internal void NavigateDocumentPage(NavigationAnimation navigationAnimation, Page newPage)
        {
            p_Document pageDocument = DocumentFrame.Content as p_Document;
            if (pageDocument == null)
                throw new Exception("Main page needs to be p_Document object");

            p_DocumentMenu documentMenu = pageDocument.DocumentFrame.Content as p_DocumentMenu;
            if (documentMenu == null)
                throw new Exception("Document page cannot be found");

            AnimatedNavigate(documentMenu.DocumentPageFrame, newPage, navigationAnimation);
        }        

        public void AnimatedNavigate(Frame targetFrame, 
                                     Page newPage, NavigationAnimation navigationAnimation = NavigationAnimation.FadeToLeft, bool addToStack = true)
        {
            //var newFrame = CloneFrame(targetFrame);
            
            if (addToStack)
                __NavigationStack.Push(
                        new NavigationStackItem(targetFrame, targetFrame.Content, navigationAnimation));

            targetFrame.Content = newPage;

            //__TransitionTargetFrame = targetFrame;
            //__TransitionNewFrame = newFrame;

            double frameWidth = targetFrame.ActualWidth;
            double frameHeight = targetFrame.ActualHeight;

            TranslateTransform newFrameTransform = new TranslateTransform();

            DoubleAnimation newFrameAnimation = new DoubleAnimation();
            newFrameAnimation.AccelerationRatio = 1;

            newFrameAnimation.Completed += new EventHandler(AnimationCompleted);
            newFrameAnimation.Duration = new Duration(TimeSpan.FromSeconds(TRANSITION_TIME));

            TranslateTransform targetFrameTransform = new TranslateTransform();
            DoubleAnimation targetFrameAnimation = new DoubleAnimation();
            targetFrameAnimation.Duration = new Duration(TimeSpan.FromSeconds(TRANSITION_TIME));

            targetFrameAnimation.DecelerationRatio = 1;

            int animationLength;

            if (navigationAnimation == NavigationAnimation.FadeToLeft || navigationAnimation == NavigationAnimation.FadeToRight)
                animationLength = 20;//(int)(frameWidth * ANIMATION_DISTANCE);
            else
                animationLength = 20;// (int)(frameHeight * ANIMATION_DISTANCE);

            if (navigationAnimation == NavigationAnimation.FadeToLeft || navigationAnimation == NavigationAnimation.FadeUp)
            {
                newFrameAnimation.From = 0;
                newFrameAnimation.To = -animationLength;
                targetFrameAnimation.From = animationLength;
                targetFrameAnimation.To = 0;
            }
            else
            {
                newFrameAnimation.From = 0;
                newFrameAnimation.To = animationLength;

                targetFrameAnimation.From = -animationLength;
                targetFrameAnimation.To = 0;
            }


            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(TRANSITION_TIME));
            opacityAnimation.From = 1;
            opacityAnimation.To = 0;

            DoubleAnimation opacityAnimation2 = new DoubleAnimation();
            opacityAnimation2.Duration = new Duration(TimeSpan.FromSeconds(TRANSITION_TIME / 2));
            opacityAnimation2.From = 0;
            //opacityAnimation2.DecelerationRatio = 1;
            opacityAnimation2.To = 1;

            //newFrame.RenderTransform = newFrameTransform;
            targetFrame.RenderTransform = targetFrameTransform;
            //newFrame.BeginAnimation(Frame.OpacityProperty, opacityAnimation);
            targetFrame.BeginAnimation(Frame.OpacityProperty, opacityAnimation2);

            DependencyProperty translationProperty;

            if (navigationAnimation == NavigationAnimation.FadeToLeft || navigationAnimation == NavigationAnimation.FadeToRight)
                translationProperty = TranslateTransform.XProperty;
            else
                translationProperty = TranslateTransform.YProperty;

            newFrameTransform.BeginAnimation(translationProperty, newFrameAnimation);
            targetFrameTransform.BeginAnimation(translationProperty, targetFrameAnimation);
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {            

        }

        public Frame MainFrame
        {
            get
            {
                return DocumentFrame;
            }
        }

        public Frame MiddleLayerFrame
        {
            get
            {
                p_Document middleDocument = DocumentFrame.Content as p_Document;
                if (middleDocument == null)
                    throw new Exception("Main page needs to be p_Document object");

                return middleDocument.DocumentFrame;
            }
        }

        public Frame DocumentPageFrame
        {
            get
            {
                p_DocumentMenu pageDocument = MiddleLayerFrame.Content as p_DocumentMenu;
                if (pageDocument == null)
                    throw new Exception("Document page cannot be found");

                return pageDocument.DocumentPageFrame;
            }
        }

        public Document Document
        {
            get
            {
                return __Document;
            }

            set
            {
                __Document = value;
                DoPropertyChanged("Document");
                LoadDocumentPage();
            }
        }

        private async void GoBack_Cmd(object sender, ExecutedRoutedEventArgs e)
        {
            if (__NavigationStack.Count == 0)
                return;

            if (__NavigationStack.Count == 1)
            {
                await CloseProject();
                return;
            }

            var navigationItem = __NavigationStack.Pop();

            NavigationAnimation animation = NavigationAnimation.FadeToRight;

            if (navigationItem.Animation == NavigationAnimation.FadeUp)
                animation = NavigationAnimation.FadeDown;
            if (navigationItem.Animation == NavigationAnimation.FadeDown)
                animation = NavigationAnimation.FadeUp;
            if (navigationItem.Animation == NavigationAnimation.FadeToLeft)
                animation = NavigationAnimation.FadeToRight;
            if (navigationItem.Animation == NavigationAnimation.FadeToRight)
                animation = NavigationAnimation.FadeToLeft;

            AnimatedNavigate(navigationItem.Frame, navigationItem.Content as Page, animation, false);


        }

        private void Hyperlink_Site_Click(object sender, RoutedEventArgs e)
        {
            UrlHelper.OpenUrl("http://visuallocbaml.com");
        }

        public async Task<MessageBoxResult> ShowMessage(string message, MessageBoxButton buttons = MessageBoxButton.YesNo)
        {
            MessageBoxText.Text = message;
            MessageBoxButtons = buttons;
            IsMessageBoxVisible = true;
            __MessageBoxShown = true;

            await Task.Run(() =>
            {                
                __ButtonPressEvent.WaitOne();
            });

            return __MessageBoxResult;
        }

        public bool IsMessageBoxVisible
        {
            get
            {
                return __IsMessageBoxVisible;
            }

            set
            {
                __IsMessageBoxVisible = value;
                DoPropertyChanged("IsMessageBoxVisible");
            }
        }

        public MessageBoxButton MessageBoxButtons
        {
            get
            {
                return __MessageBoxButton;
            }
            set
            {
                __MessageBoxButton = value;
                DoPropertyChanged("MessageBoxButtons");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void DoPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void YesClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(MessageBoxResult.Yes);
        }

        private void NoClock(object sender, RoutedEventArgs e)
        {
            SetDialogResult(MessageBoxResult.No);
        }

        private void SetDialogResult(MessageBoxResult dialogResult)
        {
            if (!IsMessageBoxVisible)
                return;

            __MessageBoxResult = dialogResult;
            __ButtonPressEvent.Set();
            IsMessageBoxVisible = false;
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(MessageBoxResult.OK);
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            SetDialogResult(MessageBoxResult.Cancel);
        }
    }
}
