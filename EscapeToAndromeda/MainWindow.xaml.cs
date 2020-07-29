using System;
using System.Windows;
using System.Windows.Controls;
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

			var stryPlanetRotation = TryFindResource("PlanetRotation") as Storyboard;
			stryPlanetRotation.Begin();

			PlayMusic("Interstellar", this.MedMusic);

			//this._gameTimer.Interval = TimeSpan.FromMilliseconds(20);
			//// link the game engine event to the timer
			//this._gameTimer.Tick += this.GameEngine;
			//// start the timer
			//this._gameTimer.Start();
		}

		/// <summary>
		/// A static method to play music off a given media element control.
		/// </summary>
		/// <param name="strSoundFileName"></param>
		/// <param name="medPlayer"></param>
		private static void PlayMusic(string strSoundFileName, MediaElement medPlayer)
		{
			medPlayer.Source = new Uri($@"Resources/{strSoundFileName}.mp3", UriKind.Relative);
		}

		private void GameEngine(object sender, EventArgs e)
		{
		}

		private void BtnExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void medMusic_MediaEnded(object sender, RoutedEventArgs e)
		{
			this.MedMusic.Position = new TimeSpan(0, 0, 1);
			this.MedMusic.Play();
		}
	}
}