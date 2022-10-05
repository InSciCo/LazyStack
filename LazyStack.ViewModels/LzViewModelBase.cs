using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace LazyStack.ViewModels
{
    public abstract class LzViewModelBase: ReactiveObject, IDisposable
    {

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Subscriptions?.Dispose();
            }
        }
        protected readonly CompositeDisposable Subscriptions = new CompositeDisposable();

        protected virtual string Log(MethodBase m, string msg)
        {
            var msgLoc = $"{m!.ReflectedType!.Name}.{m.Name}";
            msg = $"{msgLoc} failed {msg}";
            Console.WriteLine(msg);
            return msg;
        }

    }
}
