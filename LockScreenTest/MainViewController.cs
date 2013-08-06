using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LockScreen;

namespace LockScreenTest
{
    public sealed class LufthansaLockScreenMessages : LockScreenMessages
    {
        public LufthansaLockScreenMessages()
        {
            this.EnterPasswordTitle = "Bitte geben Sie Ihr Passwort ein";
            this.SetPasswordTitle = "Legen Sie Ihr neues Passwort fest";
        }
    }

    public sealed class LufthansaLockScreenAppearence : LockScreenAppearence
    {
        public LufthansaLockScreenAppearence()
        {
            this.BackgroundColor = UIColor.FromRGB(255, 179, 0);
            this.TitleColor = UIColor.FromRGB(9, 9, 72);
            this.PinBoxColor = UIColor.White;
        }
    }

    public sealed class LufthansaLockScreenSettings : LockScreenSettings
    {
        public LufthansaLockScreenSettings()
            : base(new LufthansaLockScreenAppearence(), new LufthansaLockScreenMessages())
        {
        }
    }

    public partial class MainViewController : UIViewController
    {
        private readonly LockScreenController _lockScreenController;

        static bool UserInterfaceIdiomIsPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        UIPopoverController flipsidePopoverController;
        
        public MainViewController()
            : base (UserInterfaceIdiomIsPhone ? "MainViewController_iPhone" : "MainViewController_iPad" , null)
        {
            _lockScreenController = new LockScreenController(new LufthansaLockScreenSettings());
//            _lockScreenController = new DefaultLockScreenController();
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            showLockScreenButton.TouchUpInside += (object sender, EventArgs e) => 
            {
                _lockScreenController.Activate(this, LockScreenController.Mode.CheckPassword, WhenPasswordEntered);

            };

            definePasswordButton.TouchUpInside += (object sender, EventArgs e) => {
                _lockScreenController.Activate(this, LockScreenController.Mode.SetPassword, WhenPasswordDefined);
            };
            // Perform any additional setup after loading the view, typically from a nib.
        }

        private void WhenPasswordEntered(string password)
        {
            if (password == "1234")
                NSTimer.CreateScheduledTimer(0.5, () => _lockScreenController.AnimateValidPassword(true));
            else
                NSTimer.CreateScheduledTimer(0.5, () => _lockScreenController.AnimateInvalidPassword(false));
        }

        private void WhenPasswordDefined(string password)
        {
            UIAlertView alertView = new UIAlertView("Password defined", "You have defined password: " + password, null, "OK");
            alertView.Show();
            NSTimer.CreateScheduledTimer(0.5, _lockScreenController.DeactivateImmediately);
        }
        
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.Portrait;
        }
        
        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
            
            // Release any cached data, images, etc that aren't in use.
        }
        
        partial void showInfo(NSObject sender)
        {
            if (UserInterfaceIdiomIsPhone)
            {
                var controller = new FlipsideViewController() {
                    ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal,
                };
                
                controller.Done += delegate
                {
                    this.DismissViewController(true, Do.Nothing);
                };
                
                this.PresentViewController(this, true, Do.Nothing);
            } else
            {
                if (flipsidePopoverController == null)
                {
                    var controller = new FlipsideViewController();
                    flipsidePopoverController = new UIPopoverController(controller);
                    controller.Done += delegate
                    {
                        flipsidePopoverController.Dismiss(true);
                    };
                }
                
                if (flipsidePopoverController.PopoverVisible)
                {
                    flipsidePopoverController.Dismiss(true);
                } else
                {
                    flipsidePopoverController.PresentFromBarButtonItem((UIBarButtonItem)sender, UIPopoverArrowDirection.Any, true);
                }
            }
        }
    }
}

