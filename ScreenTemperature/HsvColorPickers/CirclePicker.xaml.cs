using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace HSVColorPickers
{
    /// <summary>
    /// Pick a color in the circle hue wheel.
    /// </summary>
    public sealed partial class CirclePicker : UserControl, IColorPicker, IHSVPicker, INotifyPropertyChanged
    {

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        #region Helpher


        private sealed class CircleSize
        {
            public static float VectorToH(Vector2 vector) => ((((float)Math.Atan2(vector.Y, vector.X)) * 180.0f / (float)Math.PI) + 360.0f) % 360.0f;
            public static Vector2 HSToVector(float h, float s, float radio, Vector2 center) => new Vector2((float)Math.Cos(h), (float)Math.Sin(h)) * radio * s / 100.0f + center;
            public static float VectorToS(Vector2 vector, float radio)
            {
                float s = vector.Length() / radio;
                if (s < 0) return 0.0f;
                if (s > 1) return 100.0f;
                return s * 100.0f;
            }
        }


        private static class HueWheelHelpher
        {
            public static Task<Bitmap> CreateHueCircle(int sizeX, int sizeY)
            {
                return HueWheelHelpher.FillBitmap(sizeX, sizeY, (x, y) =>
                {
                    return HueWheelHelpher.CalcWheelColor(x, y);
                });
            }

            public static async Task<Bitmap> FillBitmap(int sizeX, int sizeY, Func<float, float, Color> fillPixel)
            {
                var bmp = new Bitmap(sizeX, sizeY);

                int width = bmp.Width;
                int height = bmp.Height;
                await Task.Run(() =>
                {
                    for (int y = 0; y < width; y++)
                    {
                        for (int x = 0; x < height; x++)
                        {
                            var color = fillPixel((float)x / width, (float)y / height);

                            bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
                        }
                    }
                });

                return bmp;
            }

            public static Color CalcWheelColor(float x, float y)
            {
                x = x - 0.5f;
                y = y - 0.5f;

                float saturation = 200.0f * (float)Math.Sqrt(x * x + y * y);
                if (saturation > 100.0f) saturation = 100.0f;

                float hue = y < 0 ?
                    (float)(Math.Atan2(-y, -x) / Math.PI) * 180.0f + 180.0f :
                    (float)(Math.Atan2(y, x) / Math.PI) * 180.0f;

                return HSV.HSVtoRGB(255, hue, saturation, 100.0f);
            }
        }


        #endregion


        //@Delegate
        /// <summary> Occurs when the color value changed. </summary>
        public event ColorChangeHandler ColorChanged;
        /// <summary> Occurs when the color change starts. </summary>
        public event ColorChangeHandler ColorChangeStarted;
        /// <summary> Occurs when color change. </summary>
        public event ColorChangeHandler ColorChangeDelta;
        /// <summary> Occurs when the color change is complete. </summary>
        public event ColorChangeHandler ColorChangeCompleted;
        /// <summary> Occurs when the hsv value changed. </summary>
        public event HSVChangeHandler HSVChanged;
        /// <summary> Occurs when the hsv change starts. </summary>
        public event HSVChangeHandler HSVChangeStarted;
        /// <summary> Occurs when hsv change. </summary>
        public event HSVChangeHandler HSVChangeDelta;
        /// <summary> Occurs when the hsv change is complete. </summary>
        public event HSVChangeHandler HSVChangeCompleted;


        /// <summary> Gets picker's type name. </summary>
        public string Type => "Circle";
        /// <summary> Gets picker self. </summary>
        public Control Self => this;

        /// <summary> Gets or sets picker's color. </summary>
        //public Color Color
        //{
        //    get => HSV.HSVtoRGB(this.HSV);
        //    set => this.HSV = HSV.RGBtoHSV(value);
        //}

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set 
            {
                this.HSV = HSV.RGBtoHSV(value);
                SetValue(ColorProperty, value); 
            }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(CirclePicker), new PropertyMetadata(Colors.White, OnColorChanged));

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue != null)
                ((CirclePicker)d).Color = (Color)e.NewValue;
        }


        #region Color


        /// <summary> Gets or sets picker's hsv. </summary>
        public HSV HSV
        {
            get => this.hsv;
            set
            {
                byte A = value.A;
                float H = value.H;
                float S = value.S;
                float V = value.V;

                this.UpdateColor(HSV.HSVtoRGB(value));
                this.UpdateThumb(H, S);

                this.hsv = value;
            }
        }


        private HSV hsv = new HSV(255, 0, 100, 100);
        private HSV _HSV
        {
            get => this.hsv;
            set
            {
                this.ColorChanged?.Invoke(this, HSV.HSVtoRGB(value));//Delegate
                this.HSVChanged?.Invoke(this, value);//Delegate

                this.hsv = value;
            }
        }


        #endregion

        //Circle
        Vector2 _center;// Circle's center
        float _radio;// Circle's radio
        float _maxRadio;

        //Manipulation
        Vector2 _position;

        //@Construct
        /// <summary>
        /// Construct a CirclePicker.
        /// </summary>
        public CirclePicker()
        {
            this.InitializeComponent();
            this.Loaded += async (s, e) =>
            {
                float width = (float)this.RootGrid.ActualWidth;
                float height = (float)this.RootGrid.ActualHeight;

                this._center = new Vector2(width, height) / 2;

                float radio = Math.Min(width, height) / 2;
                this._radio = radio;

                int size = (int)(radio * 2);

                await this.UpdateImage(size);
                this.UpdateEllipse(size);

                if (Color == null)
                    this.hsv = HSV.RGBtoHSV(Colors.White);
                else
                    this.hsv = HSV.RGBtoHSV(Color);

                this.Change(this.hsv);
            };

            //Image
            this.RootGrid.SizeChanged += async (s, e) =>
            {
                if (e.NewSize == e.PreviousSize) return;
                float width = (float)e.NewSize.Width;
                float height = (float)e.NewSize.Height;

                this._center = new Vector2(width, height) / 2;

                float radio = Math.Min(width, height) / 2;
                this._radio = radio;

                int size = (int)(radio * 2);

                if (this._maxRadio + 30 < radio)//30 cache width, for performance optimization.
                {
                    this._maxRadio = radio;
                    await this.UpdateImage(size);
                }

                this.UpdateEllipse(size);
                this.UpdateThumb(this.hsv.H, this.hsv.S);
            };
            
            //Manipulation
            this.Canvas.MouseLeftButtonDown += (s, e) =>
            {
                var position = e.GetPosition(s as Canvas);
                this._position = new Vector2((float)position.X, (float)position.Y) - this._center;
                this._HSV = this.Change(this._position);
            };
            this.Canvas.MouseMove += (s, e) =>
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
                    return;

                var delta = e.GetPosition(s as Canvas) - new System.Windows.Point(this._position.X, this._position.Y);
                this._position += new Vector2((float)delta.X, (float)delta.Y) - this._center;
                this._HSV = this.Change(this._position);
            };
        }


        #region Change


        private HSV Change(Vector2 position)
        {
            float H = CircleSize.VectorToH( position);
            float S = CircleSize.VectorToS( position, this._radio);
            HSV hsv = new HSV(255,H,S,this.hsv.V);

            this.Change(hsv);
            return hsv;
        }
        private HSV Change(float value)
        {
            float V = value;
            HSV hsv = new HSV(255, this.hsv.H, this.hsv.S, V);

            this.Change(hsv);
            return hsv;
        }
        private void Change(HSV hsv)
        {
            Color = HSV.HSVtoRGB(hsv);
            this.UpdateColor(Color);
            this.UpdateThumb(hsv.H, hsv.S);
        }


        #endregion

        private SolidColorBrush _solidColorBrush = new SolidColorBrush(Colors.White);

        public SolidColorBrush SolidColorBrush
        {
            get { return _solidColorBrush; }
            set
            {
                _solidColorBrush = value;
                NotifyPropertyChanged(nameof(SolidColorBrush));
            }
        }

        private ImageBrush _imageBrush = new ImageBrush();

        public ImageBrush ImageBrush
        {
            get { return _imageBrush; }
            set
            {
                _imageBrush = value;
                NotifyPropertyChanged(nameof(ImageBrush));
            }
        }

        #region Update

        private void UpdateColor(Color color)=>    SolidColorBrush.Color = color;
        private void UpdateThumb(float H, float S)
        {
            Vector2 wheel = CircleSize.HSToVector((float)((H + 360.0) * Math.PI / 180.0), S, this._radio, this._center);
            Thumb thumb = this.HSThumb;
            Canvas.SetLeft(thumb, wheel.X - thumb.ActualWidth / 2);
            Canvas.SetTop(thumb, wheel.Y - thumb.ActualHeight / 2);
        }

        private void UpdateEllipse(int size) => this.HSEllipse.Width = this.HSEllipse.Height = size;
        private async Task UpdateImage(int size)
        {
            if (size < 32) return;

            try
            {
                var bmp = await HueWheelHelpher.CreateHueCircle(size, size);
                ImageBrush.ImageSource = ImageSourceFromBitmap(bmp);
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Implémentation INotifyPropertyChanged

        //INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}