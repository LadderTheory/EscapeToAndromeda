using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace EscapeToAndromeda
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public sealed partial class MainWindow
	{
		private readonly DispatcherTimer _gameTimer = new DispatcherTimer();

		public MainWindow()
		{
			InitializeComponent();

			CanMain.Visibility = Visibility.Visible;

			var sboPlanetRotation = TryFindResource("PlanetRotation") as Storyboard;
			sboPlanetRotation.Begin();

			PlayMusic("Interstellar", MedMusic);

			//this._gameTimer.Interval = TimeSpan.FromMilliseconds(20);
			//// link the game engine event to the timer
			//this._gameTimer.Tick += this.GameEngine;
			//// start the timer
			//this._gameTimer.Start();
		}

		/// <summary>
		///     A static method to play music off a given media element control.
		/// </summary>
		/// <param name="strSoundFileName"></param>
		/// <param name="medPlayer"></param>
		private static void PlayMusic(string strSoundFileName, MediaElement medPlayer)
		{
			medPlayer.Source = new Uri($@"Resources/Sound/{strSoundFileName}.mp3", UriKind.Relative);
		}

		private void GameEngine(object sender, EventArgs e)
		{
		}

		private void BtnExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MedMusic_MediaEnded(object sender, RoutedEventArgs e)
		{
			MedMusic.Position = new TimeSpan(0, 0, 1);
			MedMusic.Play();
		}

		private async void BtnStart_Click(object sender, RoutedEventArgs e)
		{
			// Fade away the main menu
			var sboFadeMainMenu = TryFindResource("FadeMainMenu") as Storyboard;
			sboFadeMainMenu.Begin();

			// Turn down music
			MedMusic.Volume *= 0.75;

			txtConflicted.Text = English.Conflicted;

			// Waits 3 seconds before starting the next storyboard
			await Task.Delay(3000);
			var sboStoryMode = TryFindResource("StoryMode") as Storyboard;
			sboStoryMode.Begin();
		}

		private async void txtConflicted_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var sboConflictedEnable  = TryFindResource("txtConflictedEnable") as Storyboard;
			var sboConflictedDisable = TryFindResource("txtConflictedDisable") as Storyboard;

			if (txtConflicted.Text == English.Conflicted)
			{
				sboConflictedDisable.Begin();
				await Task.Delay(2000);
				txtConflicted.Text = English.Searching;
				sboConflictedEnable.Begin();
			}
			else if (txtConflicted.Text == English.Searching)
			{
				sboConflictedDisable.Begin();
				await Task.Delay(2000);
				txtConflicted.Text = English.Trusted;
				sboConflictedEnable.Begin();
			}
			else if (txtConflicted.Text == English.Trusted)
			{
				sboConflictedDisable.Begin();
				await Task.Delay(2000);
				txtConflicted.Text = English.Stars;
				sboConflictedEnable.Begin();
			}
			else if (txtConflicted.Text == English.Stars)
			{
			}
		}
	}
}