using Promises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Examples
{
	class Program
	{

		public static void Main(string[] args)
		{
			Promise<string> hello = CreateSimplePromise();
			hello.Success(mesg => Log(mesg));

			var webPromise = GetWebPagePromise("http://google.com");
			webPromise.Fail(error => Log(error.ToString()));
			webPromise.Success(x => Log("Web request successfully executed!"));

			var lastLinePromise = webPromise.FlatMap(webResponse => GetLastLine(webResponse.GetResponseStream()));
			lastLinePromise.Fail(error => Log(error.ToString()));
			lastLinePromise.Success(lastLine => Log("last line of the requested web page:\n{0}", lastLine));

			Func<int, int, string> slow = (x, y) => { System.Threading.Thread.Sleep(x); return "bla " + (x + y); };
			Action<string> cb = (s) => { Log(s); };

			var p = Promise.Wrap(slow, 40, 2);
			p.Success(cb);


			Console.ReadLine();
		}

		private static void Log(string format, params object[] args)
		{
			Console.WriteLine("{0:HH:mm:ss:fff}: {1}", DateTime.Now, string.Format(format, args));
		}

		/// <summary>
		/// Create a simple synchronous promise.
		/// </summary>
		public static Promise<string> CreateSimplePromise()
		{
			// The promise constructor takes a function that takes a callback. 
			// By passing it's own contruction function as the required callback, 
			// the promise can encapsulate the results returned via the callback.
			Action<Action<PromiseError, string>> constructor = (cb) =>
			{
				cb(null, "Hello World");
			};

			return new Promise<string>(constructor);
		}

		public static Promise<HttpWebResponse> GetWebPagePromise(string address)
		{
			Action<Action<PromiseError, HttpWebResponse>> constructor = (cb) =>
			{
				try
				{
					var request = HttpWebRequest.CreateHttp(address);
					cb(null, (HttpWebResponse)request.GetResponse());
				}
				catch (Exception ex)
				{
					cb(new PromiseError(ex), null);
				}
			};
			return new Promise<HttpWebResponse>(constructor);
		}

		public static Promise<string> GetLastLine(Stream stream)
		{
			Action<Action<PromiseError, string>> constructor = (cb) =>
			{
				try
				{
					string lastLine = null;
					using (stream)
					using (var sr = new StreamReader(stream))
					{
						while (!sr.EndOfStream)
						{
							lastLine = sr.ReadLine();
							//artificial delay
							System.Threading.Thread.Sleep(50);
						}
					}
					cb(null, lastLine);
				}
				catch (Exception ex)
				{
					cb(new PromiseError(ex), null);
				}
			};
			return new Promise<string>(constructor);
		}

	}
}
