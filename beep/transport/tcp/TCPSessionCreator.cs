/*
* TCPSessionCreator.java  $Revision: 1.1 $ $Date: 2004/09/21 08:47:00 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2001 Huston Franklin.  All rights reserved.
*
* The contents of this file are subject to the Blocks Public License (the
* "License"); You may not use this file except in compliance with the License.
*
* You may obtain a copy of the License at http://www.beepcore.org/
*
* Software distributed under the License is distributed on an "AS IS" basis,
* WITHOUT WARRANTY OF ANY KIND, either express or implied.  See the License
* for the specific language governing rights and limitations under the
* License.
*
*/
using System;
using BEEPException = beepcore.beep.core.BEEPException;
using ProfileRegistry = beepcore.beep.core.ProfileRegistry;
namespace beepcore.beep.transport.tcp
{
	
	
	/// <summary>This class provides a means for applications or other libraries to create
	/// a TCP-based BEEP Session with another BEEP peer.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:47:00 $</version>
	public class TCPSessionCreator
	{
		
		// Constants
		private const int DEFAULT_TABLE_SIZE = 4;
		private const int DEFAULT_BACKLOG_SIZE = 100;
		private const string ERR_TCP_SOCKET_FAILURE = "Unable to create a TCP socket";
		private const string ERR_BIND_FAILURE = "Bind Failed";
		private const string ERR_CONNECT_FAILURE = "Connect Failed";
		private const string ERR_LISTEN_FAILURE = "Accept Failed";
		
		// Data
		private static System.Collections.Hashtable listenerSockets = null;
		
		/// <summary>Method initiate</summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession initiate(System.Net.IPAddress host, int port)
		{
			try
			{
				return TCPSession.createInitiator(new System.Net.Sockets.TcpClient(host.ToString(), port), new ProfileRegistry());
			}
			catch (System.IO.IOException x)
			{
				throw new BEEPException(x);
			}
		}
		
		/// <summary>Method initiate</summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="registry"></param>
		/// <param name="servername"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession initiate(System.Net.IPAddress host, int port, ProfileRegistry registry, string servername)
		{
			try
			{
				return TCPSession.createInitiator(new System.Net.Sockets.TcpClient(host.ToString(), port), registry, servername);
			}
			catch (System.IO.IOException x)
			{
				throw new BEEPException(x);
			}
		}
		
		/// <summary>Method initiate</summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="registry"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession initiate(System.Net.IPAddress host, int port, ProfileRegistry registry)
		{
			return initiate(host, port, registry, null);
		}
		
		/// <summary>Method initiate</summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession initiate(string host, int port)
		{
			try
			{
				return initiate(System.Net.Dns.GetHostByName(host).AddressList[0], port);
			}
			catch (System.Exception)
			{
				throw new BEEPException("Unable to connect, unkown host");
			}
		}
		
		/// <summary>Method initiate</summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="registry"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession initiate(string host, int port, ProfileRegistry registry)
		{
			try
			{
				return initiate(System.Net.Dns.GetHostByName(host).AddressList[0], port, registry);
			}
			catch (System.Exception)
			{
				throw new BEEPException("Unable to connect, unkown host");
			}
		}
		
		public static TCPSession initiate(string host, int port, ProfileRegistry registry, string servername)
		{
			try
			{
				return initiate(System.Net.Dns.GetHostByName(host).AddressList[0], port, registry, servername);
			}
			catch (System.Exception x)
			{
				throw new BEEPException("Unable to connect, unkown host", x);
			}
		}
		
		/// <summary>Method listen</summary>
		/// <param name="port"></param>
		/// <param name="registry"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession listen(int port, ProfileRegistry registry)
		{
			return listen((System.Net.IPAddress)null, port, registry);
		}
		
		/// <summary>Method listen</summary>
		/// <param name="localInterface"></param>
		/// <param name="port"></param>
		/// <param name="registry"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession listen(System.Net.IPAddress localInterface, int port, ProfileRegistry registry)
		{
			System.Net.Sockets.TcpListener socket = null;
			System.Net.Sockets.TcpClient peer = null;
			
			if (listenerSockets == null)
			{
				listenerSockets = new System.Collections.Hashtable(DEFAULT_TABLE_SIZE);
			}
			
			socket = (System.Net.Sockets.TcpListener) listenerSockets[System.Convert.ToString(port)];
			
			// Bind if we're not listening on this port
			if (socket == null)
			{
				// Bind to interface/port pair
				try
				{
					System.Net.Sockets.TcpListener temp_socket;
					temp_socket = new System.Net.Sockets.TcpListener(
							new System.Net.IPEndPoint(
								localInterface!=null ? localInterface : System.Net.IPAddress.Any,
								port));
					temp_socket.Start();
					socket = temp_socket;
					
					listenerSockets[port.ToString()] = socket;
				}
				catch (System.Exception x)
				{
					throw new BEEPException(x);
				}
			}
			
			// Listen
			try
			{
				peer = socket.AcceptTcpClient();
				
				return TCPSession.createListener(peer, registry);
			}
			catch (System.Exception e)
			{
				throw new BEEPException(e);
			}
		}
		
		/// <summary>Method listen</summary>
		/// <param name="port"></param>
		/// <param name="registry"></param>
		/// <param name="localInterface"></param>
		/// <exception cref="BEEPException" />
		public static TCPSession listen(string localInterface, int port, ProfileRegistry registry)
		{
			try
			{
				System.Net.IPAddress addr = null;
				
				if ((object) localInterface != null)
				{
					addr = System.Net.Dns.GetHostByName(localInterface).AddressList[0];
				}
				
				return listen(addr, port, registry);
			}
			catch (System.Exception x)
			{
				throw new BEEPException(x);
			}
		}
	}
}