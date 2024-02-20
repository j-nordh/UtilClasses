using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;

namespace UtilClasses.Plugins {

	// Wraps an instance of TInterface. If the instance is a 
	// MarshalByRefObject, this class acts as a sponsor for its lifetime 
	// service (until disposed/finalized). Disposing the sponsor implicitly 
	// disposes the instance.

	[Serializable]
	[SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
	public sealed class Sponsor<T> : ISponsor, IDisposable where T : class {

		private T _instance;
		public T Instance {
			get
			{
			    if (IsDisposed)
					throw new ObjectDisposedException("Instance");
			    return _instance;
			}
			private set {
				_instance = value;
			}
		}
		public bool IsDisposed { get; private set; }

		public Sponsor(T instance) {
			Instance = instance;

		    var obj = Instance as MarshalByRefObject;
		    if (obj == null) return;
		    (RemotingServices.GetLifetimeService(obj) as ILease)?.Register(this);
		}

		~Sponsor() {
			Dispose(false);
		}

		// Disposes the sponsor and the instance it wraps.
		public void Dispose() {
            (Instance as IDisposable)?.Dispose();
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Disposes the sponsor and the instance it wraps.
		private void Dispose(bool disposing) {
		    if (IsDisposed) return;
		    if (disposing) {
		        (Instance as IDisposable)?.Dispose();

		        var byRefObject = Instance as MarshalByRefObject;
		        if (byRefObject != null) {
		            (RemotingServices.GetLifetimeService(byRefObject) as  ILease)?.Unregister(this);
		        }
		    }

		    Instance = null;
		    IsDisposed = true;
		}

		// Renews the lease on the instance as though it has been called normally.
		TimeSpan ISponsor.Renewal(ILease lease) => IsDisposed ? TimeSpan.Zero : LifetimeServices.RenewOnCallTime;
	}
}