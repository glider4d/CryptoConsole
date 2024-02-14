using System;/*  ww    w . d   e   m o 2   s .    co    m*/
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Reflection;
using MatthiWare.CommandLine.Abstractions.Command;
using MatthiWare.CommandLine;
using ConsoleApp1.Options;
using System.Text.Unicode;
using System.ComponentModel;

class Program
{
    //private static long pageSize = 512000000 * 2;
    private static long mbSize = 300;
    private static long pageSize = mbSize * 1024 * 1024;//128000000;
    private static int pageCount = 0;
    private static List<MemoryStream> listMemoryStream = new List<MemoryStream>();

    static void EncryptFilePagedFromMemory(string inputFile, string outputFile)
    {
        try
        {
            long len = new FileInfo(inputFile).Length;
            pageCount = (int)(len / pageSize + 1);
            string password = @"myKey123"; // Your Key Here
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);
            string cryptFile = outputFile;
            FileStream fsIn = new FileStream(inputFile, FileMode.Open);
            long position = 0;
            for (int i = 0; i < pageCount; i++)
            {

                MemoryStream ms = new MemoryStream();

#pragma warning disable SYSLIB0022 // Type or member is obsolete
                RijndaelManaged RMCrypto = new RijndaelManaged();
#pragma warning restore SYSLIB0022 // Type or member is obsolete




                byte[] bytes = new byte[13107];
                int bufferSize = 13107;
                long offset = 0;
                int read = 0;
                CryptoStream cs = new CryptoStream(ms,
                    RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);
                fsIn.Position = position;// *(i+1);
                while ((read = fsIn.Read(bytes, 0, bufferSize)) != 0)
                {
                    //1 023 997 482
                    //1 024 000 000
                    bool endOfPage = false;
                    if (offset + read > pageSize)
                    {
                        long a = pageSize - offset;
                        read = (int)a;
                        endOfPage = true;
                    }
                    cs.Write(bytes, 0, read);
                    offset += read;
                    if (endOfPage)
                    {
                        if (i == 20)
                            Console.WriteLine("i == 20");
                        break;
                    }


                }
                position += offset;
                cs.FlushFinalBlock();
                //FileStream fsPageCrypto = new FileStream(outputFile + i, FileMode.Create);
                //ms.WriteTo(fsPageCrypto);
                var msItem = new MemoryStream();
                ms.WriteTo(msItem);
                listMemoryStream.Add(msItem);

                cs.Close();
                ms.Close();
                //!!!!fsPageCrypto.Close();
            }


            fsIn.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("ERRRRRRRRRRRROR e = " + e.Message);
        }
    }
    static void DecryptFilePagedFromMemory(string inputFile, string outputFile)
    {
        string password = @"myKey123"; // Your Key Here


        UnicodeEncoding UE = new UnicodeEncoding();
        byte[] key = UE.GetBytes(password);


        //!!!!FileStream fsOut = new FileStream(outputFile, FileMode.Create);
        for (int i = 0; i < pageCount; i++)
        {

            //!!!!FileStream fsCrypt = new FileStream(inputFile+i, FileMode.Open);
            MemoryStream ms = new MemoryStream();
            RijndaelManaged RMCrypto = new RijndaelManaged();
            listMemoryStream[i].Position = 0;

            CryptoStream cs = new CryptoStream(listMemoryStream[i],                  //!!!fsCrypt,
                RMCrypto.CreateDecryptor(key, key),
                CryptoStreamMode.Read);




            //fsOut.CopyTo(fsOut);

            byte[] bytes = new byte[131072];
            int bufferSize = 131072;
            int offset = 0;
            int read = 0;
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {

                ///fsOut.Write(bytes, 0, read);
                ms.Write(bytes, 0, read);

                offset += read;
            }
            listMemoryStream[i] = ms;

            //cs.FlushFinalBlock();
            cs.Close();
            //!!!fsCrypt.Close();

        }
        ///!!!!fsOut.Close();

    }
    static void EncryptFile(string inputFile, string outputFile)
    {


        try
        {
            string password = @"myKey123"; // Your Key Here
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);


            string cryptFile = outputFile;
            FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);


            RijndaelManaged RMCrypto = new RijndaelManaged();


            CryptoStream cs = new CryptoStream(fsCrypt,
                RMCrypto.CreateEncryptor(key, key),
                CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            byte[] bytes = new byte[13107];
            int bufferSize = 13107;
            int offset = 0;
            int read = 0;
            while ((read = fsIn.Read(bytes, 0, bufferSize)) != 0)
            {

                cs.Write(bytes, 0, read);
                offset += read;
                //fsOut.Write(bytes, 0, read);
                //offset += read;
            }
            /*
            int data;
            while ((data = fsIn.ReadByte()) != -1)
                cs.WriteByte((byte)data);
            */



            fsIn.Close();
            cs.Close();
            fsCrypt.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    static void DecryptFile(string inputFile, string outputFile)
    {


        {
            string password = @"myKey123"; // Your Key Here


            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);


            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);


            RijndaelManaged RMCrypto = new RijndaelManaged();


            CryptoStream cs = new CryptoStream(fsCrypt,
                RMCrypto.CreateDecryptor(key, key),
                CryptoStreamMode.Read);


            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            //fsOut.CopyTo(fsOut);

            byte[] bytes = new byte[131072];
            int bufferSize = 131072;
            int offset = 0;
            int read = 0;
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {

                fsOut.Write(bytes, 0, read);
                offset += read;
            }

            /*
            int data;
            while ((data = cs.ReadByte()) != -1)
                fsOut.WriteByte((byte)data);
            */

            fsOut.Close();
            cs.Close();
            fsCrypt.Close();


        }
    }

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
    static void EncryptFilePagedFromFiles(string inputFile, string outputFile)
    {
        try
        {
            long len = new FileInfo(inputFile).Length;
            pageCount = (int)(len / pageSize + 1);
            string password = @"myKey123"; // Your Key Here
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);
            string cryptFile = outputFile;
            FileStream fsIn = new FileStream(inputFile, FileMode.Open);
            long position = 0;
            for (int i = 0; i < pageCount; i++)
            {

                //MemoryStream ms = new MemoryStream();
                FileStream fsPageCrypto = new FileStream(outputFile + i, FileMode.Create);
#pragma warning disable SYSLIB0022 // Type or member is obsolete
                RijndaelManaged RMCrypto = new RijndaelManaged();
#pragma warning restore SYSLIB0022 // Type or member is obsolete




                byte[] bytes = new byte[13107];
                int bufferSize = 13107;
                long offset = 0;
                int read = 0;
                CryptoStream cs = new CryptoStream(fsPageCrypto,
                    RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);
                fsIn.Position = position;// *(i+1);
                while ((read = fsIn.Read(bytes, 0, bufferSize)) != 0)
                {
                    //1 023 997 482
                    //1 024 000 000
                    bool endOfPage = false;
                    if (offset + read > pageSize)
                    {
                        long a = pageSize - offset;
                        read = (int)a;
                        endOfPage = true;
                    }
                    cs.Write(bytes, 0, read);
                    offset += read;
                    if (endOfPage)
                    {
                        if (i == 20)
                            Console.WriteLine("i == 20");
                        break;
                    }


                }
                position += offset;
                cs.FlushFinalBlock();
                //FileStream fsPageCrypto = new FileStream(outputFile + i, FileMode.Create);
                //ms.WriteTo(fsPageCrypto);

                cs.Close();
                fsPageCrypto.Close();
            }


            fsIn.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("ERRRRRRRRRRRROR e = " + e.Message);
        }
    }
    static void DecryptFilePagedFromFiles(string inputFile, string outputFile)
    {
        string password = @"myKey123"; // Your Key Here


        UnicodeEncoding UE = new UnicodeEncoding();
        byte[] key = UE.GetBytes(password);


        FileStream fsOut = new FileStream(outputFile, FileMode.Create);
        for (int i = 0; i < pageCount; i++)
        {

            FileStream fsCrypt = new FileStream(inputFile + i, FileMode.Open);

            RijndaelManaged RMCrypto = new RijndaelManaged();

            CryptoStream cs = new CryptoStream(fsCrypt,                  //!!!fsCrypt,
                RMCrypto.CreateDecryptor(key, key),
                CryptoStreamMode.Read);




            //fsOut.CopyTo(fsOut);

            byte[] bytes = new byte[131072];
            int bufferSize = 131072;
            int offset = 0;
            int read = 0;
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {

                fsOut.Write(bytes, 0, read);
                //fsCrypt.Write(bytes, 0, read);
                fsOut.Flush();
                offset += read;
            }

            //cs.FlushFinalBlock();

            cs.Close();
            //!!!fsCrypt.Close();

        }
        fsOut.Close();
        ///!!!!fsOut.Close();

    }
    [Serializable]
     public class MapCrypto
    {
        public long encryptSizeBlock;
        public long decryptSizeBlock;
    }

    //public static List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();

    static void EncryptFilePagedFromOneFilesWithCert(string inputFile, string outputFile, string ps = @"myKey123", string ivPs = @"myKey123", DateTime[] dt = null)
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
            /*
            FileStream nup2 = new FileStream(outputFile, FileMode.Append);
            byte[] z2 = new byte[1048576];

            nup2.Write(z, 0, 1048576);
            nup2.Close();
            */
 
            for (int i = 0; i < pageCount; i++)
            {
                Console.WriteLine("1");
                FileStream fsPageCrypto = new FileStream(outputFile, FileMode.Append);
                Console.WriteLine("2");
                Console.WriteLine("3 "+cryptoLen + " "+ fsPageCrypto.Position);
         

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
                while ((read = fsIn.Read(bytes, 0, bufferSize)) != 0)
                {
                    //1 023 997 482
                    //1 024 000 000
                    bool endOfPage = false;
                    if (offset + read > pageSize)
                    {
                        long a = pageSize - offset;
                        read = (int)a;
                        endOfPage = true;
                    }
                    cs.Write(bytes, 0, read);
                    offset += read;
                    if (endOfPage)
                    {
                        if (i == 20)
                            Console.WriteLine("i == 20");
                        break;
                    }


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


    static void EncryptFilePagedFromOneFiles(string inputFile, string outputFile, string ps = @"myKey123", string ivPs = @"myKey123", DateTime[] dt = null)
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
            long cryptoLen = 0;
            if (File.Exists(outputFile)) File.Delete(outputFile);
            for (int i = 0; i < pageCount; i++)
            {
                FileStream fsPageCrypto = new FileStream(outputFile, FileMode.OpenOrCreate);
                //fsPageCrypto.Position = cryptoLen + 1048576;
                //MemoryStream ms = new MemoryStream();

#pragma warning disable SYSLIB0022 // Type or member is obsolete
                RijndaelManaged RMCrypto = new RijndaelManaged();
#pragma warning restore SYSLIB0022 // Type or member is obsolete




                byte[] bytes = new byte[13107];
                int bufferSize = 13107;
                long offset = 0;
                int read = 0;
                CryptoStream cs = new CryptoStream(fsPageCrypto,
                    RMCrypto.CreateEncryptor(key, IV),
                    CryptoStreamMode.Write);
                fsIn.Position = position;// *(i+1);
                while ((read = fsIn.Read(bytes, 0, bufferSize)) != 0)
                {
                    //1 023 997 482
                    //1 024 000 000
                    bool endOfPage = false;
                    if (offset + read > pageSize)
                    {
                        long a = pageSize - offset;
                        read = (int)a;
                        endOfPage = true;
                    }
                    cs.Write(bytes, 0, read);
                    offset += read;
                    if (endOfPage)
                    {
                        if (i == 20)
                            Console.WriteLine("i == 20");
                        break;
                    }


                }
                cs.FlushFinalBlock();
                long len1 = fsPageCrypto.Length - cryptoLen;
                cryptoLen = fsPageCrypto.Length;
                
                lenCryptoBlock.Add(new MapCrypto() { encryptSizeBlock = len1, decryptSizeBlock = offset });
                Console.WriteLine($"_ encryptSizeBlock = {len1}, decryptSizeBlock = {offset}, pageCount = {pageCount}, i = {i}" );
                position += offset;

                cs.Close();
                fsPageCrypto.Close();
            }


            fsIn.Close();
            //MapCryptoListWriteJson(outputFile + "_list", lenCryptoBlock);

            CryptoInfo cryptoInfo = new CryptoInfo() { list = lenCryptoBlock, pageSize = pageSize };

            CertWriteJson(outputFile + "_cert", passwordIV,  new CertInfo() { Key = password, dateTime = dt, certInfo = cryptoInfo });
        }
        catch (Exception e)
        {
            Console.WriteLine("ERRRRRRRRRRRROR e = " + e.Message);
        }
    }


    static void EncryptFilePagedFromOneFiles2(string inputFile, string outputFile)
    {
        try
        {
            List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();

            long len = new FileInfo(inputFile).Length;
            pageCount = (int)(len / pageSize + 1);
            string password = @"myKey123"; // Your Key Here
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] key = UE.GetBytes(password);
            string cryptFile = outputFile;
            FileStream fsIn = new FileStream(inputFile, FileMode.Open);
            long position = 0;
            long cryptoLen = 0;
            if (File.Exists(outputFile)) File.Delete(outputFile);
            for (int i = 0; i < pageCount; i++)
            {
                FileStream fsPageCrypto = new FileStream(outputFile, FileMode.OpenOrCreate);
                fsPageCrypto.Position = cryptoLen;
                //MemoryStream ms = new MemoryStream();

#pragma warning disable SYSLIB0022 // Type or member is obsolete
                RijndaelManaged RMCrypto = new RijndaelManaged();
#pragma warning restore SYSLIB0022 // Type or member is obsolete

                MemoryStream memoryStream = new MemoryStream();

                byte[] bytes = new byte[13107];
                int bufferSize = 13107;
                long offset = 0;
                int read = 0;

                //read = fsIn.Read(bytes, 0, (int)pageSize);

                //memoryStream.Write(bytes, 0, read);
                
                CryptoStream cs = new CryptoStream(fsPageCrypto,
                    RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);
                CryptoStream csDebug = new CryptoStream(memoryStream,
                    RMCrypto.CreateEncryptor(key,key),
                    CryptoStreamMode.Read);

                fsIn.Position = position;// *(i+1);
                while ((read = fsIn.Read(bytes, 0, bufferSize)) != 0)
                {
                    //1 023 997 482
                    //1 024 000 000
                    bool endOfPage = false;
                    if (offset + read > pageSize)
                    {
                        long a = pageSize - offset;
                        read = (int)a;
                        endOfPage = true;
                    }
                    cs.Write(bytes, 0, read);
                    offset += read;
                    if (endOfPage)
                    {
                        if (i == 20)
                            Console.WriteLine("i == 20");
                        break;
                    }


                }
                cs.FlushFinalBlock();
                /*
                byte[] debugBuffer = new byte[read];
                int debugRead = 0;
                fsPageCrypto.Position = 0;
                while (( debugRead= fsPageCrypto.Read(debugBuffer, 0, read)) != 0){
                    memoryStream.Write(debugBuffer, 0, debugRead);
                }
                memoryStream.Position = 0;
                var arr = memoryStream.ToArray();

                var memoryStream3 = new MemoryStream();

                while ((read = csDebug.Read(bytes, 0, bufferSize)) != 0)
                {
                    memoryStream3.Write(bytes, 0, read);
                }
                FileStream newFs = new FileStream("D:\\vlc\\1\\debug.txt", FileMode.Create);
                memoryStream3.Position = 0;
                memoryStream3.CopyTo(newFs);

                newFs.Flush();
                newFs.Close();
                */
                    long len1 = fsPageCrypto.Length - cryptoLen;
                cryptoLen = fsPageCrypto.Length;

                lenCryptoBlock.Add(new MapCrypto() { encryptSizeBlock = len1, decryptSizeBlock = offset });
                Console.WriteLine($"_ encryptSizeBlock = {len1}, decryptSizeBlock = {offset}");
                position += offset;

                cs.Close();
                fsPageCrypto.Close();
            }


            fsIn.Close();
            MapCryptoListWriteJson(outputFile + "_list", lenCryptoBlock);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERRRRRRRRRRRROR e = " + e.Message);
        }
    }


    static void DecryptFilePagedFromOneFilesWithCert(string inputFile, string outputFile, string passwordIV = @"myKey123")
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
                /*
                long a = pageSizeDecrypt - offset;
                bufferSize = (int)a;*/
                bufferSize = (int)pageSizeDecrypt;
                endOfPage = true;
            }
            int offset = 0;
            if (i == 5)
            {
                int z = 0;
            }
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {
                //Console.WriteLine($"read = {read}");


                fsOut.Write(bytes, 0, read);
                fsOut.Flush();
                offset += read;
                //position += read;
                if (offset >= pageSizeDecrypt)
                    break;
                if (offset + read > pageSizeDecrypt)
                {
                    long a = pageSizeDecrypt - offset;

                    bufferSize = (int)a;
                }




                /*
                {
                    
                    if (offset + read > pageSize)
                    {
                        long a = pageSize - offset;
                        read = (int)a;
                        endOfPage = true;
                    }
                    cs.Write(bytes, 0, read);
                    offset += read;
                    if (endOfPage)
                    {
                        if (i == 20)
                            Console.WriteLine("i == 20");
                        break;
                    }
                }*/

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

    static void DecryptFilePagedFromOneFiles(string inputFile, string outputFile, string passwordIV = @"myKey123")
    {
        List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();
        //lenCryptoBlock = MapCryptoListRead(inputFile + "_list");
        //lenCryptoBlock = MapCryptoListReadJson(inputFile + "_list");
        string password = @"myKey123"; // Your Key Here
        string IV = passwordIV == ""? @"myKey123" : passwordIV;

        
        CertInfo ci = CertReadInfo(inputFile + "_cert", IV);
        lenCryptoBlock = ci.certInfo.list;//MapCryptoListReadJson(inputFile + "_list");
        password = ci.Key;

        UnicodeEncoding UE = new UnicodeEncoding();
        byte[] key = UE.GetBytes(password);
        byte[] IVbt = UE.GetBytes(IV);

        long position = 0;
        FileStream fsOut = new FileStream(outputFile, FileMode.Create);
        for (int i = 0; i < pageCount; i++)
        {

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
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
            if ( bufferSize > pageSizeDecrypt)
            {
                /*
                long a = pageSizeDecrypt - offset;
                bufferSize = (int)a;*/
                bufferSize = (int)pageSizeDecrypt;
                endOfPage = true;
            }
            int offset = 0;
            if (i == 5)
            {
                int z = 0;
            }
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {
                //Console.WriteLine($"read = {read}");


                fsOut.Write(bytes, 0, read);
                fsOut.Flush();
                offset += read;
                //position += read;
                if (offset >= pageSizeDecrypt)
                    break;
                if (offset + read > pageSizeDecrypt)
                {
                    long a = pageSizeDecrypt - offset;

                    bufferSize = (int)a;
                }




                /*
                {
                    
                    if (offset + read > pageSize)
                    {
                        long a = pageSize - offset;
                        read = (int)a;
                        endOfPage = true;
                    }
                    cs.Write(bytes, 0, read);
                    offset += read;
                    if (endOfPage)
                    {
                        if (i == 20)
                            Console.WriteLine("i == 20");
                        break;
                    }
                }*/
                
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







    public static byte[] GetDecryptBytePage(string path,  long position, byte[] key)
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
            /*
            long a = pageSizeDecrypt - offset;
            bufferSize = (int)a;*/
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


    static void DecryptFilePagedFromOneFiles2(string inputFile, string outputFile)
    {
        List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();
        //lenCryptoBlock = MapCryptoListRead(inputFile + "_list");
        lenCryptoBlock = MapCryptoListReadJson(inputFile + "_list");
        string password = @"myKey123"; // Your Key Here


        UnicodeEncoding UE = new UnicodeEncoding();
        byte[] key = UE.GetBytes(password);


        long position = 0;
        FileStream fsOut = new FileStream(outputFile, FileMode.Create);
        for (int i = 0; i < pageCount; i++)
        {
            
            FileStream fsCrypt2 = new FileStream(inputFile, FileMode.Open);
            fsCrypt2.Position = position;
            MemoryStream msCrypt = new MemoryStream();
            //fsCrypt2.CopyTo(msCrypt);
            pageSize = lenCryptoBlock[i].encryptSizeBlock;
            byte[] buf = new byte[32] { 8, 230, 28, 213, 28, 136, 62, 219, 171, 74, 86, 29, 80, 16, 32, 15, 41, 212, 64, 194, 74, 206, 6, 181, 248, 16, 193, 86, 88, 117, 39, 183 };
            //int rd = fsCrypt2.Read(buf, 0, (int)pageSize);
            int rd = 32;

            msCrypt.Write(buf, 0, rd);

            foreach (var item in buf)
            {
                Console.Write(item + " ");
            }
            msCrypt.Position = 0;
            RijndaelManaged RMCrypto = new RijndaelManaged();

            CryptoStream cs = new CryptoStream(msCrypt,                  //!!!fsCrypt,
                RMCrypto.CreateDecryptor(key, key),
                CryptoStreamMode.Read);


            
            long pageSizeDecrypt = lenCryptoBlock[i].decryptSizeBlock;
            int bfs = 32;
            byte[] bts = new byte[32];
            int r = cs.Read(bts, 0, bfs);

            byte[] bytes = new byte[131072];
            int bufferSize = 131072;
            int read = 0;

            bool endOfPage = false;
            if (bufferSize > pageSizeDecrypt)
            {
                /*
                long a = pageSizeDecrypt - offset;
                bufferSize = (int)a;*/
                bufferSize = (int)pageSizeDecrypt;
                endOfPage = true;
            }
            int offset = 0;
            if (i == 5)
            {
                int z = 0;
            }
            while ((read = cs.Read(bytes, 0, bufferSize)) != 0)
            {
                //Console.WriteLine($"read = {read}");


                fsOut.Write(bytes, 0, read);
                fsOut.Flush();
                offset += read;
                //position += read;
                if (offset >= pageSizeDecrypt)
                    break;
                if (offset + read > pageSizeDecrypt)
                {
                    long a = pageSizeDecrypt - offset;

                    bufferSize = (int)a;
                }




                /*
                {
                    
                    if (offset + read > pageSize)
                    {
                        long a = pageSize - offset;
                        read = (int)a;
                        endOfPage = true;
                    }
                    cs.Write(bytes, 0, read);
                    offset += read;
                    if (endOfPage)
                    {
                        if (i == 20)
                            Console.WriteLine("i == 20");
                        break;
                    }
                }*/
            }
            Console.WriteLine($"offset = {offset}");
            position += lenCryptoBlock[i].encryptSizeBlock;
            //cs.FlushFinalBlock();

            cs.Close();
            fsCrypt2.Close();
        }
        fsOut.Close();
        ///!!!!fsOut.Close();

    }

    public static void CalcEncryptDecrypt()
    {
        /*
        long encryptCount = 0;
        long decryptCount = 0;
        

        foreach(var item in lenCryptoBlock)
        {
            Console.WriteLine($"encryptLen = {item.encryptSizeBlock}, descryptLen = {item.decryptSizeBlock}");
            encryptCount += item.encryptSizeBlock;
            decryptCount += item.decryptSizeBlock;
        }
        Console.WriteLine($"encryptCount = {encryptCount}, descryptCount = {decryptCount}");*/
    }

    //    public static List<MapCrypto> lenCryptoBlock = new List<MapCrypto>();


    public class CryptoInfo
    {
        public List<MapCrypto> list;
        public long pageSize = 0;
    }

    public class CertInfo
    {
        public string Key;
        public DateTime[] dateTime;
        public CryptoInfo certInfo;

    }
    public static void MapCryptoListWriteJson(string path, List<MapCrypto> list)
    {
        
        string filename = path;

        CryptoInfo cryptoInfo = new CryptoInfo() { list = list , pageSize = pageSize };

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

            /*
            var writer = new StreamWriter(ms);
            writer.Write(jsonString);
            writer.Flush();
            ms.Position = 0;*/
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

            /*
            var writer = new StreamWriter(ms);
            writer.Write(jsonString);
            writer.Flush();
            ms.Position = 0;*/
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

            /*
            //test
            FileStream fs2 = new FileStream(path, FileMode.Open);
            byte[] readBytes = new byte[4];
            fs2.Read(readBytes, 0, readBytes.Length);
            
            var int32 = BitConverter.ToInt32(readBytes);

            byte[] memRead = new byte[int32];

            fs2.Read(memRead, 0, memRead.Length);

            Console.WriteLine("MEMlen = " + memRead.Length);

            fs2.Close();
            Console.WriteLine("int32 = " + int32);*/
            //------------test
            Console.WriteLine("len = " + len);
        }

        //if (File.Exists(path)) File.Delete(path);
        //File.WriteAllText(path, jsonString);
    }
    public static CertInfo CertReadInfoOneFile(string path, string IV)
    {
         
        Console.WriteLine("CertReadInfoOneFile "+ path);
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

        
        /*
        Console.WriteLine("before"); 
        Console.WriteLine("before____________");
        FileStream fs3 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        fs3.Close();
        FileStream fs4 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        fs4.Close();
        Console.WriteLine("after");*/
        
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
    public static byte[] GeEncrypt(string path )
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
    static void Main(string[] args)
    {

        //EncryptFilePagedFromOneFilesWithCert("test2.txt", "test2_test")
        //ConsoleApp1.exe -f "D:\\vlc\\1\\1\\5.txt" -d -g  -t "2005-05-05 22:12" "2011-05-11 23:11:12"
        //DecryptFile("D:\\vlc\\1\\456.txt", "D:\\vlc\\1\\result.txt");
        /*
        CalcFilesSize("D:\\1\\crypto\\best.mp6");
        EncryptFile("D:\\1\\crypto\\best.mp4", "D:\\1\\crypto\\best.mp6");
        */
        //long lenBestmp4 = new FileInfo("D:\\1\\crypto\\best.mp4").Length;
        //long lenBestmp6 = new FileInfo("D:\\1\\crypto\\best.mp6").Length;
        //DecryptFilePagedFromMemory("D:\\\\vlc\\\\1\\\\456.txt", "D:\\vlc\\1\\test2.txt");
        /*
        byte[] encryptBlock  = GeEncrypt("D:\\vlc\\1\\456.txt");
        GetDecryptBytePage("D:\\vlc\\1\\456.txt", encryptBlock);
        return;*/
        /*
        EncryptFilePagedFromMemory("D:\\1\\crypto\\5.txt", "D:\\1\\crypto\\456.txt");

        DecryptFilePagedFromMemory("D:\\1\\crypto\\456.txt", "D:\\1\\crypto\\result.txt");
        return;*/

        string sourceFile = "";// "D:\\vlc\\1\\sql.mp4";
        string outputFile = "";// "D:\\vlc\\1\\sql_mp4";
        string resultFileName = "";// "D:\\vlc\\1\\result.mp4";
        /*
        EncryptFilePagedFromOneFiles(sourceFile, outputFile);
        DecryptFilePagedFromOneFiles(outputFile, resultFileName);
        return;*/
        /*
        EncryptFilePagedFromOneFiles(sourceFile, outputFile );
        Console.WriteLine("4567");
        
        DecryptFilePagedFromOneFiles(outputFile, resultFile, deviceKey);

        */


        //FFOPPRNS
        //12345678
        /*
        EncryptFilePagedFromOneFiles("D:\\vlc\\1\\1\\5.txt", "D:\\vlc\\1\\1\\51.txt", @"FFOPPRNS", "12345678");
        
        
        
        DecryptFilePagedFromOneFiles("D:\\vlc\\1\\1\\51.txt", "D:\\vlc\\1\\1\\result.txt","12345678");
        
        


        return;*/

        var options = new CommandLineParserOptions
        {
            AppName = "CryptoConsole"
        };

        var parser = new CommandLineParser<ProgramOptions>();

        var result = parser.Parse(args);

        if (result.HasErrors)
        {
            Console.WriteLine("Parsing has error..");
            return;
        }

        var programOptions = result.Result;
        sourceFile = programOptions.FileName;

        if ( !File.Exists(sourceFile) )
        {
            Console.WriteLine("could not find the file");
            return;
        }

        outputFile = programOptions.OutputFile;
        if ( outputFile.Equals(""))
        {
            outputFile = sourceFile;
            string ext = Path.GetExtension(sourceFile);

            //sourceFile.
            if (ext.Length > 0)
                outputFile = Path.ChangeExtension(sourceFile, "extra");
            else
                outputFile = sourceFile + ".extra";
            
        }
        resultFileName = "";
        if (sourceFile.Length > 0)
        {
             string ext = Path.GetExtension(sourceFile);
            if (ext.Equals("")) ext = "mp4";
            resultFileName = Path.ChangeExtension(sourceFile, "result");
            resultFileName += ext;
        }

        bool needDecryptFile = programOptions.NeedDecryptFile;
        string programKey = "";
        if (programOptions.GenerateKey)
        {
            programKey = RandomString(8);
            Console.WriteLine(programKey);
        }

        if (programOptions.arr != null) Console.WriteLine("!=null" + programOptions.arr.Count());
 
        string deviceKey = "";
        if ( programOptions.DeviceKey.Length > 0)
        {
            deviceKey = programOptions.DeviceKey;
        }

        if (programOptions.dateTime!= null)
        {
            DateTime foo = DateTime.Now;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            long utc = foo.ToFileTimeUtc();
            DateTime convertedDate = DateTime.FromFileTimeUtc(utc);
            DateTime runTime = System.DateTime.UtcNow;

            Console.WriteLine($"unixTime = {unixTime}, utc = {utc}, convertedDate = {convertedDate.ToString()}, now = {new DateTime(DateTime.Now.ToFileTimeUtc()).ToString()}, utcNow = {DateTime.UtcNow}");
            foreach (var item in programOptions.dateTime)
            Console.WriteLine($"dateTime = {item.ToString()}");

        }

        string resultFile = sourceFile;
        mbSize = programOptions.PageSize;
        //DirectoryInfo parentDir = Directory.GetParent(path.EndsWith("\\") ? path : string.Concat(path, "\\"));



        System.Console.WriteLine($"sourceFile = {sourceFile}");
        System.Console.WriteLine($"outputFile = {outputFile}");
        if (needDecryptFile)
            System.Console.WriteLine($"resultFile = {resultFileName}");
        if (programOptions.GenerateKey)
        {

        }

        Console.WriteLine("123");
        //tut**** EncryptFilePagedFromOneFiles(sourceFile, outputFile, programKey, deviceKey, programOptions.dateTime);
        Console.WriteLine("________________________________________________");
        Console.WriteLine($"deviceKey = {deviceKey}");
        Console.WriteLine($"programKey = {programKey}");
        EncryptFilePagedFromOneFilesWithCert(sourceFile, outputFile, programKey, deviceKey, programOptions.dateTime);
        //return;
        Console.WriteLine("4567");
        if (needDecryptFile)
        {
            //tut**** DecryptFilePagedFromOneFiles(outputFile, resultFileName, deviceKey);
            Console.WriteLine($"--------------------------------------- deviceKey for decrypt = {deviceKey}");
            DecryptFilePagedFromOneFilesWithCert(outputFile, resultFileName, deviceKey);
            Console.WriteLine("777");
        }
        /*
        EncryptFilePagedFromOneFiles(sourceFile, outputFile);
        if (needDecryptFile)
            DecryptFilePagedFromOneFiles(outputFile, "D:\\vlc\\1\\result.mp4");
        */
        /*
        return;
        var result2 = RandomString(15);
        Console.WriteLine(result);
        return;
        //EncryptFilePagedFromOneFiles("D:\\vlc\\1\\LOTR.mkv", "D:\\vlc\\1\\456.txt");
        EncryptFilePagedFromOneFiles(sourceFile, outputFile, programKey, deviceKey);
        if (needDecryptFile)
            DecryptFilePagedFromOneFiles(outputFile, "D:\\vlc\\1\\result.mp4", deviceKey);*/
        /*
        

        DecryptFilePagedFromOneFiles("D:\\vlc\\1\\456.txt", "D:\\vlc\\1\\result.mp4");
        */
        //CalcEncryptDecrypt();


        /*
        EncryptFilePagedFromMemory("D:\\1\\crypto\\best.mp4", "D:\\1\\crypto\\best.mp6");

        DecryptFilePagedFromMemory("D:\\1\\crypto\\best.mp6", "D:\\1\\crypto\\test2.txt");
        WriteMemoryListInFile();*/
        //Console.WriteLine($"lenBestmp4 = {lenBestmp4}, lenBestmp6 = {lenBestmp6}");

        /*
    EncryptFilePaged("D:\\1\\crypto\\best.mp4", "D:\\1\\crypto\\best.mp6");

    DecryptFilePaged("D:\\1\\crypto\\best.mp6", "D:\\1\\crypto\\testpage2.mp4");
    */
        /*
        EncryptFile("D:\\1\\crypto\\test.txt", "D:\\1\\crypto\\test.tt");
        DecryptFile("D:\\1\\crypto\\test.tt", "D:\\1\\crypto\\test2.txt");*/
        /*
        EncryptFile("D:\\1\\crypto\\best.mp4", "D:\\1\\crypto\\best.mp6");
        DecryptFile("D:\\1\\crypto\\best.mp6", "D:\\1\\crypto\\clear.mp4");*/
        //!!!EncryptFile("D:\\vlc\\1\\_midle.mp4", "D:\\vlc\\1\\_.mp6");
        //DecryptFile("D:\\vlc\\1\\_.mp6", "D:\\vlc\\1\\testR.mp4");
        //encryptexample();
        //decryptexample();

        /*
        var actualFilepath = "D:\\vlc\\Робин_и_Мэриан.mp4";
        var videoBytes = ConvertVideoToBytes(actualFilepath);
        var encryptedvideoBytes = EncryptVideo(videoBytes);
        ConvertEncryptFileToFile(encryptedvideoBytes);
        var encryptedFilepath = "D:\\vlc\\VideosEncryptedFile.deific";
        var readVideoBytes = ConvertVideoToBytes(encryptedFilepath);
        var decryptedVideoBytes = DecryptVideo(readVideoBytes);
        ConvertDecryptFileToFile(decryptedVideoBytes);*/
    }

    static void encryptexample()
    {
        FileStream stream = new FileStream("D:\\vlc\\test.txt",
         FileMode.OpenOrCreate, FileAccess.Write);

        DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

        cryptic.Key = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");
        cryptic.IV = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");

        CryptoStream crStream = new CryptoStream(stream,
           cryptic.CreateEncryptor(), CryptoStreamMode.Write);


        byte[] data = ASCIIEncoding.ASCII.GetBytes("Hello World!");

        crStream.Write(data, 0, data.Length);

        crStream.Close();
        stream.Close();

    }

    static void decryptexample()
    {
        FileStream stream = new FileStream("D:\\vlc\\test.txt",
                              FileMode.Open, FileAccess.Read);

        DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

        cryptic.Key = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");
        cryptic.IV = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");

        CryptoStream crStream = new CryptoStream(stream,
            cryptic.CreateDecryptor(), CryptoStreamMode.Read);

        StreamReader reader = new StreamReader(crStream);

        string data = reader.ReadToEnd();
        System.Console.WriteLine("data = " + data);
        reader.Close();
        stream.Close();
    }

    private static byte[] ConvertVideoToBytes(string filePath)
    {
        return System.IO.File.ReadAllBytes(filePath);
    }


    private static byte[] EncryptVideo(byte[] videoBytes)
    {
        string passPhrase = "mypassphrase27092019";
        string saltValue = "mysaltvalue";
        RijndaelManaged RijndaelCipher = new RijndaelManaged();
        RijndaelCipher.Mode = CipherMode.CBC;
        byte[] salt = Encoding.ASCII.GetBytes(saltValue);
        PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, salt,
     "SHA1", 2);
        ICryptoTransform Encryptor =
        RijndaelCipher.CreateEncryptor(password.GetBytes(32), password.GetBytes(16));

        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor,
        CryptoStreamMode.Write);
        cryptoStream.Write(videoBytes, 0, videoBytes.Length);
        cryptoStream.FlushFinalBlock();
        byte[] cipherBytes = memoryStream.ToArray();

        memoryStream.Close();
        cryptoStream.Close();

        return cipherBytes;
    }


    private static void ConvertEncryptFileToFile(byte[] encryptedvideoBytes)
    {
        var filePath = string.Empty;
        filePath = "D:\\vlc";
        System.IO.File.WriteAllBytes(filePath + "\\VideosEncryptedFile.deific",
     encryptedvideoBytes);
    }


    private static byte[] DecryptVideo(byte[] encryptedVideoBytes)
    {
        string passPhrase = "mypassphrase27092019";
        string saltValue = "mysaltvalue";

        RijndaelManaged RijndaelCipher = new RijndaelManaged();

        RijndaelCipher.Mode = CipherMode.CBC;
        byte[] salt = Encoding.ASCII.GetBytes(saltValue);
        PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, salt, "SHA1", 2);

        ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(password.GetBytes(32), password.GetBytes(16));

        MemoryStream memoryStream = new MemoryStream(encryptedVideoBytes);
        CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);
        byte[] plainBytes = new byte[encryptedVideoBytes.Length];

        int decryptedCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

        memoryStream.Close();
        cryptoStream.Close();

        return plainBytes;
    }


    private static void ConvertDecryptFileToFile(byte[] decryptedVideoBytes)
    {
        var filePath = string.Empty;
        filePath = "D:\\vlc";
        System.IO.File.WriteAllBytes(filePath + "\\FinalFile.mp4",
        decryptedVideoBytes);
    }
}
