using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace minecraft_qr_generator
{

    public static class StopwatchEx
    {
        public static TimeSpan Context(Action f, int count = 1)
        {
            var sw = new Stopwatch();
            for (int i = 0; i < count; i++)
            {
                sw.Start();
                f();
                sw.Stop();
            }
            return sw.Elapsed;
            //return TimeSpan.FromTicks(sw.ElapsedTicks);
        }

        public static TimeSpan Context<TResult>(Func<TResult> f, int count = 1)
        {
            var sw = new Stopwatch();
            sw.Reset();
            for (int i = 0; i < count; i++)
            {
                sw.Start();
                TResult restul = f(); // 読み捨て
                sw.Stop();
            }
            return sw.Elapsed;
            //return TimeSpan.FromTicks(sw.ElapsedTicks);
        }
    }
    class Program
    {

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        private static String UserPath = Environment.GetEnvironmentVariable("USERPROFILE");


        private static String iniDir = UserPath + "\\mcqr" + "\\settings.ini";

        private static Stopwatch sw = new Stopwatch();

        private static String Output_mcfunc = readini(iniDir, "Data", "OutputPath");

        private static String BlockName = readini(iniDir, "Data", "BlockName");
        private static String BlockID = readini(iniDir, "Data", "BlockID");
        private static String BackgroundBlockName = readini(iniDir, "Data", "BackGroundBlockName");
        private static String BackgroundBlockID = readini(iniDir, "Data", "BackGroundBlockID");
        
        private static int BlockIDint = int.Parse(BlockID);
        private static int BackgroundBlockIDint = int.Parse(readini(iniDir, "Data", "BackGroundBlockID"));


        public static Image ImgCreator(Image img)
        {
            try
            {
                //グレースケールの描画先となるImageオブジェクトを作成
                Bitmap newImg = new Bitmap(img.Width, img.Height);
                //newImgのGraphicsオブジェクトを取得
                Graphics g = Graphics.FromImage(newImg);

                //ColorMatrixオブジェクトの作成
                //グレースケールに変換するための行列を指定する
                System.Drawing.Imaging.ColorMatrix cm =
                    new System.Drawing.Imaging.ColorMatrix(
                        new float[][]{
                new float[]{0.299f, 0.299f, 0.299f, 0 ,0},
                new float[]{0.587f, 0.587f, 0.587f, 0, 0},
                new float[]{0.114f, 0.114f, 0.114f, 0, 0},
                new float[]{0, 0, 0, 1, 0},
                new float[]{0, 0, 0, 0, 1}
                        });
                //ImageAttributesオブジェクトの作成
                System.Drawing.Imaging.ImageAttributes ia =
                    new System.Drawing.Imaging.ImageAttributes();
                //ColorMatrixを設定する
                ia.SetColorMatrix(cm);

                //ImageAttributesを使用してグレースケールを描画
                g.DrawImage(img,
                    new Rectangle(0, 0, img.Width, img.Height),
                    0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

                //リソースを解放する
                g.Dispose();

                return newImg;

            }
            catch
            {
                Console.WriteLine("ファイルを読み込めませんでした。");
                Console.WriteLine($"設定は間違っていませんか？\n設定ファイルは、{iniDir}にあります。");
                Environment.Exit(1);
            }

            return null;

        }


        public static Image AdjustContrast(Image img, float contrast)
        {
            try
            {
                //コントラストを変更した画像の描画先となるImageオブジェクトを作成
                Bitmap newImg = new Bitmap(img.Width, img.Height);
                //newImgのGraphicsオブジェクトを取得
                Graphics g = Graphics.FromImage(newImg);

                //ColorMatrixオブジェクトの作成
                float scale = (100f + contrast) / 100f;
                scale *= scale;
                float append = 0.5f * (1f - scale);
                System.Drawing.Imaging.ColorMatrix cm =
                    new System.Drawing.Imaging.ColorMatrix(
                        new float[][] {
                        new float[] {scale, 0, 0, 0, 0},
                        new float[] {0, scale, 0, 0, 0},
                        new float[] {0, 0, scale, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {append, append, append, 0, 1}

                });

                //ImageAttributesオブジェクトの作成
                System.Drawing.Imaging.ImageAttributes ia =
                    new System.Drawing.Imaging.ImageAttributes();
                //ColorMatrixを設定する
                ia.SetColorMatrix(cm);

                //ImageAttributesを使用して描画
                g.DrawImage(img,
                    new Rectangle(0, 0, img.Width, img.Height),
                    0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

                //リソースを解放する
                g.Dispose();

                return newImg;

            }
            catch
            {
                Console.WriteLine("ファイルを読み込めませんでした。");
                Console.WriteLine($"設定は間違っていませんか？\n設定ファイルは、{iniDir}にあります。");
                Environment.Exit(2);
            }

            return null;
        }

        static void GenCont(String filename, String Outputpath)
        {
            try
            {
                String Outputpath2 = GetPathWithoutExtension(Outputpath) + ".png";

                Bitmap bmp = new Bitmap(filename);

                bmp.Save(Outputpath, ImageFormat.Bmp);
                bmp.Dispose();

                //グレースケールにする画像
                Bitmap img = new Bitmap(Outputpath);
                //グレースケールに変換
                Image glayImg = ImgCreator(img);

                img.Dispose();
                //PictureBox1に表示
                Outputpath = Outputpath.Remove(Outputpath.Length - 4) + ".tmp";
                glayImg.Save(Outputpath, ImageFormat.Png);

                //コントラストを変更する画像
                Bitmap img_ = new Bitmap(Outputpath);
                //コントラストを50にした画像を作成する
                Image newImg = AdjustContrast(img_, 100);
                img.Dispose();
                img_.Dispose();
                newImg.Save(Outputpath2);
                newImg.Dispose();
                glayImg.Dispose();
            }
            catch
            {
                Console.WriteLine("ファイルを読み込めませんでした。");
                Console.WriteLine($"設定は間違っていませんか？\n設定ファイルは、{iniDir}にあります。");
                Environment.Exit(3);
            }

        }

        static void Gensource(Bitmap bitmap, StreamWriter writer, int startx, int starty, int startz, int imagew, int mode = 0)
        {
            try
            {
                if(BlockName.Length == 0)
                {
                    BlockName = "concrete";
                }

                if (BlockID.ToString().Length == 0)
                {
                    BlockIDint = 15;
                }

                if (BackgroundBlockName.ToString().Length == 0)
                {
                    BackgroundBlockName = "concrete";
                }

                if (BackgroundBlockID.ToString().Length == 0)
                {
                    BackgroundBlockIDint = 0;
                }


                int R;
                int G;
                int B;

                int w = bitmap.Width;
                int h = bitmap.Height;

                int x = 0;
                int z = 0;

                int counter = 0;

                float per;
                int MaxCount = w + h;

                for (z = 0; z < h; z++)
                {
                    for (x = 0; x < w; x++)
                    {
                        Color pixelColor = bitmap.GetPixel(x, z);

                        R = pixelColor.R;
                        G = pixelColor.G;
                        B = pixelColor.B;

                        if (R == 255 && G == 255 && B == 255)
                        {

                            if (mode == 0)
                            {
                                writer.WriteLine("setblock {0} {1} {2} {3}", startx + x, starty, startz + z,BackgroundBlockName);
                            }
                            else if (mode == 1)//縦
                            {
                                writer.WriteLine("setblock {0} {1} {2} {3}", startx + x, h - starty - z, startz,BackgroundBlockName);
                            }
                        }
                        else
                        {

                            if (mode == 0)
                            {
                                writer.WriteLine("setblock {0} {1} {2} {3} {4}", startx + x, starty, startz + z,BlockName, BlockIDint);
                            }
                            else if (mode == 1)//縦
                            {
                                writer.WriteLine("setblock {0} {1} {2} {3} {4}", startx + x, h - starty - z, startz,BlockName, BlockIDint);
                            }
                        }

                        per = (counter/MaxCount);
                        if (per < 101) {
                            Console.Write("mcfunctionを生成中...:" + per + "%\r");
                        }
                        counter++;
                    }
                }

                Console.WriteLine("\n");

            }
            catch
            {
                Console.WriteLine("ファイルを読み込めませんでした。");
                Console.WriteLine($"設定は間違っていませんか？\n設定ファイルは、{iniDir}にあります。");
                Environment.Exit(4);
            }

        }

        public static string GetPathWithoutExtension(string path)
        {
            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                return path;
            }
            return path.Replace(extension, string.Empty);
        }

        static String readini(String iniDir, String section, String key, String nostring = "ありません。")
        {
            try
            {

                // iniファイル名を決める（実行ファイルが置かれたフォルダと同じ場所）
                string iniFileName = iniDir;

                // iniファイルから文字列を取得
                StringBuilder sb = new StringBuilder(1024);
                GetPrivateProfileString(
                    section,      // セクション名
                    key,          // キー名    
                    nostring,   // 値が取得できなかった場合に返される初期値
                    sb,             // 格納先
                    Convert.ToUInt32(sb.Capacity), // 格納先のキャパ
                    iniFileName);   // iniファイル名

                return sb.ToString();
            }
            catch
            {
                Console.WriteLine("ファイルを読み込めませんでした。");
                Console.WriteLine($"設定は間違っていませんか？\n設定ファイルは、{iniDir}にあります。");
                Environment.Exit(5);
            }
            return null;
        }

        static void SetProgress(int Value, int Max)
        {
            Console.Write(Value + "/" + Max + " ");

            int w = Console.BufferWidth - Console.CursorLeft - 1;
            int p = ((Value * w) / Max);

            Console.Write("".PadRight(p, '-').PadRight(w, '*'));

            Console.CursorLeft = 0;
        }

        static void Submain()
        {
            try
            {
                Directory.CreateDirectory(UserPath+"/mcqr");

                if (File.Exists(iniDir) == false)
                {
                    File.WriteAllText(iniDir, "[Data]\nInputFilePath=\nOutputPath=\nx=\ny=\nz=\nmode=\nBlockName=\nBlockID=\nBackGroundBlockName=\nBackGroundBlockID=");
                }

                String sb = readini(iniDir, "Data", "OutputPath");

                String Input = readini(iniDir, "Data", "InputFilePath");
                String Output_mcfunc = readini(iniDir, "Data", "OutputPath");

                String Output = (GetPathWithoutExtension(Output_mcfunc)) + ".png";

                String Output_mcfunc_no_ex = GetPathWithoutExtension(Output_mcfunc);

                String Input_filename = Path.GetFileName(Input);
                String Output_filename = Path.GetFileName(Output_mcfunc);

                String Output_Path = (Output_mcfunc_no_ex + "\\" + GetPathWithoutExtension(Output_filename));
                String OutputFullPath = GetPathWithoutExtension(Output) + "\\" + Path.GetFileName(GetPathWithoutExtension(Output)) + ".mcfunction";

                int startx = int.Parse(readini(iniDir, "Data", "x"));
                int starty = int.Parse(readini(iniDir, "Data", "y"));
                int startz = int.Parse(readini(iniDir, "Data", "z"));

                int mode = int.Parse(readini(iniDir, "Data", "mode"));

                int edition = int.Parse(readini(iniDir, "Data", "edition"));
                
                int filecount = 0;

                String JE = "JE";
                String BE = "BE";

                String TATE = "縦";
                String YOKO = "横";


                Console.WriteLine("読み取る画像:" + Input);

                if (mode == 0)
                {
                    Console.Write("モード:" + TATE + " ");
                }
                else if (mode == 1)
                {
                    Console.Write("モード:" + YOKO + " ");
                }

                if (edition == 0)
                {
                    Console.Write("エディション:" + JE + " ");
                }
                else if (edition == 1)
                {
                    Console.Write("エディション:" + BE + " ");
                }

                Console.WriteLine($"位置:({startx},{starty},{startz})\n");

                Console.WriteLine("実行中...\n");
                
                String filename;

                Directory.CreateDirectory(Output_mcfunc_no_ex);

                Console.WriteLine($"画像を生成中...\n");

                sw = new Stopwatch();
                sw.Start();

                GenCont(Input, Output);

                Bitmap bitmap = new Bitmap(Output);

                Color color = bitmap.GetPixel(24, 24);

                using (System.Drawing.Image image = System.Drawing.Image.FromFile(Output))
                {
                    int imagew = image.Width; //横幅
                    int imageh = image.Height; //高さ

                    System.Text.Encoding enc = new System.Text.UTF8Encoding(false);

                    StreamWriter writer =
                        new StreamWriter(Output_mcfunc, false, enc);

                    Console.Write("mcfunctionを生成中...:\r");
                    Gensource(bitmap, writer, startx, starty, startz, imagew, mode);

                    writer.Close();
                    writer.Dispose();
                }

                sw.Stop();
                Console.WriteLine($"実行時間:{TimeSpan.FromTicks(sw.ElapsedTicks)}\n");

                sw = new Stopwatch();
                sw.Start();

                Console.WriteLine($"テキストファイルを分割中...\n");
                String cmdtask = "$i=1; cat " + @$"{Output_mcfunc}" + " -ReadCount 10000 | % " + "{ $_ | Out-File" + $" {Output_Path}_" + "0$i.mcfunction -Encoding UTF8" + ";$i++ }";

                //Console.WriteLine(Output_mcfunc_no_ex + "\\" + Input_filename);
                //Console.WriteLine(Output_Path);

                // パワーシェルコマンド固定値定義
                const string ps_command = @"Get-Process | Get-Process | Sort-Object PM -Descending | Out-File -FilePath ";

                // パワーシェルのGet-Processをダンプしたかったのでここで出力ファイル定義(可変)を実行させる。
                string option = ps_command + cmdtask;
                OpenWithArguments(option);  // メソッド呼び出し

                File.Delete(Output_mcfunc);
                File.Delete(Output_mcfunc_no_ex + ".tmp");

                File.WriteAllText(OutputFullPath, "");
                try
                {
                    string[] names = Directory.GetFiles(GetPathWithoutExtension(Output));
                    
                    foreach (string name in names)
                    {
                        filename = Path.GetFileName(name);

                        if(filename.Contains(GetPathWithoutExtension(Path.GetFileName(Output))) == true)
                        {
                            filecount++;   
                        }
                    }
                }
                catch
                {

                }

                filecount--;

                //Console.WriteLine("ファイルの数:" + filecount);

                for (int i = 0; i < filecount; i++)
                {
                    File.AppendAllText(OutputFullPath, $"function {GetPathWithoutExtension(Path.GetFileName(OutputFullPath))}" + "_" + i.ToString() + "\n");
                }

                sw.Stop();
                Console.WriteLine($"実行時間:{TimeSpan.FromTicks(sw.ElapsedTicks)}\n");
            }
            catch(Exception er)
            {
                Console.WriteLine(er.ToString());
                Console.WriteLine("ファイルを読み込めませんでした。");
                Console.WriteLine($"設定は間違っていませんか？\n設定ファイルは、{iniDir}にあります。");
                Environment.Exit(6);
            }
        }

        //PowerShellの実行メソッド（引数:PowerShellコマンド)
        static void OpenWithArguments(string options)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "PowerShell.exe";
            //PowerShellのWindowを立ち上げずに実行。
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.CreateNoWindow = true;
            //cmd.StartInfo.CreateNoWindow = true;
            // 引数optionsをShellのコマンドとして渡す。
            cmd.StartInfo.Arguments = options;
            cmd.Start();
            cmd.WaitForExit();
        }
        static void Main(string[] args)
        {
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            Submain();
            sw2.Stop();
            Console.WriteLine($"全体の実行時間:{TimeSpan.FromTicks(sw2.ElapsedTicks)}\n");
            Console.WriteLine(GetPathWithoutExtension(Output_mcfunc) + "にテキストファイルを出力しました。");
            Console.WriteLine($"設定ファイルは、{iniDir}にあります。");
        }
    }
}
