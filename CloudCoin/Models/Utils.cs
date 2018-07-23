using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;

using Android.Content;
using Android.Database;
using Android.Net;
using Android.OS;
using Android.Provider;

using Uri = Android.Net.Uri;
using Environment = Android.OS.Environment;

namespace CloudCoin
{
    public class Utils
    {
        public static CloudCoin[] LoadJson(string filename)
        {
            try
            {
                using (StreamReader r = File.OpenText(filename))
                {
                    string json = r.ReadToEnd();
                    Stack coins = JsonConvert.DeserializeObject<Stack>(json);
                    return coins.cc;
                }
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static String importJSON(String jsonfile)
        {
            String jsonData = "";
            String line;

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.

                using (var sr = File.OpenText(jsonfile))
                {
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while (true)
                    {
                        line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }//End if line is null
                        jsonData = (jsonData + line + "\n");
                    }//end while true
                }//end using
            }
            catch (Exception)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file " + jsonfile + " could not be read:");
            }
            return jsonData;
        }//end importJSON

        public static StringBuilder CoinsToCSV(IEnumerable<CloudCoin> coins)
        {
            var csv = new StringBuilder();


            var headerLine = string.Format("sn,denomination,nn,");
            string headeranstring = "";
            for (int i = 0; i < Config.NodeCount; i++)
            {
                headeranstring += "an" + (i + 1) + ",";
            }

            // Write the Header Record
            csv.AppendLine(headerLine + headeranstring);

            // Write the Coin Serial Numbers
            foreach (var coin in coins)
            {
                csv.AppendLine(coin.GetCSV());
            }
            return csv;
        }
        public static string WriteObjectToString()
        {
            MemoryStream ms = new MemoryStream();

            // Serializer the User object to the stream.  
            return "";
        }
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /**
        * Method ordinalIndexOf used to parse cloudcoins. Finds the nth number of a character within a string
        *
        * @param str The string to search in
        * @param substr What to count in the string
        * @param n The nth number
        * @return The index of the nth number
        */
        public static int ordinalIndexOf(string str, string substr, int n)
        {
            int pos = str.IndexOf(substr);
            while (--n > 0 && pos != -1)
            {
                pos = str.IndexOf(substr, (pos + 1));
            }
            return pos;
        }//end ordinal Index of


        public static async Task<String> GetHtmlFromURL(String urlAddress)
        {
            
            string data = "";
            try
            {
                using (var cli = new HttpClient())
                {
                    HttpResponseMessage response = await cli.GetAsync(urlAddress);
                    if (response.IsSuccessStatusCode)
                        data = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(data);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return data;
        }//end get HTML

    }

    public class FilePath
    {

        /**
         * Method for return file path of Gallery image/ Document / Video / Audio
         *
         * @param context
         * @param uri
         * @return path of the selected image file from gallery
         */
        public static string GetPath(Context context, Uri uri)
        {

            String realPath;
            // SDK < API11
            if (Build.VERSION.SdkInt < BuildVersionCodes.Honeycomb)
            {
                realPath = GetRealPathFromURI_BelowAPI11(context, uri);
            }
            // SDK >= 11 && SDK < 19
            else if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
            {
                realPath = GetRealPathFromURI_API11to18(context, uri);
            }
            // SDK > 19 (Android 4.4) and up
            else
            {
                realPath = GetRealPathFromURI_API19(context, uri);
            }
            return realPath;
        }


        //@SuppressLint("NewApi")
        public static String GetRealPathFromURI_API11to18(Context context, Uri contentUri)
            {
                String[] proj = { MediaStore.Images.Media.InterfaceConsts.Data };
                String result = null;

                CursorLoader cursorLoader = new CursorLoader(context, contentUri, proj, null, null, null);
                ICursor cursor = (ICursor)cursorLoader.LoadInBackground();

                if (cursor != null)
                {
                    int column_index = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                    cursor.MoveToFirst();
                    result = cursor.GetString(column_index);
                    cursor.Close();
                }
                return result;
            }

        public static String GetRealPathFromURI_BelowAPI11(Context context, Uri contentUri)
        {
            String[] proj = { MediaStore.Images.Media.InterfaceConsts.Data };
            ICursor cursor = context.ContentResolver.Query(contentUri, proj, null, null, null);
            int column_index = 0;
            String result = "";
            if (cursor != null)
            {
                column_index = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                result = cursor.GetString(column_index);
                cursor.Close();
                return result;
            }
            return result;
        }

        /**
         * Get a file path from a Uri. This will get the the path for Storage Access
         * Framework Documents, as well as the _data field for the MediaStore and
         * other file-based ContentProviders.
         *
         * @param context The context.
         * @param uri     The Uri to query.
         * @author paulburke
         */
       // @SuppressLint("NewApi")
        public static string GetRealPathFromURI_API19(Context context, Uri uri)
        {

            bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            // DocumentProvider
            if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
            {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    string type = split[0];

                    if ("primary".Equals(type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Environment.ExternalStorageDirectory + "/" + split[1];
                    }

                    // TODO handle non-primary volumes
                }
                // DownloadsProvider
                else if (isDownloadsDocument(uri))
                {

                    string id = DocumentsContract.GetDocumentId(uri);
                    Uri contentUri;
                    try
                    {
                        long iid = long.Parse(id);
                        contentUri = ContentUris.WithAppendedId(
                                Uri.Parse("content://downloads/public_downloads"), iid);

                    }
                    catch
                    {
                        return id;
                    }

                    return getDataColumn(context, contentUri, null, null);
                }
                // MediaProvider
                else if (isMediaDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(':');
                    string type = split[0];

                    Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    string selection = "_id=?";
                    string[] selectionArgs = new string[]{ split[1] };

                    return getDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase))
            {

                // Return the remote address
                if (isGooglePhotosUri(uri)) return uri.LastPathSegment;

                return getDataColumn(context, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        /**
         * Get the value of the data column for this Uri. This is useful for
         * MediaStore Uris, and other file-based ContentProviders.
         *
         * @param context       The context.
         * @param uri           The Uri to query.
         * @param selection     (Optional) Filter used in the query.
         * @param selectionArgs (Optional) Selection arguments used in the query.
         * @return The value of the _data column, which is typically a file path.
         */
        public static string getDataColumn(Context context, Uri uri, string selection,
                                           string[] selectionArgs)
        {

            ICursor cursor = null;
            string column = "_data";
            string[] projection = { column };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs,
                        null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null) cursor.Close();
            }
            return null;
        }


        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is ExternalStorageProvider.
         */
        public static bool isExternalStorageDocument(Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is DownloadsProvider.
         */
        public static bool isDownloadsDocument(Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is MediaProvider.
         */
        public static bool isMediaDocument(Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is Google Photos.
         */
        public static bool isGooglePhotosUri(Uri uri)
        {
            return "com.google.android.apps.photos.content".Equals(uri.Authority);
        }

    }
}
