using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace LockScreenTest
{
    public partial class FlipsideViewController : UIViewController
    {
        public FlipsideViewController() : base ("FlipsideViewController", null)
        {
            this.ContentSizeForViewInPopover = new SizeF(320f, 480f);
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            // Perform any additional setup after loading the view, typically from a nib.
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
        
        partial void done(UIBarButtonItem sender)
        {
            if (Done != null)
                Done(this, EventArgs.Empty);
        }
        
        public event EventHandler Done;
    }
}

