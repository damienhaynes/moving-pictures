using NLog;
using System;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Reflection;
using MediaPortal.GUI.Library;
using System.IO;

namespace Cornerstone.MP {
    public delegate void AsyncImageLoadComplete(AsyncImageResource image);
    
    public class AsyncImageResource {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private Object loadingLock = new Object();
        private int pendingToken = 0;
        private int threadsWaiting = 0;


        /// <summary>
        /// This event is triggered when a new image file has been successfully loaded
        /// into memory.
        /// </summary>
        public event AsyncImageLoadComplete ImageLoadingComplete;

        /// <summary>
        /// True if this resources will actively load into memory when assigned a file.
        /// </summary>
        public bool Active {
            get {
                return _active;
            }

            set {
                if (_active == value)
                    return;

                Thread newThread = new Thread(new ThreadStart(activeWorker));
                newThread.Name = "AsyncImageResource.activeWorker";
                newThread.Start();
            }
        }
        private bool _active = true;

        /// <summary>
        /// If multiple changes to the Filename property are made in rapid succession, this delay
        /// will be used to prevent uneccisary loading operations. Most useful for large images that
        /// take a non-trivial amount of time to load from memory.
        /// </summary>
        public int Delay {
            get { return _delay; }
            set { _delay = value; }
        } private int _delay = 250;

        private void activeWorker() {
            lock (loadingLock) {
                if (!_active) {
                    // load the resource
                    _identifier = loadResource(_filename);
                    _active = true;

                    // notify any listeners a resource has been loaded
                    if (ImageLoadingComplete != null)
                        ImageLoadingComplete(this);
                }
                else {
                    unloadResource(_filename);
                    _identifier = null;
                    _active = false;
                }
            }
        }
        

        /// <summary>
        /// This MediaPortal property will automatically be set with the renderable identifier
        /// once the resource has been loaded. Appropriate for a texture field of a GUIImage 
        /// control.
        /// </summary>
        public string Property {
            get { return _property; }
            set { 
                _property = value;

                writeProperty();
            }
        }
        private string _property = null;

        private void writeProperty() {
            if (_active && _property != null && _identifier != null)
                GUIPropertyManager.SetProperty(_property, _identifier);
            else
                if (_property != null)
                    GUIPropertyManager.SetProperty(_property, "-");
        }


        /// <summary>
        /// The identifier used by the MediaPortal GUITextureManager to identify this resource.
        /// This changes when a new file has been assigned, if you need to know when this changes
        /// use the ImageLoadingComplete event.
        /// </summary>
        public string Identifier {
            get { return _identifier; }
        } 
        string _identifier = null;


        /// <summary>
        /// The filename of the image backing this resource. Reassign to change textures.
        /// </summary>
        public string Filename {
            get {
                return _filename;
            }

            set {
                Thread newThread = new Thread(new ParameterizedThreadStart(setFilenameWorker));
                newThread.Name = "AsyncImageResource.setFilenameWorker";
                newThread.Start(value);
            }
        }
        string _filename = null;

        // Unloads the previous file and sets a new filename. 
        private void setFilenameWorker(object newFilenameObj) {
            int localToken = ++pendingToken;
            string oldFilename = _filename;

            // check if another thread has locked for loading
            bool loading = Monitor.TryEnter(loadingLock);
            if (loading) Monitor.Exit(loadingLock);

            // if a loading action is in progress or another thread is waiting, we wait too
            if (loading || threadsWaiting > 0) {
                threadsWaiting++;
                for (int i = 0; i < 5; i++) {
                    Thread.Sleep(_delay / 5);
                    if (localToken < pendingToken)
                        return;
                }
                threadsWaiting--;
            }

            lock (loadingLock) {
                if (localToken < pendingToken) 
                    return;

                // type cast and clean our filename
                string newFilename = (string)newFilenameObj;
                if (newFilename != null && newFilename.Trim().Length == 0)
                    newFilename = null;
                else
                    newFilename = newFilename.Trim();

                // if there is no change, quit
                if (_filename != null && _filename.Equals(newFilename))
                    return;

                // load the new resource then assign it to our property
                string newIdentifier = loadResource(newFilename);

                // check if we have a new loading action pending, if so just quit
                if (localToken < pendingToken) {
                    unloadResource(newFilename);
                    return;
                }

                // update MediaPortal about the image change
                _identifier = newIdentifier;
                _filename = newFilename;
                writeProperty();

                // notify any listeners a resource has been loaded
                if (ImageLoadingComplete != null)
                    ImageLoadingComplete(this);
            }

            // wait a few seconds in case we want to quickly reload the previous resource
            // if it's not reassigned, unload from memory.
            Thread.Sleep(5000);
            lock (loadingLock) {
                if (_filename != oldFilename)
                    unloadResource(oldFilename);
            }
        }


        /// <summary>
        /// Loads the given file into memory and registers it with MediaPortal.
        /// </summary>
        /// <param name="filename">The image file to be loaded.</param>
        /// <returns>The resource identifier used by MediaPortal.</returns>
        private string loadResource(string filename) {
            if (!_active || filename == null || !File.Exists(filename))
                return null;

            string identifier = getIdentifier(filename);

            // check if MP already has this image loaded
            if (GUITextureManager.LoadFromMemory(null, identifier, 0, 0, 0) > 0) {
                return identifier;
            }

            // load image ourselves and pass to MediaPortal
            Image image = LoadImageFastFromFile(filename);
            if (GUITextureManager.LoadFromMemory(image, identifier, 0, 0, 0) > 0) {
                return identifier;
            }

            return null;
        }

        /// <summary>
        /// If previously loaded, unloads the resource from memory and removes it 
        /// from the MediaPortal GUITextureManager.
        /// </summary>
        private void unloadResource(string filename) {

            if (filename == null)
                return;

            GUITextureManager.ReleaseTexture(getIdentifier(filename));
        }

        private string getIdentifier(string filename) {
            return "[MovingPictures:" + filename.GetHashCode() + "]";
        }

        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
        private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

        // Loads an Image from a File by invoking GDI Plus instead of using build-in 
        // .NET methods, or falls back to Image.FromFile. GDI Plus should be faster.
        public static Image LoadImageFastFromFile(string filename) {
            IntPtr imagePtr = IntPtr.Zero;
            Image image = null;

            try {
                if (GdipLoadImageFromFile(filename, out imagePtr) != 0) {
                    logger.Warn("gdiplus.dll method failed. Will degrade performance.");
                    image = Image.FromFile(filename);
                }

                else 
                    image = (Image)typeof(Bitmap).InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { imagePtr });
            }
            catch (Exception) {
                logger.Error("Failed to load image from " + filename);
                image = null;
            }

            return image;

        }

    }
}
