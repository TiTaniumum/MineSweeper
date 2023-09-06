using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace winformsExam
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Difficulty difficulty = Difficulty.Normal;
        Random random = new Random();
        Button[][] buttons;
        bool isGameOver = false;
        bool isFirstClick = true;
        public MainWindow()
        {
            InitializeComponent();
            Restart();
            comboBoxDifficultyInit();
        }

        private void InitPole()
        {
            isGameOver = false;
            isFirstClick = true;
            gridPole.Children.Clear();
            buttons = new Button[gridPole.RowDefinitions.Count][];
            for (int i = 0; i< buttons.Length; i++)
            {
                buttons[i] = new Button[gridPole.ColumnDefinitions.Count];
            }
            for (int i = 0; i<gridPole.RowDefinitions.Count; i++)
            {
                for (int j = 0; j< gridPole.ColumnDefinitions.Count; j++)
                {
                    Button button = new Button();
                    button.Tag = i+":"+j;
                    button.Background = new SolidColorBrush(Color.FromRgb(0, 254, 0));
                    Grid.SetColumn(button, j);
                    Grid.SetRow(button, i);
                    button.Click+= new RoutedEventHandler(buttonPole_Click);
                    button.ContextMenu = new ContextMenu();
                    MenuItem mi = new MenuItem();
                    mi.Header = "Highlight";
                    mi.Click += new RoutedEventHandler(menuItem_Click);
                    mi.Tag = i+":"+j;
                    button.ContextMenu.Items.Add(mi);
                    gridPole.Children.Add(button);
                    buttons[i][j] = button;
                }
            }

        }
        private void menuItem_Click(object sender, RoutedEventArgs e)
        {
            (int i, int j) = GetIJ((sender as MenuItem).Tag);
            SolidColorBrush brush = buttons[i][j].Background as SolidColorBrush;
            if (brush.Color.G == 254)
            {
                buttons[i][j].Background = new SolidColorBrush(Color.FromRgb(254, 0, 0));
            }
            else
            {
                buttons[i][j].Background = new SolidColorBrush(Color.FromRgb(0, 254, 0));
            }
        }
        private string[][] GetBombArr(int bombCount_)
        {
            string[][] pole = new string[gridPole.RowDefinitions.Count][];
            for (int i = 0; i< pole.Length; i++)
            {
                pole[i] = new string[gridPole.ColumnDefinitions.Count];
            }
            for (int i = 0, bombCount = 0; i< pole.Length; i++)
            {
                for (int j = 0; j<pole[i].Length; j++, bombCount++)
                {
                    if (bombCount < bombCount_) pole[i][j] = ""+1;
                    else pole[i][j] = ""+0;
                }
            }
            Shuffle(pole, 3);
            return pole;
        }
        private void Shuffle(string[][] pole, int shuffleCount)
        {
            int maxIterations = gridPole.RowDefinitions.Count * gridPole.ColumnDefinitions.Count;
            while (shuffleCount-->0)
            {
                int iteration = 0;
                while (iteration++ < maxIterations)
                {
                    int i1 = random.Next(0, gridPole.RowDefinitions.Count);
                    int j1 = random.Next(0, gridPole.ColumnDefinitions.Count);
                    int i2 = random.Next(0, gridPole.RowDefinitions.Count);
                    int j2 = random.Next(0, gridPole.ColumnDefinitions.Count);
                    (pole[i1][j1], pole[i2][j2]) = (pole[i2][j2], pole[i1][j1]);
                }
            }
        }
        private void PlaceBombs()
        {
            int bombCount = (int)difficulty*10;
            string[][] pole = GetBombArr(bombCount);
            foreach (Button button in gridPole.Children)
            {
                int separaterIndex = button.Tag.ToString().IndexOf(':');
                int i = int.Parse(button.Tag.ToString().Substring(0, separaterIndex));
                int j = int.Parse(button.Tag.ToString().Substring(separaterIndex+1, button.Tag.ToString().Length - separaterIndex-1));
                button.Tag = pole[i][j]+";"+button.Tag.ToString();
                //раскоментировать что бы подсветить заминированные клетки
                //if (button.Tag.ToString()[0] == '1')button.Background = new SolidColorBrush(Colors.Red);
            }
        }

        private void buttonRestart_Click(object sender, RoutedEventArgs e)
        {
            Restart();
        }

        private void buttonPole_Click(object sender, RoutedEventArgs e)
        {
            if (isGameOver) return;
            Button button = sender as Button;
            if ((button.Background as SolidColorBrush).Color.R == 254) return;
            (int i, int j) = GetIJ(button);
            if (isBomb(i, j))
            {
                if (isFirstClick)
                {
                    changeBomb(i, j);
                    isFirstClick = false;
                }
                else
                {
                    GameOver(button);
                    return;
                }
            }
            isFirstClick = false;
            int count = checkAroundPoint(i, j);
            if (count>0)
            {
                uncoverButton(button, count);
                IsWin();
                return;
            }
            uncoverButton(button);
            startUncoveringProcess(i, j);
            IsWin();
        }

        private void changeBomb(int i, int j)
        {
            int i1;
            int j1;
            do
            {
                i1= random.Next(0, gridPole.RowDefinitions.Count);
                j1= random.Next(0, gridPole.ColumnDefinitions.Count);
            } while (isBomb(i1, j1));
            buttons[i][j].Tag = 0 +";"+i+":"+j;
            buttons[i1][j1].Tag = 1+";"+i1+":"+j1;
        }
        private void IsWin()
        {
            int count = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                for (int j = 0; j < buttons[i].Length; j++)
                {
                    if (buttons[i][j].IsEnabled) count++;
                }
            }
            int bombCount = (int)difficulty*10;
            if (bombCount == count)
            {
                uncoverAllBombs();
                MessageBox.Show("You Win!");
                if (MessageBox.Show("Do you Want to start a new game?", "Question!", MessageBoxButton.YesNo, MessageBoxImage.Question) ==MessageBoxResult.Yes)
                {
                    Restart();
                    return;
                }
            }
        }

        private void startUncoveringProcess(int i, int j)
        {
            List<Button> toCheck = new List<Button>();
            AddToCheckAroundPoint(toCheck, i, j);
            for (int k = 0; k< toCheck.Count; k++)
            {
                Button button = toCheck[k];
                if (button.IsEnabled == false) continue;
                (int i1, int j1) = GetIJ(button);
                int bombCount = checkAroundPoint(i1, j1);
                if (bombCount > 0)
                {
                    uncoverButton(button, bombCount);
                    continue;
                }
                uncoverButton(button);
                startUncoveringProcess(i1, j1);
            }
        }
        private void uncoverButton(Button button)
        {
            button.BorderThickness = new Thickness(0);
            button.IsEnabled = false;
        }

        private void uncoverButton(Button button, int count)
        {
            button.Content = count.ToString();
            button.IsEnabled = false;
        }
        private void uncoverButton(Button button, string text)
        {
            button.Content = text;
            button.IsEnabled = false;
            button.BorderThickness = new Thickness(10);
        }

        private void uncoverAllBombs()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                for (int j = 0; j < buttons[i].Length; j++)
                {
                    if (isBomb(i, j)) uncoverButton(buttons[i][j], "*");
                }
            }
        }

        private (int i, int j) GetIJ(Button button)
        {
            int separatorIndex = button.Tag.ToString().IndexOf(":");
            int i = int.Parse(button.Tag.ToString().Substring(2, separatorIndex-2));
            int j = int.Parse(button.Tag.ToString().Substring(separatorIndex+1, button.Tag.ToString().Length-separatorIndex-1));
            return (i, j);
        }
        private (int i, int j) GetIJ(object Tag)
        {
            int separatorIndex = Tag.ToString().IndexOf(":");
            int i = int.Parse(Tag.ToString().Substring(0, separatorIndex));
            int j = int.Parse(Tag.ToString().Substring(separatorIndex+1, Tag.ToString().Length-separatorIndex-1));
            return (i, j);
        }
        private void GameOver(Button button)
        {
            isGameOver = true;
            uncoverButton(button, "*");
            MessageBox.Show("GameOver!");
            if (MessageBox.Show("Do you Want to start a new game?", "Question!", MessageBoxButton.YesNo, MessageBoxImage.Question) ==MessageBoxResult.Yes)
            {
                Restart();
                return;
            }
            uncoverAllBombs();
        }

        private int checkAroundPoint(int i, int j)
        {
            int count = 0;
            count+= i==0 ? 0 : isBomb(i-1, j) ? 1 : 0;
            count+= i==0 || j==buttons[i].Length-1 ? 0 : isBomb(i-1, j+1) ? 1 : 0;
            count+= j==buttons[i].Length-1 ? 0 : isBomb(i, j+1) ? 1 : 0;
            count+= i==buttons.Length-1 || j == buttons[i].Length-1 ? 0 : isBomb(i+1, j+1) ? 1 : 0;
            count+= i==buttons.Length-1 ? 0 : isBomb(i+1, j) ? 1 : 0;
            count+= i==buttons.Length-1 || j == 0 ? 0 : isBomb(i+1, j-1) ? 1 : 0;
            count+= j==0 ? 0 : isBomb(i, j-1) ? 1 : 0;
            count+= i==0 || j==0 ? 0 : isBomb(i-1, j-1) ? 1 : 0;
            return count;
        }
        private void AddToCheckAroundPoint(List<Button> toCheck, int i, int j)
        {
            if (i != 0 && buttons[i-1][j].IsEnabled) toCheck.Add(buttons[i-1][j]);
            if (j != buttons[i].Length-1 && buttons[i][j+1].IsEnabled) toCheck.Add(buttons[i][j+1]);
            if (i != buttons.Length-1 && buttons[i+1][j].IsEnabled) toCheck.Add(buttons[i+1][j]);
            if (j != 0 && buttons[i][j-1].IsEnabled) toCheck.Add(buttons[i][j-1]);
            if ((int)difficulty<3)
            {
                if (i != 0 && j!=buttons[i].Length-1 && buttons[i-1][j+1].IsEnabled) toCheck.Add(buttons[i-1][j+1]);
                if (i != buttons.Length-1 && j != buttons[i].Length-1 && buttons[i+1][j+1].IsEnabled) toCheck.Add(buttons[i+1][j+1]);
                if (i != buttons.Length-1 && j != 0 && buttons[i+1][j-1].IsEnabled) toCheck.Add(buttons[i+1][j-1]);
                if (i != 0 && j != 0 && buttons[i-1][j-1].IsEnabled) toCheck.Add(buttons[i-1][j-1]);
            }
        }
        private bool isBomb(int i, int j)
        {
            return buttons[i][j].Tag.ToString()[0] == '1';
        }

        private void comboBoxDifficulty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            difficulty = (Difficulty)(comboBoxDifficulty.SelectedIndex+1);
            Restart();
        }

        private void comboBoxDifficultyInit()
        {
            Array arr = Enum.GetValues(typeof(Difficulty));
            foreach (var item in arr)
            {
                comboBoxDifficulty.Items.Add(item.ToString());
            }
            comboBoxDifficulty.SelectedIndex = 0;
        }
        private void Restart()
        {
            InitPole();
            PlaceBombs();
        }
    }
}
