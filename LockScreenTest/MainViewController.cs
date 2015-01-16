using System;

using Foundation;
using UIKit;
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
            showLockScreenButton.TouchUpInside += async (sender, e) => {
				var pwd = await _lockScreenController.Activate (this, LockScreenController.Mode.CheckPassword);

				while (pwd != "1234")
					pwd = await _lockScreenController.AnimateInvalidPassword (false);
					
				await _lockScreenController.AnimateValidPassword (true);
			};

            definePasswordButton.TouchUpInside += async (sender, e) => {
				var pwd = await _lockScreenController.Activate (this, LockScreenController.Mode.SetPassword);
				UIAlertView alertView = new UIAlertView("Password defined", "You have defined password: " + pwd, null, "OK");
				alertView.Show();
			};

			changePasswordButton.TouchUpInside += async (sender, e) => {
				var pwd = await _lockScreenController.Activate (this, LockScreenController.Mode.ChangePassword);

				while(pwd != "1234")
				{
					pwd = await _lockScreenController.AnimateInvalidPassword(false);
				}
				await _lockScreenController.AnimateValidPassword(false);
				var newPwd = await _lockScreenController.ChangeMode(LockScreenController.Mode.SetPassword);
				UIAlertView alertView = new UIAlertView("Password defined", "You have defined password: " + newPwd, null, "OK");
				alertView.Show();
			};
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

