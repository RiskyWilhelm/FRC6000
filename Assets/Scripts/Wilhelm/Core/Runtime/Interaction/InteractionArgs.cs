using System;

public class InteractionArgs : EventArgs, IDisposable
{
	public new static readonly InteractionArgs Empty;


	// Dispose
	public virtual void Dispose()
	{ }
}