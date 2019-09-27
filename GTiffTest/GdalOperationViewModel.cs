//  ****************************************************************************
//  Ranplan Wireless Network Design Ltd.
//  __________________
//   All Rights Reserved. [2019]
// 
//  NOTICE:
//  All information contained herein is, and remains the property of
//  Ranplan Wireless Network Design Ltd. and its suppliers, if any.
//  The intellectual and technical concepts contained herein are proprietary
//  to Ranplan Wireless Network Design Ltd. and its suppliers and may be
//  covered by U.S. and Foreign Patents, patents in process, and are protected
//  by trade secret or copyright law.
//  Dissemination of this information or reproduction of this material
//  is strictly forbidden unless prior written permission is obtained
//  from Ranplan Wireless Network Design Ltd.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using GTiffTest.Commands;
using OSGeo.GDAL;
using RanOpt.iBuilding.Common.UI;

namespace GTiffTest
{
    public class GdalOperationViewModel : INotifyPropertyChanged
    {
        private string _message;
        private string _fileName;
        private string _compress;
        private bool _enablePredictor;
        private string _predictor = "1";
        private string _zLevel = "6";
        private bool _enableZLevel;
        public IEnumerable<string> FileList => new[] {"Small.tif"};

        public string Message
        {
            get => _message;
            set => PropertyChanged.RaiseIfChanged(this, ref _message, value, nameof(Message));
        }

        public string FileName
        {
            get => _fileName;
            set => PropertyChanged.RaiseIfChanged(this, ref _fileName, value, nameof(FileName));
        }

        public IEnumerable<string> CompressList => CompressMode.GetAll().ToList();

        public string Compress
        {
            get => _compress;
            set
            {
                if (PropertyChanged.RaiseIfChanged(this, ref _compress, value, nameof(Compress)))
                {
                    EnablePredictor = new[] {CompressMode.Lzw, CompressMode.Deflate, CompressMode.Zstd}.Contains(_compress);
                    EnableZLevel = new[] {CompressMode.Deflate, CompressMode.LercDeflate}.Contains(_compress);
                }
            }
        }

        public bool EnablePredictor
        {
            get => _enablePredictor;
            private set => PropertyChanged.RaiseIfChanged(this, ref _enablePredictor, value, nameof(EnablePredictor));
        }

        public IEnumerable<string> PredictorList => new[] {"1", "2", "3"};

        public string Predictor
        {
            get => _predictor;
            set => PropertyChanged.RaiseIfChanged(this, ref _predictor, value, nameof(Predictor));
        }

        public bool EnableZLevel
        {
            get => _enableZLevel;
            set => PropertyChanged.RaiseIfChanged(this, ref _enableZLevel, value, nameof(EnableZLevel));
        }

        public IEnumerable<string> ZLevelList => Enumerable.Range(1, 9).Select(i => i.ToString());

        public string ZLevel
        {
            get => _zLevel;
            set => PropertyChanged.RaiseIfChanged(this, ref _zLevel, value, nameof(ZLevel));
        }

        public ICommand NewCommand { get; }
        public ICommand UpdateCommand { get; }

        public ICommand GetDriverCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public GdalOperationViewModel()
        {
            FileName = FileList.First();
            Compress = CompressList.ElementAt(3);

            NewCommand = new RelayCommand(New);
            UpdateCommand = new RelayCommand(Update);
            GetDriverCommand = new RelayCommand(GetDriver);
        }

        private void GetDriver()
        {
            var filePath = GetFilePath();
            using (var dataSet = Gdal.Open(filePath, Access.GA_ReadOnly))
            {
                var driver = dataSet.GetDriver();
                AppendMessage($"Get driver, ShortName={driver.ShortName}, LongName={driver.LongName}");
            }
        }

        private void Update()
        {
            var filePath = GetFilePath();
            var dataSet = Gdal.Open(filePath, Access.GA_Update);

            var driver = dataSet.GetDriver();

            var driver1 = Gdal.GetDriverByName("GTiff");
        }

        private void New()
        {
            try
            {
                var filePath = System.IO.Path.Combine(GetDirectory(), Compress + "_" + System.IO.Path.GetRandomFileName());
                filePath = System.IO.Path.ChangeExtension(filePath, ".tiff");

                var driver = Gdal.GetDriverByName("GTiff");
                var dataSet = driver.Create(filePath, 4000, 4000, 200, DataType.GDT_Float64, GetOptions().ToArray());

                //            var data = Enumerable.Repeat(10f, dataSet.RasterXSize * dataSet.RasterYSize * dataSet.RasterCount).ToArray();
                //            var error = dataSet.WriteRaster(0, 0, dataSet.RasterXSize, dataSet.RasterYSize, data, dataSet.RasterXSize, dataSet.RasterYSize, 10, Enumerable.Range(1, 10).ToArray(), 0, 0, 0);
                //
                //            // data1 should equals data
                //            var data1 = new float[dataSet.RasterXSize * dataSet.RasterYSize * dataSet.RasterCount];
                //            var error1 = dataSet.ReadRaster(0, 0, dataSet.RasterXSize, dataSet.RasterYSize, data1, dataSet.RasterXSize, dataSet.RasterYSize, 10, Enumerable.Range(1, 10).ToArray(), 0, 0, 0);

                dataSet.FlushCache();
                dataSet.Dispose();

                AppendMessage($"{Compress} succeed, FileSize={new System.IO.FileInfo(filePath).Length}");
            }
            catch (Exception e)
            {
                AppendMessage($"{Compress} failed, Message={e.Message}");
            }
        }

        private IEnumerable<string> GetOptions()
        {
            yield return "TILED=YES";

            yield return $"COMPRESS={Compress}";

            if (EnablePredictor)
                yield return $"PREDICTOR={Predictor}";

            if (EnableZLevel)
                yield return $"ZLEVEL={ZLevel}";
        }

        private void AppendMessage(string message)
        {
            var newLine = string.IsNullOrEmpty(Message) ? string.Empty : Environment.NewLine;
            Message += $"{newLine}{DateTime.Now:yyyy-MM-dd hh:mm}  {message}";
        }

        private string GetFilePath()
        {
            return System.IO.Path.Combine(GetDirectory(), FileName);
        }

        private string GetDirectory()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }

    public class CompressMode
    {
        public const string Jpeg = "JPEG";
        public const string Lzw = "LZW";
        public const string Packbits = "PACKBITS";
        public const string Deflate = "DEFLATE";
        public const string Ccittrle = "CCITTRLE";
        public const string Ccittfax3 = "CCITTFAX3";
        public const string Ccittfax4 = "CCITTFAX4";
        public const string Lzma = "LZMA";
        public const string Zstd = "ZSTD";
        public const string Lerc = "LERC";
        public const string LercDeflate = "LERC_DEFLATE";
        public const string LercZstd = "LERC_ZSTD";
        public const string Webp = "WEBP";
        public const string None = "NONE";

        public static IEnumerable<string> GetAll()
        {
            return typeof(CompressMode).GetFields().Select(fieldInfo => fieldInfo.GetValue(null).ToString());
        }
    }
}