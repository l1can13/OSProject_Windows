using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
            this.uploadTime = DateTime.Now;
        }

        public string getFilename()
        {
            return this.filename.Remove(0, this.filename.LastIndexOf("\\") + 1);
        }

        public void upload()
        {
            //для имени файла
            string shortName = fbAuth.LocalId + "/" + this.filename.Remove(0, this.filename.LastIndexOf("\\") + 1);

            FileStream uploadedFile = new FileStream(this.filename, FileMode.Open, FileAccess.Read);

            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru/var/www/s222776/data/" + shortName);
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
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://backup-storage5.hostiman.ru/var/www/s222776/data/" + this.fbAuth.LocalId + "/" + this.filename);

            ftpRequest.Credentials = new NetworkCredential("s222776", "Tmmm8eTKwZ9fHUqh");
            //команда фтп RETR
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            //Файлы будут копироваться в кталог программы
            FileStream downloadedFile = new FileStream(Environment.SpecialFolder.Desktop.ToString() + "/" + this.filename, FileMode.Create, FileAccess.ReadWrite);

            FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            //Получаем входящий поток
            Stream responseStream = ftpResponse.GetResponseStream();

            //Буфер для считываемых данных
            byte[] buffer = new byte[1024];
            int size = 0;

            while ((size = responseStream.Read(buffer, 0, 1024)) > 0)
            {
                downloadedFile.Write(buffer, 0, size);

            }
            ftpResponse.Close();
            downloadedFile.Close();
            responseStream.Close();
        }
    }
}
