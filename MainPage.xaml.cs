using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics; // For Colors and Rect
using System;

namespace Draw
{
	public partial class MainPage : ContentPage
	{
		private Drawable _drawable;
		private float _startX, _startY, _endX, _endY, _currentX, _currentY, _strokeSize = 1;
		private Color _fillColor = Colors.White, _strokeColor = Colors.Black;

		private string? _drawType = null;

		public MainPage()
		{
			InitializeComponent();
			_drawable = new Drawable();
			Canvas.Drawable = _drawable;
			Canvas.StartInteraction += OnCanvasStartInteraction;
			Canvas.DragInteraction += OnCanvasDragInteraction;
			Canvas.EndInteraction += OnCanvasEndInteraction;
		}

		public bool OnlyHexInString(string test)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(test, @"^#([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$");
		}

		public void OnFillColorChanged(object sender, EventArgs e)
		{
			if(OnlyHexInString(FillColor.Text))
			{
				_fillColor = !string.IsNullOrEmpty(FillColor.Text) ? Color.FromHex(FillColor.Text) : Colors.White;
				FillColorFrame.BorderColor = Colors.Black;
			}else{
				FillColorFrame.BorderColor = Colors.Red;
			}
		}

		public void OnStrokeColorChanged(object sender, EventArgs e)
		{
			if(OnlyHexInString(StrokeColor.Text))
			{
				_strokeColor = !string.IsNullOrEmpty(StrokeColor.Text) ? Color.FromHex(StrokeColor.Text) : Colors.Black;
				StrokeColorFrame.BorderColor = Colors.Black;
			}else{
				StrokeColorFrame.BorderColor = Colors.Red;
			}
		}

		public void OnStrokeSizeChanged(object sender, EventArgs e)
		{
			if(float.TryParse(StrokeSize.Text, out float result))
			{
				 _strokeSize = result;
				StrokeSizeFrame.BorderColor = Colors.Black;
			}else{
				StrokeSizeFrame.BorderColor = Colors.Red;
			}
		}

		private void DrawRectangle(object sender, EventArgs e)
		{
			_drawType = "rectangle";
			ShapeLabel.Text = $"Shape: {_drawType}";
		}

		private void DrawEllipse(object sender, EventArgs e)
		{
			_drawType = "ellipse";
			ShapeLabel.Text = $"Shape: {_drawType}";
		}

		private void DrawCircle(object sender, EventArgs e)
		{
			_drawType = "circle";
			ShapeLabel.Text = $"Shape: {_drawType}";
		}

		private void DrawTriangle(object sender, EventArgs e)
		{
			_drawType = "triangle";
			ShapeLabel.Text = $"Shape: {_drawType}";
			Canvas.Invalidate();
		}

		private void DrawLine(object sender, EventArgs e)
		{
			_drawType = "line";
			ShapeLabel.Text = $"Shape: {_drawType}";
			Canvas.Invalidate();
		}

		private void DrawDragLine(object sender, EventArgs e)
		{
			_drawType = "dragLine";
			ShapeLabel.Text = $"Shape: {_drawType}";
			Canvas.Invalidate();
		}

		private void UndoLastShape(object sender, EventArgs e)
		{
			if (_drawable != null)
			{
				_drawable.RemoveLastShape();
				Canvas.Invalidate();
			}
		}

		private void ClearCanvas(object sender, EventArgs e)
		{
			_drawable.ClearList();
			Canvas.Invalidate();
		}

		private void OnCanvasStartInteraction(object? sender, TouchEventArgs e)
		{
			var x = e.Touches[0].X;
			var y = e.Touches[0].Y;
			_startX = x;
			_startY = y;
			if(_drawType == "dragLine")
			{
				_startX = x;
				_startY = y;
				_endX = x;
				_endY = y;

			}
			StartLabel.Text = $"X: {_startX} Y: {_startY}";
		}

		private void OnCanvasDragInteraction(object? sender, TouchEventArgs e)
		{

			_currentX = _endX;
			_currentY = _endY;

			var x = e.Touches[0].X;
			var y = e.Touches[0].Y;

			_endX = x;
			_endY = y;

			if(_drawType == "dragLine")
			{
				_startX = _currentX;
				_startY = _currentY;
				StartLabel.Text = $"X: {_startX} Y: {_startY}";
				OnCanvasDraw();
			}

			DragLabel.Text = $"X: {_endX} Y: {_endY}";
			EndLabel.Text = $"X: {_endX} Y: {_endY}";
		}

		private void OnCanvasEndInteraction(object? sender, TouchEventArgs e)
		{
			var x = e.Touches[0].X;
			var y = e.Touches[0].Y;

			_endX = x;
			_endY = y;

			EndLabel.Text = $"X: {_endX} Y: {_endY}";
			if(_drawType != "dragLine"){
				OnCanvasDraw();
			}
		}

		private void OnCanvasDraw()
		{

			if (_drawType == null)
			{
				return;
			}

			_drawable.AddShape(
				type: _drawType, 
				startX: _startX, 
				startY: _startY, 
				endX: _endX, 
				endY: _endY, 
				fillColor: _fillColor, 
				strokeColor: _strokeColor, 
				strokeSize: _strokeSize
			);
			Canvas.Invalidate();
		}
	}

	public class Drawable : IDrawable
	{
		private List<Shape> _shapes = new List<Shape>();

		public List<Shape> Shapes => _shapes;

		public void ClearList()
		{
			_shapes.Clear();
		}

		public void AddShape(string type, float startX, float startY, float endX, float endY, Color fillColor, Color strokeColor, float strokeSize)
		{
			_shapes.Add(new Shape
			{
				Type = type,
				StartX = startX,
				StartY = startY,
				EndX = endX,
				EndY = endY,
				FillColor = fillColor,
				StrokeColor = strokeColor,
				StrokeSize = strokeSize
			});
		}

		public void RemoveLastShape()
		{
			if (_shapes.Count > 0)
			{
				_shapes.RemoveAt(_shapes.Count - 1);
			}
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			foreach (var shape in _shapes)
			{
				float width = shape.EndX - shape.StartX;
				float height = shape.EndY - shape.StartY;

				switch (shape.Type)
				{
					case "rectangle":
						canvas.FillColor = shape.FillColor;
						canvas.FillRectangle(shape.StartX, shape.StartY, width, height);

						canvas.StrokeColor = shape.StrokeColor;
						canvas.StrokeSize = shape.StrokeSize;
						canvas.DrawRectangle(shape.StartX, shape.StartY, width, height);
						break;

					case "ellipse":
						canvas.FillColor = shape.FillColor;
						canvas.FillEllipse(shape.StartX, shape.StartY, width, height);

						canvas.StrokeColor = shape.StrokeColor;
						canvas.StrokeSize = shape.StrokeSize;
						canvas.DrawEllipse(shape.StartX, shape.StartY, width, height);
						break;

					case "circle":
						var radius = Math.Min(width, height) / 2;
						var centerX = shape.StartX + radius;
						var centerY = shape.StartY + radius;

						canvas.FillColor = shape.FillColor;
						canvas.FillEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);

						canvas.StrokeColor = shape.StrokeColor;
						canvas.StrokeSize = shape.StrokeSize;
						canvas.DrawEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);
						break;

					case "triangle":

						var a = (shape.StartX-shape.EndX);
						var b = (shape.StartY - shape.EndY);
						var c = Math.Sqrt(a*a + b*b);

						var d = shape.StartX+c;
						var e = shape.StartY+c;

						//Drag left to right
						if(shape.StartX>shape.EndX){
							d=shape.StartX-c;
						}

						//Drag up
						if(shape.StartY>shape.EndY){
							e=shape.StartY-c;
						}

						PathF path = new PathF();
						path.MoveTo(shape.StartX, shape.StartY);
						path.LineTo((float)d, (float)e);
						path.LineTo(shape.EndX, shape.EndY);
						path.LineTo(shape.StartX, shape.StartY);
						canvas.FillColor = shape.FillColor;
						canvas.StrokeColor = shape.StrokeColor;
						canvas.StrokeSize = shape.StrokeSize;
						canvas.FillPath(path);
						canvas.DrawPath(path);
						break;

					case "line":
						canvas.StrokeColor = shape.StrokeColor;
						canvas.StrokeSize = shape.StrokeSize;
						canvas.DrawLine(shape.StartX, shape.StartY, shape.EndX, shape.EndY);
						break;

					case "dragLine":
						canvas.StrokeColor = shape.StrokeColor;
						canvas.StrokeSize = shape.StrokeSize;
						canvas.DrawLine(shape.StartX, shape.StartY, shape.EndX, shape.EndY);
						break;

					default:
						break;
				}
			}
		}
	}

	public class Shape
	{
		public required string Type { get; set; }
		public float StartX { get; set; }
		public float StartY { get; set; }
		public float EndX { get; set; }
		public float EndY { get; set; }
		public required Color FillColor { get; set; }
		public required Color StrokeColor { get; set; }
		public float StrokeSize { get; set; }
	}
}