using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using OxyPlot.Wpf;

namespace EulerianCycleVisualizer
{
    public partial class MainWindow : Window
    {
        private PlotModel plotModel;
        private List<ScatterPoint> nodes;
        private List<LineSeries> edges;
        private List<bool> visited;
        private int[,] adjacencyMatrix;
        private const double MarkerSize = 10;
        private const double NodeRadius = 100;
        private bool graphBuilt = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializePlot();
            InitializeMatrix();
        }

        private void InitializePlot()
        {
            plotModel = new PlotModel { Title = "Эйлеров цикл" };

            // Hide coordinate axes
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = 1, TickStyle = TickStyle.None });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = 1, TickStyle = TickStyle.None });
            GraphPlot.Model = plotModel;
        }

        private void InitializeMatrix()
        {
            // Initialize adjacency matrix with default values
            adjacencyMatrix = new int[3, 3];
            UpdateMatrixGrid();
        }

        private void UpdateMatrixGrid()
        {
            MatrixDataGrid.Columns.Clear();

            // Add column headers
            for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
            {
                MatrixDataGrid.Columns.Add(new DataGridTemplateColumn
                {
                    Header = (i + 1).ToString(),
                    CellTemplate = CreateCellTemplate(i)
                });
            }

            // Add row headers
            MatrixDataGrid.RowHeaderWidth = 50;
            MatrixDataGrid.RowHeaderTemplate = CreateRowHeaderTemplate();

            MatrixDataGrid.ItemsSource = GetMatrixRows();
        }

        private DataTemplate CreateCellTemplate(int columnIndex)
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding($"[{columnIndex}]"));
            template.VisualTree = factory;
            return template;
        }

        private DataTemplate CreateRowHeaderTemplate()
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetValue(TextBlock.TextProperty, "{Binding}");
            template.VisualTree = factory;
            return template;
        }

        private List<int[]> GetMatrixRows()
        {
            var rows = new List<int[]>();
            for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
            {
                var row = new int[adjacencyMatrix.GetLength(1)];
                for (int j = 0; j < adjacencyMatrix.GetLength(1); j++)
                {
                    row[j] = adjacencyMatrix[i, j];
                }
                rows.Add(row);
            }
            return rows;
        }

        private void AddRowColumn_Click(object sender, RoutedEventArgs e)
        {
            // Increase matrix size
            int newSize = adjacencyMatrix.GetLength(0) + 1;
            var newMatrix = new int[newSize, newSize];
            for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < adjacencyMatrix.GetLength(1); j++)
                {
                    newMatrix[i, j] = adjacencyMatrix[i, j];
                }
            }
            adjacencyMatrix = newMatrix;
            UpdateMatrixGrid();
        }

        private void RemoveRowColumn_Click(object sender, RoutedEventArgs e)
        {
            if (adjacencyMatrix.GetLength(0) > 1)
            {
                int newSize = adjacencyMatrix.GetLength(0) - 1;
                var newMatrix = new int[newSize, newSize];
                for (int i = 0; i < newSize; i++)
                {
                    for (int j = 0; j < newSize; j++)
                    {
                        newMatrix[i, j] = adjacencyMatrix[i, j];
                    }
                }
                adjacencyMatrix = newMatrix;
                UpdateMatrixGrid();
            }
        }

        private void RunAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for algorithm
            plotModel.Series.Clear();
            plotModel.Annotations.Clear();
            DrawGraph();
            graphBuilt = true;
            FindEulerianCycleButton.IsEnabled = true;
        }

        private void DrawGraph()
        {
            int numVertices = adjacencyMatrix.GetLength(0);
            nodes = new List<ScatterPoint>();
            edges = new List<LineSeries>();

            var vertexSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = MarkerSize,
                MarkerFill = OxyColors.Red
            };

            var vertexPositions = new Dictionary<int, (double X, double Y)>();
            double angleStep = 2 * Math.PI / numVertices;

            for (int i = 0; i < numVertices; i++)
            {
                double angle = i * angleStep;
                double x = 300 + NodeRadius * Math.Cos(angle);
                double y = 300 + NodeRadius * Math.Sin(angle);
                vertexPositions[i] = (x, y);
                vertexSeries.Points.Add(new ScatterPoint(x, y));

                var textAnnotation = new TextAnnotation
                {
                    Text = (i + 1).ToString(),
                    TextPosition = new DataPoint(x, y),
                    StrokeThickness = 0,
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                    TextVerticalAlignment = OxyPlot.VerticalAlignment.Middle
                };
                plotModel.Annotations.Add(textAnnotation);
            }

            plotModel.Series.Add(vertexSeries);

            for (int i = 0; i < numVertices; i++)
            {
                for (int j = i + 1; j < numVertices; j++)
                {
                    if (adjacencyMatrix[i, j] == 1)
                    {
                        var edgeSeries = new LineSeries
                        {
                            Color = OxyColors.Black,
                            StrokeThickness = 1
                        };
                        edgeSeries.Points.Add(new DataPoint(vertexPositions[i].X, vertexPositions[i].Y));
                        edgeSeries.Points.Add(new DataPoint(vertexPositions[j].X, vertexPositions[j].Y));
                        edges.Add(edgeSeries);
                        plotModel.Series.Add(edgeSeries);
                    }
                }
            }

            GraphPlot.InvalidatePlot(true);
        }

        private void FindEulerianCycle_Click(object sender, RoutedEventArgs e)
        {
            if (!graphBuilt) return;

            // Placeholder for Eulerian cycle algorithm
            // Implement the Eulerian cycle finding algorithm here
            // This is just a placeholder
            MessageBox.Show("Eulerian cycle algorithm implementation needed.");
        }
    }
}
