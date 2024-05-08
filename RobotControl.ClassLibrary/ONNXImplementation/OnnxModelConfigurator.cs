using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML.Transforms.Onnx;

namespace RobotControl.ClassLibrary.ONNXImplementation
{
    internal class OnnxModelConfigurator
    {
        private readonly MLContext    _mlContext = new MLContext();
        private readonly ITransformer _mlModel;
        private readonly IDataView    _dataView;

        public OnnxModelConfigurator(IOnnxModel onnxModel, int GPUDeviceId)
        {
            var onnxOptions = new OnnxOptions
            {
                FallbackToCpu     = true,
                GpuDeviceId       = GPUDeviceId,
                ModelFile         = onnxModel.ModelPath, 
                InputColumns      = new string[] { onnxModel.ModelInput},
                OutputColumns     = new string[] { onnxModel.ModelOutput},
                InterOpNumThreads = Environment.ProcessorCount / 2,
                IntraOpNumThreads = Environment.ProcessorCount / 2,
            };

            _dataView = _mlContext.Data.LoadFromEnumerable(new List<ImageInputData>());
            _mlModel  = SetupMlNetModel(onnxModel, onnxOptions);
        }

        private ITransformer SetupMlNetModel(IOnnxModel onnxModel, OnnxOptions onnxOptions) =>
            _mlContext
                .Transforms
                    .ResizeImages(
                        resizing:          ImageResizingEstimator.ResizingKind.Fill,
                        outputColumnName:  onnxModel.ModelInput,
                        imageWidth:        ImageSettings.imageWidth,
                        imageHeight:       ImageSettings.imageHeight,
                        inputColumnName:   nameof(ImageInputData.Image))

                    .Append(_mlContext.Transforms.ExtractPixels(
                        outputColumnName:  onnxModel.ModelInput))

                    .Append(_mlContext.Transforms.ApplyOnnxModel(
                        options: onnxOptions))

                    .Fit(
                        input: _dataView);

        public PredictionEngine<ImageInputData, T> GetMlNetPredictionEngine<T>()
            where T : class, IOnnxObjectPrediction, new() => _mlContext.Model.CreatePredictionEngine<ImageInputData, T>(_mlModel);

        public void SaveMLNetModel(string mlnetModelFilePath) => _mlContext.Model.Save(_mlModel, null, mlnetModelFilePath);
    }
}
