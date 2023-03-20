using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Patching.States
{
	/// <summary>
	/// A mirror of JumpKing.Player.InputComponent.State
	/// </summary>
	public struct InputState
	{
		public Point Dpad
		{
			get
			{
				int num = 0;
				if (this.Right)
				{
					num++;
				}
				if (this.Left)
				{
					num--;
				}
				return new Point(num, 0);
			}
		}

		public bool Left { get; private set; }
		public bool Right { get; private set; }
		public bool Jump { get; private set; }

		/// <summary>
		/// Ctor for creating an <see cref="InputState"/>
		/// </summary>
		public InputState(bool left, bool right, bool jump)
        {
			Left = left;
			Right = right;
			Jump = jump;
        }
	}
}
