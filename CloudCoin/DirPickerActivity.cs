using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

using Environment = Android.OS.Environment;
using CloudCoinCore;

namespace CloudCoinApp
{
    public class Item
    {
        public String file;
        public int icon;
        public bool selected;

        public Item(String file, int icon)
        {
            this.file = file;
            this.icon = icon;
            this.selected = false;
        }

        public override String ToString()
        {
            return file;
        }
    }

    public class FileListAdapter : ArrayAdapter<Item>
    {
        private DirPickerActivity _owner;

        public FileListAdapter(DirPickerActivity owner) 
            : base(owner, Android.Resource.Layout.SelectDialogItem, Android.Resource.Id.Text1, owner.fileList)
        {
            _owner = owner;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = base.GetView(position, convertView, parent);
            TextView textView = (TextView)view.FindViewById(Android.Resource.Id.Text1);

            int drawableID = 0;
            if (_owner.fileList[position].icon != -1)
            {
                drawableID = _owner.fileList[position].icon;
            }
            textView.SetCompoundDrawablesWithIntrinsicBounds(drawableID, 0, 0, 0);
            textView.Ellipsize = null;

            int dp3 = (int)(4 * Application.Context.Resources.DisplayMetrics.Density + 0.5f);
            textView.CompoundDrawablePadding = dp3;
            if (_owner.fileList[position].selected)
                textView.SetBackgroundColor(Android.Graphics.Color.Rgb(0x34,0x8E, 0xFB));
            else
                textView.SetBackgroundColor(Android.Graphics.Color.LightGray);

            return view;
        }

        public void Refresh()
        {
            Clear();
            AddAll(_owner.fileList);
            NotifyDataSetChanged();
        }
    }


    [Activity(Label = "DirPickerActivity")]
    public class DirPickerActivity : Activity
    {
        // Intent parameters names constants
        public const String returnParameter = "directoryPathRet";

        // Stores names of traversed directories
        List<String> pathDirsList = new List<String>();
        List<String> chosenFiles = new List<String>();

        static String TAG = "CLOUDCOIN";

        public List<Item> fileList = new List<Item>();
        private String path = null;
        private String chosenFile;

        FileListAdapter adapter;

        private bool showHiddenFilesAndDirs = true;

        private bool directoryShownIsEmpty = false;

        private String filterFileExtension = null;

        private const int MAX_FILES = 100;

        // Action constants
        private const int SELECT_DIRECTORY = 1;
        private const int SELECT_FILE = 2;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.dirpicker);

            // Set action for this activity
            Intent thisInt = this.Intent;

            SetInitialDirectory();
            ParseDirectoryPath();
            LoadFileList();
            adapter = new FileListAdapter(this);
            InitializeButtons();
            InitializeFileListView();
            UpdateCurrentDirectoryTextView();
        }

        private void SetInitialDirectory()
        {
            if (Environment.ExternalStorageDirectory.IsDirectory
                    && Environment.ExternalStorageDirectory.CanRead())
                path = Environment.ExternalStorageDirectory.AbsolutePath;
            else
                path = Environment.RootDirectory.AbsolutePath;
        }

        private void ParseDirectoryPath()
        {
            pathDirsList.Clear();
            String[] parts = path.Split('/');
            int i = 0;
            while (i < parts.Length)
            {
                pathDirsList.Add(parts[i]);
                i++;
            }
        }

        private void InitializeButtons()
        {
            Button upDirButton = (Button)this.FindViewById(Resource.Id.upDirectoryButton);
            upDirButton.Click += delegate
            {
                LoadDirectoryUp();
                LoadFileList();
                adapter.Refresh();
                UpdateCurrentDirectoryTextView();
            };

		    Button selectFolderButton = (Button)this.FindViewById(Resource.Id.selectCurrentDirectoryButton);
            selectFolderButton.Click += delegate
            {
                ReturnDirectoryFinishActivity();
            };
        }

        private void LoadDirectoryUp()
        {
            // present directory removed from list
            String s = pathDirsList.Last();
            if (s == "") return;
            pathDirsList.RemoveAt(pathDirsList.Count - 1);

            // path modified to exclude present directory
            path = path.Substring(0, path.LastIndexOf(s));
            fileList.Clear();
        }

        private void UpdateCurrentDirectoryTextView()
        {
            int i = 0;
            String curDirString = "";
            while (i < pathDirsList.Count)
            {
                curDirString += pathDirsList[i] + "/";
                i++;
            }
            if (pathDirsList.Count == 0)
            {
                ((Button)this.FindViewById(Resource.Id.upDirectoryButton)).Enabled = false;
                curDirString = "/";
            }
            else
                ((Button)this.FindViewById(Resource.Id.upDirectoryButton)).Enabled = true;

            String cd = Resources.GetString(Resource.String.currentdirectory);
            String fp = Resources.GetString(Resource.String.filespicked);

            ((TextView)this.FindViewById(Resource.Id.currentdir)).Text = cd + ": " + curDirString;

            ((TextView)this.FindViewById(Resource.Id.filespicked)).Text = fp + ": " + chosenFiles.Count;

        }

        private void ReturnDirectoryFinishActivity()
        {
            Intent retIntent = new Intent();
            retIntent.PutStringArrayListExtra(returnParameter, chosenFiles);
            this.SetResult(Result.Ok, retIntent);
            this.Finish();
        }

        private void LoadFileList()
        {
            fileList.Clear();
            var dir = new DirectoryInfo(path);
            this.directoryShownIsEmpty = false;

            try
            {
                foreach (var item in dir.GetFileSystemInfos().Where(item => item.IsVisible()))
                {
                    int drawableID;
                    if (item.IsDirectory())
                    {
                        drawableID = Resource.Drawable.folder_icon;
                        //drawableID = Resource.Drawable.folder_icon_light;
                    }
                    else
                    {
                        drawableID = Resource.Drawable.file_icon;
                    }

                    fileList.Add(new Item(item.Name, drawableID));
                }

                if (fileList.Count == 0)
                {
                    this.directoryShownIsEmpty = true;
                    fileList.Add(new Item(Resources.GetString(Resource.String.emptydir), -1));
                }
                else
                {
                    //Collections.sort(fileList, new ItemFileNameComparator());
                }
            }
            catch (Exception ex)
            {
                Log.Error("FileListFragment", "Couldn't access the directory " + path + "; " + ex);
                Toast.MakeText(this, "Problem retrieving contents of " + path, ToastLength.Long).Show();
                return;
            }
        }

        private void InitializeFileListView()
        {
            ListView lView = (ListView)this.FindViewById(Resource.Id.fileListView);
            lView.SetBackgroundColor(Android.Graphics.Color.LightGray);
            LinearLayout.LayoutParams lParam = new LinearLayout.LayoutParams(
                WindowManagerLayoutParams.FillParent, WindowManagerLayoutParams.FillParent);
            lParam.SetMargins(15, 5, 15, 5);
            lView.SetAdapter(this.adapter);
            lView.ItemClick += (object sender, ListView.ItemClickEventArgs e) =>
            {
                if (fileList[e.Position].icon == -1) return;

                chosenFile = fileList[e.Position].file;
                string chosenPath = (path == "/" ? path + chosenFile : path + "/" + chosenFile);
                FileAttributes selattr = File.GetAttributes(chosenPath);
                if (selattr.HasFlag(FileAttributes.Directory))
                {
                    pathDirsList.Add(chosenFile);
                    path = chosenPath;
                    LoadFileList();
                    adapter.Refresh();
                    UpdateCurrentDirectoryTextView();
                }
                else
                {
                    int rv = pickPath(chosenPath);

                    if (rv == -1)
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.toomanyfiles), ToastLength.Long).Show();
                        return;
                    }

                    if (rv == 0)
                    {
                        fileList[e.Position].selected = false;
                        e.View.SetBackgroundColor(Android.Graphics.Color.LightGray);
                    }
                    else
                    {
                        fileList[e.Position].selected = true;
                        e.View.SetBackgroundColor(Android.Graphics.Color.Rgb(0x34, 0x8E, 0xFB));
                    }

                    UpdateCurrentDirectoryTextView();
                }
            };
	    }

        private int pickPath(String path)
        {
            bool toDelete = false;
            foreach (String file in chosenFiles)
            {
                if (file.Equals(path))
                {
                    toDelete = true;
                    break;
                }
            }

            if (toDelete)
            {
                chosenFiles.Remove(path);
                return 0;
            }

            if (chosenFiles.Count >= MAX_FILES)
                return -1;

            chosenFiles.Add(path);
            return 1;
        }
    }
}

