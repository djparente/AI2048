/*
 * Created by SharpDevelop.
 * User: Dan
 * Date: 2014-03-24
 * Time: 23:11
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Agent2048
{
	
	
	/// <summary>
	/// Description of State2048.
	/// </summary>
	public class State2048
	{
		static Random rng = new Random();
		
		public int [,] grid;
		public int rows;
		public int cols;
		
		//De novo constructor
		public State2048(int rows, int cols)
		{
			this.grid = new int[rows,cols];
			this.rows = rows;
			this.cols = cols;
		}
		
		//Copy constructor
		public State2048(State2048 s)
		{
			this.rows = s.rows;
			this.cols = s.cols;
			this.grid = new int[rows, cols];
			
			for(int r = 0; r < this.rows; r++)
				for(int c = 0; c < this.cols; c++)
					this.grid[r,c] = s.grid[r,c];
		}
		
		public void pushLeft()
		{
			for(int r = 0; r < this.rows; r++)
				pushRowLeft(r);
		}
		
		public void pushRight()
		{
			for(int r = 0; r < this.rows; r++)
				pushRowRight(r);
		}
		
		private void pushRowLeft(int r)
		{
			//Collect the items
			List<int> items = new List<int>(this.cols);
			for(int c = 0; c < cols; c++)
				if( this.grid[r,c] != 0 )
					items.Add(this.grid[r,c]);
			
			//Consolidate duplicates
			for(int i = 0; i < items.Count - 1; i++)
			{
				if( items[i] == items[i+1] )
				{
					items[i]++;
					items.RemoveAt(i+1);
				}
			}
			
			//Write the data back to the row
			for(int c = 0; c < cols; c++)
				this.grid[r,c] = (c < items.Count ? items[c] : 0);
		}
		
		private void pushRowRight(int r)
		{
			//Collect the items
			List<int> items = new List<int>(this.cols);
			for(int c = 0; c < cols; c++)
				if( this.grid[r,c] != 0 )
					items.Add(this.grid[r,c]);
			
			//Consolidate duplicates
			for(int i = items.Count - 1; i > 0; i--)
			{
				if( items[i] == items[i-1] )
				{
					items[i]++;
					items.RemoveAt(i-1);
					i--;
				}
			}
			
			//Write the data back to the row
			for(int i = 0; i < this.cols; i++)
				this.grid[r,this.cols - 1 - i] = (items.Count - 1 - i >= 0 ? items[items.Count - 1 - i] : 0);
		}

		public void pushUp()
		{
			for(int c = 0; c < this.cols; c++)
				pushColUp(c);
		}
		
		private void pushColUp(int c)
		{
			//Collect the items
			List<int> items = new List<int>(this.cols);
			for(int r = 0; r < rows; r++)
				if( this.grid[r,c] != 0 )
					items.Add(this.grid[r,c]);
			
			//Consolidate duplicates
			for(int i = 0; i < items.Count - 1; i++)
			{
				if( items[i] == items[i+1] )
				{
					items[i]++;
					items.RemoveAt(i+1);
				}
			}
			
			//Write the data back to the row
			for(int r = 0; r < rows; r++)
				this.grid[r,c] = (r < items.Count ? items[r] : 0);
		}
		
		public void pushDown()
		{
			for(int c = 0; c < this.cols; c++)
				pushColDown(c);
		}
		
		private void pushColDown(int c)
		{
			//Collect the items
			List<int> items = new List<int>(this.cols);
			for(int r = 0; r < rows; r++)
				if( this.grid[r,c] != 0 )
					items.Add(this.grid[r,c]);
			
			//Consolidate duplicates
			for(int i = items.Count - 1; i > 0; i--)
			{
				if( items[i] == items[i-1] )
				{
					items[i]++;
					items.RemoveAt(i-1);
					i--;
				}
			}
			
			//Write the data back to the row
			for(int i = 0; i < this.rows; i++)
				this.grid[this.cols - 1 - i,c] = (items.Count - 1 - i >= 0 ? items[items.Count - 1 - i] : 0);
		}
		
		private List<Tuple<int, int>> getFree()
		{
			List<Tuple<int,int>> free = new List<Tuple<int, int>>();
			for(int r = 0; r < rows; r++)
				for(int c = 0; c < cols; c++)
					if( this.grid[r,c] == 0 )
						free.Add(new Tuple<int,int>(r,c));
			
			return free;
		}
		
		public void spawnRandom()
		{
			List<Tuple<int,int>> free = getFree();
			if( free.Count == 0 )
				return;
			
			Tuple<int,int> target = free[rng.Next(0, free.Count)];
			this.grid[target.Item1, target.Item2] = (rng.NextDouble() < .9 ? 1 : 2);
		}

		
		public double rate()
		{
			List<StateTrans> moves = this.getAllMoveStates();
			
			//If we can't move, the game is over; worst penalty
			if( moves.Count == 0 )
				return double.MaxValue;

			
			int numZero = 0;
			double entropy = 0;
			
			Dictionary<int, int> count = new Dictionary<int, int>();
			
			int cur;
			for(int r = 0; r < rows; r++)
			{
				for(int c = 0; c < cols; c++)
				{
					
					
					if( grid[r,c] == 0 )
					{
						numZero++;
						continue;
					}
					
					if( !count.TryGetValue(grid[r,c], out cur) )
						count[grid[r,c]] = 1;
					else
						count[grid[r,c]] = cur + 1;
				}
			}
			
			int numNonZero = rows*cols - numZero;
			foreach(int k in count.Keys)
			{
				double freq = (double)count[k] / numNonZero;
				entropy -= freq * Math.Log(freq);
			}
			
			entropy /= Math.Log(rows*cols);
			
			return numNonZero + entropy;
		}
		
		public static double alphabetarate(State2048 root, int depth, double alpha, double beta, bool player)
		{	
			if( depth == 0 )
			{
				return root.rate();
			}
			
			if( player )
			{
				List<StateTrans> moves = root.getAllMoveStates();
			
				//If we can't move, the game is over; worst penalty
				if( moves.Count == 0 )
					return double.MaxValue;
				
				foreach(StateTrans st in moves)
				{
					alpha = Math.Min(alpha, alphabetarate(st.state, depth - 1, alpha, beta, !player));
					if ( beta >= alpha )
						break;
				}
				return alpha;
			}
			else
			{
				List<State2048> moves = root.getAllRandom();
				
				foreach(State2048 st in moves)
				{
					beta = Math.Max(beta, alphabetarate(st, depth - 1, alpha, beta, !player));
					if ( beta <= alpha )
						break;
				}
				return beta;
			}
		}
		
		public static List<RecRateResult> recRate(State2048 root, int depth)
		{
			List<StateTrans> moves = root.getAllMoveStates();
			
			//If we can't move, the game is over; worst penalty
			if( moves.Count == 0 )
				return new List<RecRateResult> { new RecRateResult(double.MaxValue, MoveDir.Self) };
			
			//If the depth is zero, return the result from this state
			if( depth == 0 )
				return new List<RecRateResult> { new RecRateResult(root.rate(), MoveDir.Self) };
			
			List<RecRateResult> result = new List<RecRateResult>();
			foreach(StateTrans st in moves)
			{
				double bestOfWorst = double.MaxValue;
				
				List<State2048> allRandom = st.state.getAllRandom();
				foreach(State2048 rand in allRandom )
				{
					List<RecRateResult> randRes = recRate(rand, depth-1);
					
					//Take the maximum penalty
					double worstRate = randRes[0].rating;
					for(int i = 1; i < randRes.Count; i++)
						if( randRes[i].rating > worstRate )
							worstRate = randRes[i].rating;
					
					if( worstRate < bestOfWorst )
						bestOfWorst = worstRate;
				}
				
				result.Add(new RecRateResult(bestOfWorst, st.dir));
			}
			
			return result;
		}
		
		public List<StateTrans> getAllMoveStates()
		{
			List<StateTrans> allMoves = new List<StateTrans>();
			
			State2048 next;

			next = new State2048(this);
			next.pushLeft();
			if( !this.equalTo(next) )
				allMoves.Add(new StateTrans(next, MoveDir.Left));
			
			next = new State2048(this);
			next.pushRight();
			if( !this.equalTo(next) )
				allMoves.Add(new StateTrans(next, MoveDir.Right));
			
			next = new State2048(this);
			next.pushUp();
			if( !this.equalTo(next) )
				allMoves.Add(new StateTrans(next, MoveDir.Up));
			
			next = new State2048(this);
			next.pushDown();
			if( !this.equalTo(next) )
				allMoves.Add(new StateTrans(next, MoveDir.Down));
			
			return allMoves;
		}
		
		public List<State2048> getAllRandom()
		{
			List<State2048> res = new List<State2048>();
			List<Tuple<int,int>> free = this.getFree();
			
			foreach(Tuple<int, int> x in free)
			{
				State2048 next;
				
				next = new State2048(this);
				next.grid[x.Item1, x.Item2] = 1;
				res.Add(next);
				
				next = new State2048(this);
				next.grid[x.Item1, x.Item2] = 2;
				res.Add(next);
			}
			
			return res;
		}
				
		public bool equalTo(State2048 alt)
		{
			if( this.rows != alt.rows )
				return false;
			if( this.cols != alt.cols )
				return false;
			
			for(int r = 0; r < rows; r++)
				for(int c = 0; c < cols; c++)
					if( this.grid[r,c] != alt.grid[r,c] )
						return false;
			
			return true;
		}
		
		public void display()
		{
			for(int r = 0; r < this.rows; r++)
			{
				for(int c = 0; c < this.cols; c++)
				{
					string number = "";
					
					if ( this.grid[r,c] != 0 )
					{
						number = ((int)Math.Pow(2,this.grid[r,c])).ToString();
					}
					
					Console.Write(number.PadLeft(5,' '));
				}
				Console.WriteLine();
			}
			Console.WriteLine(" ---- ---- ---- ----");
			Console.WriteLine("Rating: {0}", this.rate());
		}
		
	}
}
