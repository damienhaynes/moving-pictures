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
            SUCCESS, INCOMPLETE, TIMED_OUT, FAILED
        }

        protected static Logger logger = LogManager.GetCurrentClassLogger();

        public virtual string Filename {
            get { return _filename; }
            set { _filename = value; }
        }
        private string _filename;

        protected bool Download(string url) {
            int maxRetries = MovingPicturesCore.Settings.MaxTimeouts;
            int timeoutBase = MovingPicturesCore.Settings.TimeoutLength;
            int timeoutIncrement = MovingPicturesCore.Settings.TimeoutIncrement;

            DownloadStatus status = DownloadStatus.INCOMPLETE;
            long position = 0;
            int resumeAttempts = 0;
            int retryAttempts = 0;

            while (status != DownloadStatus.SUCCESS && status != DownloadStatus.FAILED) {
                int timeout = timeoutBase + (timeoutIncrement * retryAttempts);

                status = Download(url, position, timeout);

                switch (status) {
                    case DownloadStatus.INCOMPLETE:
                        // if the download ended half way through, log a warning and update the 
                        // position for a resume
                        if (File.Exists(_filename)) position = new FileInfo(_filename).Length;
                        resumeAttempts++;
                        if (resumeAttempts > 1)
                            logger.Warn("Connection lost while downloading resource. Attempting to resume...");
                        break;
                    case DownloadStatus.TIMED_OUT:
                        // if we timed out past our try limit, fail
                        retryAttempts++;
                        if (retryAttempts == maxRetries) {
                            logger.Error("Failed downloading resource from: " + url + ". Reached retry limit of " + maxRetries);
                            status = DownloadStatus.FAILED;
                        }                        
                        break;
                }
            }

            if (status == DownloadStatus.SUCCESS)
                return true;
            else {
                if (File.Exists(_filename))
                    File.Delete(_filename);

                return false;
            }
        }

        private DownloadStatus Download(string url, long startPosition, int timeout) {
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

                // setup and open our connection
                request = (HttpWebRequest)WebRequest.Create(url);
                request.AddRange((int)startPosition);
                request.Timeout = timeout;
                request.ReadWriteTimeout = 20000;
                request.UserAgent = MovingPicturesCore.Settings.UserAgent;
                request.Proxy = WebRequest.DefaultWebProxy;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials; 
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

                // if the downloaded ended prematurely, close the stream but save the file
                // for resuming
                if (fileStream.Length != totalBytes) {
                    fileStream.Close();
                    fileStream = null;

                    rtn = DownloadStatus.INCOMPLETE;
                }

            }
            catch (UriFormatException) {
                // url was invalid
                logger.Warn("Invalid URL: {0}", url);
                rtn = DownloadStatus.FAILED;
            }
            catch (WebException e) {
                // file doesnt exist
                if (e.Message.Contains("404")) {
                    logger.Warn("File does not exist: {0}", url);
                    rtn = DownloadStatus.FAILED;
                }
                
                // timed out or other similar error
                else 
                    rtn = DownloadStatus.TIMED_OUT;

            }
            catch (ThreadAbortException) {
                // user is shutting down the program
                fileStream.Close();
                fileStream = null;
                if (File.Exists(Filename)) File.Delete(Filename);
                rtn = DownloadStatus.FAILED;
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error downloading file from: " + url, e);
                rtn = DownloadStatus.FAILED;
            }

            // if we failed delete the file
            if (fileStream != null && rtn == DownloadStatus.FAILED) {
                fileStream.Close();
                fileStream = null;
                if (File.Exists(Filename)) File.Delete(Filename);
            }

            if (webStream != null) webStream.Close();
            if (fileStream != null) fileStream.Close();
            if (response != null) response.Close();
            if (request != null) request.Abort();

            return rtn;
        }

        /// <summary>
        /// Deletes the specified file. If an exception occurs this is logged.
        /// </summary>
        /// <param name="filepath">the file path.</param>
        protected void DeleteFile(string filepath)
        {
            try
            {
                File.Delete(filepath);
            }
            catch (Exception e)
            {
                logger.Error("File '{0}' could not be deleted: {1}", filepath, e.Message);
            }
        }
    }
}
