using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using NLog;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources {
    public class FileBasedResource {
        public enum DownloadStatus {
            SUCCESS, INCOMPLETE, FAILED
        }

        protected static Logger logger = LogManager.GetCurrentClassLogger();

        public virtual string Filename {
            get { return _filename; }
            set { _filename = value; }
        }
        private string _filename;

        protected bool Download(string url) {
            DownloadStatus status = DownloadStatus.INCOMPLETE;
            long position = 0;
            int attempts = 0;

            while (status == DownloadStatus.INCOMPLETE) {
                status = Download(url, position);
                position = new FileInfo(_filename).Length;

                attempts++;
                if (attempts > 1 && status == DownloadStatus.INCOMPLETE)
                    logger.Warn("Connection lost while downloading resource. Attempting to resume...");
            }
            
            if (status == DownloadStatus.SUCCESS)
                return true;
            else
                return false;
        }

        private DownloadStatus Download(string url, long startPosition) {
            DownloadStatus rtn = DownloadStatus.SUCCESS;

            HttpWebRequest request = null;
            WebResponse response = null;
            Stream webStream = null;
            FileStream fileStream = null;

            try {
                // setup our file to be written to
                if (startPosition == 0)
                    fileStream = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.None);
                else
                    fileStream = new FileStream(Filename, FileMode.Append, FileAccess.Write, FileShare.None);

                // open our connection
                request = (HttpWebRequest)WebRequest.Create(url);
                request.AddRange((int)startPosition);
                response = request.GetResponse();
                webStream = response.GetResponseStream();
                
                // setup our tracking variables for progress
                int bytesRead = 0;
                long totalBytesRead = 0;
                long totalBytes = response.ContentLength + startPosition;

                // download the file and progressively write it to disk
                byte[] buffer = new byte[2048];
                bytesRead = webStream.Read(buffer, 0, buffer.Length);
                while (bytesRead > 0) {
                    // write to our file
                    fileStream.Write(buffer, 0, bytesRead);
                    totalBytesRead = fileStream.Length;

                    //logger.Debug("Download progress: {2:0.0}% ({0:###,###,###} / {1:###,###,###} bytes)", totalBytesRead, totalBytes, 100.0 * totalBytesRead / totalBytes);

                    // read the next stretch of data
                    bytesRead = webStream.Read(buffer, 0, buffer.Length);
                }

                // if we failed delete the file and quit
                if (fileStream.Length != totalBytes) {
                    fileStream.Close();
                    fileStream = null;

                    //File.Delete(Filename);
                    rtn = DownloadStatus.INCOMPLETE;
                }

            }
            catch (Exception e) {
                if (e is ThreadAbortException && fileStream != null) {
                    fileStream.Close();
                    fileStream = null;
                    File.Delete(Filename);
                }
                else {
                    logger.ErrorException("Unexpected error downloading file from: " + url, e);
                }

                rtn = DownloadStatus.FAILED;
            }          

            if (webStream != null) webStream.Close();
            if (fileStream != null) fileStream.Close();
            if (response != null) response.Close();
            if (request != null) request.Abort();

            return rtn;
        }
    }
}
