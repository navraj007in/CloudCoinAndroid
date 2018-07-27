using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Content;
using Android.Util;
using Android.Preferences;
using Android.Content.PM;
using Android.Net;

using Android.Graphics.Drawables;
using Android.Graphics;

using Environment = System.Environment;
using Uri = Android.Net.Uri;
using Android.Database;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android;

namespace CloudCoin
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

        public bool isImportDialog;
        public TextView subTv;

        public TextView[][] ids;
        public int[][] stats;
        public int size;
        public int lastProgress;

        public NumberPicker[] nps;
        public TextView[] tvs;

        public EditText et;
        public TextView tvTotal, exportTv;

        public Button button, emailButton;

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

            size = Banker.denominations.Length;
            nps = new NumberPicker[size];
            tvs = new TextView[size];

            dlgType = dialogtype;

            switch (dialogtype)
            {
                case DialogType.Import:
                    break;
                case DialogType.Export:
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

                    bankCoins = owner.bank.countCoins(owner.bank.fileUtils.BankFolder);
                    frackedCoins = owner.bank.countCoins(owner.bank.fileUtils.FrackedFolder);

                    int overall = 0;
                    for (i = 0; i < size; i++)
                    {
                        lTotal = bankCoins[i + 1] + frackedCoins[i + 1];

                        nps[i].MinValue = 0;
                        nps[i].MaxValue = lTotal;
                        nps[i].Value = 0;
                        nps[i].ValueChanged += delegate
                        {
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

                case DialogType.Bank:
                    size = Banker.denominations.Length + 1;
                    ids = new TextView[3][];
                    stats = new int[3][];

                    allocId(IDX_BANK, "bs");
                    allocId(IDX_COUNTERFEIT, "cs");
                    allocId(IDX_FRACTURED, "fs");

                    stats[IDX_BANK] = owner.bank.countCoins(owner.bank.fileUtils.BankFolder);
                    stats[IDX_FRACTURED] = owner.bank.countCoins(owner.bank.fileUtils.FrackedFolder);

                    break;
            }

        }

        public int getTotal()
        {
            int total = 0;
            int j;
            int tval;


            switch(dlgType)
            {
                case DialogType.Export:
                   for (int i = 0; i < size; i++)
                    {
                        int denomination = Banker.denominations[i];
                        total += denomination * nps[i].Value;
                    }
                    break;

                case DialogType.Bank:
                    for (int i = 0; i < size; i++)
                    {
                        if (i == 0)
                        {
                            j = size - 1;
                            tval = 0;
                        }
                        else
                        {
                            j = i - 1;
                            tval = Banker.denominations[i - 1];
                        }

                        int authCount = stats[IDX_BANK][i] + stats[IDX_FRACTURED][i];

                        total += tval * authCount;

                        ids[IDX_BANK][j].Text = "" + authCount;
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

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
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


        public int raidaReady = 0, raidaNotReady = 0;
        private bool asyncFinished = true;
        private int lastProgress;
        private ProgressBar importBar;
        private TextView importText;
        private bool isImportSuspect = false;
        private bool isImportDialog;

        public event EventHandler ProgressChanged;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 100);

            Init();
        }


        public async static Task EchoTask()
        {
            Console.Out.WriteLine(String.Format("Starting Echo to RAIDA Network {0}\n", 1));
            Console.Out.WriteLine("----------------------------------\n");

            var echos = raida.GetEchoTasks();
            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));

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

            if(isImportSuspect)
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
                var detects = raida.GetMultiDetectTasks(bank.fileUtils.importCoins.ToArray(), 1000);
                await Task.WhenAll(detects.AsParallel().Select(async task => await task()));

                /*int i = 0;
                foreach(CloudCoin coin in bank.fileUtils.importCoins)
                {
                    List<Func<Task<Response>>> detects = raida.GetDetectTasks(coin);
                    await Task.WhenAll(detects.AsParallel().Select(async task => await task()));

                    // coin detect finished
                    if (detects.Any(t => t().Result.success))
                        bank.fileUtils.MoveCoins(new List<CloudCoin> { coin }, coin.folder, bank.fileUtils.BankFolder);


                    i++;
                    await Progress(i);

                }*/

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
                        if(path.Length > 1)
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

            switch (importState)
            {
                case ImportState.ImportIng:
                    dialog.Update(Resource.Layout.importraida);
                    importText = dialog.FindViewById<TextView>(Resource.Id.infotext);
                    importText.Text = getStatusString(lastProgress);

                    TextView subTv = dialog.FindViewById<TextView>(Resource.Id.infotextsub);

                    importBar = dialog.FindViewById<ProgressBar>(Resource.Id.firstBar);
                    importBar.Max = bank.fileUtils.importCoins.Count();
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

                    int toBankValue, toBank, failed;

                    toBankValue = bank.fileUtils.bankCoins.Count();
                    toBank = bank.fileUtils.detectedCoins.Count();
                    failed = bank.fileUtils.lostCoins.Count();

                    ttv = (TextView)dialog.FindViewById(Resource.Id.closebuttontext);
                    if (failed > 0 || toBank == 0)
                        ttv.SetText(Resource.String.back);
                    else
                        ttv.SetText(Resource.String.awesome);


                    ttv = dialog.FindViewById<TextView>(Resource.Id.imptotal);
                    ttv.Text = "" + toBankValue;

                    ttv = dialog.FindViewById<TextView>(Resource.Id.auth);
                    ttv.Text = "" + toBank;

                    ttv = dialog.FindViewById<TextView>(Resource.Id.failed);
                    ttv.Text = "" + failed;

                    try {
                        dialog.Show();
                    } catch (Exception) {
                        Log.Verbose("CLOUDCOIN", "Activity is gone. No result will be shown");
                    }
                    break;

                case ImportState.ImportInit:
                    if (bank.fileUtils.suspectCoins.Count() > 0)
                    {
                        dialog.Update(Resource.Layout.importsuspect);
                        LinearLayout goButton = dialog.FindViewById<LinearLayout>(Resource.Id.gobutton);
                        goButton.Click += delegate
                        {
                            isImportSuspect = true;
                            ImportTask();
                        };
                        dialog.Show();
                        return;
                    }

                    if (files != null && files.Count > 0)
                    {
                        //bank.loadIncomeFromFiles(files);
                        foreach(string file in files)
                        {
                             IEnumerable<CloudCoin> coins = bank.fileUtils.LoadCoins(file);
                             //IEnumerable<CloudCoin> coins = bank.fileUtils.LoadCoins(file);
                             if (coins != null)
                             {
                                 ((List<CloudCoin>)(bank.fileUtils.importCoins)).AddRange(coins);
                                 bank.fileUtils.WriteCoin(bank.fileUtils.importCoins,
                                     bank.fileUtils.ImportFolder, ".stack");
                                 //bank.fileUtils.WriteCoinsToFile(coins, file);
                                 //bank.fileUtils.LoadFileSystem();
                             }
                        }
                    }
                    else
                    {
                        /*                      String savedImportDir = mSettings.GetString(APP_PREFERENCES_IMPORTDIR, "");

                                                if (savedImportDir == "") {
                                                    importDir = bank.getDefaultRelativeImportDirPath();
                                                    if (importDir == null) {
                                                        tv.SetText(Resource.String.errmnt);
                                                        dialog.Show();
                                                        return;
                                                    }
                                                } else {
                                                    importDir = savedImportDir;
                                                    bank.setImportDirPath(importDir);
                                                }*/


                        //bank.fileUtils.LoadFolderCoins(bank.fileUtils.ImportFolder);
                     }

                    dialog.Update(Resource.Layout.importdialog);
                    tv = dialog.FindViewById<TextView>(Resource.Id.infotext);
                    LinearLayout fileButton = dialog.FindViewById<LinearLayout>(Resource.Id.filebutton);
                    fileButton.Click += delegate
                    {
                        selectFile();
                    };

                    totalIncomeLength = bank.fileUtils.importCoins.Count();
                    if (totalIncomeLength == 0)
                    {
                        result = String.Format(Resources.GetString(Resource.String.erremptyimport), 
                            bank.fileUtils.ImportFolder);
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
                                bank.fileUtils.ImportedFolder, totalIncomeLength);            
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
                        importButton.Click += delegate
                        {
                            EchoTask().Wait(); // get echos from raida
                            if (raida.ReadyCount == 0)
                            {
                                dialog.Update(Resource.Layout.importdialog2);
                                dialog.Show();
                                return;
                            }

                            isImportSuspect = false;
                            ProcessCoins(true).Wait();
                            //task = ImportTask(); // import selected coins
                        };
                        tv.Text = result;
                        dialog.Show();
                    }
                    break;
            }

        }

        public void ShowBankScreen()
        {
            dialog.Update(Resource.Layout.bankdialog);
            dialog.Init(CoinDialog.DialogType.Bank);

            TextView tcv = dialog.FindViewById<TextView>(Resource.Id.totalcoinstxt);
            String msg = Resources.GetString(Resource.String.acc);
            msg += " " + dialog.getTotal();
            tcv.Text = msg;

            dialog.Show();
        }

        public void ShowExportScreen()
        {
            dialog.Update(Resource.Layout.exportdialog);
            dialog.Init(CoinDialog.DialogType.Export);

            LinearLayout exportButton = dialog.FindViewById<LinearLayout>(Resource.Id.exportbutton);
            exportButton.Click += delegate
            {
                    String exportTag;
                    int[] values;
                    int[] failed;
                    int totalFailed = 0;


 
        //DoExport();

        dialog.Update(Resource.Layout.exportdialog2);
                TextView infoText = dialog.FindViewById<TextView>(Resource.Id.infotext);
                //infoText.Text = msg;
                LinearLayout emailButton = dialog.FindViewById<LinearLayout>(Resource.Id.emailbutton);
                emailButton.Click += delegate
                {
                    //DoSendEmail();
                };
                dialog.Show();
            };

            dialog.Show();
        }


        public void OnClick(View v)
        {
            throw new NotImplementedException();
        }

        public async Task ProcessCoins(bool ChangeANs = true)
        {
            var networks = (from x in bank.fileUtils.importCoins
                            select x.nn).Distinct().ToList();

            foreach (var nn in networks)
            {
                Console.WriteLine("Starting Coins detection for Network " + nn);
                /*ActiveRAIDA = (from x in RAIDA.networks
                               where x.NetworkNumber == nn
                               select x).FirstOrDefault();*/
                await ProcessNetworkCoins(nn, ChangeANs);
                Console.WriteLine("Coins detection for Network " + nn + "Finished.");
            }
        }

        public async Task ProcessNetworkCoins(int NetworkNumber, bool ChangeANS = true)
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
                var tasks = raida.GetMultiDetectTasks(coins.ToArray(), Config.milliSecondsToTimeOut, ChangeANS);
                try
                {
                    string requestFileName = Utils.RandomString(16).ToLower() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    FS.WriteCoinsToFile(coins, FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
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
                        Console.WriteLine("Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult);
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
            detectedCoins.ForEach(x => x.SortToFolder());
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

