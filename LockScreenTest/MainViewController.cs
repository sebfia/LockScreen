using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LockScreen;

namespace LockScreenTest
{
    public sealed class MyLockScreenMessages : LockScreenMessages
    {
        public MyLockScreenMessages()
        {
            this.EnterPasswordTitle = "Please enter your password.";
            this.SetPasswordTitle = "Define your new password.";
			this.EnterOldPasswordTitle = "Please enter your old password.";
			this.ReEnterNewPasswortTitle = "Please re-enter your new password.";
        }
    }

    public sealed class MyLockScreenAppearence : LockScreenAppearence
    {
        public MyLockScreenAppearence()
        {
            this.BackgroundColor = UIColor.FromRGB(255, 179, 0);
            this.TitleColor = UIColor.FromRGB(9, 9, 72);
            this.PinBoxColor = UIColor.White;
        }
    }

    public sealed class MyLockScreenSettings : LockScreenSettings
    {
        public MyLockScreenSettings()
            : base(new MyLockScreenAppearence(), new MyLockScreenMessages())
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
            _lockScreenController = new LockScreenController(new MyLockScreenSettings());
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            showLockScreenButton.TouchUpInside += (object sender, EventArgs e) => 
            {
                _lockScreenController.Activate(this, LockScreenController.Mode.CheckPassword, WhenPasswordEntered);

            };

            definePasswordButton.TouchUpInside += (object sender, EventArgs e) => 
			{
                _lockScreenController.Activate(this, LockScreenController.Mode.SetPassword, WhenPasswordDefined);
            };

			changePasswordButton.TouchUpInside += (object sender, EventArgs e) => 
			{
				_lockScreenController.Activate(this, LockScreenController.Mode.ChangePassword, WhenOldPasswordEntered);
			};
        }

		private void WhenOldPasswordEntered(string oldPassword)
		{
			if (oldPassword == "1234") {
				_lockScreenController.AnimateValidPassword (false);
				_lockScreenController.ChangeMode (LockScreenController.Mode.SetPassword);
				_lockScreenController.SwapContinuation (WhenPasswordDefined);
			} else
				_lockScreenController.AnimateInvalidPassword (false);
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
            base.DidReceiveMemoryWarning();
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

