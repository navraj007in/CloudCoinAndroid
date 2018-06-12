using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using CloudCoinCore;
using CloudCoinCore.CoreClasses;
using Android.Util;
using System.Threading.Tasks;
using System;
using System.Linq;
using Android.Content;

namespace CloudCoin
{

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, View.IOnClickListener
    {
        private LinearLayout linearLayoutImport;
        private LinearLayout linearLayoutBank;
        private LinearLayout linearLayoutExport;
        public static readonly int PickImageId = 1000;

        FileSystem fileSystem;
        public int raidaReady = 0, raidaNotReady = 0;

        public static RAIDA raida = RAIDA.GetInstance();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            linearLayoutImport = FindViewById<LinearLayout>(Resource.Id.limport);
            linearLayoutImport.Click += delegate
            {
                Intent = new Intent();
                Intent.SetType("image/*");
                Intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(Intent, "Select Image"), PickImageId);

            };
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            Log.Info("Path", path);
            fileSystem = new FileSystem(path);
            bool result = fileSystem.CreateDirectories();
            Log.Info("result", "" + result);
            Task task = echoRaida();

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

        public void OnClick(View v)
        {

        }
        private void Init()
        {
            linearLayoutImport = FindViewById<LinearLayout>(Resource.Id.limport);
            linearLayoutBank = FindViewById<LinearLayout>(Resource.Id.lbank);
            linearLayoutExport = FindViewById<LinearLayout>(Resource.Id.lexport);
            linearLayoutImport.SetOnClickListener(this);
            linearLayoutBank.SetOnClickListener(this);
            linearLayoutExport.SetOnClickListener(this);

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;

            }


        }



            }
}

