/*
 * Created by SharpDevelop.
 * User: Dan
 * Date: 2014-03-25
 * Time: 16:35
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Agent2048
{
	
	
	public class RecRateResult
	{
		
		public MoveDir dir;
		public double rating;
		
		public RecRateResult(double rating, MoveDir dir)
		{
			this.dir = dir;
			this.rating = rating;
		}
	}
}
