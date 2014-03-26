/*
 * Created by SharpDevelop.
 * User: Dan
 * Date: 2014-03-24
 * Time: 23:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Agent2048
{
	class Program
	{
		public static Color hex2color(string hex)
		{
			int r = int.Parse(hex.Substring(0,2),System.Globalization.NumberStyles.HexNumber);
			int g = int.Parse(hex.Substring(2,2),System.Globalization.NumberStyles.HexNumber);
			int b = int.Parse(hex.Substring(4,2),System.Globalization.NumberStyles.HexNumber);
			
			return Color.FromArgb(255,r,g,b);
		}
			
		public static void Main(string[] args)
		{
			Dictionary<Color, int> colorMap = new Dictionary<Color, int>() { 
				{ hex2color("eee4da"), 1},
				{ hex2color("ede0c8"), 2},
				{ hex2color("f2b179"), 3},
				{ hex2color("f59563"), 4},
				{ hex2color("f67c5f"), 5},
				{ hex2color("f65e3b"), 6},
				{ hex2color("edcf72"), 7},
				{ hex2color("edcc61"), 8},
				{ hex2color("edc850"), 9},
				{ hex2color("edc53f"), 10},
				{ hex2color("edc22e"), 11},
				{ hex2color("3c3a32"), 12}

			};
			
			Point loc = estimateLocationOfGameOnScreen(Color.FromArgb(255,187,173,160));
			
			Console.WriteLine(loc);
			
			//Point loc = new Point(2300,400);
			Size size = new Size(500,500);
			
			
			
			//getStateFromScreen();
			
			//State2048 s = new State2048(4,4);
			
			
			State2048 initS = estimateBoardStateFromScreen(loc,size, colorMap);
			initS.display();
			
			for(int timeout = 10; timeout > 0; timeout--)
			{
				Console.WriteLine("Waiting... {0}", timeout);
				System.Threading.Thread.Sleep(1000);
			}
				
				
			while( true )
			{
				State2048 s = estimateBoardStateFromScreen(loc,size, colorMap);
				s.display();
				
				Console.WriteLine();
				
				double bestScore = double.MaxValue;
				List<StateTrans> movesBest = new List<StateTrans>();
				
				List<StateTrans> moves = s.getAllMoveStates();
				foreach(StateTrans move in moves) 
				{
					double rating = State2048.alphabetarate(move.state, 13, double.MaxValue, double.MinValue, false);
					Console.WriteLine("{0}\t{1}",move.dir, rating);
					
					if( rating < bestScore )
					{
						bestScore = rating;
						movesBest.Clear();
					}
					
					if( rating == bestScore )
					{
						movesBest.Add(move);
					}
				}
				
				if( movesBest.Count == 0 )
					break;
				
//				List<RecRateResult> res = State2048.recRate(s, 1);
//				foreach(RecRateResult record in res)
//				{
//					Console.WriteLine("{0}\t{1}",record.dir, record.rating);
//				}
				
				
				Console.CursorLeft = 0;
				Console.CursorTop = 0;
				
				//Send Keys - AI
				if( movesBest[0].dir == MoveDir.Left )
					System.Windows.Forms.SendKeys.SendWait("{LEFT}");
				
				if( movesBest[0].dir == MoveDir.Right )
					System.Windows.Forms.SendKeys.SendWait("{RIGHT}");
				
				if( movesBest[0].dir == MoveDir.Up )
					System.Windows.Forms.SendKeys.SendWait("{UP}");
				
				if( movesBest[0].dir == MoveDir.Down )
					System.Windows.Forms.SendKeys.SendWait("{DOWN}");
				
				System.Threading.Thread.Sleep(500);
				
				
				//AI
//				if( movesBest[0].dir == MoveDir.Left )
//					s.pushLeft();
//				
//				if( movesBest[0].dir == MoveDir.Right )
//					s.pushRight();
//				
//				if( movesBest[0].dir == MoveDir.Up )
//					s.pushUp();
//				
//				if( movesBest[0].dir == MoveDir.Down )
//					s.pushDown();
				
				//Manual
//				ConsoleKeyInfo key = Console.ReadKey();
//				if( key.Key == ConsoleKey.LeftArrow )
//					s.pushLeft();
//				
//				if( key.Key == ConsoleKey.RightArrow )
//					s.pushRight();
//				
//				if( key.Key == ConsoleKey.UpArrow )
//					s.pushUp();
//				
//				if( key.Key == ConsoleKey.DownArrow )
//					s.pushDown();
				
				//s.spawnRandom();
				
				
				Console.Clear();
				//s.display();
			}
			
			Console.Write("Game over");
			Console.ReadKey(true);
		}
		
		static Point estimateLocationOfGameOnScreen(Color targetColor)
		{
			Bitmap gameBmp = new Bitmap(3200,1080);
			Graphics g = Graphics.FromImage(gameBmp);
			g.CopyFromScreen(0, 0, 0, 0, gameBmp.Size);
			g.Flush();
			
			BitmapData locked = gameBmp.LockBits(new Rectangle(0,0,gameBmp.Width, gameBmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			
			bool found = false;
	        int r = -1;
	        int c = -1;
			
	        
	        
			unsafe
		    {
		        PixelData* pixelPtr = (PixelData*)(void*)locked.Scan0;
		        
		        //Iterate through rows and columns
		        for(int row = 0; row < gameBmp.Height & !found; row++)
		        {
		            for(int col = 0; col < gameBmp.Width & !found; col++)
		            {
		            	if( pixelPtr->R == targetColor.R && 
		            	    pixelPtr->G == targetColor.G && 
		            	    pixelPtr->B == targetColor.B)
		                {
		                	//found= true;
		                	if( r == -1 || ( col < c ) )
		                	{
			                	r = row;
			                	c = col;
		                	}
		                }
		                
		                //Update the pointer
		                pixelPtr++;
		            }
		        }
		    }
			
			return new Point(c,r);

			
			gameBmp.UnlockBits(locked);
			
		}
		
		static State2048 estimateBoardStateFromScreen(Point loc, Size size, Dictionary<Color, int> colorMap)
		{
			Bitmap gameBmp = new Bitmap(size.Width, size.Height);
			Graphics g = Graphics.FromImage(gameBmp);
			g.CopyFromScreen(loc, new Point(0,0), size);
			g.Flush();
			
			
			
			int xOffset = (int)(size.Width * .05);
			int yOffset = (int)(size.Height* .05);
			
			State2048 state = new State2048(4,4);
			for(int r = 0; r < state.rows; r++)
			{
				for(int c = 0; c < state.cols; c++)
				{
					int sampleX = c * size.Width / state.cols + xOffset;
					int sampleY = r * size.Height / state.rows + xOffset;
					
					int tileVal;
					if( colorMap.TryGetValue(gameBmp.GetPixel(sampleX, sampleY), out tileVal) )
						state.grid[r,c] = tileVal;
					else
						state.grid[r,c] = 0;
				}
			}
			
			return state;
		}
	}
	
}