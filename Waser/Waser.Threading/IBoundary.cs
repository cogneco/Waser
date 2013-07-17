using System;
namespace Waser.Threading
{
	public interface IBoundary
	{
		void ExecuteOnTargetLoop (Action action);
	}
}

