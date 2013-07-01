using System;
using System.IO;

namespace Common.Utility.Encryption
{
    /// <summary>
    /// Access���ݿ����
    /// </summary>
    public class AccessMDBEncrypt
    {
        public AccessMDBEncrypt()
        {
            bIsEncrypt = true;
        }

        private static bool bIsEncrypt;

        /// <summary>
        /// ����
        /// </summary>
        public static bool BIsEncrypt
        {
            get { return bIsEncrypt; }
            set { bIsEncrypt = value; }
        }

        /// <summary>
        /// ���ܱ�־
        /// </summary>
        private static long KEY_YES = 0x1111111;

        private static long KEY_NO = 0x1010101;

        private bool GetFileNameByPath(string strPath, ref string strFilePath, ref string strFileName)
        {
            for (int i = strPath.Length - 1; i >= 0; i--)
            {
                char c = strPath[i];
                if (c == '/' || c == '\\')
                {
                    strFileName = strPath.Substring(i + 1, strPath.Length - i - 1);
                    strFilePath = strPath.Substring(0, i);
                    break;
                }
            }
            return true;
        }

        /// <summary>
        /// Reads the file info.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private byte[] ReadFileInfo(string filePath)
        {
            try
            {
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                long nOffset = 0;
                int nCount = 1024;
                long nMaxLength = fileStream.Length;
                byte[] arrData = new byte[nMaxLength];
                do
                {
                    if (nMaxLength - nOffset < nCount)
                    {
                        nCount = (int)(nMaxLength - nOffset);
                    }
                    byte[] temp = new byte[nCount];
                    fileStream.Seek(nOffset, SeekOrigin.Begin);
                    fileStream.Read(temp, 0, nCount);
                    for (long i = nOffset; i < nOffset + nCount; i++)
                    {
                        arrData[i] = temp[i- nOffset];
                    }
                    nOffset += nCount;
                } while (nOffset < nMaxLength);
                fileStream.Close();
                return arrData;
            }
            catch(Exception exp)
            {
                Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());

            }
            return null;
        }
        //�������ܣ��ļ�����
        //�����ˣ���ΰ
        //ʱ�䣺2008-6-4
        public bool FileToEncrypt(string strFullPath)
        {
            try
            {
                string strFilePath = "";
                string strFileName = "";
                GetFileNameByPath(strFullPath, ref strFilePath, ref strFileName);
                string strTempPath = strFilePath + "/temp.mdb";
                File.Copy(strFullPath, strTempPath, true);

                BinaryReader binReader = new BinaryReader(File.Open(strTempPath, FileMode.Open));
                binReader.BaseStream.Seek(-8, SeekOrigin.End);
                BinaryWriter binWriter = null;

                long flg = binReader.ReadInt64();

                binReader.BaseStream.Seek(0, SeekOrigin.Begin);
                byte[] headInfo = new byte[16];
                binReader.Read(headInfo, 0, 16);
                binReader.Close();

                

                if (flg == KEY_YES)
                {
                    //�Ѿ����ܹ�
                    return true;
                }
                else
                {
                    //��һ�μ���
                    binWriter = new BinaryWriter(File.Open(strTempPath, FileMode.Open));
                    binWriter.Seek(-8, SeekOrigin.End);
                    if (flg != KEY_NO)
                    {
                        //��һ�μ���
                        binWriter.Seek(0, SeekOrigin.End);
                    }
                    if (bIsEncrypt == true)
                    {
                        flg = KEY_YES;
                        binWriter.Write(flg);
                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    headInfo[i] = (byte)(headInfo[i] ^ 0xff);
                }
                if (bIsEncrypt == true)
                {
                    /*
                    int j;
                    for(j=0;j<16;j++)
                    {
                        if(headInfo[j] != this.m_FileHead[j])
                        {
                            break;
                        }
                    }

                    if(j >= 16)
                    {
                        //û�м���
                        for (int i = 0; i < 16; i++)
                        {
                            headInfo[i] = (byte) (headInfo[i] ^ 0xff);
                        }
                    }
                    */
                    binWriter.Seek(0, SeekOrigin.Begin);
                    binWriter.Write(headInfo);
                }
                binWriter.Close();

                File.Copy(strTempPath, strFullPath, true);
                File.Delete(strTempPath);
                return true;
            }
            catch (Exception exp)
            {
                Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());
            }
            return false;
        }

        //�������ܣ��ļ�����
        //�����ˣ���ΰ
        //ʱ�䣺2008-6-4
        public bool FileToUntie(string strFullPath)
        {
            try
            {
                string strFilePath = "";
                string strFileName = "";
                GetFileNameByPath(strFullPath, ref strFilePath, ref strFileName);
                string strTempPath = strFilePath + "/temp.mdb";

                File.Copy(strFullPath, strTempPath, true);

                BinaryWriter binWriter = null;
                BinaryReader binReader = new BinaryReader(File.Open(strTempPath, FileMode.Open));
                binReader.BaseStream.Seek(-8, SeekOrigin.End);
                long flg;

                flg = binReader.ReadInt64();

                binReader.BaseStream.Seek(0, SeekOrigin.Begin);

                byte[] headInfo = new byte[16];
                binReader.Read(headInfo, 0, 16);
                binReader.Close();
                /*
                for (int i = 0; i < 16; i++)
                {
                    this.m_FileHead[i] = headInfo[i];
                }
                */
                if (flg != KEY_YES && flg != KEY_NO)
                {
                    //û�м���
                    binReader.Close();
                    binWriter = new BinaryWriter(File.Open(strTempPath, FileMode.Open));
                    binWriter.Seek(0, SeekOrigin.End);
                    flg = KEY_NO;
                    binWriter.Write(flg);
                    binWriter.Close();
                    File.Copy(strTempPath, strFullPath, true);
                    File.Delete(strTempPath);
                    return true;
                }
                else
                {
                    if (flg == KEY_YES)
                    {
                        //�����ܱ�����Ϊ����״̬
                        binReader.Close();
                        binWriter = new BinaryWriter(File.Open(strTempPath, FileMode.Open));
                        binWriter.Seek(-8, SeekOrigin.End);
                        flg = KEY_NO;
                        binWriter.Write(flg);
                    }
                    else
                    {
                        //û�м���
                        binReader.Close();
                        File.Delete(strTempPath);
                        return false;
                    }
                }

                for (int i = 0; i < 16; i++)
                {
                    headInfo[i] = (byte) (headInfo[i] ^ 0xff);
                }
                binWriter.Seek(0, SeekOrigin.Begin);
                binWriter.Write(headInfo, 0, 16);
                binWriter.Close();
                File.Copy(strTempPath, strFullPath, true);
                File.Delete(strTempPath);
                return true;
            }
            catch (Exception exp)
            {
                Common.Utility.Log.OperationalLogManager.AppendMessage(exp.ToString());
            }
            return false;
        }
    }
}