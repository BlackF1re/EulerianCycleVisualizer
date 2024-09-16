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

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, IsAxisVisible = false });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, IsAxisVisible = false });
            GraphPlot.Model = plotModel;
        }

        private void InitializeMatrix()
        {
            adjacencyMatrix = new int[3, 3];
            UpdateMatrixGrid();
        }

        private void UpdateMatrixGrid()
        {
            MatrixDataGrid.Columns.Clear();

            for (int i = 0; i < adjacencyMatrix.GetLength(0); i++)
            {
                MatrixDataGrid.Columns.Add(new DataGridTemplateColumn
                {
                    Header = (i + 1).ToString(),
                    CellTemplate = CreateCellTemplate(i)
                });
            }

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

            double centerX = GraphPlot.ActualWidth / 2;
            double centerY = GraphPlot.ActualHeight / 2;

            for (int i = 0; i < numVertices; i++)
            {
                double angle = i * angleStep;
                double x = centerX + NodeRadius * Math.Cos(angle);
                double y = centerY + NodeRadius * Math.Sin(angle);
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

            // Отрисовка рёбер между вершинами
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
                        plotModel.Series.Add(edgeSeries);
                    }
                }
            }

            GraphPlot.InvalidatePlot(true);
        }

        private void FindEulerianCycle_Click(object sender, RoutedEventArgs e)
        {
            if (!graphBuilt) return;

            var cycle = new List<int>();
            var stack = new Stack<int>();
            var tempAdjMatrix = new int[adjacencyMatrix.GetLength(0), adjacencyMatrix.GetLength(1)];
            Array.Copy(adjacencyMatrix, tempAdjMatrix, adjacencyMatrix.Length);

            stack.Push(0);

            while (stack.Count > 0)
            {
                int v = stack.Peek();
                bool hasEdges = false;

                for (int u = 0; u < tempAdjMatrix.GetLength(1); u++)
                {
                    if (tempAdjMatrix[v, u] > 0)
                    {
                        stack.Push(u);
                        tempAdjMatrix[v, u] = tempAdjMatrix[u, v] = 0;
                        hasEdges = true;
                        break;
                    }
                }

                if (!hasEdges)
                {
                    cycle.Add(v);
                    stack.Pop();
                }
            }

            foreach (var point in cycle)
            {
                MessageBox.Show($"Вершина: {point + 1}");
            }
        }
    }
}
