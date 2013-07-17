using System;

namespace Waser.Threading
{
	public static class BoundaryExtensions
	{
		public static void Synchronize (this IContext context, Action action)
		{
			Boundary.Instance.ExecuteOnTargetLoop (action);
		}
	}
}

