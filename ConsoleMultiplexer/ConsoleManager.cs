﻿using ConsoleMultiplexer.Common;
using ConsoleMultiplexer.Data;
using ConsoleMultiplexer.Space;
using ConsoleMultiplexer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleMultiplexer
{
	public class ConsoleManager : IDrawingContextListener
	{
		private bool _frozen;

		private DrawingContext _contentContext = DrawingContext.Dummy;
		private DrawingContext ContentContext
		{
			get => _contentContext;
			set => Setter
				.SetDisposable(ref _contentContext, value)
				.Then(Initialize)
				.Then(Redraw);
		}

		private IControl _content;
		public IControl Content
		{
			get => _content;
			set => Setter
				.Set(ref _content, value)
				.Then(BindContent);
		}

		private Size _size;
		public Size Size
		{
			get => _size;
			set => Setter
				.Set(ref _size, ClipSize(value))
				.Then(Initialize)
				.Then(Redraw);
		}

		public ConsoleManager()
		{
			Size = new Size(Console.WindowWidth, Console.WindowHeight);
			Console.CursorVisible = false;
		}

		//Character[,] _memory = new Character[100, 30];

		private void Initialize()
		{
			ContentContext.SetLimits(Size, Size);
			Redraw();
		}

		private void Redraw()
		{
			Update(ContentContext.Size.AsRect());
		}

		private void Update(Rect rect)
		{
			Console.CursorVisible = false;

			CheckConsoleSize();

			foreach (var position in rect)
			{
				var c = ContentContext[position];

				//if (c.Content != _memory[position.X, position.Y].Content)
				{
					//_memory[position.X, position.Y] = c;

					var content = c.Content ?? ' ';
					var foreground = c.Foreground ?? Color.White;
					var background = c.Background ?? Color.Black;

					try
					{
						Console.SetCursorPosition(position.X, position.Y);
						Console.Write($"\x1b[38;2;{foreground.Red};{foreground.Green};{foreground.Blue}m\x1b[48;2;{background.Red};{background.Green};{background.Blue}m{content}");
					}
					catch (ArgumentOutOfRangeException)
					{ }
				}
			}

			Console.BackgroundColor = ConsoleColor.Black;
			Console.SetCursorPosition(0, 0);
		}

		private void CheckConsoleSize()
		{
			try
			{
				if (Console.BufferWidth != Console.WindowWidth || Console.BufferHeight != Console.WindowHeight)
					Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
			}
			catch (ArgumentOutOfRangeException) { }
			catch (System.IO.IOException) { }

			Size = new Size(Console.WindowWidth, Console.WindowHeight);
		}

		private void BindContent()
		{
			ContentContext = new DrawingContext(this, Content);
		}

		private static Size ClipSize(in Size size) => Size.Clip(new Size(1, 1), size, new Size(Console.LargestWindowWidth, Console.LargestWindowHeight));

		void IDrawingContextListener.OnRedraw(DrawingContext drawingContext)
		{
			Redraw();
		}

		void IDrawingContextListener.OnUpdate(DrawingContext drawingContext, Rect rect)
		{
			Update(rect);
		}

		public event SizeLimitsChangedHandler SizeLimitsChanged;
	}
}