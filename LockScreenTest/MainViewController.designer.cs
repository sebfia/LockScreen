// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace LockScreenTest
{
	[Register ("MainViewController")]
	partial class MainViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton changePasswordButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton definePasswordButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton showLockScreenButton { get; set; }

		[Action ("showInfo:")]
		partial void showInfo (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (changePasswordButton != null) {
				changePasswordButton.Dispose ();
				changePasswordButton = null;
			}

			if (showLockScreenButton != null) {
				showLockScreenButton.Dispose ();
				showLockScreenButton = null;
			}

			if (definePasswordButton != null) {
				definePasswordButton.Dispose ();
				definePasswordButton = null;
			}
		}
	}
}
