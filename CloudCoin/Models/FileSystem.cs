﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SkiaSharp;

namespace CloudCoinCore
{
    public class FileSystem : IFileSystem
    {
        public IEnumerable<CloudCoin> importCoins;
        public IEnumerable<CloudCoin> exportCoins;
        public IEnumerable<CloudCoin> importedCoins;
        public IEnumerable<FileInfo> templateFiles;
        public IEnumerable<CloudCoin> languageCoins;
        public IEnumerable<CloudCoin> counterfeitCoins;
        public IEnumerable<CloudCoin> partialCoins;
        public IEnumerable<CloudCoin> frackedCoins;
        public IEnumerable<CloudCoin> detectedCoins;
        public IEnumerable<CloudCoin> suspectCoins;
        public IEnumerable<CloudCoin> trashCoins;
        public IEnumerable<CloudCoin> bankCoins;
        public IEnumerable<CloudCoin> lostCoins;
        public IEnumerable<CloudCoin> predetectCoins;
        public IEnumerable<CloudCoin> dangerousCoins;


        public FileSystem(string RootPath)
        {
            this.RootPath = RootPath;
            ImportFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_IMPORT + Path.DirectorySeparatorChar;
            ExportFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_EXPORT + Path.DirectorySeparatorChar;
            ImportedFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_IMPORTED + Path.DirectorySeparatorChar;
            TemplateFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_TEMPLATES + Path.DirectorySeparatorChar;
            LanguageFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_LANGUAGE + Path.DirectorySeparatorChar;
            CounterfeitFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_COUNTERFEIT + Path.DirectorySeparatorChar;
            PartialFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_PARTIAL + Path.DirectorySeparatorChar;
            FrackedFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_FRACKED + Path.DirectorySeparatorChar;
            DetectedFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_DETECTED + Path.DirectorySeparatorChar;
            SuspectFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_SUSPECT + Path.DirectorySeparatorChar;
            TrashFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_TRASH + Path.DirectorySeparatorChar;
            BankFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_BANK + Path.DirectorySeparatorChar;
            PreDetectFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_PREDETECT + Path.DirectorySeparatorChar;
            LostFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_LOST + Path.DirectorySeparatorChar;
            RequestsFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_REQUESTS + Path.DirectorySeparatorChar;
            DangerousFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_DANGEROUS + Path.DirectorySeparatorChar;
            LogsFolder = RootPath + Path.DirectorySeparatorChar + Config.TAG_LOGS + Path.DirectorySeparatorChar;
            /*var documents =
             Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var directoryname = Path.Combine(documents, "NewDirectory");
            Directory.CreateDirectory(directoryname);*/
        }
        public override bool CreateFolderStructure()
        {

            // Create the Actual Folder Structure
            return CreateDirectories();
            //return true;
        }

        public void CopyTemplates()
        {
            string[] fileNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (String fileName in fileNames)
            {
                if (fileName.Contains("jpeg") || fileName.Contains("jpg"))
                {

                }
            }
        }

        public bool CreateDirectories()
        {
            // Create Subdirectories as per the RootFolder Location
            // Failure will return false

            try
            {
                Directory.CreateDirectory(RootPath);
                Directory.CreateDirectory(ImportFolder);
                Directory.CreateDirectory(ExportFolder);
                Directory.CreateDirectory(BankFolder);
                Directory.CreateDirectory(ImportedFolder);
                Directory.CreateDirectory(LostFolder);
                Directory.CreateDirectory(TrashFolder);
                Directory.CreateDirectory(SuspectFolder);
                Directory.CreateDirectory(DetectedFolder);
                Directory.CreateDirectory(FrackedFolder);
                Directory.CreateDirectory(TemplateFolder);
                Directory.CreateDirectory(PartialFolder);
                Directory.CreateDirectory(CounterfeitFolder);
                Directory.CreateDirectory(LanguageFolder);
                Directory.CreateDirectory(PreDetectFolder);
                Directory.CreateDirectory(RequestsFolder);
                Directory.CreateDirectory(DangerousFolder);
                Directory.CreateDirectory(LogsFolder);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }


            return true;
        }



        public override void LoadFileSystem()
        {
            importCoins = LoadFolderCoins(ImportFolder);
            //exportCoins = LoadFolderCoins(ExportFolder);
            bankCoins = LoadFolderCoins(BankFolder);
            lostCoins = LoadFolderCoins(LostFolder);
            //importedCoins = LoadFolderCoins(ImportedFolder);
            //trashCoins = LoadFolderCoins(TrashFolder);
            suspectCoins = LoadFolderCoins(SuspectFolder);
            detectedCoins = LoadFolderCoins(DetectedFolder);
            frackedCoins = LoadFolderCoins(FrackedFolder);
            //LoadFolderCoins(TemplateFolder);
            partialCoins = LoadFolderCoins(PartialFolder);
            //counterfeitCoins = LoadFolderCoins(CounterfeitFolder);
            predetectCoins = LoadFolderCoins(PreDetectFolder);
            dangerousCoins = LoadFolderCoins(DangerousFolder);

        }


        public override void DetectPreProcessing()
        {
            foreach (var coin in importCoins)
            {
                string fileName = getCelebriumName(coin.FileName);
                int coinExists = (from x in predetectCoins
                                  where x.sn == coin.sn
                                  select x).Count();
                //if (coinExists > 0)
                //{
                //    string suffix = Utils.RandomString(16);
                //    fileName += suffix.ToLower();
                //}
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                Stack stack = new Stack(coin);
                using (StreamWriter sw = new StreamWriter(PreDetectFolder + fileName + ".stack"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, stack);
                }
            }
        }

        public override void ProcessCoins(IEnumerable<CloudCoin> coins)
        {

            var detectedCoins = LoadFolderCoins(DetectedFolder);


            foreach (var coin in detectedCoins)
            {
                if (coin.PassCount >= Config.PassCount)
                {
                    WriteCoin(coin, BankFolder);
                }
                else
                {
                    WriteCoin(coin, CounterfeitFolder);
                }
            }
        }

        public void WriteCoin(CloudCoin coin, string folder)
        {
            var folderCoins = LoadFolderCoins(folder);
            string fileName = getCelebriumName(coin.FileName);
            int coinExists = (from x in folderCoins
                              where x.sn == coin.sn
                              select x).Count();
            if (coinExists > 0)
            {
                string suffix = Utils.RandomString(16);
                fileName += suffix.ToLower();
            }
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            Stack stack = new Stack(coin);
            using (StreamWriter sw = new StreamWriter(folder + Path.DirectorySeparatorChar + fileName + ".stack"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, stack);
            }
        }

        public void WriteCoin(CloudCoin coin, string folder,string extension)
        {
            var folderCoins = LoadFolderCoins(folder);
            string fileName = getCelebriumName(coin.FileName);
            int coinExists = (from x in folderCoins
                              where x.sn == coin.sn
                              select x).Count();
            if (coinExists > 0)
            {
                string suffix = Utils.RandomString(16);
                fileName += suffix.ToLower();
            }
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            Stack stack = new Stack(coin);
            using (StreamWriter sw = new StreamWriter(folder + Path.DirectorySeparatorChar + fileName + extension))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, stack);
            }
        }
        public string getCelebriumName(string CoinName)
        {
            return CoinName.Replace("CloudCoin", "Celebrium");
        }
        public void TransferCoins(IEnumerable<CloudCoin> coins, string sourceFolder, string targetFolder,string extension = ".stack")
        {
            var folderCoins = LoadFolderCoins(targetFolder);

            foreach (var coin in coins)
            {
                string fileName = getCelebriumName(coin.FileName);
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new JavaScriptDateTimeConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    Stack stack = new Stack(coin);
                    using (StreamWriter sw = new StreamWriter(targetFolder + fileName + extension))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, stack);
                    }
                    File.Delete(sourceFolder + getCelebriumName(coin.FileName) + extension);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }


            }
        }

        public void MoveCoins(IEnumerable<CloudCoin> coins, string sourceFolder, string targetFolder, bool replaceCoins = false)
        {
            var folderCoins = LoadFolderCoins(targetFolder);

            foreach (var coin in coins)
            {
                string fileName = getCelebriumName(coin.FileName);
                int coinExists = (from x in folderCoins
                                  where x.sn == coin.sn
                                  select x).Count();
                if (coinExists > 0 && !replaceCoins)
                {
                    string suffix = Utils.RandomString(16);
                    fileName += suffix.ToLower();
                }
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new JavaScriptDateTimeConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    Stack stack = new Stack(coin);
                    using (StreamWriter sw = new StreamWriter(targetFolder + fileName + ".stack"))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, stack);
                    }
                    File.Delete(sourceFolder + getCelebriumName(coin.FileName) + ".stack");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }


            }
        }

        public void MoveCoins(IEnumerable<CloudCoin> coins, string sourceFolder, string targetFolder,string extension, bool replaceCoins = false)
        {
            var folderCoins = LoadFolderCoins(targetFolder);

            foreach (var coin in coins)
            {
                string fileName = getCelebriumName(coin.FileName);
                int coinExists = (from x in folderCoins
                                  where x.sn == coin.sn
                                  select x).Count();
                if (coinExists > 0 && !replaceCoins)
                {
                    string suffix = Utils.RandomString(16);
                    fileName += suffix.ToLower();
                }
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new JavaScriptDateTimeConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    Stack stack = new Stack(coin);
                    using (StreamWriter sw = new StreamWriter(targetFolder + fileName + extension))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, stack);
                    }
                    File.Delete(sourceFolder + getCelebriumName(coin.FileName) + extension);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }


            }
        }

        public void RemoveCoins(IEnumerable<CloudCoin> coins, string folder)
        {

            foreach (var coin in coins)
            {
                File.Delete(folder + getCelebriumName(coin.FileName) + ".stack");

            }
        }

        public void RemoveCoins(IEnumerable<CloudCoin> coins, string folder,string extension)
        {

            foreach (var coin in coins)
            {
                File.Delete(folder + getCelebriumName(coin.FileName) + extension);

            }
        }

        public void WriteCoinsToFile(IEnumerable<CloudCoin> coins, string fileName,string extension=".stack")
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            Stack stack = new Stack(coins.ToArray());
            using (StreamWriter sw = new StreamWriter(fileName + extension))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, stack);
            }
        }

        public override bool WriteCoinToJpeg(CloudCoin cloudCoin, string TemplateFile, string OutputFile, string tag)
        {
            OutputFile = OutputFile.Replace("\\\\", "\\");
            bool fileSavedSuccessfully = true;

            /* BUILD THE CLOUDCOIN STRING */
            String cloudCoinStr = "01C34A46494600010101006000601D05"; //THUMBNAIL HEADER BYTES
            for (int i = 0; (i < 25); i++)
            {
                cloudCoinStr = cloudCoinStr + cloudCoin.an[i];
            } // end for each an

            //cloudCoinStr += "204f42455920474f4420262044454645415420545952414e545320";// Hex for " OBEY GOD & DEFEAT TYRANTS "
            //cloudCoinStr += "20466f756e6465727320372d352d3137";// Founders 7-5-17
            cloudCoinStr += "4c6976652046726565204f7220446965";// Live Free or Die
            cloudCoinStr += "00000000000000000000000000";//Set to unknown so program does not export user data
                                                         // for (int i =0; i < 25; i++) {
                                                         //     switch () { }//end switch pown char
                                                         // }//end for each pown
            cloudCoinStr += "00"; // HC: Has comments. 00 = No
            cloudCoin.CalcExpirationDate();
            cloudCoinStr += cloudCoin.edHex; // 01;//Expiration date Sep 2016 (one month after zero month)
            cloudCoinStr += "01";//  cc.nn;//network number
            String hexSN = cloudCoin.sn.ToString("X6");
            String fullHexSN = "";
            switch (hexSN.Length)
            {
                case 1: fullHexSN = ("00000" + hexSN); break;
                case 2: fullHexSN = ("0000" + hexSN); break;
                case 3: fullHexSN = ("000" + hexSN); break;
                case 4: fullHexSN = ("00" + hexSN); break;
                case 5: fullHexSN = ("0" + hexSN); break;
                case 6: fullHexSN = hexSN; break;
            }
            cloudCoinStr = (cloudCoinStr + fullHexSN);
            /* BYTES THAT WILL GO FROM 04 to 454 (Inclusive)*/
            byte[] ccArray = this.hexStringToByteArray(cloudCoinStr);


            /* READ JPEG TEMPLATE*/
            byte[] jpegBytes = null;
            switch (cloudCoin.getDenomination())
            {
                case 1: jpegBytes = readAllBytes(this.TemplateFolder + "jpeg1.jpg"); break;
                case 5: jpegBytes = readAllBytes(this.TemplateFolder + "jpeg5.jpg"); break;
                case 25: jpegBytes = readAllBytes(this.TemplateFolder + "jpeg25.jpg"); break;
                case 100: jpegBytes = readAllBytes(this.TemplateFolder + "jpeg100.jpg"); break;
                case 250: jpegBytes = readAllBytes(this.TemplateFolder + "jpeg250.jpg"); break;
            }// end switch


            /* WRITE THE SERIAL NUMBER ON THE JPEG */

            //Bitmap bitmapimage;
            SKBitmap bitmapimage;
            //using (var ms = new MemoryStream(jpegBytes))
            {

                //bitmapimage = new Bitmap(ms);
                bitmapimage = SKBitmap.Decode(jpegBytes);
            }
            SKCanvas canvas = new SKCanvas(bitmapimage);
            //Graphics graphics = Graphics.FromImage(bitmapimage);
            //graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            SKPaint textPaint = new SKPaint()
            {
                IsAntialias = true,
                Color = SKColors.White,
                TextSize = 14,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };
            //PointF drawPointAddress = new PointF(30.0F, 25.0F);

            canvas.DrawText(String.Format("{0:N0}", cloudCoin.sn) + " of 16,777,216 on Network: 1", 30, 40, textPaint);
            //graphics.DrawString(String.Format("{0:N0}", cc.sn) + " of 16,777,216 on Network: 1", new Font("Arial", 10), Brushes.White, drawPointAddress);

            //ImageConverter converter = new ImageConverter();
            //byte[] snBytes = (byte[])converter.ConvertTo(bitmapimage, typeof(byte[]));
            SKImage image = SKImage.FromBitmap(bitmapimage);
            SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            byte[] snBytes = data.ToArray();

            List<byte> b1 = new List<byte>(snBytes);
            List<byte> b2 = new List<byte>(ccArray);
            b1.InsertRange(4, b2);

            if (tag == "random")
            {
                Random r = new Random();
                int rInt = r.Next(100000, 1000000); //for ints
                tag = rInt.ToString();
            }

            string fileName = ExportFolder + cloudCoin.FileName + tag + ".jpg";
            File.WriteAllBytes(fileName, b1.ToArray());
            Console.Out.WriteLine("Writing to " + fileName);
            //CoreLogger.Log("Writing to " + fileName);
            return fileSavedSuccessfully;
        }

        public void WriteCoin(IEnumerable<CloudCoin> coins, string folder,string extension, bool writeAll = false)
        {
            if (writeAll)
            {
                string fileName = Utils.RandomString(16) ;
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                Stack stack = new Stack(coins.ToArray());
                using (StreamWriter sw = new StreamWriter(folder + fileName +extension))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, stack);
                }
                return;
            }
            var folderCoins = LoadFolderCoins(folder);

            foreach (var coin in coins)
            {
                string fileName = getCelebriumName(coin.FileName);
                int coinExists = (from x in folderCoins
                                  where x.sn == coin.sn
                                  select x).Count();
                if (coinExists > 0)
                {
                    string suffix = Utils.RandomString(16);
                    fileName += suffix.ToLower();
                }
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                Stack stack = new Stack(coin);
                using (StreamWriter sw = new StreamWriter(folder + fileName + extension))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, stack);
                }

            }
        }

        public override void ClearCoins(string FolderName)
        {

            var fii = GetFiles(FolderName, Config.allowedExtensions);

            DirectoryInfo di = new DirectoryInfo(FolderName);


            foreach (FileInfo file in fii)
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

        }

        public bool WriteTextFile(string fileName,string text)
        {
            try
            {
                StreamWriter OurStream;
                OurStream = File.CreateText(fileName);
                OurStream.Write(text);
                OurStream.Close();
            }
            catch(Exception)
            {
               // MainWindow.logger.Error(e.Message);
                return false;
            }
            return true;
        }
        public List<FileInfo> GetFiles(string path, params string[] extensions)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (string ext in extensions)
                list.AddRange(new DirectoryInfo(path).GetFiles("*" + ext).Where(p =>
                      p.Extension.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
                      .ToArray());
            return list;
        }
        public override void MoveImportedFiles()
        {
            var files = Directory
              .GetFiles(ImportFolder)
              .Where(file => Config.allowedExtensions.Any(file.ToLower().EndsWith))
              .ToList();

            string[] fnames = new string[files.Count()];
            for (int i = 0; i < files.Count(); i++)
            {
                MoveFile(files[i], ImportedFolder + Path.DirectorySeparatorChar + Path.GetFileName(files[i]), FileMoveOptions.Rename);
            }
        }

    }


}
