﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RobotControl.ClassLibrary.ONNXImplementation
{
    internal class OnnxOutputParser
    {
        class BoundingBoxPrediction : BoundingBoxDimensions
        {
            public float Confidence { get; set; }
        }

        // The number of rows and columns in the grid the image is divided into.
        public const int ROW_COUNT = 13, COLUMN_COUNT = 13;

        // The number of features contained within a box (x, y, height, width, confidence).
        public const int FEATURES_PER_BOX = 5;

        // Labels corresponding to the classes the onnx model can predict. For example, the
        // Tiny YOLOv2 model included with this sample is trained to predict 20 different classes.
        private readonly string[] _classLabels;

        // Predetermined anchor offsets for the bounding boxes in a cell.
        private readonly (float x, float y)[] _boxAnchors;
        private const float _cellWidth = ImageSettings.imageWidth / COLUMN_COUNT;
        private const float _cellHeight = ImageSettings.imageHeight / ROW_COUNT;

        public OnnxOutputParser(IOnnxModel onnxModel)
        {
            _classLabels = onnxModel.Labels;
            _boxAnchors  = onnxModel.Anchors;
        }

        // Applies the sigmoid function that outputs a number between 0 and 1.
        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);
            return k / (1.0f + k);
        }

        // Normalizes an input vector into a probability distribution.
        private float[] Softmax(float[] classProbabilities)
        {
            var max = classProbabilities.Max();
            var exp = classProbabilities.Select(v => (float)Math.Exp(v - max));
            var sum = exp.Sum();
            return exp.Select(v => v / sum).ToArray();
        }

        // Onnx outputst a tensor that has a shape of (for Tiny YOLOv2) 125x13x13. ML.NET flattens
        // this multi-dimensional into a one-dimensional array. This method allows us to access a
        // specific channel for a givin (x,y) cell position by calculating the offset into the array.
        private int GetOffset(int row, int column, int channel) => (channel * ROW_COUNT * COLUMN_COUNT) + (column * COLUMN_COUNT) + row;

        // Extracts the bounding box features (x, y, height, width, confidence) method from the model
        // output. The confidence value states how sure the model is that it has detected an object.
        // We use the Sigmoid function to turn it that confidence into a percentage.
        private BoundingBoxPrediction ExtractBoundingBoxPrediction(float[] modelOutput, int row, int column, int channel) =>
            new BoundingBoxPrediction
            {
                X = modelOutput[GetOffset(row, column, channel++)],
                Y = modelOutput[GetOffset(row, column, channel++)],
                Width = modelOutput[GetOffset(row, column, channel++)],
                Height = modelOutput[GetOffset(row, column, channel++)],
                Confidence = Sigmoid(modelOutput[GetOffset(row, column, channel++)])
            };

        // The predicted x and y coordinates are relative to the location of the grid cell; we use
        // the logistic sigmoid to constrain these coordinates to the range 0 - 1. Then we add the
        // cell coordinates (0-12) and multiply by the number of pixels per grid cell (32).
        // Now x/y represent the center of the bounding box in the original 416x416 image space.
        // Additionally, the size (width, height) of the bounding box is predicted relative to the
        // size of an "anchor" box. So we transform the width/weight into the original 416x416 image space.
        private BoundingBoxDimensions MapBoundingBoxToCell(int row, int column, int box, BoundingBoxPrediction boxDimensions)
        {
            var mappedBox = new BoundingBoxDimensions
            {
                X = (row + Sigmoid(boxDimensions.X)) * _cellWidth,
                Y = (column + Sigmoid(boxDimensions.Y)) * _cellHeight,
                Width = (float)Math.Exp(boxDimensions.Width) * _cellWidth * _boxAnchors[box].x,
                Height = (float)Math.Exp(boxDimensions.Height) * _cellHeight * _boxAnchors[box].y,
            };

            // The x,y coordinates from the (mapped) bounding box prediction represent the center
            // of the bounding box. We adjust them here to represent the top left corner.
            mappedBox.X -= mappedBox.Width / 2;
            mappedBox.Y -= mappedBox.Height / 2;

            return mappedBox;
        }

        // Extracts the class predictions for the bounding box from the model output using the
        // GetOffset method and turns them into a probability distribution using the Softmax method.
        public float[] ExtractClassProbabilities(float[] modelOutput, int row, int column, int channel, float confidence)
        {
            var classProbabilitiesOffset = channel + FEATURES_PER_BOX;
            float[] classProbabilities = new float[_classLabels.Length];
            for (int classProbability = 0; classProbability < _classLabels.Length; classProbability++)
                classProbabilities[classProbability] = modelOutput[GetOffset(row, column, classProbability + classProbabilitiesOffset)];
            return Softmax(classProbabilities).Select(p => p * confidence).ToArray();
        }

        // IoU (Intersection over union) measures the overlap between 2 boundaries. We use that to
        // measure how much our predicted boundary overlaps with the ground truth (the real object
        // boundary). In some datasets, we predefine an IoU threshold (say 0.5) in classifying
        // whether the prediction is a true positive or a false positive. This method filters
        // overlapping bounding boxes with lower probabilities.
        private float IntersectionOverUnion(RectangleF boundingBoxA, RectangleF boundingBoxB)
        {
            var areaA = boundingBoxA.Width * boundingBoxA.Height;
            var areaB = boundingBoxB.Width * boundingBoxB.Height;

            if (areaA <= 0 || areaB <= 0)
                return 0;

            var minX = (float)Math.Max(boundingBoxA.Left, boundingBoxB.Left);
            var minY = (float)Math.Max(boundingBoxA.Top, boundingBoxB.Top);
            var maxX = (float)Math.Min(boundingBoxA.Right, boundingBoxB.Right);
            var maxY = (float)Math.Min(boundingBoxA.Bottom, boundingBoxB.Bottom);

            var intersectionArea = (float)Math.Max(maxY - minY, 0) * (float)Math.Max(maxX - minX, 0);

            return intersectionArea / (areaA + areaB - intersectionArea);
        }

        public List<BoundingBox> ParseOutputs(float[] modelOutput, float probabilityThreshold = .3f)
        {
            var boxes = new List<BoundingBox>();

            for (int row = 0; modelOutput != null && row < ROW_COUNT; row++)
            {
                for (int column = 0; column < COLUMN_COUNT; column++)
                {
                    for (int box = 0; box < _boxAnchors.Length; box++)
                    {
                        var channel = box * (_classLabels.Length + FEATURES_PER_BOX);

                        var boundingBoxPrediction = ExtractBoundingBoxPrediction(modelOutput, row, column, channel);

                        var mappedBoundingBox = MapBoundingBoxToCell(row, column, box, boundingBoxPrediction);

                        if (boundingBoxPrediction.Confidence < probabilityThreshold)
                            continue;

                        float[] classProbabilities = ExtractClassProbabilities(modelOutput, row, column, channel, boundingBoxPrediction.Confidence);

                        var (topProbability, topIndex) = classProbabilities.Select((probability, index) => (Score: probability, Index: index)).Max();

                        if (topProbability < probabilityThreshold)
                            continue;

                        boxes.Add(new BoundingBox
                        {
                            Dimensions = mappedBoundingBox,
                            Confidence = topProbability,
                            Label = _classLabels[topIndex]
                        });
                    }
                }
            }
            return boxes;
        }

        public List<BoundingBox> FilterBoundingBoxes(List<BoundingBox> boxes, int limit, float iouThreshold)
        {
            var results       = new List<BoundingBox>();
            var filteredBoxes = new bool[boxes.Count];
            var sortedBoxes   = boxes.OrderByDescending(b => b.Confidence).ToArray();

            for (int i = 0; i < boxes.Count; i++)
            {
                if (filteredBoxes[i])
                    continue;

                results.Add(sortedBoxes[i]);

                if (results.Count >= limit)
                    break;

                for (var j = i + 1; j < boxes.Count; j++)
                {
                    if (filteredBoxes[j])
                        continue;

                    if (IntersectionOverUnion(sortedBoxes[i].Rect, sortedBoxes[j].Rect) > iouThreshold)
                        filteredBoxes[j] = true;

                    if (filteredBoxes.Count(b => b) <= 0)
                        break;
                }
            }
            return results;
        }
    }
}
