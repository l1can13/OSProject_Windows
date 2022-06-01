using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ByteSizeLib;

namespace OSProject
{
    internal class FileCustom
    {
        private string filename;
        private Uri uri;
        private double size;
        private DateTime uploadTime;
        private Firebase.Auth.User fbAuth;

        public FileCustom(string filename, Firebase.Auth.User fbAuth)
        {
            this.filename = filename;
            this.fbAuth = fbAuth;
        }

        public string getTypeOfFile()
        {
            string buf = "";

            if (this.filename.EndsWith("-folder"))
            {
                return "Папка";
            }
            else
            {
                for (int i = this.filename.Length - 1; this.filename[i] != '.' && i > 0; --i)
                {
                    buf += this.filename[i];
                }

                char[] charArray = buf.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }
        }

        public string getFilename()
        {
            return this.filename.Remove(0, this.filename.LastIndexOf("\\") + 1);
        }

        public string getUploadTime()
        {
            return this.uploadTime.ToString();
        }

        public double getSize()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

            try
            {
                FtpWebResponse ftpResponse = (FtpWebResponse)request.GetResponse();
                long sizeBytes = ftpResponse.ContentLength;
                ftpResponse.Close();

                var result = ByteSize.FromBits(sizeBytes);
                return result.KiloBytes;
            }
            catch (WebException e)
            {
                Console.WriteLine(((FtpWebResponse)e.Response).StatusDescription);
                return 0;
            }
        }

        public void rename(String newName, bool isTrash)
        {
            if (isTrash)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + "(deleted) " + this.filename);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

                try
                {
                    request.RenameTo = newName;
                    FtpWebResponse ftpResponse = (FtpWebResponse)request.GetResponse();
                    ftpResponse.Close();
                }
                catch (WebException e)
                {
                    Console.WriteLine(((FtpWebResponse)e.Response).StatusDescription);
                }
            }
            else
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

                try
                {
                    request.RenameTo = newName;
                    FtpWebResponse ftpResponse = (FtpWebResponse)request.GetResponse();
                    ftpResponse.Close();
                }
                catch (WebException e)
                {
                    Console.WriteLine(((FtpWebResponse)e.Response).StatusDescription);
                }
            }
        }

        public void delete(bool isTrash)
        {
            if (isTrash)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + "(deleted) " + this.filename);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

                try
                {
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                }
                catch (WebException e)
                {
                    Console.WriteLine(((FtpWebResponse)e.Response).StatusDescription);
                }
            }
            else
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

                try
                {
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                }
                catch (WebException e)
                {
                    Console.WriteLine(((FtpWebResponse)e.Response).StatusDescription);
                }
            }
        }

        public void delete_directory()
        {
            List<string> files = GetFileList();
            FtpWebRequest request;
            foreach (string file in files)
            {
                request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename + "/" + file);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.UseBinary = true;

                // если требуется логин и пароль, устанавливаем их
                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

                try
                {
                    // получаем ответ от сервера в виде объекта FtpWebResponse
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    // получаем поток ответа
                    Stream responseStream = response.GetResponseStream();
                    // сохраняем файл в дисковой системе
                    // создаем поток для сохранения файла
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing");
                    FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing\\" + file, FileMode.Create);

                    //Буфер для считываемых данных
                    byte[] buffer = new byte[64];
                    int size = 0;

                    while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, size);

                    }
                    fs.Close();
                    response.Close();
                }
                catch (WebException e)
                {
                    String status = ((FtpWebResponse)e.Response).StatusDescription;
                    Console.WriteLine(status);
                }
            }

            request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename);
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;
            request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (WebException e)
            {
                Console.WriteLine(((FtpWebResponse)e.Response).StatusDescription);
            }
        }

        public void move_to_trash()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename);
            request.Method = WebRequestMethods.Ftp.Rename;
            request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

            try
            {
                request.RenameTo = "(deleted) " + this.filename;
                FtpWebResponse ftpResponse = (FtpWebResponse)request.GetResponse();
                ftpResponse.Close();
            }
            catch (WebException e)
            {
                Console.WriteLine(((FtpWebResponse)e.Response).StatusDescription);
            }
        }

        bool CheckIfFtpDirectoryExists()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId);
            request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (FtpStatusCode.ActionNotTakenFileUnavailable == response.StatusCode)
                {
                    return false;
                }
            }
            return true;
        }

        public void upload()
        {
            FtpWebRequest ftpRequest;
            try
            {
                if (CheckIfFtpDirectoryExists())
                {
                    ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId);
                    ftpRequest.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");
                    ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

                    FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();

                    response.Close();
                }
                // для имени файла
                string shortName = fbAuth.LocalId + "/" + this.filename.Remove(0, this.filename.LastIndexOf("\\") + 1);

                FileStream uploadedFile = new FileStream(this.filename, FileMode.Open, FileAccess.Read);

                ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + shortName);
                ftpRequest.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                // Буфер для загружаемых данных
                byte[] file_to_bytes = new byte[uploadedFile.Length];
                // Считываем данные в буфер
                uploadedFile.Read(file_to_bytes, 0, file_to_bytes.Length);

                uploadedFile.Close();

                // Поток для загрузки файла 
                Stream writer = ftpRequest.GetRequestStream();

                writer.Write(file_to_bytes, 0, file_to_bytes.Length);
                writer.Close();
            }
            catch (WebException e)
            {
                String status = ((FtpWebResponse)e.Response).StatusDescription;
                Console.WriteLine(status);
            }
        }

        public List<string> GetFileList()
        {
            List<string> list = new List<string>();
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename + "/");
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");
                var response = (FtpWebResponse)request.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream, true);
                while (!reader.EndOfStream)
                {
                    list.Add(reader.ReadLine());
                }
                reader.Close();
                stream.Close();
                response.Close();

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void download_folder()
        {
            List<string> files = GetFileList();
            foreach (string file in files)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename + "/" + file);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;

                // если требуется логин и пароль, устанавливаем их
                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

                try
                {
                    // получаем ответ от сервера в виде объекта FtpWebResponse
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    // получаем поток ответа
                    Stream responseStream = response.GetResponseStream();
                    // сохраняем файл в дисковой системе
                    // создаем поток для сохранения файла
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing");
                    FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing\\" + file, FileMode.Create);

                    //Буфер для считываемых данных
                    byte[] buffer = new byte[64];
                    int size = 0;

                    while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, size);

                    }
                    fs.Close();
                    response.Close();
                }
                catch (WebException e)
                {
                    String status = ((FtpWebResponse)e.Response).StatusDescription;
                    Console.WriteLine(status);
                }
            }
        }

        public void download(bool isTrash)
        {
            if (isTrash)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + "(deleted) " + this.filename);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;

                // если требуется логин и пароль, устанавливаем их
                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

                try
                {
                    // получаем ответ от сервера в виде объекта FtpWebResponse
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    // получаем поток ответа
                    Stream responseStream = response.GetResponseStream();
                    // сохраняем файл в дисковой системе
                    // создаем поток для сохранения файла
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing");
                    FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing\\" + this.filename, FileMode.Create);

                    //Буфер для считываемых данных
                    byte[] buffer = new byte[64];
                    int size = 0;

                    while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, size);

                    }
                    fs.Close();
                    response.Close();
                }
                catch (WebException e)
                {
                    String status = ((FtpWebResponse)e.Response).StatusDescription;
                    Console.WriteLine(status);
                }
            }
            else
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;

                // если требуется логин и пароль, устанавливаем их
                request.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");

                try
                {
                    // получаем ответ от сервера в виде объекта FtpWebResponse
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    // получаем поток ответа
                    Stream responseStream = response.GetResponseStream();
                    // сохраняем файл в дисковой системе
                    // создаем поток для сохранения файла
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing");
                    FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\FileSharing\\" + this.filename, FileMode.Create);

                    //Буфер для считываемых данных
                    byte[] buffer = new byte[64];
                    int size = 0;

                    while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, size);

                    }
                    fs.Close();
                    response.Close();
                }
                catch (WebException e)
                {
                    String status = ((FtpWebResponse)e.Response).StatusDescription;
                    Console.WriteLine(status);
                }
            }
        }
    }
}
