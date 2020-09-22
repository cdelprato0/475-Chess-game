using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.WpfApp {
	/// <summary>
	/// Interaction logic for GameChoiceWindow.xaml
	/// </summary>
	public partial class GameChoiceWindow : Window {
		private IEnumerable<IWpfGameFactory> GameFactories { get; set; }

		public GameChoiceWindow() {
			FindAllGameFactories();
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Button b = sender as Button;
			// Retrieve the game type bound to the button
			IWpfGameFactory gameType = b.DataContext as IWpfGameFactory;
			// Construct a GameWindow to play the game.
			var gameWindow = new GameWindow(gameType,
				mHumanBtn.IsChecked.Value ? NumberOfPlayers.Two : NumberOfPlayers.One)
			{
				Title = gameType.GameName
			};
			// When the GameWindow closes, we want to show this window again.
			gameWindow.Closed += GameWindow_Closed;

			// Show the GameWindow, hide the Choice window.
			gameWindow.Show();
			this.Hide();
		}

		private void GameWindow_Closed(object sender, EventArgs e) {
			this.Show();
		}

		private void FindAllGameFactories()
		{
			//declare a type variable representing the tpyeof Iwpfgamefactory
			Type gameFactoryType = typeof(IWpfGameFactory);

			var matchedTypes = new List<Type>();

			//Enumerate all files in games abd uses loadForm to load into current domain
			string gamesFolder = @"games";
			var files = Directory.EnumerateFiles(gamesFolder);
			foreach(var file in files)
			{
				//cuts off the .dll at the end of the current file
				string filename = System.IO.Path.GetFileName(file.Substring(0, file.Length - 4));
				//var asmFile = Assembly.LoadFrom(file);

				//loads the assembly file to the current domain
				var asmFile = Assembly.Load(filename + ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=68e71c13048d452a");
			}

			//enumerate over all assemblies loaded into the current appDomain
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				//enumerate over all types defined in that assembly
				var types = assembly.GetTypes();
				
				//filter list of types to only contain types t is assignable from t
				var filter = types.Where(t => gameFactoryType.IsAssignableFrom(t) && t.IsClass).Select(t => t);

				if (filter.Any())
				{
					matchedTypes.AddRange(filter);
				}
			}
			GameFactories = matchedTypes.Select(t => (IWpfGameFactory) Activator.CreateInstance(t));
			//set as a dynamic resource
			this.Resources.Add("GameTypes", GameFactories);

		}
	}
}
