using System;
using CoreGraphics;
using AudioToolbox;
using CoreAnimation;
using Foundation;
using UIKit;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LockScreen
{
    public static class Do
    {
        public static readonly Action Nothing = () => { };
    }

    public abstract class LockScreenSettings
    {
        public LockScreenAppearence Appearence { get; private set; }
        public LockScreenMessages Messages { get; private set; }

        protected LockScreenSettings(LockScreenAppearence appearence, LockScreenMessages messages)
        {
            Appearence = appearence;
            Messages = messages;
        }
    }

    public abstract class LockScreenAppearence
    {
        public UIColor BackgroundColor { get; protected set; }
        public UIColor PinBoxBallColor { get; protected set; }
        public UIColor InvalidPasswordPinTextColor { get; protected set; }
        public UIColor ValidPasswordPinTextColor { get; protected set; }
        public UIColor PinBoxColor { get; protected set; }
        public UIColor TitleColor { get; protected set; }

        protected LockScreenAppearence()
        {
            BackgroundColor = UIColor.White;
            TitleColor = UIColor.Black;
            PinBoxColor = UIColor.LightGray;
            PinBoxBallColor = UIColor.Black;
            InvalidPasswordPinTextColor = UIColor.Red;
            ValidPasswordPinTextColor = UIColor.Green;
        }
    }

    public abstract class LockScreenMessages
    {
        public string EnterPasswordTitle { get; protected set; }
        public string SetPasswordTitle { get; protected set; }
        public string EnterOldPasswordTitle { get; protected set; }
        public string ReEnterNewPasswortTitle { get; protected set; }

        protected LockScreenMessages()
        {
            EnterPasswordTitle = "Please enter your password";
            SetPasswordTitle = "Enter your new password";
            EnterOldPasswordTitle = "Enter your old password";
            ReEnterNewPasswortTitle = "Enter your new password again";
        }
    }

    public sealed class DefaultSettings : LockScreenSettings
    {
        public DefaultSettings() 
            : base(new DefaultAppearence(), new DefaultMessages())
        {
        }
    }

    public sealed class DefaultAppearence : LockScreenAppearence
    {
        
    }

    public sealed class DefaultMessages : LockScreenMessages
    {
    }

    public partial class LockScreenController : UIViewController
    {
        public enum Mode
        {
            CheckPassword,
            SetPassword,
            ChangePassword
        }

        private static readonly UIImage _normalNumericButtonImage = UIImage.FromBundle("Images/normal.png");
        private static readonly UIImage _highlightedNumericButtonImage = UIImage.FromBundle("Images/highlight.png");
        private string _previousPasscode;
        private string _passcode;
        private UITextView _fakeField;
		private TaskCompletionSource<string> _passwordEntryFinished;
        private readonly LockScreenSettings _settings;
        private Mode _mode = Mode.CheckPassword;
        private bool _isActivated;

        static bool IsIPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        public LockScreenController(LockScreenSettings settings)
            : base (IsIPhone ? "LockScreenController_iPhone" : "LockScreenController_iPad", null)
        {
            _settings = settings;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            titleLabel.Text = _settings.Messages.EnterPasswordTitle;

            ChangePinBoxBallColor(_settings.Appearence.PinBoxBallColor);
            View.BackgroundColor = _settings.Appearence.BackgroundColor;
            titleLabel.TextColor = _settings.Appearence.TitleColor;
            pinBox0.BackgroundColor = _settings.Appearence.PinBoxColor;
            pinBox1.BackgroundColor = _settings.Appearence.PinBoxColor;
            pinBox2.BackgroundColor = _settings.Appearence.PinBoxColor;
            pinBox3.BackgroundColor = _settings.Appearence.PinBoxColor;

            _fakeField = new UITextView(CGRect.Empty)
                             {
                                 KeyboardType = UIKeyboardType.NumberPad,
                                 SecureTextEntry = true
                             };

            if(IsIPhone)
            {
                _fakeField.BecomeFirstResponder();
            }
            else
            {
                InitializeIPadNumericButton(zero, 0);
                InitializeIPadNumericButton(one, 1);
                InitializeIPadNumericButton(two, 2);
                InitializeIPadNumericButton(three, 3);
                InitializeIPadNumericButton(four, 4);
                InitializeIPadNumericButton(five, 5);
                InitializeIPadNumericButton(six, 6);
                InitializeIPadNumericButton(seven, 7);
                InitializeIPadNumericButton(eight, 8);
                InitializeIPadNumericButton(nine, 9);
                InitializeIPadNumericButton(back, -1);
            }

            _fakeField.Changed += PasswordChanged;
            View.AddSubview(_fakeField);
        }

        private void InitializeIPadNumericButton(UIButton button, int representedNumber)
        {
            button.Tag = representedNumber;
            button.SetBackgroundImage(_normalNumericButtonImage, UIControlState.Normal);
            button.SetBackgroundImage(_highlightedNumericButtonImage, UIControlState.Highlighted);
            button.TouchUpInside += NumberButtonTouched;
        }

        private void NumberButtonTouched(object sender, EventArgs args)
        {
            var number = ((UIButton)sender).Tag;
            Debug.WriteLine(number);
            var oldValue = _fakeField.Text;

            if(number > -1)
            {
                _fakeField.Text = (new StringBuilder(oldValue).Append(number)).ToString();
            }
            else
            {
                if(oldValue.Length > 0)
                {
                    _fakeField.Text = oldValue.Remove(oldValue.Length-1, 1);
                }
            }

            PasswordChanged(_fakeField, EventArgs.Empty);
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.Portrait;
        }

		public async Task<string> ChangeMode(Mode newMode)
        {
            SetMode(newMode);
			return await PasswordEntered ();
        }

        private void SetMode(Mode mode)
        {
            _mode = mode;

            switch (_mode)
            {
                case Mode.ChangePassword:
                    titleLabel.Text = _settings.Messages.EnterOldPasswordTitle;
                    break;
                case Mode.SetPassword:
                    titleLabel.Text = _settings.Messages.SetPasswordTitle;
                    break;
                default:
                    titleLabel.Text = _settings.Messages.EnterPasswordTitle;
                    break;
            }
        }

		Task<string> PasswordEntered()
		{
			_passwordEntryFinished = null;
			_passwordEntryFinished = new TaskCompletionSource<string> ();
			return _passwordEntryFinished.Task;
		}

		public async Task<string> Activate(UIViewController parent, Mode mode)
		{
			_isActivated = true;
			parent.PresentViewController(this, false, Do.Nothing);

			if(IsIPhone)
				_fakeField.BecomeFirstResponder();
				
			ResetPassword();
			ResetPinBoxes();
			ChangePinBoxBallColor(_settings.Appearence.PinBoxBallColor);

			SetMode(mode);

			return await PasswordEntered ();
		}

        public bool IsActivated
        {
            get { return _isActivated; }
        }

		public async Task Deactivate()
		{
			if(_isActivated)
			{
				_passwordEntryFinished = null;
				_isActivated = false;
				ResetPassword();
				ResetPinBoxes();
				ChangePinBoxBallColor(_settings.Appearence.PinBoxBallColor);
				SetMode(Mode.CheckPassword);
				await DismissViewControllerAsync (false);
				DismissViewController(false, Do.Nothing);
			}
		}

		public async Task DeactivateWithDelay(int milliseconds)
		{
			await Task.Delay (TimeSpan.FromMilliseconds (milliseconds));
			await Deactivate ();
		}

		public async Task<string> AnimateInvalidPassword(bool deactivateAfterAnimation)
        {
            ChangePinBoxBallColor(_settings.Appearence.InvalidPasswordPinTextColor);
			await AnimateInvalidEntryAsync ();

			if (deactivateAfterAnimation) {
				await DeactivateWithDelay (200);
				return null;
			}
			else
			{
				return await PasswordEntered ();
			}
        }

		public async Task AnimateValidPassword(bool deactivateAfterAnimation)
        {
            ChangePinBoxBallColor(_settings.Appearence.ValidPasswordPinTextColor);
			await AnimateValidEntryAsnyc();

			if (_mode == Mode.ChangePassword)
			{
				_mode = Mode.SetPassword;
				titleLabel.Text = _settings.Messages.SetPasswordTitle;
				await AnimateSetupForSecondEntry ();
			}

			if (deactivateAfterAnimation && _mode != Mode.ChangePassword)
				await DeactivateWithDelay (200);
        }

		private async void PasswordChanged(object sender, EventArgs e)
        {
            _passcode = _fakeField.Text;

            switch (_passcode.Length)
            {
                case 0:
                    pinBox0.Text = null;
                    pinBox1.Text = null;
                    pinBox2.Text = null;
                    pinBox3.Text = null;
                    break;
                case 1:
                    pinBox0.Text = "*";
                    pinBox1.Text = null;
                    pinBox2.Text = null;
                    pinBox3.Text = null;
                    break;
                case 2:
                    pinBox0.Text = "*";
                    pinBox1.Text = "*";
                    pinBox2.Text = null;
                    pinBox3.Text = null;
                    break;
                case 3:
                    pinBox0.Text = "*";
                    pinBox1.Text = "*";
                    pinBox2.Text = "*";
                    pinBox3.Text = null;
                    break;
                case 4:
                    pinBox0.Text = "*";
                    pinBox1.Text = "*";
                    pinBox2.Text = "*";
                    pinBox3.Text = "*";
                    await WhenPasswordEntryFinished();
                    break;
            }
        }

		private void TrySetPassword(string password)
		{
			if (_passwordEntryFinished != null)
				_passwordEntryFinished.TrySetResult (password);
		}

		private async Task WhenPasswordEntryFinished()
		{
			switch (_mode) {
			case Mode.SetPassword:
				if (_previousPasscode == null) {//passcode has been entered for the first time
					_previousPasscode = _passcode;
					titleLabel.Text = _settings.Messages.ReEnterNewPasswortTitle;
					await AnimateSetupForSecondEntry ();
				} else {
					if (_previousPasscode != _passcode) {//passcodes do not match
						ChangePinBoxBallColor (UIColor.Red);
						await AnimateInvalidEntryAsync ();
						titleLabel.Text = _settings.Messages.SetPasswordTitle;
					} else {//passcodes match
						TrySetPassword (_passcode);
						ResetPassword ();
						await DeactivateWithDelay (100);
					}
				}
				break;

			default:
				TrySetPassword (_passcode);
				break;
			}
		}

        private void ChangePinBoxBallColor(UIColor color)
        {
            pinBox0.TextColor = color;
            pinBox1.TextColor = color;
            pinBox2.TextColor = color;
            pinBox3.TextColor = color;
        }

        private void ResetPinBoxes()
        {
            pinBox0.Text = null;
            pinBox1.Text = null;
            pinBox2.Text = null;
            pinBox3.Text = null;
        }

        private void ResetPassword()
        {
            _previousPasscode = null;
            _passcode = null;
            _fakeField.Text = null;
        }

		Task<bool?> AnimateInvalidEntryAsync()
		{
			var tcs = new TaskCompletionSource<bool?> ();

			ResetPassword();

			SystemSound.Vibrate.PlaySystemSound();

			var animation = CABasicAnimation.FromKeyPath("position");
			animation.RemovedOnCompletion = true;
			animation.AnimationStopped += (s,e) => {
				ResetPinBoxes();
				ChangePinBoxBallColor(_settings.Appearence.PinBoxBallColor);
				tcs.SetResult(null);
			};
			animation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
			animation.Duration = 0.08;
			animation.RepeatCount = 4;
			animation.AutoReverses = true;
			animation.From = NSValue.FromCGPoint (new CGPoint (animationView.Center.X - 10.0f, animationView.Center.Y));
			animation.To = NSValue.FromCGPoint(new CGPoint(animationView.Center.X + 10.0f, animationView.Center.Y));
			animationView.Layer.AddAnimation(animation, "position");

			return tcs.Task;
		}


		Task<bool?> AnimateValidEntryAsnyc()
		{
			var tcs = new TaskCompletionSource<bool?> ();

			ResetPassword();

			var animation = CABasicAnimation.FromKeyPath("position");
			animation.RemovedOnCompletion = true;
			animation.AnimationStopped += (s, e) => {
				ResetPinBoxes();
				ChangePinBoxBallColor(_settings.Appearence.PinBoxBallColor);
				tcs.SetResult(null);
			};
			animation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
			animation.Duration = 0.08;
			animation.RepeatCount = 2;
			animation.AutoReverses = true;
			animation.From = NSValue.FromCGPoint(new CGPoint(animationView.Center.X, animationView.Center.Y - 10.0f));
			animation.To = NSValue.FromCGPoint(new CGPoint(animationView.Center.X, animationView.Center.Y + 10.0f));
			animationView.Layer.AddAnimation(animation, "position");
			return tcs.Task;
		}

		public Task<string> AnimateForAnotherEntry()
		{
			var tcs = new TaskCompletionSource<string> ();

			ResetPinBoxes();

			var transition = new CATransition
			{
				Type = CAAnimation.TransitionPush,
				Subtype = CAAnimation.TransitionFromRight,
				Duration = 0.5,
				TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut),
				RemovedOnCompletion = true
			};
			transition.AnimationStopped += (s, e) => {
				_passcode = null;
				_fakeField.Text = null;

				if(IsIPhone)
					_fakeField.BecomeFirstResponder();

				_passwordEntryFinished = tcs;
			};
			View.ExchangeSubview(0, 1);
			animationView.Layer.AddAnimation(transition, "swipe");

			return tcs.Task;
		}

        private Task<bool?> AnimateSetupForSecondEntry()
        {
			var tcs = new TaskCompletionSource<bool?> ();

            ResetPinBoxes();

            var transition = new CATransition
                                 {
                                     Type = CAAnimation.TransitionPush,
                                     Subtype = CAAnimation.TransitionFromRight,
                                     Duration = 0.5,
                                     TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut),
                                     RemovedOnCompletion = true
                                 };
			transition.AnimationStopped += (s,e) => {
				_passcode = null;
				_fakeField.Text = null;

				if(IsIPhone)
					_fakeField.BecomeFirstResponder();
				tcs.SetResult(null);
			};
            
			View.ExchangeSubview(0, 1);
            animationView.Layer.AddAnimation(transition, "swipe");

			return tcs.Task;
        }

    }
}

