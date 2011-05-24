using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using NLog;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources {
    public enum ImageLoadResults { SUCCESS, SUCCESS_REDUCED_SIZE, FAILED_TOO_SMALL, FAILED_ALREADY_LOADED, FAILED }
    
    public class ImageResource: FileBasedResource {

        protected static Logger logger = LogManager.GetCurrentClassLogger();
        
        public class ImageSize {
            public int Width;
            public int Height;
        }

        public string ThumbFilename {
            get;
            set;
        }

        protected void GenerateThumbnail() {
            throw new NotImplementedException();
        }

        public ImageLoadResults FromUrl(string url, bool ignoreRestrictions, ImageSize minSize, ImageSize maxSize, bool redownload) {
            // if this resource already exists
            if (File.Exists(Filename)) {
                // if we are redownloading, just delete what we have
                if (redownload) {
                    DeleteFile(Filename);
                    DeleteFile(ThumbFilename);
                }
                // otherwise return an "already loaded" failure
                else {
                    return ImageLoadResults.FAILED_ALREADY_LOADED;
                }
            }

            // try to grab the image if failed, exit
            if (!Download(url)) return ImageLoadResults.FAILED;

            // verify the image file and resize it as needed
            return VerifyAndResize(minSize, maxSize);
        }

        protected ImageLoadResults VerifyAndResize(ImageSize minSize, ImageSize maxSize) {
            Image img = null;
            Image newImage = null;
            try {
                ImageLoadResults rtn = ImageLoadResults.SUCCESS;

                // try to open the image
                img = Image.FromFile(Filename);
                int newWidth = img.Width;
                int newHeight = img.Height;

                // check if the image is too small
                if (minSize != null) {
                    if (img.Width < minSize.Width || img.Height < minSize.Height) {
                        img.Dispose();
                        img = null;
                        DeleteFile(Filename);
                        return ImageLoadResults.FAILED_TOO_SMALL;
                    }
                }

                // check if the image is too big
                if (maxSize != null) {
                    if (img.Width > maxSize.Width || img.Height > maxSize.Height) {

                        // calculate new dimensions
                        newWidth = maxSize.Width;
                        newHeight = maxSize.Width * img.Height / img.Width;

                        if (newHeight > maxSize.Height) {
                            newWidth = maxSize.Height * img.Width / img.Height;
                            newHeight = maxSize.Height;
                        }

                        rtn = ImageLoadResults.SUCCESS_REDUCED_SIZE;
                    }
                }

                // resize / rebuild image
                newImage = (Image)new Bitmap(newWidth, newHeight);
                Graphics g = Graphics.FromImage((Image)newImage);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, newWidth, newHeight);
                g.Dispose();
                img.Dispose();
                img = null;

                // determine compression quality
                int quality = MovingPicturesCore.Settings.JpgCompressionQuality;
                if (quality > 100) quality = 100;
                if (quality < 0) quality = 0;

                // save image as a jpg
                ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder qualityParamID = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameter qualityParam = new EncoderParameter(qualityParamID, quality);
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;
                newImage.Save(Filename, jgpEncoder, encoderParams);
                newImage.Dispose();
                newImage = null;

                return rtn;
            }
            catch (Exception e) {
                logger.Error("An error occured while processing '{0}': {1}", Filename, e.Message);
                
                // even though we already have this disposing logic in the finally statement we
                // make sure the objects are disposed before File.Delete is called. 
                if (img != null) {
                    img.Dispose();
                    img = null;
                }

                if (newImage != null) {
                    newImage.Dispose();
                    newImage = null;
                }

                DeleteFile(Filename);

                return ImageLoadResults.FAILED;
            }
            finally {
                if (img != null) img.Dispose();
                if (newImage != null) newImage.Dispose();
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs) 
                if (codec.FormatID == format.Guid) 
                    return codec;

            return null;
        }
    }
}
