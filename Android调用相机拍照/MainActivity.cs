using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using AndroidX.Core.App;
using AndroidX.Core.Content;


namespace Android调用相机拍照
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public const int TAKE_PHOTO = 1;//声明一个请求码，用于识别返回的结果
        private ImageView picture;
        private Android.Net.Uri imageUri;
        private Button CaptureButton;
        private String filePath = Android.OS.Environment.ExternalStorageDirectory + Java.IO.File.Separator + "output_image.jpg";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            //针对 Android.OS.Environment.ExternalStorageDirectory 过时问题
            //filePath = GetExternalFilesDir(Android.OS.Environment.DirectoryPictures) + File.Separator + "output_image.jpg";
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            CaptureButton = (Button)FindViewById(Resource.Id.take_photo);
            picture = (ImageView)FindViewById(Resource.Id.picture);
           
            CaptureButton.Click += (object sender, System.EventArgs e) =>
            {
                RequestPermission();
            };

            SetDefualtImage();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case TAKE_PHOTO:
                    if (resultCode == Result.Ok)
                    {
                        try
                        {

                            Bitmap bitmap = BitmapFactory.DecodeStream(ContentResolver.OpenInputStream(imageUri));
                            picture.SetImageBitmap(bitmap);
                            //将图片解析成Bitmap对象，并把它显现出来
                        }
                        catch (Java.IO.FileNotFoundException e)
                        {
                            e.PrintStackTrace();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        //设置保存拍照图片——>再次关闭app重新打开显示为上次拍照照片
        private void SetDefualtImage()
        {
           Java.IO.File outputImage = new Java.IO.File(filePath);
            if (!outputImage.Exists())
            {
                return;
            }
            picture.SetImageBitmap(Android.Graphics.BitmapFactory.DecodeFile(filePath));
        }


        //动态请求权限
        private void RequestPermission()
        {

            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
            {
                //请求权限
                ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.WriteExternalStorage, Android.Manifest.Permission.Camera }, 1);
               
            }
            //调用
            RequestCamera();
        }

        private void RequestCamera()
        {
            Java.IO.File outputImage = new Java.IO.File(filePath);
            /*
            创建一个File文件对象，用于存放摄像头拍下的图片，我们把这个图片命名为output_image.jpg
            并把它存放在应用关联缓存目录下，调用getExternalCacheDir()可以得到这个目录，为什么要
            用关联缓存目录呢？由于android6.0开始，读写sd卡列为了危险权限，使用的时候必须要有权限，
            应用关联目录则可以跳过这一步
             */
            try//判断图片是否存在，存在则删除在创建，不存在则直接创建
            {
                if (!outputImage.ParentFile.Exists())
                {
                    outputImage.ParentFile.Mkdirs();
                }
                if (outputImage.Exists())
                {
                    outputImage.Delete();
                }

                outputImage.CreateNewFile();

                if (Build.VERSION.SdkInt >= (BuildVersionCodes)24)
                {
                    imageUri = FileProvider.GetUriForFile(this,
                            "com.example.mydemo.fileprovider", outputImage);
                }
                else
                {
                    imageUri = Android.Net.Uri.FromFile(outputImage);
                }

                //使用隐示的Intent，系统会找到与它对应的活动，即调用摄像头，并把它存储
                Intent intent = new Intent("android.media.action.IMAGE_CAPTURE");
                intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, imageUri);
                StartActivityForResult(intent, TAKE_PHOTO);
                //调用会返回结果的开启方式，返回成功的话，则把它显示出来
            }
            catch (Java.IO.IOException e)
            {
                e.PrintStackTrace();
            }

        }


    }
}
