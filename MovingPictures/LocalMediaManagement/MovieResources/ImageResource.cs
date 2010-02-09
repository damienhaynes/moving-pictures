using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieResources {
    public enum ImageLoadResults { SUCCESS, SUCCESS_REDUCED_SIZE, FAILED_TOO_SMALL, FAILED_ALREADY_LOADED, FAILED }
    
    public class ImageResource: FileBasedResource {

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
                    try {
                        File.Delete(Filename);
                        File.Delete(ThumbFilename);
                    }
                    catch (Exception) { }
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
                        if (File.Exists(Filename)) File.Delete(Filename);
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
                Image newImage = (Image)new Bitmap(newWidth, newHeight);
                Graphics g = Graphics.FromImage((Image)newImage);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, newWidth, newHeight);
                g.Dispose();
                img.Dispose();
                img = null;

                // save image as a jpg
                ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder qualityParamID = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameter qualityParam = new EncoderParameter(qualityParamID, 90L);
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;
                newImage.Save(Filename, jgpEncoder, encoderParams);
                newImage.Dispose();

                return rtn;
            }
            catch (Exception) {
                if (File.Exists(Filename)) File.Delete(Filename);
                return ImageLoadResults.FAILED;
            }
            finally {
                if (img != null) img.Dispose();
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
