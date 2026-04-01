#region Assembly QREncrypter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Project\Github\IFS-EIVO03\Model\bin\Debug\QREncrypter.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace com.tradevan.qrutil;

public class QREncrypter
{
    public string QRCodeINV(string InvoiceNumber, string InvoiceDate, string InvoiceTime, string RandomNumber, decimal SalesAmount, decimal TaxAmount, decimal TotalAmount, string BuyerIdentifier, string RepresentIdentifier, string SellerIdentifier, string BusinessIdentifier, string AESKey)
    {
        try
        {
            inputValidate(InvoiceNumber, InvoiceDate, InvoiceTime, RandomNumber, SalesAmount, TaxAmount, TotalAmount, BuyerIdentifier, RepresentIdentifier, SellerIdentifier, BusinessIdentifier, AESKey);
        }
        catch (Exception ex)
        {
            throw ex;
        }

        string text = InvoiceNumber + InvoiceDate + RandomNumber + Convert.ToInt32(SalesAmount).ToString("x8") + Convert.ToInt32(TotalAmount).ToString("x8") + BuyerIdentifier + SellerIdentifier;
        return text + AESEncrypt(InvoiceNumber + RandomNumber, AESKey).PadRight(24);
    }

    public string AESEncrypt(string plainText, string AESKey)
    {
        byte[] bytes = Encoding.Default.GetBytes(plainText);
        RijndaelManaged rijndaelManaged = new RijndaelManaged();
        rijndaelManaged.KeySize = 128;
        rijndaelManaged.Key = convertHexToByte(AESKey);
        rijndaelManaged.BlockSize = 128;
        rijndaelManaged.IV = Convert.FromBase64String("Dt8lyToo17X/XkXaQvihuA==");
        ICryptoTransform transform = rijndaelManaged.CreateEncryptor();
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
        cryptoStream.Write(bytes, 0, bytes.Length);
        cryptoStream.FlushFinalBlock();
        cryptoStream.Close();
        byte[] inArray = memoryStream.ToArray();
        return Convert.ToBase64String(inArray);
    }

    public string AESDecrype(string cipherText, string AESKey)
    {
        byte[] array = Convert.FromBase64String(cipherText);
        RijndaelManaged rijndaelManaged = new RijndaelManaged();
        rijndaelManaged.KeySize = 128;
        rijndaelManaged.Key = convertHexToByte(AESKey);
        rijndaelManaged.BlockSize = 128;
        rijndaelManaged.IV = Convert.FromBase64String("Dt8lyToo17X/XkXaQvihuA==");
        ICryptoTransform transform = rijndaelManaged.CreateDecryptor();
        MemoryStream stream = new MemoryStream(array);
        CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
        byte[] array2 = new byte[array.Length];
        cryptoStream.Read(array2, 0, array2.Length);
        cryptoStream.Close();
        return Encoding.UTF8.GetString(array2);
    }

    private void inputValidate(string InvoiceNumber, string InvoiceDate, string InvoiceTime, string RandomNumber, decimal SalesAmount, decimal TaxAmount, decimal TotalAmount, string BuyerIdentifier, string RepresentIdentifier, string SellerIdentifier, string BusinessIdentifier, string AESKey)
    {
        if (string.IsNullOrEmpty(InvoiceNumber) || InvoiceNumber.Length != 10)
        {
            throw new Exception("Invaild InvoiceNumber: " + InvoiceNumber);
        }

        if (string.IsNullOrEmpty(InvoiceDate) || InvoiceDate.Length != 7)
        {
            throw new Exception("Invaild InvoiceDate: " + InvoiceDate);
        }

        try
        {
            long.Parse(InvoiceDate);
            int num = int.Parse(InvoiceDate.Substring(3, 2));
            int num2 = int.Parse(InvoiceDate.Substring(5));
            if (num < 1 || num > 12)
            {
                throw new Exception();
            }

            if (num2 < 1 || num2 > 31)
            {
                throw new Exception();
            }
        }
        catch (Exception)
        {
            throw new Exception("Invaild InvoiceDate: " + InvoiceDate);
        }

        if (string.IsNullOrEmpty(InvoiceTime) || InvoiceTime.Length != 6)
        {
            throw new Exception("Invaild InvoiceTime: " + InvoiceTime);
        }

        if (string.IsNullOrEmpty(RandomNumber) || RandomNumber.Length != 4)
        {
            throw new Exception("Invaild RandomNumber: " + RandomNumber);
        }

        if (SalesAmount < 0m)
        {
            throw new Exception("Invaild SalesAmount: " + SalesAmount);
        }

        if (TotalAmount <= 0m)
        {
            throw new Exception("Invaild TotalAmount: " + TotalAmount);
        }

        if (TaxAmount < 0m)
        {
            throw new Exception("Invaild TaxAmount: " + TaxAmount);
        }

        if (string.IsNullOrEmpty(BuyerIdentifier) || BuyerIdentifier.Length != 8)
        {
            throw new Exception("Invaild BuyerIdentifier: " + BuyerIdentifier);
        }

        if (string.IsNullOrEmpty(RepresentIdentifier))
        {
            throw new Exception("Invaild RepresentIdentifier: " + RepresentIdentifier);
        }

        if (string.IsNullOrEmpty(SellerIdentifier) || SellerIdentifier.Length != 8)
        {
            throw new Exception("Invaild SellerIdentifier: " + SellerIdentifier);
        }

        if (string.IsNullOrEmpty(BusinessIdentifier))
        {
            throw new Exception("Invaild BusinessIdentifier: " + BusinessIdentifier);
        }

        if (string.IsNullOrEmpty(AESKey))
        {
            throw new Exception("Invaild AESKey");
        }
    }

    private byte[] convertHexToByte(string hexString)
    {
        byte[] array = new byte[hexString.Length / 2];
        int num = 0;
        for (int i = 0; i < hexString.Length; i += 2)
        {
            int value = Convert.ToInt32(hexString.Substring(i, 2), 16);
            array[num] = BitConverter.GetBytes(value)[0];
            num++;
        }

        return array;
    }
}
#if false // Decompilation log
'27' items in cache
------------------
Resolve: 'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
WARN: Version mismatch. Expected: '2.0.0.0', Got: '4.0.0.0'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll'
#endif
