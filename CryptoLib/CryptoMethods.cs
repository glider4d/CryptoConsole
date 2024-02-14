using Newtonsoft.Json;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace CryptoLib
{
    public class CryptoMethods
    {
        private readonly static long mbSize = 300;
        private static long pageSize = mbSize * 1024 * 1024;//128000000;
        private static int pageCount = 0;
        private static List<MemoryStream> listMemoryStream = new List<MemoryStream>();
        public static void CalcFilesSize(string path)
        {
            long countLength = 0;
            for (int i = 0; i < 22; i++)
            {
                long len = new FileInfo(path + i).Length;
                countLength += len;
                Console.WriteLine($"len = {len}, countLength = {countLength}");
            }
        }

        public static void WriteMemoryListInFile()
        {
            FileStream fs = new FileStream("D:\\1\\crypto\\listMemoryStream.txt", FileMode.Create);
            foreach (var item in listMemoryStream)
            {
                item.Position = 0;
                fs.Write(item.ToArray(), 0, (int)item.Length);
                fs.Flush();
            }
            fs.Close();
        }

        [Serializable]
        public class MapCrypto
        {
            public long encryptSizeBlock;
            public long decryptSizeBlock;
        }


        
        //public static List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();

        public static async Task<bool> EncryptFilePagedFromOneFilesWithCertAsync(IProgress<int> progress, string inputFile, string outputFile, string ps = @"myKey123", string ivPs = @"ssk11ll_", DateTime[] dt = null)
        {
            bool result = false;
            try
            {
                Console.WriteLine(inputFile);
                Console.WriteLine(outputFile);
                List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();

                long len = new FileInfo(inputFile).Length;
                pageCount = (int)(len / pageSize + 1);
                string password = ps == "" ? @"myKey123" : ps;//@"myKey123"; // Your Key Here
                string passwordIV = ivPs == "" ? @"myKey123" : ivPs;
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);
                byte[] IV = UE.GetBytes(passwordIV);
                string cryptFile = outputFile;
                FileStream fsIn = new FileStream(inputFile, FileMode.Open);
                long position = 0;
                long cryptoLen = 1048576;
                if (File.Exists(outputFile)) File.Delete(outputFile);
                FileStream nup = new FileStream(outputFile, FileMode.OpenOrCreate);
                byte[] z = new byte[1048576];

                nup.Write(z, 0, 1048576);
                nup.Close();

                int percents = 0;
                for (int i = 0; i < pageCount; i++)
                {
                    double maxPercentPage = ((i + 1) * 100 / pageCount);
                    double minPercentPage = ((i) * 100 / pageCount);
                    percents = (int)minPercentPage;
                    int percentsCount = percents;
                    progress.Report(percents);
                    Console.WriteLine("1");
                    FileStream fsPageCrypto = new FileStream(outputFile, FileMode.Append);
                    Console.WriteLine("2");
                    Console.WriteLine("3 " + cryptoLen + " " + fsPageCrypto.Position);


#pragma warning disable SYSLIB0022 // Type or member is obsolete
                    RijndaelManaged RMCrypto = new RijndaelManaged();
#pragma warning restore SYSLIB0022 // Type or member is obsolete



                    Console.WriteLine("3");

                    byte[] bytes = new byte[13107];
                    int bufferSize = 13107;
                    long offset = 0;
                    int read = 0;
                    CryptoStream cs = new CryptoStream(fsPageCrypto,
                        RMCrypto.CreateEncryptor(key, IV),
                        CryptoStreamMode.Write);
                    Console.WriteLine("4");
                    
                    fsIn.Position = position;// *(i+1);

                    double currentPercentPage = 0;
                    int checkMaxCount = 100;
                    int countCheck = 100;

                    while ((read = await fsIn.ReadAsync(bytes, 0, bufferSize)) != 0)
                    {
                        //1 023 997 482
                        //1 024 000 000
                        bool endOfPage = false;


                        if (countCheck < checkMaxCount) countCheck++;
                        else
                        {
                            countCheck = 0;
                            currentPercentPage = (double)((double)(offset + read) / pageSize) * 100;
                            var percentsTemp = (int)((double)((maxPercentPage-minPercentPage) / 100) * currentPercentPage) + (int)minPercentPage;
                            if (percents != percentsTemp)
                            {
                                percents = percentsTemp;
                                progress.Report(percents);
                            }
                            
                            Console.WriteLine("CurrentPage > 0");

                        }


                        if (offset + read > pageSize)
                        {
                            long a = pageSize - offset;
                            read = (int)a;
                            endOfPage = true;
                        }
                        await cs.WriteAsync(bytes, 0, read);
                        //cs.Write(bytes, 0, read);
                        offset += read;
                        if (endOfPage)
                        {
                            if (i == 20)
                                Console.WriteLine("i == 20");
                            break;
                        }


                    }
                    await cs.FlushFinalBlockAsync();
                    //cs.FlushFinalBlock();
                    long len1 = fsPageCrypto.Length - cryptoLen;// - 1048576;
                    cryptoLen = fsPageCrypto.Length;
                    Console.WriteLine("len1 = " + len1 + ".Length = " + fsPageCrypto.Length + " cryptoLen = " + cryptoLen);

                    lenCryptoBlock.Add(new MapCrypto() { encryptSizeBlock = len1, decryptSizeBlock = offset });
                    Console.WriteLine($"_ encryptSizeBlock = {len1}, decryptSizeBlock = {offset}, pageCount = {pageCount}, i = {i}");
                    position += offset;
                    
                    cs.Close();
                    fsPageCrypto.Close();
                }



                fsIn.Close();
                //MapCryptoListWriteJson(outputFile + "_list", lenCryptoBlock);

                CryptoInfo cryptoInfo = new CryptoInfo() { list = lenCryptoBlock, pageSize = pageSize };

                //CertWriteJson(outputFile, passwordIV, new CertInfo() { Key = password, dateTime = dt, certInfo = cryptoInfo });
                CertWriteJsonOneFile(outputFile, passwordIV, new CertInfo() { Key = password, dateTime = dt, certInfo = cryptoInfo });
                percents = 100;
                progress.Report((int)percents);
                result = true;






            }
            catch (Exception e)
            {
                result = false;
                Console.WriteLine("ERRRRRRRRRRRROR e = " + e.Message);
            }
            return result;
        }
        

        public static void EncryptFilePagedFromOneFilesWithCert(string inputFile, string outputFile, string ps = @"myKey123", string ivPs = @"ssk11ll_", DateTime[] dt = null)
        {
            try
            {
                Console.WriteLine(inputFile);
                Console.WriteLine(outputFile);
                List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();

                long len = new FileInfo(inputFile).Length;
                pageCount = (int)(len / pageSize + 1);
                string password = ps == "" ? @"myKey123" : ps;//@"myKey123"; // Your Key Here
                string passwordIV = ivPs == "" ? @"myKey123" : ivPs;
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);
                byte[] IV = UE.GetBytes(passwordIV);
                string cryptFile = outputFile;
                FileStream fsIn = new FileStream(inputFile, FileMode.Open);
                long position = 0;
                long cryptoLen = 1048576;
                if (File.Exists(outputFile)) File.Delete(outputFile);
                FileStream nup = new FileStream(outputFile, FileMode.OpenOrCreate);
                byte[] z = new byte[1048576];

                nup.Write(z, 0, 1048576);
                nup.Close();

                
                for (int i = 0; i < pageCount; i++)
                {

                    double maxPercentPage = ((i + 1) *100 /pageCount) ;
                    Console.WriteLine("1");
                    FileStream fsPageCrypto = new FileStream(outputFile, FileMode.Append);
                    Console.WriteLine("2");
                    Console.WriteLine("3 " + cryptoLen + " " + fsPageCrypto.Position);


#pragma warning disable SYSLIB0022 // Type or member is obsolete
                    RijndaelManaged RMCrypto = new RijndaelManaged();
#pragma warning restore SYSLIB0022 // Type or member is obsolete



                    Console.WriteLine("3");

                    byte[] bytes = new byte[13107];
                    int bufferSize = 13107;
                    long offset = 0;
                    int read = 0;
                    CryptoStream cs = new CryptoStream(fsPageCrypto,
                        RMCrypto.CreateEncryptor(key, IV),
                        CryptoStreamMode.Write);
                    Console.WriteLine("4");

                    fsIn.Position = position;// *(i+1);
                    double currentPercentPage = 0;

                    int checkMaxCount = 10;
                    int countCheck = 10;

                    while ((read = fsIn.Read(bytes, 0, bufferSize)) != 0)
                    {
//                        int readPercent
//                        pageSize/(offset+read)
                        //1 023 997 482
                        //1 024 000 000
                        bool endOfPage = false;


                        if (countCheck < checkMaxCount) countCheck++;
                        else
                        {
                            countCheck = 0;
                            currentPercentPage = (double)((double)(offset + read) / pageSize) * 100;
                            if (currentPercentPage > 80)
                            {
                                var z1 = (double)(maxPercentPage / 100) * currentPercentPage;
                                Console.WriteLine("CurrentPage > 0");
                            }
                        }

                        if (offset + read > pageSize)
                        {
                            long a = pageSize - offset;
                            read = (int)a;
                            endOfPage = true;
                        }
                        cs.Write(bytes, 0, read);
                        offset += read;
                        if (endOfPage) break;
                    }
                    cs.FlushFinalBlock();
                    long len1 = fsPageCrypto.Length - cryptoLen;// - 1048576;
                    cryptoLen = fsPageCrypto.Length;
                    Console.WriteLine("len1 = " + len1 + ".Length = " + fsPageCrypto.Length + " cryptoLen = " + cryptoLen);

                    lenCryptoBlock.Add(new MapCrypto() { encryptSizeBlock = len1, decryptSizeBlock = offset });
                    Console.WriteLine($"_ encryptSizeBlock = {len1}, decryptSizeBlock = {offset}, pageCount = {pageCount}, i = {i}");
                    position += offset;

                    cs.Close();
                    fsPageCrypto.Close();
                }



                fsIn.Close();
                //MapCryptoListWriteJson(outputFile + "_list", lenCryptoBlock);

                CryptoInfo cryptoInfo = new CryptoInfo() { list = lenCryptoBlock, pageSize = pageSize };

                //CertWriteJson(outputFile, passwordIV, new CertInfo() { Key = password, dateTime = dt, certInfo = cryptoInfo });
                CertWriteJsonOneFile(outputFile, passwordIV, new CertInfo() { Key = password, dateTime = dt, certInfo = cryptoInfo });








            }
            catch (Exception e)
            {
                Console.WriteLine("ERRRRRRRRRRRROR e = " + e.Message);
            }
        }
        public static async Task DecryptFilePagedFromOneFilesWithCertAsync(IProgress<int> progress, string inputFile, string outputFile, string passwordIV = @"ssk11ll_")
        {
            try
            {
                List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();

                //long len = new FileInfo(inputFile).Length;
                //pageCount = (int)(len / pageSize + 1);

                //lenCryptoBlock = MapCryptoListRead(inputFile + "_list");
                //lenCryptoBlock = MapCryptoListReadJson(inputFile + "_list");
                string password = @"myKey123"; // Your Key Here
                string IV = passwordIV == "" ? @"myKey123" : passwordIV;
                Console.WriteLine("DecryptFilePagedFromOneFilesWithCert 1");
                CertInfo ci = CertReadInfoOneFile(inputFile, IV);

                Console.WriteLine("DecryptFilePagedFromOneFilesWithCert 2");
                //        CertInfo ci = CertReadInfo(inputFile + "_cert", IV);
                lenCryptoBlock = ci.certInfo.list;//MapCryptoListReadJson(inputFile + "_list");
                pageCount = ci.certInfo.list.Count;
                password = ci.Key;
                Console.WriteLine("DecryptFilePagedFromOneFilesWithCert 3 " + ci.Key);
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);
                byte[] IVbt = UE.GetBytes(IV);

                long position = 1048576;//0;

                FileStream fsOut = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                //FileStream fsOut = new FileStream(outputFile, FileMode.Create);
                int percents = 0;
                for (int i = 0; i < pageCount; i++)
                {
                    double maxPercentPage = ((i + 1) * 100 / pageCount);
                    double minPercentPage = ((i) * 100 / pageCount);
                    percents = (int)minPercentPage;
                    int percentsCount = percents;
                    progress.Report(percents);

                    FileStream fsCrypt = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //MemoryStream msCrypt = new MemoryStream();
                    //fsCrypt2.CopyTo(msCrypt);


                    fsCrypt.Position = position;
                    RijndaelManaged RMCrypto = new RijndaelManaged();

                    CryptoStream cs = new CryptoStream(fsCrypt,                  //!!!fsCrypt,
                        RMCrypto.CreateDecryptor(key, IVbt),
                        CryptoStreamMode.Read);


                    pageSize = lenCryptoBlock[i].encryptSizeBlock;
                    long pageSizeDecrypt = lenCryptoBlock[i].decryptSizeBlock;


                    byte[] bytes = new byte[131072];
                    int bufferSize = 131072;
                    int read = 0;

                    bool endOfPage = false;
                    if (bufferSize > pageSizeDecrypt)
                    {

                        bufferSize = (int)pageSizeDecrypt;
                        endOfPage = true;
                    }
                    int offset = 0;


                    double currentPercentPage = 0;
                    int checkMaxCount = 100;
                    int countCheck = 100;
                    while ((read = await cs.ReadAsync(bytes, 0, bufferSize)) != 0)
                    {
                        //Console.WriteLine($"read = {read}");


                        await fsOut.WriteAsync(bytes, 0, read);
                        await fsOut.FlushAsync();
                        offset += read;
                        //position += read;
                        if (offset >= pageSizeDecrypt)
                            break;
                        if (countCheck < checkMaxCount) countCheck++;
                        else
                        {
                            countCheck = 0;
                            currentPercentPage = (double)((double)(offset + read) / pageSize) * 100;
                            var percentsTemp = (int)((double)((maxPercentPage - minPercentPage) / 100) * currentPercentPage) + (int)minPercentPage;
                            if (percents != percentsTemp)
                            {
                                percents = percentsTemp;
                                progress.Report(percents);
                            }

                            Console.WriteLine("CurrentPage > 0");

                        }

                        if (offset + read > pageSizeDecrypt)
                        {
                            long a = pageSizeDecrypt - offset;

                            bufferSize = (int)a;
                        }
                    }


                    Console.WriteLine($"offset = {offset}");
                    position += lenCryptoBlock[i].encryptSizeBlock;
                    //cs.FlushFinalBlock();

                    cs.Close();
                    fsCrypt.Close();
                    //fsCrypt.Close();

                }
                percents = 100;
                progress.Report(percents);
                fsOut.Close();
            } catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            ///!!!!fsOut.Close();
        }
        public static void DecryptFilePagedFromOneFilesWithCert(string inputFile, string outputFile, string passwordIV = @"ssk11ll_")
        {
            FileStream fsOut = new FileStream(outputFile, FileMode.Create);
            List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();
            try
            {

                string password = @"myKey123"; 
                string IV = passwordIV == "" ? @"myKey123" : passwordIV;

                long len = new FileInfo(inputFile).Length;
                //pageCount = (int)(len / pageSize + 1);

                CertInfo ci = CertReadInfoOneFile(inputFile, IV);
                lenCryptoBlock = ci.certInfo.list;
//                long encryptSizeBlock = lenCryptoBlock[0].encryptSizeBlock;
                //int pageSizeDebugCount
                pageCount = ci.certInfo.list.Count();
                password = ci.Key;
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);
                byte[] IVbt = UE.GetBytes(IV);

                long position = 1048576;//0;
                
                for (int i = 0; i < pageCount; i++)
                {

                    FileStream fsCrypt = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    //MemoryStream msCrypt = new MemoryStream();
                    //fsCrypt2.CopyTo(msCrypt);


                    fsCrypt.Position = position;
                    RijndaelManaged RMCrypto = new RijndaelManaged();

                    CryptoStream cs = new CryptoStream(fsCrypt,                  //!!!fsCrypt,
                        RMCrypto.CreateDecryptor(key, IVbt),
                        CryptoStreamMode.Read);


                    pageSize = lenCryptoBlock[i].encryptSizeBlock;
                    long pageSizeDecrypt = lenCryptoBlock[i].decryptSizeBlock;


                    byte[] bytes = new byte[131072];
                    int bufferSize = 131072;
                    int read = 0;

                    bool endOfPage = false;
                    if (bufferSize > pageSizeDecrypt){
                        bufferSize = (int)pageSizeDecrypt;
                        endOfPage = true;
                    }

                    int offset = 0;
                    while ((read = cs.Read(bytes, 0, bufferSize)) != 0){
                        fsOut.Write(bytes, 0, read);
                        fsOut.Flush();
                        offset += read;
                        if (offset >= pageSizeDecrypt)
                            break;
                        if (offset + read > pageSizeDecrypt)
                        {
                            long a = pageSizeDecrypt - offset;

                            bufferSize = (int)a;
                        }
                    }
                    position += lenCryptoBlock[i].encryptSizeBlock;
                    //cs.FlushFinalBlock();
                    

                    cs.Close();
                    fsCrypt.Close();
                    //fsCrypt.Close();

                }
                fsOut.Close();
            }
            catch(Exception ex)
            {
                fsOut.Close();
            }

            ///!!!!fsOut.Close();
        }


        public static async Task DecryptFilePagedFromOneFilesWithCertAsync2(string inputFile, string outputFile, string passwordIV = @"ssk11ll_")
        {

            List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();
            //lenCryptoBlock = MapCryptoListRead(inputFile + "_list");
            //lenCryptoBlock = MapCryptoListReadJson(inputFile + "_list");
            string password = @"myKey123"; // Your Key Here
            string IV = passwordIV == "" ? @"myKey123" : passwordIV;
            Console.WriteLine("DecryptFilePagedFromOneFilesWithCert 1");
            CertInfo ci = CertReadInfoOneFile(inputFile, IV);
            Console.WriteLine("DecryptFilePagedFromOneFilesWithCert 2");
            //        CertInfo ci = CertReadInfo(inputFile + "_cert", IV);
            lenCryptoBlock = ci.certInfo.list;//MapCryptoListReadJson(inputFile + "_list");
            password = ci.Key;
            Console.WriteLine("DecryptFilePagedFromOneFilesWithCert 3 " + ci.Key);
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);
            byte[] IVbt = UE.GetBytes(IV);

            long position = 1048576;//0;
            FileStream fsOut = new FileStream(outputFile, FileMode.Create);
            for (int i = 0; i < pageCount; i++)
            {

                FileStream fsCrypt = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //MemoryStream msCrypt = new MemoryStream();
                //fsCrypt2.CopyTo(msCrypt);


                fsCrypt.Position = position;
                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,                  //!!!fsCrypt,
                    RMCrypto.CreateDecryptor(key, IVbt),
                    CryptoStreamMode.Read);


                pageSize = lenCryptoBlock[i].encryptSizeBlock;
                long pageSizeDecrypt = lenCryptoBlock[i].decryptSizeBlock;


                byte[] bytes = new byte[131072];
                int bufferSize = 131072;
                int read = 0;

                bool endOfPage = false;
                if (bufferSize > pageSizeDecrypt)
                {

                    bufferSize = (int)pageSizeDecrypt;
                    endOfPage = true;
                }
                int offset = 0;
                if (i == 5)
                {
                    int z = 0;
                }
                while ((read = await cs.ReadAsync(bytes, 0, bufferSize)) != 0)
                {
                    //Console.WriteLine($"read = {read}");


                    await fsOut.WriteAsync(bytes, 0, read);
                    await fsOut.FlushAsync();
                    offset += read;
                    //position += read;
                    if (offset >= pageSizeDecrypt)
                        break;
                    if (offset + read > pageSizeDecrypt)
                    {
                        long a = pageSizeDecrypt - offset;

                        bufferSize = (int)a;
                    }






                }
                Console.WriteLine($"offset = {offset}");
                position += lenCryptoBlock[i].encryptSizeBlock;
                //cs.FlushFinalBlock();

                cs.Close();
                fsCrypt.Close();
                //fsCrypt.Close();

            }
            fsOut.Close();
            ///!!!!fsOut.Close();
        }

        public static byte[] GetDecryptBytePage(string path, long position, byte[] key)
        {
            List<MapCrypto> mapcCryptoList = MapCryptoListReadJson(path + "_list");
            FileStream fs = new FileStream(path, FileMode.Open);

            int pageNumber = 0;

            //byte[] encrypt = GetEncryptBytePage(path, mapcCryptoList, position, pageNumber);
            //FileStream fsCrypt2 = new FileStream(path, FileMode.Open);
            //fsCrypt2.Position = position;
            //MemoryStream msCrypt = new MemoryStream();
            //fsCrypt2.CopyTo(msCrypt);
            fs.Position = position;
            pageSize = mapcCryptoList[0].encryptSizeBlock;
            byte[] encrypt = new byte[pageSize];
            int rd = fs.Read(encrypt, 0, (int)pageSize);
            //msCrypt.Write(buf, 0, rd);
            //msCrypt.Position = 0;
            //MemoryStream cryptoMs = new MemoryStream(encrypt);
            MemoryStream cryptoMs = new MemoryStream();
            cryptoMs.Write(encrypt, 0, encrypt.Length);
            cryptoMs.Position = 0;
            MemoryStream decryptMs = new MemoryStream();
            RijndaelManaged RMCrypto = new RijndaelManaged();

            byte[] bytes = new byte[13107];
            int bufferSize = 13107;
            long offset = 0;
            int read = 0;


            CryptoStream cs = new CryptoStream(cryptoMs,
                RMCrypto.CreateEncryptor(key, key),
                CryptoStreamMode.Read);

            long pageSizeDecrypt = mapcCryptoList[pageNumber].decryptSizeBlock;
            bool endOfPage = false;
            if (bufferSize > pageSizeDecrypt)
            {
                bufferSize = (int)pageSizeDecrypt;
                endOfPage = true;
            }
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {

                decryptMs.Write(bytes, 0, read);
                offset += read;
            }

            cs.Close();
            cryptoMs.Close();
            fs.Close();
            return decryptMs.ToArray();

        }

        public static void MapCryptoListWriteJson(string path, List<MapCrypto> list)
        {

            string filename = path;

            CryptoInfo cryptoInfo = new CryptoInfo() { list = list, pageSize = pageSize };

            string jsonString = JsonConvert.SerializeObject(cryptoInfo);
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllText(path, jsonString);
        }




        public static List<MapCrypto> MapCryptoListReadJson(string path)
        {
            string fileName = path;
            string jsonString = File.ReadAllText(fileName);
            //List<MapCrypto> result = JsonSerializer.Deserialize<List<MapCrypto>>(jsonString)!;
            //List<MapCrypto> result = JsonConvert.DeserializeObject<List<MapCrypto>>(jsonString);
            CryptoInfo result = JsonConvert.DeserializeObject<CryptoInfo>(jsonString)!;

            return result!.list;
        }
        public static void CertWriteJson(string path, string IV, CertInfo certInfo)
        {
            Console.WriteLine("CertWriteJson");
            string filename = path;



            string jsonString = JsonConvert.SerializeObject(certInfo);

            //EncryptString(jsonString, IV);
            FileStream fs = new FileStream(path, FileMode.Create);
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] bIV = UE.GetBytes(IV);


#pragma warning disable SYSLIB0022 // Type or member is obsolete
                RijndaelManaged RMCrypto = new RijndaelManaged();
#pragma warning restore SYSLIB0022 // Type or member is obsolete

                
                Console.WriteLine("CertWriteJson 2");
                CryptoStream cs = new CryptoStream(fs,
                        RMCrypto.CreateEncryptor(bIV, bIV),
                        CryptoStreamMode.Write);
                Console.WriteLine("CertWriteJson 3" + jsonString);

                byte[] cipherBytes = Encoding.ASCII.GetBytes(jsonString);

                //           string rr = Encoding.ASCII.GetString(cipherBytes, 0, cipherBytes.Length);
                //Convert.FromBase64String(jsonString);
                //byte[] cipherBytes = utf8.GetBytes("šarže");

                Console.WriteLine("cipherBytes = " + cipherBytes.Length);
                cs.Write(cipherBytes);
                cs.FlushFinalBlock();
                fs.Close();
                cs.Close();
            }

            //if (File.Exists(path)) File.Delete(path);
            //File.WriteAllText(path, jsonString);
        }

        public static void CertWriteJsonOneFile(string path, string IV, CertInfo certInfo)
        {
            Console.WriteLine("CertWriteJson");
            string filename = path;



            string jsonString = JsonConvert.SerializeObject(certInfo);

            //EncryptString(jsonString, IV);
            //FileStream fs = new FileStream(path, FileMode.Open);
            MemoryStream ms = new MemoryStream();
            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] bIV = UE.GetBytes(IV);


#pragma warning disable SYSLIB0022 // Type or member is obsolete
                RijndaelManaged RMCrypto = new RijndaelManaged();
#pragma warning restore SYSLIB0022 // Type or member is obsolete

                
                Console.WriteLine("CertWriteJson 2");
                CryptoStream cs = new CryptoStream(ms,
                        RMCrypto.CreateEncryptor(bIV, bIV),
                        CryptoStreamMode.Write);
                Console.WriteLine("CertWriteJson 3" + jsonString);

                byte[] cipherBytes = Encoding.ASCII.GetBytes(jsonString);

                //           string rr = Encoding.ASCII.GetString(cipherBytes, 0, cipherBytes.Length);
                //Convert.FromBase64String(jsonString);
                //byte[] cipherBytes = utf8.GetBytes("šarže");

                Console.WriteLine("cipherBytes = " + cipherBytes.Length);
                cs.Write(cipherBytes);
                cs.FlushFinalBlock();

                //fs.Close();


                cs.Close();

                int len = ms.ToArray().Length;

                byte[] memArray = ms.ToArray();


                ms.Close();

                FileStream fs = new FileStream(path, FileMode.Open);
                fs.Position = 0;
                byte[] bytes = BitConverter.GetBytes(len);
                fs.Write(bytes, 0, bytes.Length);
                fs.Write(memArray, 0, memArray.Length);


                fs.Close();

                
                Console.WriteLine("len = " + len);
            }

            //if (File.Exists(path)) File.Delete(path);
            //File.WriteAllText(path, jsonString);
        }
        public static CertInfo CertReadInfoOneFile(string path, string IV)
        {

            Console.WriteLine("CertReadInfoOneFile " + path);
            string fileName = path;

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(IV);
            byte[] IVbt = UE.GetBytes(IV);

            long position = 0;
            FileStream fs7 = new FileStream(path, FileMode.Open);
            fs7.Position = 0;
            byte[] readBytesLen = new byte[4];
            fs7.Read(readBytesLen, 0, readBytesLen.Length);

            var lenCertInfoBytes = BitConverter.ToInt32(readBytesLen);

            Console.WriteLine("lenCertInfoBytes = " + lenCertInfoBytes);

            byte[] memReadBytes = new byte[lenCertInfoBytes];

            fs7.Read(memReadBytes, 0, memReadBytes.Length);
            MemoryStream ms = new MemoryStream(memReadBytes);
            ms.Position = 0;

            fs7.Close();



            //FileStream fsCrypt = new FileStream(path, FileMode.Open);
            //MemoryStream msCrypt = new MemoryStream();
            //fsCrypt2.CopyTo(msCrypt);


            //fsCrypt.Position = position;
            RijndaelManaged RMCrypto = new RijndaelManaged();

            CryptoStream cs = new CryptoStream(ms,                  //!!!fsCrypt,
                RMCrypto.CreateDecryptor(key, IVbt),
                CryptoStreamMode.Read);

            byte[] bytes = new byte[13107];
            int bufferSize = 13107;
            long offset = 0;
            int read = 0;

            MemoryStream decryptMs = new MemoryStream();
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {

                decryptMs.Write(bytes, 0, read);
                offset += read;
            }
            var resultBytes = decryptMs.ToArray();
            //string jsonString = Convert.ToBase64String(resultBytes);
            Console.WriteLine("resultBytes.Length = " + resultBytes.Length);
            string jsonString = Encoding.ASCII.GetString(resultBytes, 0, resultBytes.Length);
            Console.WriteLine(jsonString);
            //string jsonString = File.ReadAllText(fileName);
            CertInfo result = JsonConvert.DeserializeObject<CertInfo>(jsonString);
            ms.Close();
            cs.Close();


            

            return result!;
        }
        public static CertInfo CertReadInfo(string path, string IV)
        {
            Console.WriteLine("CertReadInfo");
            string fileName = path;

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(IV);
            byte[] IVbt = UE.GetBytes(IV);

            long position = 0;




            FileStream fsCrypt = new FileStream(path, FileMode.Open);
            //MemoryStream msCrypt = new MemoryStream();
            //fsCrypt2.CopyTo(msCrypt);


            fsCrypt.Position = position;
            RijndaelManaged RMCrypto = new RijndaelManaged();

            CryptoStream cs = new CryptoStream(fsCrypt,                  //!!!fsCrypt,
                RMCrypto.CreateDecryptor(key, IVbt),
                CryptoStreamMode.Read);

            byte[] bytes = new byte[13107];
            int bufferSize = 13107;
            long offset = 0;
            int read = 0;

            MemoryStream decryptMs = new MemoryStream();
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {

                decryptMs.Write(bytes, 0, read);
                offset += read;
            }
            var resultBytes = decryptMs.ToArray();
            //string jsonString = Convert.ToBase64String(resultBytes);
            Console.WriteLine("resultBytes.Length = " + resultBytes.Length);
            string jsonString = Encoding.ASCII.GetString(resultBytes, 0, resultBytes.Length);
            Console.WriteLine(jsonString);
            //string jsonString = File.ReadAllText(fileName);
            CertInfo result = JsonConvert.DeserializeObject<CertInfo>(jsonString);
            return result!;
        }
        public static void MapCryptoListWrite(string path, List<MapCrypto> list)
        {
            string filename = path;//@"(""D:\\vlc\\1\\list.bin";
            BinaryFormatter formatter = new BinaryFormatter();

            using (System.IO.Stream ms = File.OpenWrite(filename))
            {
#pragma warning disable SYSLIB0011

                formatter.Serialize(ms, list);
#pragma warning restore SYSLIB0011

            }
        }

        public static List<MapCrypto> MapCryptoListRead(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fs = File.Open(path, FileMode.Open))
            {
#pragma warning disable SYSLIB0011

                dynamic obj = formatter.Deserialize(fs);
#pragma warning restore SYSLIB0011

                return obj;
            }
        }
        public static byte[] GeEncrypt(string path)
        {

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            MemoryStream ms = new MemoryStream();
            byte[] bytes = new byte[32];
            int bufferSize = 32;
            long offset = 0;
            int read = 0;
            while ((read = fs.Read(bytes, 0, bufferSize)) != 0)
            {

                ms.Write(bytes, 0, read);
                offset += read;
                break;
            }
            byte[] result = ms.ToArray();
            return result;
        }

        public static byte[] GetDecryptBytePage(string path, byte[] encrypt)
        {
            //FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            int pageNumber = 0;
            long startRange = 0;
            long endRange = 32;
            long len = endRange - startRange;

            MemoryStream cryptoMs = new MemoryStream(encrypt);
            cryptoMs.Position = 0;
            MemoryStream decryptMs = new MemoryStream();
            RijndaelManaged RMCrypto = new RijndaelManaged();

            string password = @"myKey123"; // Your Key Here


            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);

            byte[] bytes = new byte[13107];
            int bufferSize = 13107;
            long offset = 0;
            int read = 0;
            CryptoStream cs = new CryptoStream(cryptoMs,
                RMCrypto.CreateEncryptor(key, key),
                CryptoStreamMode.Read);

            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {

                decryptMs.Write(bytes, 0, read);
                offset += read;
            }

            cs.Close();
            cryptoMs.Close();
            return decryptMs.ToArray();

        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}