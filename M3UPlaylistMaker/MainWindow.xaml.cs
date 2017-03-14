using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Xceed.Wpf.Toolkit;


namespace M3UPlaylistMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        class Album
        {
            public string Path { get; set; }
            public Album(string path)
            {
                Path = path;
            }
        }

        class Track
        {
            public string Path { get; set; }
            public TreeViewItem Item { get; set; }

            public Track (string path, TreeViewItem item)
            {
                // trim off the drive letter
                Path = path.Substring(3);

                Item = item;
            }

            static string[] musicFileTypes = new string[] { ".flac", ".mp3", ".m4a", ".wma" };

            public static bool ValidTrack(string name)
            {
                string extension = System.IO.Path.GetExtension(name).ToLower();

            
                foreach (string musicType in musicFileTypes)
                {
                    if (extension == musicType)
                        return true;
                }
                return false;
            }
        }

        List<Track> selectedTracks = new List<Track>();

        SolidColorBrush notSelectedBrush = new SolidColorBrush(Colors.White);
        SolidColorBrush selectedBrush = new SolidColorBrush(Colors.LightGray);

        string currentDrive;

        private void refreshList()
        {
            StringBuilder listing = new StringBuilder();
            foreach (Track t in selectedTracks)
            {
                listing.AppendLine(t.Path);
            }
            SelectedTracks.Text = listing.ToString();
        }

        private void selectItem(TreeViewItem item)
        {
            if (item.Tag is Track)
            {
                Track track = item.Tag as Track;

                if (!selectedTracks.Contains(track))
                {
                    item.Background = selectedBrush;
                    selectedTracks.Add(track);
                }
            }

            if(item.Tag is Album)
            {
                foreach(TreeViewItem albumitem in item.Items)
                {
                    selectItem(albumitem);
                }
            }
        }

        private void updateTrackSelection(TreeViewItem item)
        {
            if (item == null) return;

            if (item.Tag is Track)
            {
                Track track = item.Tag as Track;

                if (selectedTracks.Contains(track))
                {
                    item.Background = notSelectedBrush;
                    selectedTracks.Remove(track);
                }
                else
                {
                    selectItem(item);
                }
            }

            if(item.Tag is Album)
            {
                foreach( TreeViewItem albumItem in item.Items)
                {
                    selectItem(albumItem);
                }
            }
            refreshList();
        }

        void AddDirectory(string path, ItemCollection destinationCollection)
        {
            // skip hidden folders

            DirectoryInfo info = new DirectoryInfo(path);

            if (info.Attributes.HasFlag(FileAttributes.Hidden))
                return;

            TreeViewItem item = new TreeViewItem();

            item.Header = System.IO.Path.GetFileName(path);

            item.Tag = new Album(path);

            destinationCollection.Add(item);

            var directories = Directory.EnumerateDirectories(path);

            foreach(string dir in directories)
            {
                AddDirectory(dir, item.Items);
            }

            var files = Directory.EnumerateFiles(path);

            foreach(string file in files)
            {
                // skip hidden files
                FileInfo fileInfo = new FileInfo(file);

                if(fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    continue;

                if (!Track.ValidTrack(file))
                {
                    continue;
                }

                TreeViewItem fileItem = new TreeViewItem();
                fileItem.Background = notSelectedBrush;
                Track track = new Track(file,item);
                fileItem.Tag = track;
                fileItem.Header = System.IO.Path.GetFileName(file);

                item.Items.Add(fileItem);
            }
        }

        void buildCurrentDrive()
        {
            DriveTreeview.Items.Clear();
            SelectedFolderTextBlock.Text = currentDrive;
            AddDirectory(currentDrive, DriveTreeview.Items);
        }

        void selectFolder(string path)
        {
            currentDrive = path;
            buildCurrentDrive();
        }

        void savePlaylist()
        {
            if(playlistName.Text=="")
            {
                System.Windows.Forms.MessageBox.Show("Please enter a playlist name", "Playlist Save");
            }

            string fullFilename = currentDrive + playlistName.Text + ".M3U";

            if(File.Exists(fullFilename))
            {
                if (System.Windows.Forms.MessageBox.Show("Playlist already exists.\nDo you want to overwrite it?", "Playlist Save", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    return;
            }

            File.WriteAllText(fullFilename, SelectedTracks.Text);

            playlistName.Text = "";
        }


        void clearSelections()
        {
            buildCurrentDrive();

            selectedTracks.Clear();

            SelectedTracks.Text = "";
        }

        void browseForFolder()
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectFolder(f.SelectedPath);
            }
        }

        private void Folder_Select_Button_Click(object sender, RoutedEventArgs e)
        {
            browseForFolder();
        }

        private void DriveTreeview_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            updateTrackSelection(DriveTreeview.SelectedItem as TreeViewItem);
        }

        private void saveButtonClick(object sender, RoutedEventArgs e)
        {
            savePlaylist();
        }

        private void clearButtonClick(object sender, RoutedEventArgs e)
        {
            clearSelections();
        }
    }
}
