using System;
using System.Collections.Generic;
using System.Text;

namespace Uragano.Abstractions
{
	public interface IUraganoConfiguration
	{

		void AddServer(string ip, int port, bool libuv = false);

		void AddServer(string ip, int port, string certificateUrl, string certificatePwd, bool libuv = false);

		void AddClient();
	}
}
