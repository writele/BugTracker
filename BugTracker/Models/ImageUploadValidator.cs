using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public static class ImageUploadValidator
    {
        public static bool IsWebFriendly(HttpPostedFileBase file)
        {
            //check for actual object
            if (file == null)
            {
                return false;
            }

            if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 1024)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public static bool IsImage(HttpPostedFileBase file)
        {

            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    return ImageFormat.Jpeg.Equals(img.RawFormat) ||
                           ImageFormat.Png.Equals(img.RawFormat) ||
                           ImageFormat.Gif.Equals(img.RawFormat);
                }
            }
            catch
            {
                return false;
            }

        }
    }
}