/*
* Bing.java  $Revision: 1.1 $ $Date: 2004/09/21 08:47:01 $
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
using beepcore.beep.core;
using beepcore.beep.lib;
using EchoProfile = beepcore.beep.profile.echo.EchoProfile;
//using TLSProfile = beepcore.beep.profile.tls.TLSProfile;
using TCPSessionCreator = beepcore.beep.transport.tcp.TCPSessionCreator;
using TCPSession = beepcore.beep.transport.tcp.TCPSession;
namespace beepcore.beep.example
{
	
	/// <summary> Sample client application analogous to ping. The application uses the
	/// echo profile (http://xml.resource.org/profiles/NULL/ECHO) to get ping
	/// like behavior. TLS is also supported for servers that only advertise
	/// the echo profile once TLS is negotiated.
	/// 
	/// </summary>
	/// <author>  Eric Dixon
	/// </author>
	/// <author>  Huston Franklin
	/// </author>
	/// <author>  Jay Kint
	/// </author>
	/// <author>  Scott Pead
	/// </author>
	/// <version>  $Revision: 1.1 $, $Date: 2004/09/21 08:47:01 $
	/// </version>
	public class Bing
	{
		
		[STAThread]
		public static void  Main(string[] argv)
		{
			
			// Parse command line args
			if (parseArgs(argv) == false)
			{
				System.Console.Out.WriteLine(usage);
				return ;
			}
			
			// Initiate a session with the server
			ISession session;
			try
			{
				session = TCPSessionCreator.initiate(host, port);
			}
			catch (BEEPException e)
			{
				System.Console.Error.WriteLine("bing: Error connecting to " + host + ":" + port + "\n\t" + e.Message);
				return ;
			}
			
			try
			{
				// Start TLS if requested
				if (privacy != PRIVACY_NONE)
				{
//					try
//					{
//						session = TLSProfile.getDefaultInstance().startTLS((TCPSession) session);
//					}
//					catch (BEEPException e)
//					{
//						System.Console.Error.WriteLine("bing: Error unable to start TLS.\n\t" + e.Message);
//						if (privacy == PRIVACY_REQUIRED)
//							return ;
//					}
				}
				
				// Start a channel for the echo profile
				IChannel channel;
				try
				{
					channel = session.startChannel(EchoProfile.ECHO_URI);
				}
				catch (BEEPError e)
				{
					if (e.Code == beep.core.BEEPStatusCode.REQUESTED_ACTION_NOT_TAKEN2)
					{
						System.Console.Error.WriteLine("bing: Error host does not support " + "echo profile");
					}
					else
					{
						System.Console.Error.WriteLine("bing: Error starting channel (" + e.Code + ": " + e.Message + ")");
					}
					return ;
				}
				catch (BEEPException e)
				{
					System.Console.Error.WriteLine("bing: Error starting channel (" + e.Message + ")");
					return ;
				}
				
				string request = createRequest(size);
				
				for (int i = 0; i < count; ++i)
				{
					long time;
					int replyLength = 0;
					Reply reply = new Reply();
					
					time = System.DateTime.Now.Ticks;
					
					try
					{
						// Send the request
						channel.sendMSG(new StringOutputDataStream(request), reply);
					}
					catch (BEEPException e)
					{
						System.Console.Error.WriteLine("bing: Error sending request (" + e.Message + ")");
						return ;
					}
					
					try
					{
						// Get the reply to the request
						System.IO.Stream ins = reply.getNextReply().DataStream.InputStream;
						
						// Read the data in the reply
						while (ins.ReadByte() != - 1)
						{
							++replyLength;
						}
					}
					catch (BEEPInterruptedException e)
					{
						System.Console.Error.WriteLine("bing: Error receiving reply (" + e.Message + ")");
						return ;
					}
					catch (System.IO.IOException e)
					{
						System.Console.Error.WriteLine("bing: Error receiving reply (" + e.Message + ")");
						return ;
					}
					
					System.Console.Out.WriteLine("Reply from " + host + ": bytes=" + replyLength + " time=" + (System.DateTime.Now.Ticks - time) / 10000 + "ms");
				}
				
				// Cleanup
				
				// Close the Channel
				try
				{
					channel.close();
				}
				catch (BEEPException e)
				{
					System.Console.Error.WriteLine("bing: Error closing channel (" + e.Message + ")");
				}
			}
			finally
			{
				// Close the Session
				try
				{
					session.close();
				}
				catch (BEEPException e)
				{
					System.Console.Error.Write("bing: Error closing session (" + e.Message + ")");
				}
			}
		}
		
		private static string createRequest(int size)
		{
			char[] c = new char[size];
			
			c[0] = 'a';
			
			for (int i = 1; i < c.Length; ++i)
			{
				c[i] = (char) (c[i - 1] + 1);
				
				if (c[i] > 'z')
				{
					c[i] = 'a';
				}
			}
			
			return new string(c);
		}
		
		private static bool parseArgs(string[] argv)
		{
			
			if (argv.Length < 1)
			{
				return false;
			}
			
			int i = 0;
			while (i < (argv.Length - 1))
			{
				if (argv[i].ToUpper().Equals("-port".ToUpper()))
				{
					port = System.Int32.Parse(argv[++i]);
				}
				else if (argv[i].ToUpper().Equals("-count".ToUpper()))
				{
					count = System.Int32.Parse(argv[++i]);
				}
				else if (argv[i].ToUpper().Equals("-size".ToUpper()))
				{
					size = System.Int32.Parse(argv[++i]);
				}
				else if (argv[i].ToUpper().Equals("-privacy".ToUpper()))
				{
					++i;
					if (argv[i].ToUpper().Equals("none".ToUpper()))
					{
						privacy = PRIVACY_NONE;
					}
					else if (argv[i].ToUpper().Equals("preferred".ToUpper()))
					{
						privacy = PRIVACY_PREFERRED;
					}
					else if (argv[i].ToUpper().Equals("required".ToUpper()))
					{
						privacy = PRIVACY_REQUIRED;
					}
					else
					{
						return false;
					}
				}
				++i;
			}
			
			if (i != argv.Length - 1)
				return false;
			
			host = argv[argv.Length - 1];
			
			return true;
		}
		
		private const int PRIVACY_NONE = 0;
		private const int PRIVACY_PREFERRED = 1;
		private const int PRIVACY_REQUIRED = 2;
		
		private static int count = 4;
		private static string host;
		private static int port = 10288;

		private static int privacy = PRIVACY_NONE;
		private static int size = 1024;
		
		private const string usage = "usage: bing [-port port] [-count count] [-size size]\n" 
			/*+ "            [-privacy required|preferred|none]"*/
			+ " host\n\n" 
			+ "options:\n" 
			+ "    -port port    Specifies the port number.\n" 
			+ "    -count count  Number of echo requests to send.\n" 
			+ "    -size size    Request size.\n"; 
		/*
			+ "    -privacy      required = require TLS.\n" 
			+ "                  preferred = request TLS.\n" 
			+ "                  none = don't request TLS.\n";
		*/
		// -mechanism none
	}
}