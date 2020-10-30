using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EscapeToAndromeda
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public sealed partial class MainWindow
	{
		//Every node is a randomized possibility for the end screen to display upon a loss
		string[] quotes =
		{
			"\"I'm a better driver than both of us combined!\" ~ Tyler",
			"\"Nothing in life is so exhilarating as to be pregnant without result.\" ~ Winston Churchill",
			"\"Clap for that you stupid bastards\" ~ Joe",
			"\"Remember why we started.\" ~ Yura Sim",
			"\"Na na na, na na na, Elmo\'s World!\" ~ Elmo",
			"\"I believe in one thing only, the power of human will.\" ~ Joseph Stalin",
			"\"Better to live a day as a lion than 100 years as a sheep.\" ~ Benito Mussolini",
			"\"Nokias are real nasty. You gotta respect the Japanese; they know the way of the Samurai.\" - Simmons, Transformers (2007)",
			"\"Every tyrant who has lived has believed in freedom\" ~ Elbert Hubbard"
		};

		//every node represents one page of story text that will be automatically displayed when the game is started. There is no hard limit to the ammount of nodes
		string[] storyText =
		{
			"When we last left our hero, Criss with a Gun was at the top of his game.",
			"After defeating his rival, The Rat King, it seemed nothing could stop him.",
			"And then the unthinkable happened.",
			"Unemployment.",
			"Criss with a gun was simply too powerful. Overqualified. Too good. At everything.",
			"No one wants to hire an overqualified man.",
			"Eventually, having no money and no place to live, he retired to his dumpster.",
			"But just becuase Criss with a Gun lives in a dumpster, does not mean he slacks on his responsibility.",
			"The responsibility of saving our planet.",
		};

		// SCALABLE ATTRIBUTES

		/// <summary>
		/// The multiplier for how many enemies are required to be defeated each round
		/// </summary>
		private int difficulty = 3;

		/// <summary>
		/// determines the frequency of when the omega will appear
		/// </summary>
		private int skotina = 5;

		/// <summary>
		/// Speed that player moves
		/// </summary>
		private int _pSpeed = 5;

		/// <summary>
		/// Damage per bullet of player
		/// </summary>
		private double _pDamage = 1;

		/// <summary>
		/// Fire rate per second of  player
		/// </summary>
		private double _pFiringSpeed = 3;

		private double _pFiringCounter = 0;

		/// <summary>
		/// Resource Regen of Player
		/// </summary>
		private double _pHPRegen = 0.25;
		private double _pMPRegen = 1;


		private double _pMaxHP = 10, _pHP = 10;
		private double _pMaxMP = 10, _pMP = 10;

		/// <summary>
		///     Used to manage the game engine
		/// </summary>
		private readonly DispatcherTimer _gameTimer = new DispatcherTimer();


		/// <summary>
		///     A list of items that need to be garbage collected because they are destroyed.
		/// </summary>
		private readonly List<object> _itemstoremove = new List<object>();

		/// <summary>
		///     Random Number generator
		/// </summary>
		private readonly Random _rnGesus = new Random();

		/// <summary>
		///     Each of these help to determine the state of the player vehicle
		/// </summary>
		private bool _moveUp, _moveDown, _moveLeft, _moveRight, _isFiring;

		private Rect _playerHitbox; // hitbox for the ship
		private int damage = 1; // default damage
		private int enemyCounter = 100; // enemy spawn time
		private int enemySpriteCounter; // int to help change enemy images
		private readonly int limit = 50; // limit of enemy spawns		

		private int killCount = 0;
		private int killGoal;
		private int stageCount = 1;

		public MainWindow()
		{
			InitializeComponent();
			killGoal = difficulty;

			//showDialogBox("jig");

			// Steve Change 1				
			ImageBrush moveBack = new ImageBrush
			{
				ImageSource = new BitmapImage(new Uri($@"pack://application:,,,/Resources/Images/spacebackground1.png",
													 UriKind.RelativeOrAbsolute)) // file path for spacebackground.png		
			}; // Creates background for both moving backgrounds


			moving.Fill = moveBack; // fills the first background with spacebackground1.png
			moving2.Fill = moveBack; // fills the second background with spacebackground1.png
			moving3.Fill = moveBack;
			moving4.Fill = moveBack;
			Canvas.SetBottom(moving, 0); // set the first background to 0
			Canvas.SetBottom(moving2, 800); // set the second background to 800
			Canvas.SetBottom(moving3, 0);
			Canvas.SetBottom(moving4, 800);

			//ImageBrush bg = new ImageBrush(); // new image brush for dedicated background
			//bg.ImageSource = new BitmapImage(new Uri($@"pack://application:,,,/Resources/Images/spacebackground1.png",
			//										 UriKind.RelativeOrAbsolute)); // file path for the spacebackground.png
			//bg.TileMode = TileMode.Tile; // Creates cleaner background formatting
			//CanBattle.Background = bg; // sets the spacebackground.png as dedicated background
			// End of Steve Change 1

			ToogleCanvases(CanMain);

			var sboPlanetRotation = TryFindResource("PlanetRotation") as Storyboard;
			sboPlanetRotation.Begin();

			PlayMusic("Interstellar", MedMusic);

			_gameTimer.Interval = TimeSpan.FromMilliseconds(20);
			// link the game engine event to the timer
			_gameTimer.Tick += GameEngine;

			AdjustShipModel("default01", recPlayer);
		}

		/// <summary>
		/// Adjust the rectangle fill object to whatever name of ship.
		/// </summary>
		/// <param name="strImgName"></param>
		/// <param name="recToResize"></param>
		private void AdjustShipModel(string strImgName, Rectangle recToResize)
		{
			var imgToPaint = new BitmapImage(new Uri($@"pack://application:,,,/Resources/images/Ships/{strImgName}.png",
													 UriKind.RelativeOrAbsolute));

			recToResize.Height = imgToPaint.Height * 0.50;
			recToResize.Width = imgToPaint.Width * 0.50;

			recToResize.Fill = new ImageBrush(imgToPaint);
		}

		/// <summary>
		///     This disables ALL canvases on the display then re-enables the canvas that you wish to switch to.
		/// </summary>
		/// <param name="focusedCanvas"></param>
		private void ToogleCanvases(Canvas focusedCanvas)
		{
			foreach (Canvas leCanvas in CanBackground.Children.OfType<Canvas>())
			{
				leCanvas.Visibility = Visibility.Collapsed;
				leCanvas.IsEnabled = false;
			}

			focusedCanvas.Visibility = Visibility.Visible;
			focusedCanvas.IsEnabled = true;
			focusedCanvas.Focus();
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

		private void MakeEnemies()
		{
			// this function will make the enemies for us including assignning them images

			// make a new image brush called enemy sprite
			var enemySprite = new ImageBrush();

			// generate a random number inside the enemy sprite counter integer
			enemySpriteCounter = _rnGesus.Next(1, 3);

			// below switch statement will check what number is generated inside the enemy sprite counter
			// and then assign a new image to the enemy sprite image brush depending on the number
			enemySprite.ImageSource =
				new BitmapImage(new Uri($@"pack://application:,,,/Resources/images/Ships/desert0{enemySpriteCounter}.png"));

			// make a new rectangle called new enemy
			// this rectangle has a enemy tag, height 50 and width 56 pixels
			// background fill is assigned to the randomly generated enemy sprite from the switch statement above

			var newEnemy = new Rectangle
			{
				Tag = "enemy",
				Height = 50,
				Width = 50,
				Fill = enemySprite
				
			};

			//Boss Fight
			if ((stageCount % skotina == 0) && killCount == 0)
			{
				damage = 100;
				MessageBox.Show("Watch out, the super tough\nSKOTINA\nhas appeared, good luck.");
				newEnemy.Height = 200;
				newEnemy.Width = 200;
			}
			else
            {
				damage = 1;
            }


			Canvas.SetTop(newEnemy, -100); // set the top position of the enemy to -100
										   // randomly generate the left position of the enemy
			Canvas.SetLeft(newEnemy, _rnGesus.Next(50, (int)CanBattle.ActualWidth - 80));
			// add the enemy object to the screen
			CanBattle.Children.Add(newEnemy);

			// garbage collection
			GC.Collect(); // collect any unused resources for this game
		}

		/// <summary>
		/// This event is triggered once every 20 milliseconds as defined in the MainWIndow Concstructor. 
		/// TODO: Make it's own class
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GameEngine(object sender, EventArgs e)
		{
			lblKillCount.Content = $"{killCount}/{killGoal} kills\nStage {stageCount}";

			// Steve Change 2
			Canvas.SetBottom(moving, Canvas.GetBottom(moving) - 4); // moves background down
			Canvas.SetBottom(moving2, Canvas.GetBottom(moving2) - 4); // moves second background down
			Canvas.SetBottom(moving3, Canvas.GetBottom(moving3) - 3);
			Canvas.SetBottom(moving4, Canvas.GetBottom(moving4) - 3);


			// for background 1
			if (Canvas.GetBottom(moving) < -800)
			{
				// stops choppy transitions between background change
				Canvas.SetBottom(moving, Canvas.GetBottom(moving2) + moving2.Height);
			}
			// for background 2
			if (Canvas.GetBottom(moving2) < -800)
			{
				// stops choppy transitions between background change
				Canvas.SetBottom(moving2, Canvas.GetBottom(moving) + moving.Height);
			}
			// for background 3
			if (Canvas.GetBottom(moving3) < -800)
			{
				// stops choppy transitions between background change
				Canvas.SetBottom(moving3, Canvas.GetBottom(moving4) + moving4.Height);
			}
			// for background 4
			if (Canvas.GetBottom(moving4) < -800)
			{
				// stops choppy transitions between background change
				Canvas.SetBottom(moving4, Canvas.GetBottom(moving3) + moving3.Height);
			}
			// End of Steve Change 2

			_pHP += _pHPRegen / 50;
			_pMP += _pMPRegen / 50;

			_pHP = _pHP > _pMaxHP ? _pMaxHP : _pHP;
			_pMP = _pMP > _pMaxMP ? _pMaxMP : _pMP;


			// link the player hit box to the player rectangle
			_playerHitbox = new Rect(Canvas.GetLeft(recPlayer),
									 Canvas.GetTop(recPlayer),
									 recPlayer.Width,
									 recPlayer.Height);

			enemyCounter--;
			if (enemyCounter < 0)
			{
				MakeEnemies(); // run the make enemies function
				enemyCounter = limit; //reset the enemy counter to the limit integer
			}

			_pFiringCounter = _pFiringCounter > 1000 / _pFiringSpeed ? _pFiringCounter : _pFiringCounter + 20;

			if (_isFiring && _pMP > 1)
			{
				if (_pFiringCounter > 1000 / _pFiringSpeed)
				{
					var newBullet = new Rectangle
					{
						Tag = "bullet",
						Height = 20,
						Width = 5,
						Fill = Brushes.White,
						Stroke = Brushes.Red
					};

					// place the bullet on top of the player location
					Canvas.SetTop(newBullet, Canvas.GetTop(recPlayer) - newBullet.Height);
					// place the bullet middle of the player image
					Canvas.SetLeft(newBullet, Canvas.GetLeft(recPlayer) + recPlayer.Width / 2);
					// add the bullet to the screen
					CanBattle.Children.Add(newBullet);

					--_pMP;

					_pFiringCounter -= 1000 / _pFiringSpeed;
				}
			}


			// PLAYER MOVEMENT BEGINS //
			if (_moveLeft && Canvas.GetLeft(recPlayer) > 15)
			{
				// if move left is true AND player is inside the boundary then move player to the left
				Canvas.SetLeft(recPlayer, Canvas.GetLeft(recPlayer) - _pSpeed);
			}

			if (_moveRight &&
				Canvas.GetLeft(recPlayer) + recPlayer.ActualWidth + 45 < Application.Current.MainWindow.Width)
			{
				// if move right is true AND player left + 90 is less than the width of the form
				// then move the player to the right
				Canvas.SetLeft(recPlayer, Canvas.GetLeft(recPlayer) + _pSpeed);
			}

			if (_moveUp && Canvas.GetTop(recPlayer) > 15)
			{
				Canvas.SetTop(recPlayer, Canvas.GetTop(recPlayer) - _pSpeed);
			}

			if (_moveDown &&
				Canvas.GetTop(recPlayer) + recPlayer.ActualHeight + 100 < Application.Current.MainWindow.Height)
			{
				Canvas.SetTop(recPlayer, Canvas.GetTop(recPlayer) + _pSpeed);
			}

			// PLAYER MOVEMENT ENDS //

			foreach (Rectangle x in CanBattle.Children.OfType<Rectangle>())
			{
				// if any rectangle has the tag bullet in it
				if (x is Rectangle && (string)x.Tag == "bullet")
				{
					// move the bullet rectangle towards top of the screen
					Canvas.SetTop(x, Canvas.GetTop(x) - 20);

					// make a rect class with the bullet rectangles properties
					var bullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

					// check if bullet has reached top part of the screen
					if (Canvas.GetTop(x) < 10)
					{
						// if it has then add it to the item to remove list
						_itemstoremove.Add(x);
					}

					// run another for each loop inside of the main loop this one has a local variable called y
					foreach (Rectangle y in CanBattle.Children.OfType<Rectangle>())
					{
						// if y is a rectangle and it has a tag called enemy
						if (y is Rectangle && (string)y.Tag == "enemy")
						{
							// make a local rect called enemy and put the enemies properties into it
							var enemy = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);
							

							// now check if bullet and enemy is colliding or not
							// if the bullet is colliding with the enemy rectangle
							if (bullet.IntersectsWith(enemy))
							{
								_itemstoremove.Add(x); // remove bullet
								_itemstoremove.Add(y); // remove enemy

								killCount += 1;
							}
						}
					}
				}

				// outside the second loop lets check for the enemy again
				if (x is Rectangle && x.Tag?.ToString() == "enemy")
				{
					// if we find a rectangle with the enemy tag

					Canvas.SetTop(x, Canvas.GetTop(x) + 10); // move the enemy downwards

					// make a new enemy rect for enemy hit box
					var enemy = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

					// first check if the enemy object has gone passed the player meaning
					// its gone passed 700 pixels from the top
					if (Canvas.GetTop(x) + 150 > 700)
					{
						// if so first remove the enemy object
						_itemstoremove.Add(x);
						_pHP -= damage;
					}

					// if the player hit box and the enemy is colliding 
					if (_playerHitbox.IntersectsWith(enemy))
					{
						_pHP -= damage;
						_itemstoremove.Add(x); // remove the enemy object
					}
				}
			}

			// check how many rectangles are inside of the item to remove list
			foreach (Rectangle y in _itemstoremove)
			{
				// remove them permanently from the canvas
				CanBattle.Children.Remove(y);
			}

			prgHP.Value = _pHP;
			prgMP.Value = _pMP;

			if (_pHP <= 0)
			{
				

				MessageBox.Show($"Died Anyways\n{quotes[_rnGesus.Next(0,quotes.Length)]}\nYour progress has been reset. Try again!");
				_isFiring = false;
				_isFiring = _moveUp = _moveDown = _moveLeft = _moveRight;
				_pHP += 1;

				//reset all progress related statistics
				killCount = 0;
				killGoal = difficulty;
				stageCount = 1;
				_pMaxHP = 10;
				_pHP = _pMaxHP;
				_pMP = _pMaxMP;

				//removes all enemies from the board
				foreach (Rectangle x in CanBattle.Children.OfType<Rectangle>())
				{
					if (x is Rectangle && x.Tag?.ToString() == "enemy")
					{
						_itemstoremove.Add(x);
					}
				}
				// show the message box with the message inside of it
			}
			if (killCount >= killGoal)
            {
				MessageBox.Show("Stage Win'd!\nMore are coming!\nMax Health decreased");
				killCount = 0;
				stageCount += 1;
				killGoal = stageCount * difficulty;

				//lowers the maximum hp by 1 after every stage, but prevents it from going lower than one.
				if (_pMaxHP > 1)
                {
					_pMaxHP -= 1;
					_pHP = _pMaxHP;
                }
				else
                {
					_pMaxHP = 1;
					_pHP = _pMaxHP;
                }

				//removes all enemies from the board
				foreach (Rectangle x in CanBattle.Children.OfType<Rectangle>())
				{
					if (x is Rectangle && x.Tag?.ToString() == "enemy")
					{
						_itemstoremove.Add(x);
					}
				}
				//showDialogBox("j");
			}
		}

		private void showDialogBox(string text)
        {
			var box = new Rectangle
			{
				Tag = "dialogBox",
				Height = 100,
				Width = 400,
				Fill = Brushes.Yellow,
			};

			Canvas.SetTop(box, 200);
			// place the bullet middle of the player image
			Canvas.SetLeft(box, 100);
			// add the bullet to the screen
			CanBattle.Children.Add(box);

			box.Visibility = Visibility.Visible;
		}

		/// <summary>
		///     What happens when we click on the exit button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		/// <summary>
		/// When any particular key is held down, change the proper boolean
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Left:
					_moveLeft = true;
					break;
				case Key.Right:
					_moveRight = true;
					break;
				case Key.Up:
					_moveUp = true;
					break;
				case Key.Down:
					_moveDown = true;
					break;
				case Key.Space:
					_isFiring = true;
					break;
				case Key.K:
					_pHP = 0;
					break;
				case Key.O:
					killCount = killGoal;
					stageCount = skotina - 1;
					break;
			}
		}

		/// <summary>
		/// When any particular key is released, change the proper boolean
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Left:
					_moveLeft = false;
					break;
				case Key.Right:
					_moveRight = false;
					break;
				case Key.Up:
					_moveUp = false;
					break;
				case Key.Down:
					_moveDown = false;
					break;
				case Key.Space:
					_isFiring = false;
					break;
			}
		}

		/// <summary>
		///     Ensures that background music is in a loop
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MedMusic_MediaEnded(object sender, RoutedEventArgs e)
		{
			MedMusic.Position = new TimeSpan(0, 0, 1);
			MedMusic.Play();
		}

		/// <summary>
		///     What occurs when you hit the "Start" button on the main menu
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

			txtConflicted.Text = storyText[0];

			// Waits 3 seconds before starting the next storyboard
			await Task.Delay(3000);

			var sboStoryMode = TryFindResource("StoryMode") as Storyboard;
			sboStoryMode.Begin();

			ToogleCanvases(CanIntro);
		}

		

		/// <summary>
		///     Transitions through the intro screen
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void txtConflicted_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var sboConflictedEnable = TryFindResource("txtConflictedEnable") as Storyboard;
			var sboConflictedDisable = TryFindResource("txtConflictedDisable") as Storyboard;

			/*
				When we last left our hero, Criss with a Gun was at the top of his game.
				After defeating his rival, The Rat King, it seemed nothing could stop him.
				And then the unthinkable happened.
				Unemployment.
				Criss with a gun was simply to powerful. Overqualified. Too good. At everything.
				No one wants to hire an overqualified man.
				Eventually, having no money and no place to live, he retired to his dumpster.
				But just becuase Criss with a Gun lives in a dumpster, does not mean he slacks on his responsibility.
				The responsibility of saving our planet.
			*/

			if (txtConflicted.Text == storyText[storyText.Length - 1])
			{
				sboConflictedDisable.Begin();

				//await Task.Delay(2000);

				ToogleCanvases(CanBattle);
				_gameTimer.Start();

				prgHP.Maximum = _pMaxHP;
				prgMP.Maximum = _pMaxMP;

				sboConflictedDisable.Begin();
			}

			for (int i = 0; i < storyText.Length - 1; i++)
			{
				if (txtConflicted.Text == storyText[i])
				{
					sboConflictedDisable.Begin();
					//await Task.Delay(2000);
					txtConflicted.Text = storyText[i + 1];
					sboConflictedEnable.Begin();
					break;
				}
			}

			/*
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

				ToogleCanvases(CanBattle);
				_gameTimer.Start();

				prgHP.Maximum = _pMaxHP;
				prgMP.Maximum = _pMaxMP;
			}
			*/
		}
	}
}