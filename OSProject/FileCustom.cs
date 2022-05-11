using System;
using System.IO;
using System.Net;
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

            for (int i = this.filename.Length - 1; this.filename[i] != '.'; --i)
            {
                buf += this.filename[i];
            }

            char[] charArray = buf.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
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

        public void rename(String newName)
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

        public void delete()
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

        public void upload()
        {
            //для имени файла
            string shortName = fbAuth.LocalId + "/" + this.filename.Remove(0, this.filename.LastIndexOf("\\") + 1);

            FileStream uploadedFile = new FileStream(this.filename, FileMode.Open, FileAccess.Read);

            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + shortName);
            ftpRequest.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            //Буфер для загружаемых данных
            byte[] file_to_bytes = new byte[uploadedFile.Length];
            //Считываем данные в буфер
            uploadedFile.Read(file_to_bytes, 0, file_to_bytes.Length);

            uploadedFile.Close();

            //Поток для загрузки файла 
            Stream writer = ftpRequest.GetRequestStream();

            writer.Write(file_to_bytes, 0, file_to_bytes.Length);
            writer.Close();
        }

        public void download()
        {
            // Создаем объект FtpWebRequest
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru//" + this.fbAuth.LocalId + "/" + this.filename);
            // устанавливаем метод на загрузку файлов
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
