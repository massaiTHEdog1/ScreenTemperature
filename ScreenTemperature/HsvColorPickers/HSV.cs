using System;
using System.Windows.Media;

namespace HSVColorPickers
{
    /// <summary> 
    /// HSV (Hue, Saturation, Value).
    /// </summary>
    public struct HSV
    {

        /// <summary> Alpha </summary>
        public byte A;

        /// <summary> Hue </summary>
        public float H
        {
            get => this.h;
            set
            {
                if (value < 0) this.h = 0;
                else if (value > 360) this.h = 360;
                else this.h = value;
            }
        }
        private float h;

        /// <summary> Saturation </summary>
        public float S
        {
            get => this.s;
            set
            {
                if (value < 0) this.s = 0;
                else if (value > 100) this.s = 100;
                else this.s = value;
            }
        }
        private float s;

        /// <summary> Value </summary>
        public float V
        {
            get => this.v;
            set
            {
                if (value < 0) this.v = 0;
                else if (value > 100) this.v = 100;
                else this.v = value;
            }
        }
        private float v;


        //@Construct
        /// <summary>
        /// Construct a HSV.
        /// </summary>
        /// <param name="a"> Alpha </param>
        /// <param name="h"> hue </param>
        /// <param name="s"> Saturation </param>
        /// <param name="v"> Value </param>   
        public HSV(byte a, float h, float s, float v)
        {
            this.A = a;
            this.h = h;
            this.s = v;
            this.v = v;
            this.H = H;
            this.S = s;
            this.V = v;
        }



        /// <summary>
        /// HSV to Color.
        /// </summary>
        /// <param name="h"> Hue </param>
        /// <returns> Color </returns>
        public static Color HSVtoRGB(float h)
        {
            float hh = h / 60;
            byte xhh = (byte)((1 - Math.Abs(hh % 2 - 1)) * 255);

            if (hh < 1) return Color.FromArgb(255, 255, xhh, 0);
            else if (hh < 2) return Color.FromArgb(255, xhh, 255, 0);
            else if (hh < 3) return Color.FromArgb(255, 0, 255, xhh);
            else if (hh < 4) return Color.FromArgb(255, 0, xhh, 255);
            else if (hh < 5) return Color.FromArgb(255, xhh, 0, 255);
            else return Color.FromArgb(255, 255, 0, xhh);
        }

        /// <summary>
        /// Color to HSV.
        /// </summary>
        /// <param name="hsv"> HSV </param>
        /// <returns> Color </returns>
        public static Color HSVtoRGB(HSV hsv) => HSV.HSVtoRGB(hsv.A, hsv.H, hsv.S, hsv.V);

        /// <summary> 
        /// Color to HSV.
        /// </summary>
        /// <param name="a"> Alpha </param>
        /// <param name="h"> Hue </param>
        /// <param name="s"> Saturation </param>
        /// <param name="v"> Value </param>
        /// <returns> Color </returns>
        public static Color HSVtoRGB(byte a, float h, float s, float v)
        {
            if (h == 360) h = 0;

            if (s == 0)
            {
                byte ll = (byte)(v / 100 * 255);
                return Color.FromArgb(a, ll, ll, ll);
            }

            float S = s / 100;
            float V = v / 100;

            int H1 = (int)(h * 1.0 / 60);
            float F = h / 60 - H1;
            float P = V * (1.0f - S);
            float Q = V * (1.0f - F * S);
            float T = V * (1.0f - (1.0f - F) * S);

            float R = 0, G = 0, B = 0;
            switch (H1)
            {
                case 0: R = V; G = T; B = P; break;
                case 1: R = Q; G = V; B = P; break;
                case 2: R = P; G = V; B = T; break;
                case 3: R = P; G = Q; B = V; break;
                case 4: R = T; G = P; B = V; break;
                case 5: R = V; G = P; B = Q; break;
            }

            R = R * 255;
            while (R > 255) R -= 255;
            while (R < 0) R += 255;

            G = G * 255;
            while (G > 255) G -= 255;
            while (G < 0) G += 255;

            B = B * 255;
            while (B > 255) B -= 255;
            while (B < 0) B += 255;

            return Color.FromArgb(a, (byte)R, (byte)G, (byte)B);
        }



        /// <summary> 
        /// Color to HSV.
        /// </summary>
        /// <param name="color"> Color </param>
        /// <returns> HSV </returns>
        public static HSV RGBtoHSV(Color color) => HSV.RGBtoHSV(color.A, color.R, color.G, color.B);

        /// <summary> 
        /// Color to HSV.
        /// </summary>
        /// <param name="a"> Alpha </param>
        /// <param name="r"> Red </param>
        /// <param name="g"> Green </param>
        /// <param name="b"> Blue </param>
        /// <returns> HSV </returns>
        public static HSV RGBtoHSV(byte a, byte r, byte g, byte b)
        {
            float R = r * 1.0f / 255;
            float G = g * 1.0f / 255;
            float B = b * 1.0f / 255;

            float min = Math.Min(Math.Min(R, G), B);
            float max = Math.Max(Math.Max(R, G), B);

            float H = 0, S, V;

            if (max == min) { H = 0; }

            else if (max == R && G > B) H = 60 * (G - B) * 1.0f / (max - min) + 0;
            else if (max == R && G < B) H = 60 * (G - B) * 1.0f / (max - min) + 360;
            else if (max == G) H = H = 60 * (B - R) * 1.0f / (max - min) + 120;
            else if (max == B) H = H = 60 * (R - G) * 1.0f / (max - min) + 240;

            if (max == 0) S = 0;
            else S = (max - min) * 1.0f / max;

            V = max;

            return new HSV(a, H, (S * 100), V * 100);
        }
    }
}