namespace Sia.Physics;

public interface IComputeKernel<TContext>
{
    static abstract void Execute(in TContext context, WorkRange range);
}

