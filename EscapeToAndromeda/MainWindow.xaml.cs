using System;
using System.ComponentModel;
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
		bool moveUp, moveDown, moveLeft, moveRight; // helps to determine ship movement and position

		Rect playerHitbox; // hitbox for the ship	

		int playerSpeed = 10; // speed of player ship

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
			playerHitbox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

			// player movement begins

			if (moveLeft && Canvas.GetLeft(player) > 0)
			{
				// if move left is true AND player is inside the boundary then move player to the left
				Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
			}
			if (moveRight && Canvas.GetLeft(player) + 90 < Application.Current.MainWindow.Width)
			{
				// if move right is true AND player left + 90 is less than the width of the form
				// then move the player to the right
				Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
			}
            if (moveUp && Canvas.GetTop(player) > 15)
            {
				Canvas.SetTop(player, Canvas.GetTop(player) + playerSpeed);
            }
            if (moveDown && Canvas.GetBottom(player) + 90 < Application.Current.MainWindow.Height)
            {
				Canvas.SetBottom(player, Canvas.GetBottom(player) + playerSpeed);
            }

			// player movement ends
		}

		/// <summary>
		/// What happens when we click on the exit button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void CanBattle_KeyDown(object sender, KeyEventArgs e)
		{
			// if the left key is released
			// set move left to false


			// if the right key is released
			// set move right to false

			switch (e.Key)
			{
				case Key.Left:
					moveLeft  = true;
					break;
				case Key.Right:
					moveRight = true;
					break;
				case Key.Up:
					moveUp = true;
					break;
				case Key.Down:
				{
					moveDown = true;
					break;
				}
			}
		}

		private void CanBattle_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Left:
					moveLeft = false;
					break;
				case Key.Right:
					moveRight = false;
					break;
				case Key.Up:
					moveUp = false;
					break;
				case Key.Down:
					moveDown = false;
					break;
			}
		}

		/// <summary>
		/// Ensures that background music is in a loop
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MedMusic_MediaEnded(object sender, RoutedEventArgs e)
		{
			MedMusic.Position = new TimeSpan(0, 0, 1);
			MedMusic.Play();
		}

		/// <summary>
		/// What occurs when you hit the "Start" button on the main menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Transitions through the intro screen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
				sboConflictedDisable.Begin();
				await Task.Delay(2000);
				CanIntro.Visibility = Visibility.Collapsed;
				CanBattle.Visibility = Visibility.Visible;

				txtConflicted.Visibility = Visibility.Collapsed;
				txtConflicted.IsEnabled = false;

				CanBattle.Focus();
			}
		}
	}
}