using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
	/// <summary>
	/// Allgemeine nützliche ExtensionMethods für Tests
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Protected Method auf einer Instanz aufrufen
		/// </summary>
		/// <param name="target"></param>
		/// <param name="methodName"> </param>
		/// <param name="args"></param>
		/// <typeparam name="TReturn"></typeparam>
		/// <returns></returns>
		public static TReturn InvokeProtectedOrPrivateMethod<TReturn>(this object target, string methodName, params object[] args)
		{
			Type type = target.GetType();

			MethodInfo mi = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			while (mi == null && type != null)
			{
				type = type.BaseType;
				if (type != null) mi = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			}

			var erg = mi.Invoke(target, args);
			return (TReturn)erg;
		}
	}

}
