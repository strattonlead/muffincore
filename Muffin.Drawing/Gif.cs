using System.Drawing;
using System.Drawing.Imaging;

namespace Muffin.Drawing
{
    public class Gif
    {
        public static void FromImages(IEnumerable<string> paths, Stream outputStream, int delay = 33)
        {
            var images = paths.Select(x => Image.FromFile(x));
            FromImages(images, outputStream, delay);
        }

        public static void FromImages(IEnumerable<Stream> streams, Stream outputStream, int delay = 33)
        {
            var images = streams.Select(x => Image.FromStream(x));
            FromImages(images, outputStream, delay);
        }

        public static void FromImages(IEnumerable<Image> images, Stream outputStream, int delay = 33)
        {
            #region header Defs

            var buf2 = new byte[19];
            var buf3 = new byte[8];
            buf2[0] = 33;  //extension introducer
            buf2[1] = 255; //application extension
            buf2[2] = 11;  //size of block
            buf2[3] = 78;  //N
            buf2[4] = 69;  //E
            buf2[5] = 84;  //T
            buf2[6] = 83;  //S
            buf2[7] = 67;  //C
            buf2[8] = 65;  //A
            buf2[9] = 80;  //P
            buf2[10] = 69; //E
            buf2[11] = 50; //2
            buf2[12] = 46; //.
            buf2[13] = 48; //0
            buf2[14] = 3;  //Size of block
            buf2[15] = 1;  //
            buf2[16] = 0;  //
            buf2[17] = 0;  //
            buf2[18] = 0;  //Block terminator
            buf3[0] = 33;  //Extension introducer
            buf3[1] = 249; //Graphic control extension
            buf3[2] = 4;   //Size of block
            buf3[3] = 9;   //Flags: reserved, disposal method, user input, transparent color
            // https://github.com/mrousavy/AnimatedGif/blob/master/AnimatedGif/AnimatedGifCreator.cs @ CreateGraphicsControlExtensionBlock()
            buf3[4] = (byte)(delay / 10 % 0x100);  //Delay time low byte
            buf3[5] = (byte)(delay / 10 / 0x100);   //Delay time high byte
            buf3[6] = 255; //Transparent color index
            buf3[7] = 0;   //Block terminator

            #endregion

            using (var binaryWriter = new BinaryWriter(outputStream))
            {


                var firstTime = true;
                foreach (var image in images)
                {
                    using (var mem = new MemoryStream())
                    {
                        image.Save(mem, ImageFormat.Gif);
                        var buf1 = mem.ToArray();

                        if (firstTime)
                        {
                            firstTime = false;

                            binaryWriter.Write(buf1, 0, 781); //Header & global color table
                            binaryWriter.Write(buf2, 0, 19); //Application extension
                        }

                        binaryWriter.Write(buf3, 0, 8); //Graphic extension
                        binaryWriter.Write(buf1, 789, buf1.Length - 790); //Image data
                    }
                }

                binaryWriter.Write(";"); //Image terminator
            }
        }
    }
}