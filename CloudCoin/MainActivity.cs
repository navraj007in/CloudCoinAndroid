using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

using Android.App;
using Android.Widget;
using Android.OS;
//using Android.Support.V7.App;
using Android.Views;
using Android.Content;
using Android.Preferences;
using Android.Content.PM;
using Android.Net;

using Android.Graphics.Drawables;
using Android.Graphics;

using Environment = System.Environment;
using Uri = Android.Net.Uri;
using Android.Database;
using Android.Provider;
//using Android.Support.V4.App;
//using Android.Support.V4.Content;
using Android;

using CloudCoinCore;
using CloudCoinCoreDirectory;
using Android.Util;

using Config = CloudCoinCore.Config;
using System.Net;
using System.IO;

namespace CloudCoinApp
{

    class CoinDialog : Dialog
    {
        public enum DialogType { Import, Bank, Export }

        public static int IDX_BANK = 0;
        public static int IDX_COUNTERFEIT = 1;
        public static int IDX_FRACTURED = 2;

        public int layoutid;
        public MainActivity owner;
        DialogType dlgType;

        public int banked, fracked, failed, lost, suspect;

        public TextView[][] ids;
        public int[][] stats;
        public int size;

        public NumberPicker[] nps;
        public TextView[] tvs;

        public EditText et;
        public TextView tvTotal, exportTv;

        //public Button button, emailButton;

        public CoinDialog(Activity activity) : base(activity)
        {
            owner = (MainActivity)activity;
        }

        public CoinDialog(Activity activity, int layoutid) : base(activity)
        {
            owner = (MainActivity)activity;
            this.layoutid = layoutid;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(layoutid);
            //Window.SetSoftInputMode(WindowManagerLayoutParams.SOFT_INPUT_STATE_ALWAYS_HIDDEN);
            Window.SetLayout(WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.WrapContent);
            Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            LinearLayout closeButton = FindViewById<LinearLayout>(Resource.Id.closebutton);
            closeButton.Click += delegate {
                Dismiss();
            };
        }

        public void Update(int layout)
        {
            //if (isImportDialog)  return;
            layoutid = layout;
            Create();
            //RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(layout);
            //Window.SetSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_HIDDEN);
            Window.SetLayout(WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.WrapContent);
            Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            LinearLayout closeButton = FindViewById<LinearLayout>(Resource.Id.closebutton);
            closeButton.Click += delegate
            {
                Dismiss();
            };
        }

        public void Init(DialogType dialogtype)
        {
            int i, resId;
            String idTxt;
            int[] bankCoins, frackedCoins;
            int lTotal;

            dlgType = dialogtype;

            switch (dialogtype)
            {
                case DialogType.Import:
                    break;

                case DialogType.Export:
                    size = Banker.denominations.Length;
                    nps = new NumberPicker[size];
                    tvs = new TextView[size];

                    for (i = 0; i < size; i++)
                    {
                        idTxt = "np" + Banker.denominations[i];
                        resId = owner.Resources.GetIdentifier(idTxt, "id", owner.PackageName);
                        nps[i] = FindViewById<NumberPicker>(resId);
                        setNumberPickerTextColor(nps[i], Color.ParseColor("#348EFB"));

                        idTxt = "bs" + Banker.denominations[i];
                        resId = owner.Resources.GetIdentifier(idTxt, "id", owner.PackageName);
                        tvs[i] = FindViewById<TextView>(resId);
                    }


                    tvTotal = FindViewById<TextView>(Resource.Id.exptotal);
                    exportTv = FindViewById<TextView>(Resource.Id.exporttv);
                    et = FindViewById<EditText>(Resource.Id.exporttag);

                    bankCoins = new int[size];
                    bankCoins[0] = owner.onesCount;
                    bankCoins[1] = owner.fivesCount;
                    bankCoins[2] = owner.qtrCount;
                    bankCoins[3] = owner.hundredsCount;
                    bankCoins[4] = owner.twoFiftiesCount;

                    frackedCoins = new int[size];
                    frackedCoins[0] = owner.onesFrackedCount;
                    frackedCoins[1] = owner.fivesFrackedCount;
                    frackedCoins[2] = owner.qtrFrackedCount;
                    frackedCoins[3] = owner.hundredsFrackedCount;
                    frackedCoins[4] = owner.twoFrackedFiftiesCount;

                    int overall = 0;
                    for (i = 0; i < size; i++)
                    {
                        lTotal = bankCoins[i] + frackedCoins[i];

                        nps[i].MinValue = 0;
                        nps[i].MaxValue = lTotal;
                        nps[i].Value = 0;
                        nps[i].ValueChanged += delegate
                        {
                            updateTotal();
                        };
                        nps[i].Tag = Banker.denominations[i];
                        nps[i].WrapSelectorWheel = false;

                        tvs[i].Text = "" + lTotal;

                        overall += Banker.denominations[i] * lTotal;
                    }

                    updateTotal();
                    tvTotal.Text = "" + overall;

                    String msg = String.Format(owner.Resources.GetString(Resource.String.exportnotice), owner.bank.fileUtils.ExportFolder);
                    TextView eNotice = FindViewById<TextView>(Resource.Id.en);
                    eNotice.Text = msg;
                    break;
            }

        }

        public int getTotal()
        {
            int total = 0;

            switch (dlgType)
            {
                case DialogType.Export:
                    for (int i = 0; i < size; i++)
                    {
                        int denomination = Banker.denominations[i];
                        total += denomination * nps[i].Value;
                    }
                    break;

            }
            return total;
        }

        public void allocId(int idx, String prefix)
        {
            int resId, i;
            String idTxt;

            stats[idx] = new int[size];
            ids[idx] = new TextView[size];
            for (i = 0; i < size; i++)
            {
                if (i == size - 1)
                    idTxt = prefix + "all";
                else
                    idTxt = prefix + Banker.denominations[i];

                resId = owner.Resources.GetIdentifier(idTxt, "id", owner.PackageName);
                ids[idx][i] = FindViewById<TextView>(resId);
            }
        }

        private void updateTotal()
        {
            if (exportTv == null) return;

            int total = getTotal();

            string sb = "";
            sb += owner.Resources.GetString(Resource.String.export);
            sb += " " + total;
            exportTv.Text = sb;
        }

        public void DoExport()
        {
            /*           String exportTag;
                       int[] values;
                       int[] failed;
                       int totalFailed = 0;

                       Resources res = getResources();

                       if (getTotal() == 0)
                       {
                           Toast.MakeText(getBaseContext(), MainActivity. Resource.String.nocoins, ToastLength.Long).Show();
                           return;
                       }

                       et = dialog.FindViewById<EditText>(Resource.Id.exporttag);
                       exportTag = et.Text;

                       RadioGroup rg = (RadioGroup)dialog.findViewById(R.id.radioGroup);
                       int selectedId = rg.getCheckedRadioButtonId();

                       values = new int[size];
                       for (int i = 0; i < size; i++)
                           values[i] = nps[i].getValue();

                       if (isFixing)
                       {
                           showError(res.getString(R.string.fixing));
                           return;
                       }

                       if (selectedId == R.id.rjpg)
                       {
                           failed = bank.exportJpeg(values, exportTag);
                       }
                       else if (selectedId == R.id.rjson)
                       {
                           failed = bank.exportJson(values, exportTag);
                       }
                       else
                       {
                           Log.v("CC", "We will never be here");
                           return;
                       }

                       String msg;

                       if (failed[0] == -1)
                       {
                           msg = res.getString(R.string.globalexporterror);
                       }
                       else
                       {
                           for (int i = 0; i < size; i++)
                           {
                               totalFailed += failed[i];
                           }
                           if (totalFailed == 0)
                           {
                               msg = String.format(res.getString(R.string.exportok), bank.getRelativeExportDirPath());
                           }
                           else
                           {
                               msg = String.format(res.getString(R.string.exportfailed), totalFailed);
                           }
                       }*/

        }


        public void onValueChange(NumberPicker picker, int oldVal, int newVal)
        {
            updateTotal();
        }

        private void setNumberPickerTextColor(NumberPicker numberPicker, Color color)
        {
            int count = numberPicker.ChildCount;

            for (int i = 0; i < count; i++)
            {
                View child = numberPicker.GetChildAt(i);
                if (child is EditText)
                {
                    try
                    {
                        Java.Lang.Reflect.Field selectorWheelPaintField = numberPicker.Class
                            .GetDeclaredField("mSelectorWheelPaint");
                        selectorWheelPaintField.Accessible = true;

                        Java.Lang.Reflect.Field selectorDivider = numberPicker.Class
                            .GetDeclaredField("mSelectionDivider");
                        selectorDivider.Accessible = true;

                        ColorDrawable colorDrawable = new ColorDrawable(Color.ParseColor("#ECECEC"));

                        selectorDivider.Set(numberPicker, colorDrawable);


                        ((Paint)selectorWheelPaintField.Get(numberPicker)).Color = color;
                        ((EditText)child).SetTextColor(color);
                        numberPicker.Invalidate();

                        return;
                    }
                    catch (Java.Lang.NoSuchFieldException) { }
                    catch (Java.Lang.IllegalAccessException) { }
                    catch (Java.Lang.IllegalArgumentException) { }
                }
            }

        }

    }

    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public enum ImportState { ImportInit, ImportIng, ImportDone }

        private LinearLayout linearLayoutImport;
        private LinearLayout linearLayoutBank;
        private LinearLayout linearLayoutExport;

        public static string version = "";
        public static readonly int PickImageId = 1000;
        private CoinDialog dialog = null;
        private ImportState importState;
        private List<String> files = new List<String>();

        public static List<RAIDA> networks = new List<RAIDA>(); // Raida network chain

        public Banker bank;
        public static RAIDA raida = RAIDA.GetInstance();
        public static int NetworkNumber = 1;

        public int raidaReady = 0, raidaNotReady = 0;
        private bool asyncFinished = true;
        private int lastProgress;
        private TextView titleText;
        private ProgressBar importBar;
        private TextView importText;
        private bool isImportSuspect = false;
        private bool isImportDialog;

        #region Total Variables
        public int onesCount = 0;
        public int fivesCount = 0;
        public int qtrCount = 0;
        public int hundredsCount = 0;
        public int twoFiftiesCount = 0;

        public int onesFrackedCount = 0;
        public int fivesFrackedCount = 0;
        public int qtrFrackedCount = 0;
        public int hundredsFrackedCount = 0;
        public int twoFrackedFiftiesCount = 0;

        public int onesTotalCount = 0;
        public int fivesTotalCount = 0;
        public int qtrTotalCount = 0;
        public int hundredsTotalCount = 0;
        public int twoFiftiesTotalCount = 0;
        #endregion

        public event EventHandler ProgressChanged;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            Init();
        }


        public async Task EchoTask()
        {
            Console.Out.WriteLine(String.Format("Starting Echo to RAIDA Network {0}\n", 1));
            Console.Out.WriteLine("----------------------------------\n");

            var progressIndicator = new Progress<ProgressReport>(ReportProgress);
            var echos = raida.GetEchoTasks(progressIndicator);
            await Task.WhenAll(echos.AsParallel().Select(async task => await task));

            //MessageBox.Show("Finished Echo");
            Console.Out.WriteLine("Ready Count -" + raida.ReadyCount);
            Console.Out.WriteLine("Not Ready Count -" + raida.NotReadyCount);

            for (int i = 0; i < raida.nodes.Count(); i++)
            {
                // Console.Out.WriteLine("Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n");
                Console.Out.WriteLine("Node" + i + " Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Console.Out.WriteLine("-----------------------------------\n");
        }

        public async Task ImportTask()
        {
            importState = ImportState.ImportIng;
            isImportDialog = false;
            lastProgress = 0;
            ShowImportScreen();

            if (isImportSuspect)
            {
                int i = 0;
                foreach (CloudCoin coin in bank.fileUtils.suspectCoins)
                {
                    List<Func<Task<Response>>> detects = raida.GetDetectTasks(coin);
                    await Task.WhenAll(detects.AsParallel().Select(async task => await task()));

                    // coin detect finished
                    if (detects.Any(t => t().Result.success))
                        bank.fileUtils.MoveCoins(new List<CloudCoin> { coin }, coin.folder, bank.fileUtils.BankFolder);


                    i++;
                    await Progress(i);

                }

                dialog.Dismiss();
                isImportSuspect = false;
                importState = ImportState.ImportDone;
            }
            else
            {
                titleText.Text = "RAIDA Echo";
                importBar.Max = raida.nodes.Length;
                importText.Text = "";
                await EchoTask(); // get echos from raida
                Toast.MakeText(this, "Raida ReadyCount = " + raida.ReadyCount.ToString(), ToastLength.Long).Show();
                if (raida.ReadyCount == 0)
                {
                    dialog.Update(Resource.Layout.importdialog2);
                    dialog.Show();
                    return;
                }

                titleText.SetText(Resource.String.importcoins);
                importText.Text = "";
                importBar.Progress = 0;
                importBar.Max = Config.NodeCount;
                importBar.Invalidate();
                isImportSuspect = false;
                await ProcessCoins(true);

                importState = ImportState.ImportDone;
                dialog.Dismiss();
                isImportDialog = false;
            }

            ShowImportScreen();
        }

        async Task Progress(int i)
        {
            await Task.Run(() =>
            {
                importText.Text = String.Format(Resources.GetString(Resource.String.authstring),
                    i, bank.fileUtils.importCoins.Count());

                importBar.Progress = i;
                importBar.Invalidate();
            });
        }

        private void Init()
        {
            linearLayoutImport = FindViewById<LinearLayout>(Resource.Id.limport);
            linearLayoutImport.Click += delegate
            {
                if (!asyncFinished) return;

                // check online status
                ConnectivityManager cm = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
                NetworkInfo netInfo = cm.ActiveNetworkInfo;

                if (netInfo == null || !netInfo.IsConnectedOrConnecting)
                {
                    dialog.Update(Resource.Layout.importdialog2);
                    dialog.Show();
                    return;
                }


                files.Clear();
                ShowImportScreen();

            };

            linearLayoutBank = FindViewById<LinearLayout>(Resource.Id.lbank);
            linearLayoutBank.Click += delegate
            {
                ShowBankScreen();
            };

            linearLayoutExport = FindViewById<LinearLayout>(Resource.Id.lexport);
            linearLayoutExport.Click += delegate
            {
                ShowExportScreen();
            };

            //mSettings = PreferenceManager.GetDefaultSharedPreferences(this);

            try
            {
                version = PackageManager.GetPackageInfo(PackageName, 0).VersionName;
            }
            catch (PackageManager.NameNotFoundException)
            {
                version = "";
            }
            FindViewById<TextView>(Resource.Id.tversion).Text = version;

            asyncFinished = true;
            importState = ImportState.ImportInit;
            dialog = new CoinDialog(this, Resource.Layout.importdialog);
            dialog.Create();

            //string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Log.Info("Path", path);
            bank = new Banker(new FileSystem(path));
            bank.fileUtils.CreateDirectories();
            bank.fileUtils.LoadFileSystem();

            InitNetworks();
        }

        public void InitNetworks()
        {
            string nodesJson = "";
            networks.Clear();
            using (WebClient client = new WebClient())
            {
                try
                {
                    //client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    nodesJson = client.DownloadString(Config.URL_DIRECTORY);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (System.IO.File.Exists("directory.json"))
                    {
                        nodesJson = System.IO.File.ReadAllText(Environment.CurrentDirectory + @"\directory.json");
                    }
                    else
                    {
                        Exception raidaException = new Exception("RAIDA instantiation failed. No Directory found on server or local path");
                        throw raidaException;
                    }
                }
            }
            /*using (HttpClient client = new HttpClient())
            {
                try
                {
                    nodesJson = await client.GetStringAsync(Config.URL_DIRECTORY);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (System.IO.File.Exists("directory.json"))
                    {
                        nodesJson = System.IO.File.ReadAllText(Environment.CurrentDirectory + @"\directory.json");
                    }
                    else
                    {
                        Toast.MakeText(this, "RAIDA instantiation failed. No Directory found on server or local path", ToastLength.Long).Show();
                        Finish();
                        //Exception raidaException = new Exception("RAIDA instantiation failed. No Directory found on server or local path");
                        //throw raidaException;
                    }
                }
            }*/

            try
            {
                RAIDADirectory dir = JsonConvert.DeserializeObject<RAIDADirectory>(nodesJson);

                foreach (var network in dir.networks)
                {
                    networks.Add(RAIDA.GetInstance(network));
                }
            }
            catch (Exception)
            {
                Exception raidaException = new Exception("RAIDA instantiation failed. No Directory found on server or local path");
                throw raidaException;
            }

            if (networks == null)
            {
                Exception raidaException = new Exception("RAIDA instantiation failed. No Directory found on server or local path");
                throw raidaException;
            }

            if (networks.Count == 0)
            {
                Exception raidaException = new Exception("RAIDA instantiation failed. No Directory found on server or local path");
                throw raidaException;
            }
            else
            {
                raida = (from x in networks
                         where x.NetworkNumber == NetworkNumber
                         select x).FirstOrDefault();
                RAIDA.ActiveRAIDA = raida;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            if (requestCode == PickImageId)
            {
                if ((resultCode == Result.Ok) && (data != null))
                {
                    Uri uri;
                    String[] projection = new[] { MediaStore.Images.Media.InterfaceConsts.Data };
                    files.Clear();
                    string file;

                    if (data.Data != null)
                    {
                        uri = data.Data;
                        file = FilePath.GetPath(this, uri);
                        String[] path = file.Split(':');
                        if (path.Length > 1)
                            files.Add(path[1]);
                        else
                            files.Add(file);
                        //files.Add(file);
                    }
                    else
                    {
                        if (data.ClipData != null)
                        {
                            ClipData mClipData = data.ClipData;
                            List<Uri> mArrayUri = new List<Uri>();
                            for (int i = 0; i < mClipData.ItemCount; i++)
                            {

                                ClipData.Item item = mClipData.GetItemAt(i);
                                uri = item.Uri;
                                file = FilePath.GetPath(this, uri);
                                String[] path = file.Split(':');
                                if (path.Length > 1)
                                    files.Add(path[1]);
                                else
                                    files.Add(file);
                            }
                            Log.Verbose("LOG_TAG", "Selected Images" + mArrayUri.Count);
                        }
                    }

                }
                else
                {
                    //			showError("Internal error");
                }

                dialog.Dismiss();
                isImportDialog = false;
                ShowImportScreen();
                return;
            }

            //bank.moveExportedToSent();
            dialog.Dismiss();
        }

        private String getStatusString(int progressCoins)
        {
            String statusString;

            if (isImportSuspect) return "";

            int totalIncomeLength = bank.fileUtils.importCoins.Count();
            int importedIncomeLength = progressCoins + 1;

            statusString = String.Format(Resources.GetString(Resource.String.authstring), importedIncomeLength, totalIncomeLength);

            return statusString;
        }

        public void selectFile()
        {
            Intent intent = new Intent();
            //intent.SetType("image/*");
            intent.SetType("*/*");
            intent.PutExtra(Intent.ExtraAllowMultiple, true);
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Image"), PickImageId);
        }

        public void ShowImportScreen()
        {
            int totalIncomeLength;
            String result;
            TextView ttv;
            TextView tv;
            FileSystem FS = bank.fileUtils;

            switch (importState)
            {
                case ImportState.ImportIng:
                    dialog.Update(Resource.Layout.importraida);
                    titleText = dialog.FindViewById<TextView>(Resource.Id.titletext);
                    importText = dialog.FindViewById<TextView>(Resource.Id.infotext);
                    importText.Text = getStatusString(lastProgress);

                    TextView subTv = dialog.FindViewById<TextView>(Resource.Id.infotextsub);

                    importBar = dialog.FindViewById<ProgressBar>(Resource.Id.firstBar);
                    importBar.Max = 100;
                    dialog.Show();
                    break;

                case ImportState.ImportDone:
                    importState = ImportState.ImportInit;
                    dialog.Update(Resource.Layout.importdialog5);
                    LinearLayout emailButton = dialog.FindViewById<LinearLayout>(Resource.Id.emailbutton);
                    emailButton.Click += delegate
                    {
                        dialog.Dismiss();
                        //doEmailReceipt();
                    };

                    ttv = (TextView)dialog.FindViewById(Resource.Id.banked);
                    ttv.Text = dialog.banked.ToString();

                    ttv = (TextView)dialog.FindViewById(Resource.Id.fracked);
                    ttv.Text = dialog.fracked.ToString();

                    ttv = (TextView)dialog.FindViewById(Resource.Id.failed);
                    ttv.Text = dialog.failed.ToString();

                    ttv = (TextView)dialog.FindViewById(Resource.Id.lost);
                    ttv.Text = dialog.lost.ToString();

                    ttv = (TextView)dialog.FindViewById(Resource.Id.suspect);
                    ttv.Text = dialog.suspect.ToString();

                    try
                    {
                        dialog.Show();
                    }
                    catch (Exception)
                    {
                        Log.Verbose("CLOUDCOIN", "Activity is gone. No result will be shown");
                    }
                    break;

                case ImportState.ImportInit:
                    if (FS.suspectCoins.Count() > 0)
                    {
                        dialog.Update(Resource.Layout.importsuspect);
                        LinearLayout goButton = dialog.FindViewById<LinearLayout>(Resource.Id.gobutton);
                        goButton.Click += async delegate
                        {
                            isImportSuspect = true;
                            await ImportTask();
                        };
                        dialog.Show();
                        return;
                    }

                    if (files != null && files.Count > 0)
                    {
                        foreach (string file in files)
                        {
                            IEnumerable<CloudCoin> coins = FS.LoadCoins(file);
                            if (coins != null)
                            {
                                FS.WriteCoinsToFile(coins, FS.ImportFolder + System.IO.Path.GetFileName(file));
                                FS.LoadFileSystem();
                            }
                        }
                    }
                    else
                    {
                        FS.importCoins = FS.LoadFolderCoins(FS.ImportFolder);
                    }

                    dialog.Update(Resource.Layout.importdialog);
                    tv = dialog.FindViewById<TextView>(Resource.Id.infotext);
                    LinearLayout fileButton = dialog.FindViewById<LinearLayout>(Resource.Id.filebutton);
                    fileButton.Click += delegate
                    {
                        selectFile();
                    };

                    totalIncomeLength = FS.importCoins.Count();
                    if (totalIncomeLength == 0)
                    {
                        result = String.Format(Resources.GetString(Resource.String.erremptyimport),
                            FS.ImportFolder);
                        tv.Text = result;
                        dialog.Show();
                        break;
                    }
                    else
                    {
                        if (files != null && files.Count > 0)
                        {
                            result = String.Format(Resources.GetString(Resource.String.importfiles), totalIncomeLength);
                        }
                        else
                        {
                            result = String.Format(Resources.GetString(Resource.String.importwarn),
                                FS.ImportedFolder, totalIncomeLength);
                        }
                        files.Clear();

                        dialog.Update(Resource.Layout.importdialog3);
                        tv = dialog.FindViewById<TextView>(Resource.Id.infotext2);
                        fileButton = dialog.FindViewById<LinearLayout>(Resource.Id.filebutton);
                        fileButton.Click += delegate
                        {
                            selectFile();
                        };
                        LinearLayout importButton = dialog.FindViewById<LinearLayout>(Resource.Id.importbutton);
                        importButton.Click += async delegate
                        {
                            await ImportTask(); // import selected coins
                        };
                        tv.Text = result;
                        dialog.Show();
                    }
                    break;
            }

        }

        public void ShowBankScreen()
        {
            IFileSystem FS = bank.fileUtils;
            int[] bankTotals = bank.countCoins(FS.BankFolder);
            int[] frackedTotals = bank.countCoins(FS.FrackedFolder);
            // int[] counterfeitTotals = bank.countCoins( counterfeitFolder );

            var bankCoins = FS.LoadFolderCoins(FS.BankFolder);


            onesCount = (from x in bankCoins
                         where x.denomination == 1
                         select x).Count();
            fivesCount = (from x in bankCoins
                          where x.denomination == 5
                          select x).Count();
            qtrCount = (from x in bankCoins
                        where x.denomination == 25
                        select x).Count();
            hundredsCount = (from x in bankCoins
                             where x.denomination == 100
                             select x).Count();
            twoFiftiesCount = (from x in bankCoins
                               where x.denomination == 250
                               select x).Count();

            var frackedCoins = FS.LoadFolderCoins(FS.FrackedFolder);
            bankCoins.AddRange(frackedCoins);

            onesFrackedCount = (from x in frackedCoins
                                where x.denomination == 1
                                select x).Count();
            fivesFrackedCount = (from x in frackedCoins
                                 where x.denomination == 5
                                 select x).Count();
            qtrFrackedCount = (from x in frackedCoins
                               where x.denomination == 25
                               select x).Count();
            hundredsFrackedCount = (from x in frackedCoins
                                    where x.denomination == 100
                                    select x).Count();
            twoFrackedFiftiesCount = (from x in frackedCoins
                                      where x.denomination == 250
                                      select x).Count();

            onesTotalCount = onesCount + onesFrackedCount;
            fivesTotalCount = fivesCount + fivesFrackedCount;
            qtrTotalCount = qtrCount + qtrFrackedCount;
            hundredsTotalCount = hundredsCount + hundredsFrackedCount;
            twoFiftiesTotalCount = twoFiftiesCount + twoFrackedFiftiesCount;


            int totalAmount = onesTotalCount + (fivesTotalCount * 5) + (qtrTotalCount * 25) + (hundredsTotalCount * 100) + (twoFiftiesTotalCount * 250);

            dialog.Update(Resource.Layout.bankdialog);
            dialog.Init(CoinDialog.DialogType.Bank);

            TextView tcv = dialog.FindViewById<TextView>(Resource.Id.totalcoinstxt);
            tcv.Text = Resources.GetString(Resource.String.acc) + string.Format(" {0,8:N0}", totalAmount);

            TextView bs1 = dialog.FindViewById<TextView>(Resource.Id.bs1);
            bs1.Text = string.Format("{0,7}", onesCount);

            TextView bsf1 = dialog.FindViewById<TextView>(Resource.Id.bsf1);
            bsf1.Text = string.Format("{0,7}", onesFrackedCount);

            TextView bs5 = dialog.FindViewById<TextView>(Resource.Id.bs5);
            bs5.Text = string.Format("{0,7}", fivesCount);

            TextView bsf5 = dialog.FindViewById<TextView>(Resource.Id.bsf5);
            bsf5.Text = string.Format("{0,7}", fivesFrackedCount);

            TextView bs25 = dialog.FindViewById<TextView>(Resource.Id.bs25);
            bs25.Text = string.Format("{0,7}", qtrCount);

            TextView bsf25 = dialog.FindViewById<TextView>(Resource.Id.bsf25);
            bsf25.Text = string.Format("{0,7}", qtrFrackedCount);

            TextView bs100 = dialog.FindViewById<TextView>(Resource.Id.bs100);
            bs100.Text = string.Format("{0,7}", hundredsCount);

            TextView bsf100 = dialog.FindViewById<TextView>(Resource.Id.bsf100);
            bsf100.Text = string.Format("{0,7}", hundredsFrackedCount);

            TextView bs250 = dialog.FindViewById<TextView>(Resource.Id.bs250);
            bs250.Text = string.Format("{0,7}", twoFiftiesCount);

            TextView bsf250 = dialog.FindViewById<TextView>(Resource.Id.bsf250);
            bsf250.Text = string.Format("{0,7}", twoFrackedFiftiesCount);

            dialog.Show();
        }

        public void CalculateTotals()
        {
            IFileSystem FS = bank.fileUtils;

            var bankCoins = FS.LoadFolderCoins(FS.BankFolder);

            onesCount = (from x in bankCoins
                         where x.denomination == 1
                         select x).Count();
            fivesCount = (from x in bankCoins
                          where x.denomination == 5
                          select x).Count();
            qtrCount = (from x in bankCoins
                        where x.denomination == 25
                        select x).Count();
            hundredsCount = (from x in bankCoins
                             where x.denomination == 100
                             select x).Count();
            twoFiftiesCount = (from x in bankCoins
                               where x.denomination == 250
                               select x).Count();

            var frackedCoins = FS.LoadFolderCoins(FS.FrackedFolder);
            bankCoins.AddRange(frackedCoins);

            onesFrackedCount = (from x in frackedCoins
                                where x.denomination == 1
                                select x).Count();
            fivesFrackedCount = (from x in frackedCoins
                                 where x.denomination == 5
                                 select x).Count();
            qtrFrackedCount = (from x in frackedCoins
                               where x.denomination == 25
                               select x).Count();
            hundredsFrackedCount = (from x in frackedCoins
                                    where x.denomination == 100
                                    select x).Count();
            twoFrackedFiftiesCount = (from x in frackedCoins
                                      where x.denomination == 250
                                      select x).Count();

            onesTotalCount = onesCount + onesFrackedCount;
            fivesTotalCount = fivesCount + fivesFrackedCount;
            qtrTotalCount = qtrCount + qtrFrackedCount;
            hundredsTotalCount = hundredsCount + hundredsFrackedCount;
            twoFiftiesTotalCount = twoFiftiesCount + twoFrackedFiftiesCount;

        }

        public void ShowExportScreen()
        {
            List<string> exfilenames = new List<string>();

            int exp_1 = 0;
            int exp_5 = 0;
            int exp_25 = 0;
            int exp_100 = 0;
            int exp_250 = 0;

            IFileSystem FS = bank.fileUtils;
            FS.LoadFileSystem();
            CalculateTotals();

            dialog.Update(Resource.Layout.exportdialog);
            dialog.Init(CoinDialog.DialogType.Export);

            LinearLayout exportButton = dialog.FindViewById<LinearLayout>(Resource.Id.exportbutton);
            exportButton.Click += delegate
            {
                exp_1 = dialog.nps[0].Value;
                exp_5 = dialog.nps[1].Value;
                exp_25 = dialog.nps[2].Value;
                exp_100 = dialog.nps[3].Value;
                exp_250 = dialog.nps[4].Value;

                int totalSaved = exp_1 + (exp_5 * 5) + (exp_25 * 25) + (exp_100 * 100) + (exp_250 * 250);
                if (totalSaved == 0) return;
                List<CloudCoin> totalCoins = bank.fileUtils.bankCoins.ToList();
                totalCoins.AddRange(bank.fileUtils.frackedCoins);


                var onesToExport = (from x in totalCoins
                                    where x.denomination == 1
                                    select x).Take(exp_1);
                var fivesToExport = (from x in totalCoins
                                     where x.denomination == 5
                                     select x).Take(exp_5);
                var qtrToExport = (from x in totalCoins
                                   where x.denomination == 25
                                   select x).Take(exp_25);
                var hundredsToExport = (from x in totalCoins
                                        where x.denomination == 100
                                        select x).Take(exp_100);
                var twoFiftiesToExport = (from x in totalCoins
                                          where x.denomination == 250
                                          select x).Take(exp_250);
                List<CloudCoin> exportCoins = onesToExport.ToList();
                exportCoins.AddRange(fivesToExport);
                exportCoins.AddRange(qtrToExport);
                exportCoins.AddRange(hundredsToExport);
                exportCoins.AddRange(twoFiftiesToExport);

                RadioGroup radioGroup = FindViewById<RadioGroup>(Resource.Id.radioGroup);
                RadioButton radioButton = FindViewById<RadioButton>(radioGroup.CheckedRadioButtonId);
                exfilenames.Clear();
                String filename;
                switch (radioButton.Id)
                {
                    case Resource.Id.rjpg:
                        filename = (FS.ExportFolder + System.IO.Path.DirectorySeparatorChar + totalSaved + ".CloudCoins." + dialog.et.Text + "");
                        if (File.Exists(filename))
                        {
                            // tack on a random number if a file already exists with the same tag
                            Random rnd = new Random();
                            int tagrand = rnd.Next(999);
                            filename = (FS.ExportFolder + System.IO.Path.DirectorySeparatorChar + totalSaved + ".CloudCoins." + dialog.et.Text + tagrand + "");
                        }//end if file exists

                        foreach (var coin in exportCoins)
                        {
                            string OutputFile = FS.ExportFolder + coin.FileName + dialog.et.Text + ".jpg";
                            bool fileGenerated = FS.WriteCoinToJpeg(coin, FS.GetCoinTemplate(coin), OutputFile, "");
                            if (fileGenerated)
                            {
                                Console.WriteLine("CloudCoin exported as Jpeg to " + OutputFile);
                            }
                            exfilenames.Add(FS.ExportFolder + OutputFile);
                        }

                        FS.RemoveCoins(exportCoins, FS.BankFolder);
                        FS.RemoveCoins(exportCoins, FS.FrackedFolder);
                        break;

                    case Resource.Id.rjson:
                        foreach (var coin in exportCoins)
                        {
                            string OutputFile = FS.ExportFolder + coin.FileName + dialog.et.Text + ".stack";
                            FS.WriteCoinToFile(coin, OutputFile);

                            FS.RemoveCoins(exportCoins, FS.BankFolder);
                            FS.RemoveCoins(exportCoins, FS.FrackedFolder);
                            Console.WriteLine("CloudCoin exported as Stack to " + OutputFile);
                            exfilenames.Add(OutputFile);
                        }
                        break;

                    case Resource.Id.rcsv:
                        filename = (FS.ExportFolder + System.IO.Path.DirectorySeparatorChar + totalSaved + ".CloudCoins." + dialog.et.Text + ".csv");
                        if (File.Exists(filename))
                        {
                            // tack on a random number if a file already exists with the same tag
                            Random rnd = new Random();
                            int tagrand = rnd.Next(999);
                            filename = (FS.ExportFolder + System.IO.Path.DirectorySeparatorChar + totalSaved + ".CloudCoins." + dialog.et.Text + tagrand + "");


                        }//end if file exists

                        var csv = new StringBuilder();
                        var coins = exportCoins;

                        var headerLine = string.Format("sn,denomination,nn,");
                        string headeranstring = "";
                        for (int i = 0; i < CloudCoinCore.Config.NodeCount; i++)
                        {
                            headeranstring += "an" + (i + 1) + ",";
                        }

                        // Write the Header Record
                        csv.AppendLine(headerLine + headeranstring);

                        // Write the Coin Serial Numbers
                        foreach (var coin in coins)
                        {
                            string anstring = "";
                            for (int i = 0; i < CloudCoinCore.Config.NodeCount; i++)
                            {
                                anstring += coin.an[i] + ",";
                            }
                            var newLine = string.Format("{0},{1},{2},{3}", coin.sn, coin.denomination, coin.nn, anstring);
                            csv.AppendLine(newLine);

                        }
                        File.WriteAllText(filename, csv.ToString());
                        Console.WriteLine("Coins exported as csv to " + filename);
                        //FS.WriteCoinsToFile(exportCoins, filename, ".s");
                        FS.RemoveCoins(exportCoins, FS.BankFolder);
                        FS.RemoveCoins(exportCoins, FS.FrackedFolder);
                        exfilenames.Add(filename);
                        break;
                }

                dialog.Update(Resource.Layout.exportdialog2);
                TextView infoText = dialog.FindViewById<TextView>(Resource.Id.infotext);
                infoText.Text = Resources.GetString(Resource.String.exportok);
                LinearLayout emailButton = dialog.FindViewById<LinearLayout>(Resource.Id.emailbutton);
                emailButton.Click += delegate
                {
                    // Send email
                    var email = new Intent(Intent.ActionSendMultiple);
                    email.SetType("text/plain");
                    email.PutExtra(Intent.ExtraEmail, new string[] { "" });
                    email.PutExtra(Intent.ExtraCc, new string[] { "" });
                    email.PutExtra(Intent.ExtraSubject, "Send CloudCoins");

                    var uris = new List<IParcelable>();
                    exfilenames.ForEach(file => {
                        var fileIn = new Java.IO.File(file);
                        var uri = Android.Net.Uri.FromFile(fileIn);
                        uris.Add(uri);
                    });

                    email.PutParcelableArrayListExtra(Intent.ExtraStream, uris);

                    StartActivity(Intent.CreateChooser(email, "Send mail..."));
                };
                dialog.Show();
            };

            dialog.Show();
        }


        public void OnClick(View v)
        {
            throw new NotImplementedException();
        }

        private void ReportProgress(ProgressReport progress)
        {
            importText.Text = progress.CurrentProgressMessage;
            if (progress.Stage == ImportStage.Echo)
            {
                importBar.Progress++;
            }
            else
            {
                importBar.Progress++;
                //importBar.Progress = (int)progress.CurrentProgressAmount;
            }
            importBar.Invalidate();
        }

        public async Task ProcessCoins(bool ChangeANs = true)
        {
            var networks = (from x in bank.fileUtils.importCoins
                            select x.nn).Distinct().ToList();

            foreach (var nn in networks)
            {
                Console.WriteLine("Starting Coins detection for Network " + nn);
                RAIDA.ActiveRAIDA = (from x in MainActivity.networks
                               where x.NetworkNumber == nn
                               select x).FirstOrDefault();
                var progressIndicator = new Progress<ProgressReport>(ReportProgress);
                await ProcessNetworkCoins(progressIndicator, nn, ChangeANs);
                Console.WriteLine("Coins detection for Network " + nn + "Finished.");
            }
        }

        public async Task ProcessNetworkCoins(IProgress<ProgressReport> progress, int NetworkNumber,  bool ChangeANS = true)
        {
            IFileSystem FS = bank.fileUtils;
            FS.LoadFileSystem();
            FS.DetectPreProcessing();

            var predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
            predetectCoins = (from x in predetectCoins
                              where x.nn == NetworkNumber
                              select x).ToList();

            //IFileSystem.predetectCoins = predetectCoins;

            RAIDA raida = (from x in networks
                           where x.NetworkNumber == NetworkNumber
                           select x).FirstOrDefault();
            if (raida == null)
                return;
            // Process Coins in Lots of 200. Can be changed from Config File
            int LotCount = predetectCoins.Count() / Config.MultiDetectLoad;
            if (predetectCoins.Count() % Config.MultiDetectLoad > 0) LotCount++;
            ProgressChangedEventArgs pge = new ProgressChangedEventArgs();

            int CoinCount = 0;
            int totalCoinCount = predetectCoins.Count();
            for (int i = 0; i < LotCount; i++)
            {
                //Pick up 200 Coins and send them to RAIDA
                var coins = predetectCoins.Skip(i * Config.MultiDetectLoad).Take(Config.MultiDetectLoad);
                try
                {
                    raida.coins = coins;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                var tasks = raida.GetMultiDetectTasks(progress, coins.ToArray(), Config.milliSecondsToTimeOut, ChangeANS);
                try
                {
                    string requestFileName = Utils.RandomString(16).ToLower() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    FS.WriteCoinsToFile(coins, FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task));
                    int j = 0;
                    foreach (var coin in coins)
                    {
                        coin.pown = "";
                        for (int k = 0; k < Config.NodeCount; k++)
                        {
                            coin.response[k] = raida.nodes[k].MultiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = countp;
                        coin.FailCount = countf;
                        CoinCount++;


                        Console.Out.WriteLine("No. " + CoinCount + ". Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult + "." + coin.pown);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount) * 100 / totalCoinCount;
                        Console.WriteLine("Minor Progress- " + pge.MinorProgress);
                        OnProgressChanged(pge);
                        coin.doPostProcessing();
                        j++;
                    }
                    pge.MinorProgress = (CoinCount - 1) * 100 / totalCoinCount;
                    Console.WriteLine("Minor Progress- " + pge.MinorProgress);
                    OnProgressChanged(pge);
                    FS.WriteCoin(coins, FS.DetectedFolder, false);
                    FS.RemoveCoins(coins, FS.PreDetectFolder);

                    Console.Out.WriteLine(pge.MinorProgress + " % of Coins on Network " + NetworkNumber + " processed.");
                    //FS.WriteCoin(coins, FS.DetectedFolder);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }
            pge.MinorProgress = 100;
            Console.WriteLine("Minor Progress- " + pge.MinorProgress);
            OnProgressChanged(pge);
            var detectedCoins = FS.LoadFolderCoins(FS.DetectedFolder);
            //detectedCoins.ForEach(x => x.pown= "ppppppppppppppppppppppppp");

            // Apply Sort to Folder to all detected coins at once.
            Console.Out.WriteLine("Starting Sort.....");
            //detectedCoins.ForEach(x => x.doPostProcessing());
            detectedCoins.ForEach(x => x.SortToFolder(FS));
            Console.Out.WriteLine("Ended Sort........");

            var passedCoins = (from x in detectedCoins
                               where x.folder == FS.BankFolder
                               select x).ToList();

            var frackedCoins = (from x in detectedCoins
                                where x.folder == FS.FrackedFolder
                                select x).ToList();

            var failedCoins = (from x in detectedCoins
                               where x.folder == FS.CounterfeitFolder
                               select x).ToList();
            var lostCoins = (from x in detectedCoins
                             where x.folder == FS.LostFolder
                             select x).ToList();
            var suspectCoins = (from x in detectedCoins
                                where x.folder == FS.SuspectFolder
                                select x).ToList();

            Console.WriteLine("Total Passed Coins - " + (passedCoins.Count() + frackedCoins.Count()));
            Console.WriteLine("Total Failed Coins - " + failedCoins.Count());
            Console.Out.WriteLine("Coin Detection finished.");
            Console.Out.WriteLine("Total Passed Coins - " + (passedCoins.Count() + frackedCoins.Count()) + "");
            Console.Out.WriteLine("Total Failed Coins - " + failedCoins.Count() + "");
            Console.Out.WriteLine("Total Lost Coins - " + lostCoins.Count() + "");
            Console.Out.WriteLine("Total Suspect Coins - " + suspectCoins.Count() + "");
            dialog.banked = passedCoins.Count;
            dialog.fracked = frackedCoins.Count;
            dialog.failed = failedCoins.Count;
            dialog.lost = lostCoins.Count;
            dialog.suspect = suspectCoins.Count;

            // Move Coins to their respective folders after sort
            FS.MoveCoins(passedCoins, FS.DetectedFolder, FS.BankFolder);
            FS.MoveCoins(frackedCoins, FS.DetectedFolder, FS.FrackedFolder);

            FS.WriteCoin(failedCoins, FS.CounterfeitFolder, false, true);
            FS.MoveCoins(lostCoins, FS.DetectedFolder, FS.LostFolder);
            FS.MoveCoins(suspectCoins, FS.DetectedFolder, FS.SuspectFolder);

            // Clean up Detected Folder
            FS.RemoveCoins(failedCoins, FS.DetectedFolder);
            FS.RemoveCoins(lostCoins, FS.DetectedFolder);
            FS.RemoveCoins(suspectCoins, FS.DetectedFolder);

            FS.MoveImportedFiles();

            //after = DateTime.Now;
            //ts = after.Subtract(before);

            //Debug.WriteLine("Detection Completed in - " + ts.TotalMilliseconds / 1000);
            //updateLog("Detection Completed in - " + ts.TotalMilliseconds / 1000);


            pge.MinorProgress = 100;
            Console.WriteLine("Minor Progress- " + pge.MinorProgress);
        }

        public virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

    }
}

