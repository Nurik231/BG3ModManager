using ReactiveUI;
using ReactiveUI.Fody;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace DivinityModManager
{
	public static class ReactiveExtensions
	{
		/// <summary>
		/// ToPropertyEx with deferSubscription set to true, and the default scheduler set to RxApp.MainThreadScheduler.
		/// </summary>
		/// <typeparam name="TObj"></typeparam>
		/// <typeparam name="TRet"></typeparam>
		/// <param name="obs"></param>
		/// <param name="source"></param>
		/// <param name="property"></param>
		/// <param name="initialValue"></param>
		/// <returns></returns>
		public static ObservableAsPropertyHelper<TRet> ToUIProperty<TObj, TRet>(this IObservable<TRet> obs, TObj source, Expression<Func<TObj, TRet>> property, TRet initialValue = default(TRet)) where TObj : ReactiveObject
		{
			return obs.ToPropertyEx(source, property, initialValue, true, RxApp.MainThreadScheduler);
		}
	}
}
