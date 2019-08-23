using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UABT;
using bh3tool;
using System.Threading;
using System.Diagnostics;

namespace ModifyBH
{
    class Program
    {
        public static DateTime oldTime;
        public static bool firstUpdate_a = true;
        public static bool firstUpdate_i = true;
        Dictionary<string, Server_modinfo> mod_servers = new Dictionary<string, Server_modinfo>
            {
                { "android01", new Server_modinfo(Gameserver.android01) },
                { "bb01", new Server_modinfo(Gameserver.bb01) },
                { "hun01", new Server_modinfo(Gameserver.hun01) },
                { "hun02", new Server_modinfo(Gameserver.hun02) },
                { "ios01", new Server_modinfo(Gameserver.ios01) },
                { "yyb01", new Server_modinfo(Gameserver.yyb01) }
            };

        static void Main(string[] args)
        {
            Console.WriteLine("bh3tool is running");
            oldTime = DateTime.Now;
            
            Timer timer = new Timer(new TimerCallback((o) => {
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] It is time to update");
                ModFile(true, false);
                GC.Collect();
                ModFile(true, true);
                GC.Collect();
            }), null, 0, 900 * 1000);
            while (true)
            {
                string s = Console.ReadLine();
                if (s == "exit")
                {
                    return;
                }
                if (s == "update")
                {
                    ModFile(true, false);
                    GC.Collect();
                    ModFile(true, true);
                    GC.Collect();
                }
            }
        }

        public static void Update ()
        {
            

        }
        bool Getseverinfo(string se)
        {
            //用于匹配数据的正则
            Regex reg_serverurl = new Regex("(dispatch_url\":\")(.*?)\"");
            Regex reg_gameserver = new Regex("(gameserver\":)(\\{.*?\\})");
            Regex reg_geteway = new Regex("(gateway\":)(\\{.*?\\})");
            Regex reg_oaserver = new Regex("(oaserver_url\":)(\".*?\")");

            WebClient client = new WebClient();
            try
            {
                //note version number
                string version = "?version=3.3.0_" + GetserverStr(mod_servers[se].server);
                string globaleDispatchUrl = "http://global1.bh3.com/query_dispatch" + version;

                string rsp = client.DownloadString(globaleDispatchUrl);
                //get dispatch_url
                string dispatch_url = reg_serverurl.Match(rsp).Groups[2].Value;

                string qserverurl = dispatch_url + version;
                //get server rsponse
                string qrsp = client.DownloadString(qserverurl);
                //match server ip
                string gameserver = reg_gameserver.Match(qrsp).Groups[2].Value;
                string getway = reg_geteway.Match(qrsp).Groups[2].Value;
                string oa = reg_oaserver.Match(qrsp).Groups[2].Value;

                mod_servers[se].gameserver_url = dispatch_url;
                mod_servers[se].gameserver_rsp = qrsp;
                mod_servers[se].server_ip = gameserver;
                mod_servers[se].getway_ip = getway;
                mod_servers[se].oa_ip = oa;
                //Replace "https" with "http"
                mod_servers[se].mod_gameserver_rsp = qrsp.Replace("https://bundle.bh3.com", "http://bundle.bh3.com");
            }
            catch
            {
                return false;
            }
            return true;
        }
        string GetserverStr(Gameserver server)
        {
            switch (server)
            {
                case Gameserver.android01: return "gf_android";
                case Gameserver.bb01: return "bilibili";
                case Gameserver.hun01: return "oppo";
                case Gameserver.hun02: return "xiaomi";
                case Gameserver.ios01: return "gf_ios";
                case Gameserver.yyb01: return "tencent";
                default: return "gf_android";
            }

        }
        public static void ModFile(bool forceUpdate, bool isIOS)
        {
            if (firstUpdate_a)
            {
                forceUpdate = true;
                firstUpdate_a = false;
            } else
            {
                if (firstUpdate_i)
                {
                    forceUpdate = true;
                    firstUpdate_i = false;
                }
            }
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] Updating " + (isIOS ? "IOS" : "Android"));
            //file url
            string baseurl = isIOS ? "https://bundle.bh3.com/asset_bundle/ios01/1.0" : "https://bundle.bh3.com/asset_bundle/android01/1.0";
            string settingurl = isIOS ? "/data/iphone_compressed/data/setting_" : "/data/android_compressed/data/setting_";
            string excelurl = isIOS ? "/data/iphone_compressed/data/excel_output_" : "/data/android_compressed/data/excel_output_";
            string dataurl = isIOS ? "/data/iphone_compressed/DataVersion.unity3d" : "/data/android_compressed/DataVersion.unity3d";

            string RSAkey = @"<RSAKeyValue><Modulus>rXoKWm82JSX4UYihkt2FSjrp3pZqTxt6AyJ0ZexHssStYesCFuUOmDBrk0nxPTY2r7oB4ZC9tDhHzmA66Me56wkD47Z3fCEBfLFxmEVdUCvM1RIFdQxQCB7CMaFWXHoVfBhNcD60OtXD71vFusBLioa6HDHbKk8LdgWdV10OWaE=</Modulus><Exponent>EQ==</Exponent><P>16GiwrgCGvcYbgSZOBJRx4G9kioGgexLSyW62iK4EuT0Xu9xyflBDaC4yooFkxrflqEAIiEfTqNGlYeJks+5qw==</P><Q>zfQY4dWi/Dlo38y6xvX4pUEAj1hbeFo/Qiy7H00P089W0KC6Mdi+GY4UuRGJtgX7UZfGQdHRj8mBjijFyhUl4w==</Q><DP>cihlOejyDkaUdnrntEXvD0Svp7vlU9dzJ8iuNz+OoJdUMkKHiQt8yvq8Lv3Gt0p2Xs20xsY9wDhSi2Xfa9diSw==</DP><DQ>GDrVwDdAWeii7SclCFksT61LXCiDO1XpUxRSP+ryzZ/sGIthMwpwt7ZcynqIrAC0J7eAvHMJmHIPPeat24oEdQ==</DQ><InverseQ>P4/vgq1XF77N8K/OxTbcjWFCC1d+v3W5xWQJbmU3KfVF2wOStZeILT2X12s7AHD+uUfN9O/xdEBIeqcSLVxWjw==</InverseQ><D>o0WvZCxvMgWeatrybBvIvlWQ0X6CLFYYe2u42GXpILkbp3PFuzHvnkuwip/yG35RllS2efGjfHE0hgA3cazrNgM6gBDcFa7iznviIiQTySxFuzy3mXpjSQFaGgdvmuUQLgg5qahcdGgT455Fzo5GSu+IyTpD+dNoKy79NLTbvjE=</D></RSAKeyValue>";

            WebClient client = new WebClient();
            //Download DataVersion.unity3d.
            byte[] dataversion = client.DownloadData(baseurl + dataurl);
            string[] key = client.ResponseHeaders.AllKeys;
            //Check Last-Modified time.
            DateTime time = new DateTime(0);
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == "Last-Modified")
                {
                    time = DateTime.Parse(client.ResponseHeaders.Get(i));
                }
            }
            if (!forceUpdate)
            {
                bool a = DateTime.Compare(oldTime, time) >= 0;
                if (a)
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] No update");
                    return;
                }
            }

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] Start update");

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] edit dataversion.unity3d");

            //Decrypt DataVersion.unity3d
            for (int i = 0; i < dataversion.Length; i++) dataversion[i] = (byte)(dataversion[i] ^ 0xA5);
            //load bundle asset 
            BundleFile dataversionBundle = new BundleFile(dataversion);
            SerializedFile dataversionFile = new SerializedFile("dataversion", new EndianBinaryReader(dataversionBundle.fileList[0].stream));
            //get textasset bytes
            long Pid = dataversionFile.GetPathIDByName("packageversion.txt");
            var packageversion = dataversionFile.m_Objects.Find(x => x.m_PathID == Pid);
            //load textasset
            Textasset textasset = new Textasset(packageversion.data);

            string[] versiontext = textasset.text.Split('\n');
            //Get AES key IV and HMACSHA1 key
            var rsa = RSA.Create();
            rsa.FromXmlStringA(RSAkey);
            byte[] AES_SHA = rsa.Decrypt(Hex2bytes(versiontext[0]), RSAEncryptionPadding.Pkcs1);
            //AES_SHA 56 bytes, AES key 32 bytes|AES IV 16 bytes|HMACSHA1 8 bytes 
            byte[] AESkey = AES_SHA.Take(32).ToArray();
            byte[] AESIV = AES_SHA.Skip(32).Take(16).ToArray();
            byte[] SHA = AES_SHA.Skip(48).Take(8).ToArray();

            Regex regexCS = new Regex("(CS\":\")([1-9]\\d*)");
            Regex regexCRC = new Regex("(CRC\":\")([0-9A-Z]*)");

            string settingCRC;
            string excelCRC;
            int settingindex;
            int excelindex;
            //Get File CRC 
            if (versiontext[1].Contains("excel_output"))
            {
                excelindex = 1;
                settingindex = 2;
            }
            else
            {
                excelindex = 2;
                settingindex = 1;
            }
            excelCRC = regexCRC.Match(versiontext[excelindex]).Groups[2].Value;
            settingCRC = regexCRC.Match(versiontext[settingindex]).Groups[2].Value;


            //Download setting.unity3d and excel_output.unity3d
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] download setting");
            byte[] settingbytes = client.DownloadData(baseurl + settingurl + settingCRC);
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] download excel_output");
            byte[] excelbytes = client.DownloadData(baseurl + excelurl + excelCRC);
            client.Dispose();

            GC.Collect();

            //Decrypt File
            Aes Aes = Aes.Create();
            var AES_Encryptor = Aes.CreateEncryptor(AESkey, AESIV);

            var AES_Decryptor = Aes.CreateDecryptor(AESkey, AESIV);
            settingbytes = AES_Decryptor.TransformFinalBlock(settingbytes, 0, settingbytes.Length);

            AES_Decryptor = Aes.CreateDecryptor(AESkey, AESIV);
            excelbytes = AES_Decryptor.TransformFinalBlock(excelbytes, 0, excelbytes.Length);

            #region Modify setting.unity3d

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] edit setting");

            BundleFile settingBundle = new BundleFile(settingbytes);
            SerializedFile setting = new SerializedFile("setting", new EndianBinaryReader(settingBundle.fileList[0].stream));

            #region Edit miscdata.txt
            //Get miscdata.txt from bundle
            long miscid = setting.GetPathIDByName("miscdata.txt");
            var misc = setting.m_Objects.Find(x => x.m_PathID == miscid);
            Textasset miscasset = new Textasset(misc.data);
            //Enable All BodyPort Touch
            Regex regexMISC = new Regex("(Face|Chest|Private|Arm|Leg)(\" *\t*: )(false)");
            miscasset.text = regexMISC.Replace(miscasset.text, "$1$2true");
            misc.data = miscasset.GetBytes();
            #endregion

            #region Edit uiluadesign_x_x.lua.txt
            //version number
            long uiluaid = setting.GetPathIDByName("uiluadesign_3_3.lua.txt");
            var uilua = setting.m_Objects.Find(x => x.m_PathID == uiluaid);
            Textasset uiluaasset = new Textasset(uilua.data);
            //insert Lua code to function UITable.ModuleEndHandlePacket 
            List<string> list = new List<string>(uiluaasset.text.Split('\n'));
            int index = list.FindIndex(x => x.Contains("UITable.ModuleEndHandlePacket"));
            //Handle NetPacketV1 for GalTouchModule,and set GalTouchModule._canGalTouch to true
            string insertStr = "\tif moduleName == \"GalTouchModule\" and packet:getCmdId() == 111 then\n\t\tlocal gal = __singletonManagerType.GetSingletonInstance(\"MoleMole.GalTouchModule\")\n\t\tif gal == nil then\n\t\t\treturn\n\t\tend\n\t\tgal._canGalTouch = true\n\tend\n";
            list.Insert(index + 1, insertStr);
            uiluaasset.text = String.Join("\n", list);
            uilua.data = uiluaasset.GetBytes();
            #endregion

            #region Edit luahackconfig.txt
            //Add "GalTouchModule" to "UILuaPatchModuleList"
            long luahackpid = setting.GetPathIDByName("luahackconfig.txt");
            var luahack = setting.m_Objects.Find(x => x.m_PathID == luahackpid);
            Textasset hack = new Textasset(luahack.data);
            string inserthack = "		\"GalTouchModule\",";
            list = new List<string>(hack.text.Split('\n'));
            index = list.FindIndex(x => x.Contains("UILuaPatchModuleList"));
            list.Insert(index + 2, inserthack);
            hack.text = string.Join("\n", list);
            luahack.data = hack.GetBytes();
            #endregion

            //pack to assetbundle
            misc.data = miscasset.GetBytes();
            settingBundle.fileList[0].stream = new MemoryStream(setting.Pack());
            settingbytes = settingBundle.RePack();
            //ENCRYPT
            settingbytes = AES_Encryptor.TransformFinalBlock(settingbytes, 0, settingbytes.Length);
            #endregion

            GC.Collect();

            #region Edit excel_output.unity3d
            //
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] edit excel");

            BundleFile excelBundle = new BundleFile(excelbytes);
            SerializedFile excel = new SerializedFile("excel", new EndianBinaryReader(excelBundle.fileList[0].stream));
            //Edit touch buff
            long touchbuffid = excel.GetPathIDByName("touchbuffdata.asset");
            var bf = excel.m_Objects.Find(x => x.m_PathID == touchbuffid);
            bf.data = TouchBuff.GetNew(bf.data);
            excelBundle.fileList[0].stream = new MemoryStream(excel.Pack());
            excelbytes = excelBundle.RePack();
            //Encrypt excel_output.unity3d
            AES_Encryptor = Aes.CreateEncryptor(AESkey, AESIV);
            excelbytes = AES_Encryptor.TransformFinalBlock(excelbytes, 0, excelbytes.Length);
            #endregion

            GC.Collect();

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] edit dataversion");

            //compute new CRC(hmac-SHA1)
            HMACSHA1 hMACSHA1 = new HMACSHA1(SHA);
            settingCRC = BitConverter.ToString(hMACSHA1.ComputeHash(settingbytes)).Replace("-", "");
            excelCRC = BitConverter.ToString(hMACSHA1.ComputeHash(excelbytes)).Replace("-", "");

            //Replace fileSize("CS") and SHA1("CRC")
            versiontext[excelindex] = regexCRC.Replace(versiontext[1], "CRC\":\"" + excelCRC);
            versiontext[excelindex] = regexCS.Replace(versiontext[1], "CS\":\"" + excelbytes.Length.ToString());
            versiontext[settingindex] = regexCRC.Replace(versiontext[2], "CRC\":\"" + settingCRC);
            versiontext[settingindex] = regexCS.Replace(versiontext[2], "CS\":\"" + settingbytes.Length.ToString());

            //string[] to string
            textasset.text = string.Join("\n", versiontext);
            //get textasset bytes
            packageversion.data = textasset.GetBytes();
            //pack
            dataversionBundle.fileList[0].stream = new MemoryStream(dataversionFile.Pack());
            byte[] dz = dataversionBundle.RePack();
            //encrypt dataversion.unity3d
            for (int i = 0; i < dz.Length; i++) dz[i] = (byte)(dz[i] ^ 0xA5);
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "[FileUpdate] Update file success");
            ByteToFile(dz, isIOS ? "i_DataVersion.unity3d" : "a_DataVersion.unity3d");
            ByteToFile(excelbytes, isIOS ? "i_excel_output.unity3d" : "a_excel_output.unity3d");
            ByteToFile(settingbytes, isIOS ? "i_setting.unity3d" : "a_setting.unity3d");
            GC.Collect();
        }

        /// <summary>
        /// HEX string to byte[]
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private static byte[] Hex2bytes(string hex)
        {
            int ver = hex.Length / 2;
            byte[] data = new byte[ver];
            for (int i = 0; i < ver; i++)
            {
                data[i] = (byte)Convert.ToInt32(hex.Substring(i * 2, 2), 16);
            }
            return data;
        }

        public static bool ByteToFile(byte[] byteArray, string fileName)
        {
            bool result = false;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    result = true;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
    /// <summary>
    /// edit touch buff
    /// </summary>
    class TouchBuff
    {
        static readonly int buffId = 6;
        static string effect = "EmotionBuff_01";
        static string buffDetail = "TouchBuff_SKL_CHG";
        static float param1 = 0.0299999993f;
        static float param1Add = 0.00999999978f;

        static public byte[] GetNew(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            reader.BaseStream.Position = 0x30;
            int count = reader.ReadInt32();
            EndianBinaryWriter writer = new EndianBinaryWriter(new MemoryStream(), EndianType.LittleEndian);
            reader.BaseStream.Position = 0;
            byte[] header = new byte[0x34];
            reader.BaseStream.Read(header, 0, header.Length);
            writer.Write(header);
            for (int i = 0; i < count; i++)
            {
                writer.Write(buffId);
                writer.WriteAlignedString(effect);
                writer.WriteAlignedString(buffDetail);
                writer.Write(param1);
                writer.Write(0);
                writer.Write(0);
                writer.Write(param1Add);
                writer.Write(0);
                writer.Write(0);
            }
            byte[] output = new byte[writer.BaseStream.Length];
            writer.BaseStream.Position = 0;
            writer.BaseStream.Read(output, 0, output.Length);
            return output;
        }
    }

    /// <summary>
    /// unity TextAsset
    /// </summary>
    class Textasset
    {
        public string name;
        public string text;
        /// <summary>
        ///  TextAsset
        /// </summary>
        /// <param name="data"></param>
        public Textasset(byte[] data)
        {
            EndianBinaryReader reader = new EndianBinaryReader(new MemoryStream(data), EndianType.LittleEndian);
            name = reader.ReadAlignedString();
            text = reader.ReadAlignedString();
        }
        /// <summary>
        /// TextAsset to byte[]
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            EndianBinaryWriter writer = new EndianBinaryWriter(new MemoryStream(), EndianType.LittleEndian);
            writer.WriteAlignedString(name);
            writer.WriteAlignedString(text);
            writer.Position = 0;
            byte[] data = new byte[writer.BaseStream.Length];
            writer.BaseStream.Read(data, 0, data.Length);
            return data;
        }
    }

    /// <summary>
    /// serverinfo
    /// </summary>
    public class Server_modinfo
    {
        /// <summary>
        /// Server
        /// </summary>
        public Gameserver server;
        /// <summary>
        /// Enable?
        /// </summary>
        public bool Enabletouch;
        public string gameserver_url;
        public string gameserver_rsp;
        public string server_ip;
        public string getway_ip;
        public string oa_ip;
        /// <summary>
        /// modified response
        /// </summary>
        public string mod_gameserver_rsp;

        public Server_modinfo(Gameserver sser)
        {
            server = sser;
        }
    }

    /// <summary>
    /// Server
    /// </summary>
    public enum Gameserver
    {
        android01,
        ios01,
        hun01,
        hun02,
        bb01,
        yyb01
    }
}
