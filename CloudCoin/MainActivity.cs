﻿using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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
                    catch (Java.Lang.NoSuchFieldException e) {
                    }
                    catch (Java.Lang.IllegalAccessException e) { }
                    catch (Java.Lang.IllegalArgumentException e) { }
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
        private ISharedPreferences mSettings;
        public static readonly int PickImageId = 1000;
        private CoinDialog dialog = null;
        private ImportState importState;
        private List<String> files = new List<String>();

        public Banker bank;

        public int raidaReady = 0, raidaNotReady = 0;
        private bool asyncFinished = true;
        private int lastProgress;
        private bool isImportSuspect = false;
        private bool isImportDialog;

        public static RAIDA raida = RAIDA.GetInstance();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 100);

            Init();
        }


        public async static Task echoRaida()
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
            catch (PackageManager.NameNotFoundException e)
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
                        //file = getUriRealPath(this, uri);
                        file = uri.ToString();
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
                                file = getUriRealPath(this, uri);
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

        public void 
            ShowImportScreen()
        {
            int totalIncomeLength;
            String result;
            TextView ttv;
            TextView tv;

            switch (importState)
            {
                case ImportState.ImportIng:
                    dialog.Update(Resource.Layout.importraida);
                    tv = dialog.FindViewById<TextView>(Resource.Id.infotext);
                    tv.Text = getStatusString(lastProgress);

                    TextView subTv = dialog.FindViewById<TextView>(Resource.Id.infotextsub);

                    ProgressBar pb = dialog.FindViewById<ProgressBar>(Resource.Id.firstBar);
                    //pb.Max = RAIDA.TOTAL_RAIDA_COUNT;
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

                    toBankValue = 0;// bank.getImportStats(Bank.STAT_VALUE_MOVED_TO_BANK);
                    toBank = 0;// bank.getImportStats(Bank.STAT_AUTHENTIC);
                    failed = 0;// bank.getImportStats(Bank.STAT_FAILED);

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
                    } catch (Exception e) {
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
                            //isImportSuspect = true;
                            //iTask = new ImportTask();
                            //iTask.execute("suspect");
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


                        bank.fileUtils.LoadFolderCoins(bank.fileUtils.ImportFolder);
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


                        dialog.Update(Resource.Layout.importdialog3);
                        fileButton = dialog.FindViewById<LinearLayout>(Resource.Id.filebutton);
                        fileButton.Click += delegate
                        {
                            selectFile();
                        };
                        LinearLayout importButton = dialog.FindViewById<LinearLayout>(Resource.Id.importbutton);
                        importButton.Click += delegate
                        {
                            Task task = echoRaida();

                            //isImportSuspect = false;
                            //iTask = new ImportTask();
                            //iTask.execute("import");
                        };
                        tv = dialog.FindViewById<TextView>(Resource.Id.infotext2);
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
        private String getUriRealPath(Context ctx, Uri uri)
        {
            String ret = "";

            //ret = uri.Path;
            if (isAboveKitKat())
            {
                // Android OS above sdk version 19.
                ret = getUriRealPathAboveKitkat(ctx, uri);
            }
            else
            {
                // Android OS below sdk version 19
                ret = getImageRealPath(ContentResolver, uri, null);
            }

            return ret;
        }

        private String getUriRealPathAboveKitkat(Context ctx, Uri uri)
        {
            String ret = "";

            if (ctx != null && uri != null)
            {

                if (isContentUri(uri))
                {
                    if (isGooglePhotoDoc(uri.Authority))
                    {
                        ret = uri.LastPathSegment;
                    }
                    else
                    {
                        ret = getImageRealPath(ContentResolver, uri, null);
                    }
                }
                else if (isFileUri(uri))
                {
                    ret = uri.Path;
                }
                else if (isDocumentUri(ctx, uri))
                {

                    // Get uri related document id.
                    String documentId = DocumentsContract.GetDocumentId(uri);

                    // Get uri authority.
                    String uriAuthority = uri.Authority;

                    if (isMediaDoc(uriAuthority))
                    {
                        String[] idArr = documentId.Split(':');
                        if (idArr.Length == 2)
                        {
                            // First item is document type.
                            String docType = idArr[0];

                            // Second item is document real id.
                            String realDocId = idArr[1];

                            // Get content uri by document type.
                            Uri mediaContentUri = MediaStore.Images.Media.ExternalContentUri;
                            if ("image".Equals(docType))
                            {
                                mediaContentUri = MediaStore.Images.Media.ExternalContentUri;
                            }
                            else if ("video".Equals(docType))
                            {
                                mediaContentUri = MediaStore.Video.Media.ExternalContentUri;
                            }
                            else if ("audio".Equals(docType))
                            {
                                mediaContentUri = MediaStore.Audio.Media.ExternalContentUri;
                            }

                            // Get where clause with real document id.
                            String whereClause = MediaStore.Images.Media.InterfaceConsts.Id + " = " + realDocId;

                            ret = getImageRealPath(ContentResolver, mediaContentUri, whereClause);
                        }

                    }
                    else if (isDownloadDoc(uriAuthority))
                    {
                        // Build download uri.
                        Uri downloadUri = Uri.Parse("content://downloads/public_downloads");

                        // Append download document id at uri end.
                        Uri downloadUriAppendId = ContentUris.WithAppendedId(downloadUri, long.Parse(documentId));

                        ret = getImageRealPath(ContentResolver, downloadUriAppendId, null);

                    }
                    else if (isExternalStoreDoc(uriAuthority))
                    {
                        String[] idArr = documentId.Split(':');
                        if (idArr.Length == 2)
                        {
                            String type = idArr[0];
                            String realDocId = idArr[1];

                            if ("primary".Equals(type,StringComparison.OrdinalIgnoreCase))
                            {
                                ret = Android.OS.Environment.ExternalStorageDirectory + "/" + realDocId;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        /* Check whether current android os version is bigger than kitkat or not. */
        private bool isAboveKitKat()
        {
            bool ret = false;
            ret = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;
            return ret;
        }

        /* Check whether this uri represent a document or not. */
        private bool isDocumentUri(Context ctx, Uri uri)
        {
            bool ret = false;
            if (ctx != null && uri != null)
            {
                ret = DocumentsContract.IsDocumentUri(ctx, uri);
            }
            return ret;
        }

        /* Check whether this uri is a content uri or not.
        *  content uri like content://media/external/images/media/1302716
        *  */
        private bool isContentUri(Uri uri)
        {
            bool ret = false;
            if (uri != null)
            {
                String uriSchema = uri.Scheme;
                if ("content".Equals(uriSchema, StringComparison.OrdinalIgnoreCase))
                {
                    ret = true;
                }
            }
            return ret;
        }

        // Check whether this uri is a file uri or not.
     
        private bool isFileUri(Uri uri)
        {
            bool ret = false;
            if (uri != null)
            {
                String uriSchema = uri.Scheme;
                if ("file".Equals(uriSchema,StringComparison.OrdinalIgnoreCase))
                {
                    ret = true;
                }
            }
            return ret;
        }

        /* Check whether this document is provided by ExternalStorageProvider. */
        private bool isExternalStoreDoc(String uriAuthority)
        {
            bool ret = false;

            if ("com.android.externalstorage.documents".Equals(uriAuthority))
            {
                ret = true;
            }

            return ret;
        }

        /* Check whether this document is provided by DownloadsProvider. */
        private bool isDownloadDoc(String uriAuthority)
        {
            bool ret = false;

            if ("com.android.providers.downloads.documents".Equals(uriAuthority))
            {
                ret = true;
            }

            return ret;
        }

        /* Check whether this document is provided by MediaProvider. */
        private bool isMediaDoc(String uriAuthority)
        {
            bool ret = false;

            if ("com.android.providers.media.documents".Equals(uriAuthority))
            {
                ret = true;
            }

            return ret;
        }

        /* Check whether this document is provided by google photos. */
        private bool isGooglePhotoDoc(String uriAuthority)
        {
            bool ret = false;

            if ("com.google.android.apps.photos.content".Equals(uriAuthority))
            {
                ret = true;
            }

            return ret;
        }

        /* Return uri represented document file real local path.*/
        private String getImageRealPath(ContentResolver contentResolver, Uri uri, String whereClause)
        {
            String ret = "";

            // Query the uri with condition.
            ICursor cursor = contentResolver.Query(uri, null, whereClause, null, null);

            if (cursor != null)
            {
                bool moveToFirst = cursor.MoveToFirst();
                if (moveToFirst)
                {

                    // Get columns name by uri type.
                    String columnName = MediaStore.Images.Media.InterfaceConsts.Data;

                    if (uri == MediaStore.Images.Media.ExternalContentUri)
                    {
                        columnName = MediaStore.Images.Media.InterfaceConsts.Data;
                    }
                    else if (uri == MediaStore.Audio.Media.ExternalContentUri)
                    {
                        columnName = MediaStore.Audio.Media.InterfaceConsts.Data;
                    }
                    else if (uri == MediaStore.Video.Media.ExternalContentUri)
                    {
                        columnName = MediaStore.Video.Media.InterfaceConsts.Data;
                    }

                    // Get column index.
                    int imageColumnIndex = cursor.GetColumnIndex(columnName);

                    // Get column value which is the uri related file local path.
                    if (imageColumnIndex < 0) imageColumnIndex = 0;
                    ret = cursor.GetString(imageColumnIndex);
                }
            }

            return ret;
        }
    }
}
